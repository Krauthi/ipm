namespace iPMCloud.Mobile;

public static partial class LocalApplicationDataBackupProtection
{
    static partial void ExcludePathFromBackup(string path)
    {
        // Android backup protection is configured via AndroidManifest.xml.
        // This partial exists so shared code can call the helper on all platforms.
    }
}
