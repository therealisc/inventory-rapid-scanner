using System.IO;

namespace DScannerLibrary.Helpers;

public static class FileLoggerHelper
{
    public static void LogInfo(List<string> infoValue)
    {
        File.AppendAllLines($"{Directory.GetCurrentDirectory()}\\info_helper.log", infoValue);
    }
}
