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
            FFmpeg.SetExecutablesPath("/opt/homebrew/Cellar/ffmpeg/6.1/bin/");
        }
    }
}