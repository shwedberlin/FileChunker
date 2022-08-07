using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace FileChunker;

public class App
{
	private readonly ILogger<App> _logger;
	private readonly AppSettings _appSettings;

	private readonly object _threadLock = new object();

	public App(IOptions<AppSettings> appSettings, ILogger<App> logger)
	{
		_logger = logger;
		_appSettings = appSettings?.Value ?? throw new ArgumentNullException(nameof(appSettings));
		CheckOptions();
	}

	public async Task Run()
	{
		_logger.LogTrace("Starting...");

		_logger.LogInformation($" {nameof(_appSettings.FilePath)}: {_appSettings.FilePath}");
		_logger.LogInformation($" {nameof(_appSettings.ChunkSize)}: {_appSettings.ChunkSize}");

		//TODO: ram consumption is ok, try to optimize cpu
		ChunkFile(_appSettings.FilePath);

		_logger.LogTrace("Finished!");

		await Task.CompletedTask;
	}

	private void CheckOptions()
	{
		if (_appSettings.ChunkSize <= 0 || string.IsNullOrEmpty(_appSettings.FilePath))
			throw new ArgumentException($"Please define both arguments: {nameof(_appSettings.FilePath)} and {nameof(_appSettings.ChunkSize)}");

		//TODO: check if file exists and chunk size is: 0 < chunkSize < fileSize
	}

	private void ChunkFile(string filePath)
	{
		FileInfo fi = new FileInfo(_appSettings.FilePath);
		long fileSize = fi.Length;
		long chunkCount = fileSize / _appSettings.ChunkSize;
		if (chunkCount * _appSettings.ChunkSize < fileSize)
			chunkCount++;

		using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);

		var options = new ParallelOptions()
		{
			MaxDegreeOfParallelism = 5
		};

		Parallel.For(0, (int)chunkCount, options, index =>
		{
			var chunkData = ReadChunk(fs, index, (int)_appSettings.ChunkSize);
			var sha256 = ComputeSha256Hash(chunkData);
			Console.WriteLine($"{index}: {sha256}. (thread: {Thread.CurrentThread.ManagedThreadId})");
		});
	}

	private byte[] ReadChunk(FileStream fileStream, int chunkNr, int chunkSize)
	{
		var buffer = new byte[chunkSize];
		var readBytes = 0;
		long chunkOffset = (long)chunkNr * chunkSize;
		//TODO: check if lock needed
		lock (_threadLock)
		{
			// Set the stream position to the beginning of the file.
			fileStream.Seek(chunkOffset, SeekOrigin.Begin);
			readBytes = fileStream.Read(buffer, 0, chunkSize);
		}

		//for last chunk
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
