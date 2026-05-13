using System;
using System.IO;
using Microsoft.Maui.Storage;

namespace iPMCloud.Mobile.vo
{

    public static class StorageMigration
    {
        private const string MigrationKey = "Migrated_ipm_To_LocalApplicationData_ipm_3000014";

        public static bool HasMigrateIpmFolder()
        {
            return Preferences.Default.Get(MigrationKey, false);
        }

        public static async Task<bool> MigrateIpmFolderAsync()
        {
            return await Task.Run(() =>
            {
                try 
                {
                    if (Preferences.Default.Get(MigrationKey, false))
                        return true;

                    string oldPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        "ipm");

                    string newPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "ipm");


                    if (!Directory.Exists(oldPath))
                    {
                        Preferences.Default.Set(MigrationKey, true);
                        return true;
                    }
                    //if (Directory.Exists(newPath) && Directory.GetFiles(newPath, "*", SearchOption.AllDirectories).Length > 0)
                    //{
                    //    Preferences.Default.Set(MigrationKey, true);
                    //    return true;
                    //}

                    Directory.CreateDirectory(newPath);
                    CopyDirectory(oldPath, newPath);

                    //Optional erst später aktivieren, wenn alles geprüft wurde:
                    //Directory.Delete(oldPath, true);

                    Preferences.Default.Set(MigrationKey, true);
                    return true;
                }
                catch(Exception ex)
                {
                    AppModel.Logger.Error("ERROR: StorageMigration: " + ex.Message + " - " + (ex.StackTrace ?? ""));
                    return false;
                }
            });
        }
          

        private static void CopyDirectory(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var fileName = Path.GetFileName(file);
                var targetFile = Path.Combine(targetDir, fileName);
                File.Copy(file, targetFile, true);
            }

            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                var dirName = Path.GetFileName(dir);
                var targetSubDir = Path.Combine(targetDir, dirName);
                CopyDirectory(dir, targetSubDir);
            }
        }
    }
}