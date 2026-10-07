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
			
				sagaDirectoryName = "saga";
				var rootDirectory = Environment.CurrentDirectory;
	    		var sagaDirectoryInfo = new DirectoryInfo(rootDirectory);
				Console.WriteLine(sagaDirectoryInfo);

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
}
