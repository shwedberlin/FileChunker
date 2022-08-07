// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using FileChunker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

var configRoot = new ConfigurationBuilder()
	.AddCommandLine(args)
	.Build();

var serviceProvider = new ServiceCollection()
	.Configure<AppSettings>(o => configRoot.GetSection(nameof(AppSettings)).Bind(o))
	.AddTransient<App>()
	.AddLogging(loggingBuilder =>
	{
		loggingBuilder.ClearProviders();
		loggingBuilder.SetMinimumLevel(LogLevel.Trace);
		loggingBuilder.AddSimpleConsole(options =>
		{
			options.SingleLine = true;
		});
	})
	.BuildServiceProvider();


var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

try
{
	var app = serviceProvider.GetService<App>();
	var timer = new Stopwatch();
	timer.Start();
	await app.Run();
	timer.Stop();

	Console.WriteLine($"Elapsed: {timer.ElapsedMilliseconds}ms");
}
catch (Exception e)
{
	logger.LogError(e, e.Message);
}


static void CurrentDomainOnUnhandledException(
	object sender,
	UnhandledExceptionEventArgs e)
{
	Exception exceptionObject = e.ExceptionObject as Exception;

	Console.WriteLine(exceptionObject.Message);
	Console.WriteLine(exceptionObject.StackTrace);
	Console.ReadLine();
}