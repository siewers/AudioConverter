using System.Globalization;
using System.Runtime.CompilerServices;
using Xabe.FFmpeg;

namespace AudioConverter;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    public static void InitializeFFmpeg()
    {
        // TODO: Make this configurable or detect automatically
        if (OperatingSystem.IsMacOS())
        {
            FFmpeg.SetExecutablesPath("/opt/homebrew/bin/");
        }
        else if (OperatingSystem.IsLinux())
        {
            FFmpeg.SetExecutablesPath("/var/packages/ffmpeg6/target/bin/");
        }
        else if (OperatingSystem.IsWindows())
        {
            var paths = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User)!
                                   .Split(';')
                                   .Select(Environment.ExpandEnvironmentVariables);

            var ffmpegPath = paths.FirstOrDefault(p => File.Exists(Path.Combine(p, "ffmpeg.exe")));

            if (ffmpegPath is not null)
            {
                FFmpeg.SetExecutablesPath(ffmpegPath);
            }
            else
            {
                throw new InvalidOperationException("FFmpeg not found in PATH");
            }
        }
    }
}