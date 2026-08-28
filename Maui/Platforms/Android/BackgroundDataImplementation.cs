using Android.App;
using Android.Content;
using Android.Net;
using iPMCloud.Mobile;
using Microsoft.Maui.Controls;

[assembly: Dependency(typeof(BackgroundDataImplementation))]
namespace iPMCloud.Mobile
{
    public class BackgroundDataImplementation : IBackgroundDataInfo
    {
        public bool IsBackgroundDataRestricted()
        {
            var context = Android.App.Application.Context;
            var connectivityManager = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);

            if (connectivityManager == null)
            {
                return false;
            }

            // RestrictBackgroundStatus wurde ab API 24 (Android 7.0) eingeführt.
            if ((int)Android.OS.Build.VERSION.SdkInt < 24)
            {
                return false;
            }

            // "Disabled" bedeutet: Hintergrunddatennutzung ist NICHT eingeschränkt (also erlaubt).
            // "Enabled" oder "Whitelisted" bedeuten, dass Data Saver aktiv ist bzw. die App
            // von den Einschränkungen ausgenommen ist - hier interessiert primär, ob der Nutzer
            // die Hintergrunddatennutzung pro App deaktiviert hat, was Android als
            // RestrictBackgroundStatus.Enabled meldet, wenn der globale Data Saver aktiv ist,
            // bzw. sich separat über AppOpsManager (OP_RUN_IN_BACKGROUND) abbilden lässt.
            return connectivityManager.RestrictBackgroundStatus == RestrictBackgroundStatus.Enabled;
        }

        public void StartSetting()
        {
            var context = Android.App.Application.Context;
            var intent = new Intent(Android.Provider.Settings.ActionApplicationDetailsSettings);
            intent.SetData(Android.Net.Uri.Parse("package:" + context.PackageName));
            intent.AddFlags(ActivityFlags.NewTask);
            context.StartActivity(intent);
        }
    }
}
