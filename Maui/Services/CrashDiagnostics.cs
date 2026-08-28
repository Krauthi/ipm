using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Storage;

namespace iPMCloud.Mobile.Services
{
    /// <summary>
    /// Persists lightweight "breadcrumbs" across process death.
    ///
    /// Some crashes (e.g. Android killing the app process while an external
    /// Activity such as the Camera/Gallery is in the foreground under memory
    /// pressure) never raise a catchable .NET exception, so normal try/catch
    /// logging never fires. <see cref="Preferences"/> is backed by native
    /// SharedPreferences (Android) / NSUserDefaults (iOS) and survives a
    /// process kill, so we can write a marker right before the risky
    /// operation and check for a "dangling" marker on the next app start.
    /// If the marker is still there, the previous session never reached the
    /// matching <see cref="End"/> call - almost certainly because the OS
    /// killed the process in between.
    /// </summary>
    public static class CrashDiagnostics
    {
        private const string KeyPrefix = "crashdiag_op_";
        // IPreferences has no API to enumerate all stored keys, so we track
        // the set of currently pending operation ids explicitly in one key.
        private const string PendingIndexKey = "crashdiag_pending_ops";
        private const char IndexSeparator = ';';

        /// <summary>
        /// Call right before a risky operation (e.g. MediaPicker, Camera).
        /// </summary>
        public static void Begin(string operationId, string details)
        {
            try
            {
                Preferences.Default.Set(KeyPrefix + operationId,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}|{details}");
                AddToPendingIndex(operationId);
            }
            catch
            {
                // Never let diagnostics crash the app.
            }
        }

        /// <summary>
        /// Call in a finally block once the risky operation has completed
        /// (successfully or via a caught exception).
        /// </summary>
        public static void End(string operationId)
        {
            try
            {
                Preferences.Default.Remove(KeyPrefix + operationId);
                RemoveFromPendingIndex(operationId);
            }
            catch
            {
                // Never let diagnostics crash the app.
            }
        }

        private static void AddToPendingIndex(string operationId)
        {
            var pending = GetPendingIds();
            if (!pending.Contains(operationId))
            {
                pending.Add(operationId);
                Preferences.Default.Set(PendingIndexKey, string.Join(IndexSeparator, pending));
            }
        }

        private static void RemoveFromPendingIndex(string operationId)
        {
            var pending = GetPendingIds();
            if (pending.Remove(operationId))
            {
                Preferences.Default.Set(PendingIndexKey, string.Join(IndexSeparator, pending));
            }
        }

        private static List<string> GetPendingIds()
        {
            var raw = Preferences.Default.Get(PendingIndexKey, string.Empty);
            return string.IsNullOrEmpty(raw)
                ? new List<string>()
                : raw.Split(IndexSeparator, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        /// <summary>
        /// Call once during app startup (e.g. App.InitApp). Logs a warning
        /// for every operation that was started but never finished in the
        /// previous session, then clears it.
        /// </summary>
        public static void CheckForUnfinishedOperations()
        {
            try
            {
                var pending = GetPendingIds();
                foreach (var operationId in pending)
                {
                    var value = Preferences.Default.Get(KeyPrefix + operationId, string.Empty);

                    iPMCloud.Mobile.vo.AppModel.Logger?.Warn(
                        $"CrashDiagnostics: Vorherige Sitzung wurde w\u00e4hrend '{operationId}' unerwartet beendet " +
                        $"(vermutlich Prozess-Kill durch das Betriebssystem, z.B. wegen Speichermangel " +
                        $"w\u00e4hrend Kamera/Galerie im Vordergrund war). Details: {value}");

                    Preferences.Default.Remove(KeyPrefix + operationId);
                }

                if (pending.Count > 0)
                {
                    Preferences.Default.Remove(PendingIndexKey);
                }
            }
            catch (Exception ex)
            {
                iPMCloud.Mobile.vo.AppModel.Logger?.Error($"CrashDiagnostics.CheckForUnfinishedOperations failed: {ex.Message}");
            }
        }
    }
}
