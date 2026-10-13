using System.Runtime.InteropServices;

namespace DScannerLibrary.Helpers;

public static class DatabaseDirectoryHelper
{
    public static DirectoryInfo GetDatabaseDirectory(string dirPath)
    {
        var databaseDirectory = new DirectoryInfo(dirPath);
        return databaseDirectory;
    }

    private static DirectoryInfo? DatabaseDirectory = null;

    public static DirectoryInfo GetDatabaseDirectory()
    {
		if (DatabaseDirectory != null)
		{
			return DatabaseDirectory;
		}
		
		var drives = DriveInfo.GetDrives();
		var isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

		foreach (var drive in drives)
		{
	    	try
	    	{	
				if (drive.IsReady == false)
				{
					break;
				}
				// hardcoded path of the repo while building the app
				var sagaDirectory = SearchDirectory(drive.Name, "dev", 0);
				Console.WriteLine(sagaDirectory);
	    		sagaDirectory = "/home/runner/work/inventory-rapid-scanner/inventory-rapid-scanner/saga/1000";
				var sagaDirectoryInfo = new DirectoryInfo(sagaDirectory);
				return sagaDirectoryInfo;
	    	}
	    	catch (UnauthorizedAccessException)
	    	{
		    	continue;
	    	}
	    	catch (DirectoryNotFoundException)
	    	{
		    	continue;
	    	}
	    	catch (Exception)
	    	{
	        	throw;
	    	}
		}
	    throw new Exception("The SAGA C.3.0 missing");
    }

	private static string SearchDirectory(string rootDirectory, string directoryToSearch, int depth)
    {
		var directories = Directory.GetDirectories(rootDirectory);
        foreach (var directory in directories)
		{
	    	var directoryName = Path.GetFileName(directory);
			//Console.WriteLine(directoryName);
			
	    	if (directoryName == directoryToSearch)
	    	{
				return directory;
	    	}
			else
			{
				SearchDirectory(directory, directoryToSearch, depth + 1);
			}
		}
		
		return "";
    }
}
