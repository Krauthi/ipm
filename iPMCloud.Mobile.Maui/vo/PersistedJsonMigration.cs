using System;
using System.IO;
using System.Formats.Nrbf;
using Newtonsoft.Json;

namespace iPMCloud.Mobile.vo
{
    internal static class PersistedJsonMigration
    {
        public static bool TryLoadWithLegacyMigration<T>(
            string filePath,
            JsonSerializerSettings jsonSettings,
            string logContext,
            Func<T, bool> saveMigrated,
            out T value)
        {
            value = default;

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return false;
            }

            try
            {
                string jsonString = File.ReadAllText(filePath);

                if (string.IsNullOrWhiteSpace(jsonString))
                {
                    return false;
                }

                value = JsonConvert.DeserializeObject<T>(jsonString, jsonSettings);
                return value != null;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Warn(ex, $"{logContext}: Failed to deserialize JSON, attempting legacy migration - {filePath}");
            }

            if (!TryLoadLegacyBinaryJson(filePath, jsonSettings, out value, out Exception legacyFailure))
            {
                if (legacyFailure != null)
                {
                    AppModel.Logger?.Warn(legacyFailure, $"{logContext}: Legacy migration failed - {filePath}");
                }

                return false;
            }

            TryBackupLegacyFile(filePath, logContext);

            try
            {
                if (saveMigrated?.Invoke(value) != false)
                {
                    AppModel.Logger?.Info($"{logContext}: Migrated legacy data file to JSON - {filePath}");
                }
                else
                {
                    AppModel.Logger?.Warn($"{logContext}: Legacy data loaded but saving migrated JSON failed - {filePath}");
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, $"{logContext}: Legacy data loaded but saving migrated JSON threw an exception - {filePath}");
            }

            return true;
        }

        private static bool TryLoadLegacyBinaryJson<T>(
            string filePath,
            JsonSerializerSettings jsonSettings,
            out T value,
            out Exception failure)
        {
            value = default;
            failure = null;

            try
            {
                using FileStream stream = File.OpenRead(filePath);

                if (!NrbfDecoder.StartsWithPayloadHeader(stream))
                {
                    failure = new InvalidDataException(
                        "File is not a recognized legacy binary (NRBF) format. " +
                        "The file may be corrupted or written by an unknown serializer.");
                    return false;
                }

                stream.Position = 0;

                SerializationRecord record = NrbfDecoder.Decode(stream);
                if (record is not PrimitiveTypeRecord<string> stringRecord ||
                    string.IsNullOrWhiteSpace(stringRecord.Value))
                {
                    failure = new InvalidDataException(
                        $"Legacy NRBF payload does not contain a serialized JSON string " +
                        $"(actual record type: {record?.GetType().Name ?? "null"}). " +
                        "The payload format is not supported by the current migration path.");
                    return false;
                }

                value = JsonConvert.DeserializeObject<T>(stringRecord.Value, jsonSettings);
                return value != null;
            }
            catch (Exception ex)
            {
                failure = ex;
                return false;
            }
        }

        private static void TryBackupLegacyFile(string filePath, string logContext)
        {
            try
            {
                string backupPath = filePath + $".old_binary_{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}_{Guid.NewGuid():N}";
                File.Copy(filePath, backupPath, true);
                AppModel.Logger?.Info($"{logContext}: Legacy backup created - {backupPath}");
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Warn(ex, $"{logContext}: Failed to create legacy backup - {filePath}");
            }
        }
    }
}
