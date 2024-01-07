using System.Runtime.CompilerServices;
using Xabe.FFmpeg;

namespace AudioConverter;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
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
    }
}