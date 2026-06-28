using System;
using System.IO;

namespace iPMCloud.Mobile;

public static partial class LocalApplicationDataBackupProtection
{
    public static string LocalApplicationDataPath =>
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    public static void EnsureExcludedFromBackup()
    {
        var path = LocalApplicationDataPath;

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        ExcludePathFromBackup(path);
    }

    public static void EnsureExcludedFromBackup(string fileOrDirectoryPath)
    {
        if (string.IsNullOrWhiteSpace(fileOrDirectoryPath))
        {
            return;
        }

        ExcludePathFromBackup(fileOrDirectoryPath);
    }

    static partial void ExcludePathFromBackup(string path);
}
