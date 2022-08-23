using System.Security.Cryptography;
using System.Text;

namespace FileChunker
{
	internal class ThreadChunker
	{
		private readonly string _filePath;
		private readonly int _chunkSize;

		readonly object _locker = new object();

		public ThreadChunker(string filePath, int chunkSize)
		{
			this._filePath = filePath;
			this._chunkSize = chunkSize;
		}

		public void ChunkFile()
		{
			FileInfo fi = new FileInfo(_filePath);
			long fileSize = fi.Length;
			long chunkCount = fileSize / _chunkSize;
			if (chunkCount * _chunkSize < fileSize)
				chunkCount++;

			List<Task> tasks = new List<Task>();
			var fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read);
			var orderedOutputIndex = 0;

			for (long index = 0; index < chunkCount; index++)
			{
				var taskIndex = index;
				tasks.Add(Task.Run(() =>
				{
					var readChunk = ReadChunk(fileStream, taskIndex);
					var sha256 = ComputeSha256Hash(readChunk);

					lock (_locker)
						while (orderedOutputIndex != taskIndex)
							Monitor.Wait(_locker);

					Console.WriteLine($"{taskIndex + 1, 10}: {sha256}. (progress: {taskIndex*1.0/chunkCount:P})");

					lock (_locker)
					{
						Interlocked.Add(ref orderedOutputIndex, 1);
						Monitor.PulseAll(_locker);
					}
				}));
			}

			try
			{
				Task.WaitAll(tasks.ToArray());
				
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}
			finally
			{
				fileStream.Close();
				fileStream.Dispose();
			}
		}

		private byte[] ReadChunk(FileStream fileStream, long chunkNr)
		{
			var readBytes = 0;
			var buffer = new byte[_chunkSize];
			long chunkOffset = chunkNr * _chunkSize;

			Monitor.Enter(fileStream);
			fileStream.Seek(chunkOffset, SeekOrigin.Begin);
			readBytes = fileStream.Read(buffer, 0, _chunkSize);
			Monitor.Exit(fileStream);

			return buffer.Take(readBytes).ToArray();
		}

		private static string ComputeSha256Hash(byte[] rawData)
		{
			// Create a SHA256
			using SHA256 sha256Hash = SHA256.Create();

			// ComputeHash - returns byte array  
			byte[] bytes = sha256Hash.ComputeHash(rawData);

			// Convert byte array to a string
			StringBuilder builder = new StringBuilder();
			foreach (var singleByte in bytes)
			{
				builder.Append(singleByte.ToString("x2"));
			}
			return builder.ToString();
		}
	}
}
