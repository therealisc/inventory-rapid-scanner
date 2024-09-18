using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace DScannerLibrary.Helpers;

public static class DatabaseDirectoryHelper
{
    public static DirectoryInfo GetDatabaseDirectory(string dirPath)
    {
        var databaseDirectory = new DirectoryInfo(dirPath);
        return databaseDirectory;
    }

    private static DirectoryInfo DatabaseDirectory = null;

    public static DirectoryInfo GetDatabaseDirectory()
    {
	if (DatabaseDirectory != null)
	    return DatabaseDirectory;

	var sagaDirectoryName = "SAGA C.3.0";
	var drives = DriveInfo.GetDrives();

	foreach (var drive in drives)
	{
	    try
	    {
		if (drive.IsReady == false)
		   break;

		var rootDirectory = drive.RootDirectory.FullName;

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
	    	    rootDirectory = "/home/therealisc";

	        var sagaDirectory = SearchDirectory(rootDirectory, sagaDirectoryName, 0);

		if (string.IsNullOrEmpty(sagaDirectory))
		    continue;

	        var sagaDirectoryInfo = new DirectoryInfo(sagaDirectory);

        	DatabaseDirectory = sagaDirectoryInfo.GetDirectories()
		    .Where(x => Regex.IsMatch(x.Name, @"^\d{4}$"))
		    .OrderByDescending(x => x.Name)
		    .First();

		return DatabaseDirectory;
	    }
	    catch (UnauthorizedAccessException)
	    {
		continue;
	    }
	    catch (DirectoryNotFoundException)
	    {
		continue;
	    }
	    catch (Exception ex)
	    {
	        throw ex;
	    }
	}
	throw new Exception("The SAGA C.3.0 directory was not found.");
    }

    static string SearchDirectory(string rootDirectory, string directoryToSearch, int depth)
    {
	if (depth > 4)
	    return string.Empty;

        foreach (var directory in Directory.GetDirectories(rootDirectory))
	{
	    var directoryName = Path.GetFileName(directory);
	    var directoryFound = directoryName.Equals(directoryToSearch, StringComparison.InvariantCulture);

            Console.WriteLine(directory);

	    if (directoryFound)
	    {
		return directory;
	    }

	    SearchDirectory(directory, directoryToSearch, depth + 1);
	}

	return string.Empty;
    }
}
