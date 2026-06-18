using System;

namespace iPMCloud.Mobile;

public static partial class LocalApplicationDataBackupProtection
{
    static partial void ExcludePathFromBackup(string path)
    {
        if (!Directory.Exists(path) && !File.Exists(path))
        {
            return;
        }

        var url = NSUrl.FromFilename(path);
        NSError error;
        var success = url.SetResource(
            NSNumber.FromBoolean(true),
            NSUrl.IsExcludedFromBackupKey,
            out error);

        if (!success && error != null)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Failed to exclude from backup: {path} - {error.LocalizedDescription}");
        }
    }
}
