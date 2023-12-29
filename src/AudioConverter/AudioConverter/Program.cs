using AudioConverter;
using Spectre.Console.Cli;

var app = new CommandApp<ConvertCommand>();
return await app.RunAsync(args);
