using System.Diagnostics;
using iPMCloud.Mobile.vo;
using Microsoft.Maui.ApplicationModel;

namespace iPMCloud.Mobile.Helpers
{
    public static class PermissionHelper
    {
        private const string LogPrefix = "[PermissionHelper]";

        public static bool IsGranted(PermissionStatus status) =>
            status == PermissionStatus.Granted || status == PermissionStatus.Restricted;

        private static void LogInfo(string message)
        {
            var logMessage = $"{LogPrefix} {message}";
            Debug.WriteLine(logMessage);
            AppModel.Logger?.Info(logMessage);
        }

        private static void LogError(string message, Exception ex)
        {
            var logMessage = $"{LogPrefix} {message}";
            Debug.WriteLine($"{logMessage}{Environment.NewLine}{ex}");
            AppModel.Logger?.Error(ex, logMessage);
        }

        private static async Task<PermissionStatus> EnsurePermissionAsync<TPermission>(
            string flowName,
            string permissionName)
            where TPermission : Permissions.BasePermission, new()
        {
            var status = await Permissions.CheckStatusAsync<TPermission>();
            //LogInfo($"{flowName}: {permissionName} status before request = {status}.");

            if (!IsGranted(status))
            {
                status = await Permissions.RequestAsync<TPermission>();
                //LogInfo($"{flowName}: {permissionName} status after request = {status}.");
            }

            return status;
        }

        public static async Task<bool> EnsureCameraPermissionAsync(string flowName, Func<Task> onDenied = null)
        {
            try
            {
                var status = await EnsurePermissionAsync<Permissions.Camera>(flowName, "Camera");
                var granted = IsGranted(status);
                if (!granted && onDenied != null)
                {
                    await onDenied();
                }

                return granted;
            }
            catch (Exception ex)
            {
                LogError($"{flowName}: Camera permission request failed.", ex);
                return false;
            }
        }

        public static async Task<bool> EnsurePhotosReadPermissionAsync(string flowName, Func<Task> onDenied = null)
        {
            try
            {
                var status = await EnsurePermissionAsync<Permissions.Photos>(flowName, "Photos");
                var granted = IsGranted(status);
                if (!granted && onDenied != null)
                {
                    await onDenied();
                }

                return granted;
            }
            catch (Exception ex)
            {
                LogError($"{flowName}: PhotosRead permission request failed.", ex);
                return false;
            }
        }
    }
}
