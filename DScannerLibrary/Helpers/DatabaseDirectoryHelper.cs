using System.Text.RegularExpressions;

namespace DScannerLibrary.Helpers;

public static class DatabaseDirectoryHelper
{
    public static DirectoryInfo GetDatabaseDirectory()
    {
        // TODO urgent refactoring
        //var sagaDbfsPath = Directory.GetFiles("C:\\SAGA C.3.0\\").First(x => x.);
        var dirInfo = new DirectoryInfo("C:\\SAGA C.3.0\\");
        var databaseDirectory = dirInfo.GetDirectories()
            .Where(x => Regex.IsMatch(x.Name, @"^\d{4}$"))
            .OrderByDescending(x => x.Name)
            .First();

        return databaseDirectory;
    }
}
