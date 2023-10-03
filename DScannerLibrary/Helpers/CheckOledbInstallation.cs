using Microsoft.Win32;

namespace DScannerLibrary.Helpers;

public class CheckOledbInstallation
{
    static bool IsInstalled()
    {
        return Registry.ClassesRoot.OpenSubKey("TypeLib\\{50BAEECA-ED25-11D2-B97B-000000000000}") != null;
    }
}
