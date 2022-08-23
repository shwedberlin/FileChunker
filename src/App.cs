using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace FileChunker;

public class App
{
	private readonly ILogger<App> _logger;
	private readonly AppSettings _appSettings;

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
		
		new ThreadChunker(_appSettings.FilePath, _appSettings.ChunkSize).ChunkFile();

		_logger.LogTrace("Finished!");

		await Task.CompletedTask;
	}

	private void CheckOptions()
	{
		if (_appSettings.ChunkSize <= 0 || string.IsNullOrEmpty(_appSettings.FilePath))
			throw new ArgumentException($"Please define both arguments: {nameof(_appSettings.FilePath)} and {nameof(_appSettings.ChunkSize)}");

		if(!File.Exists(_appSettings.FilePath))
			throw new ArgumentException($"File {nameof(_appSettings.FilePath)} not exists");

		var fileSize = new FileInfo(_appSettings.FilePath).Length;
		if (_appSettings.ChunkSize > fileSize)
			throw new ArgumentOutOfRangeException($"File {fileSize/1024}kb, Chunk: {_appSettings.ChunkSize/1024}kb");
	}
}
