using Foundation;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace iPMCloud.Mobile;

public static partial class LocalApplicationDataBackupProtection
{
    static partial void ExcludePathFromBackup(string path)
    {
        try
        {
            if (!Directory.Exists(path) && !File.Exists(path))
            {
                return;
            }

            var url = NSUrl.FromFilename(path);
            NSError error;
            var success = url.SetResource(
                NSUrl.IsExcludedFromBackupKey,
                NSNumber.FromBoolean(true),
                out error);

            if (!success && error != null)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Failed to exclude from backup: {path} - {error.LocalizedDescription}");
            }
        }
        catch(Exception ex) {

            System.Diagnostics.Debug.WriteLine(
                $"ExcludePathFromBackup failed: {path} - {ex.Message}");
        }
    }
}
