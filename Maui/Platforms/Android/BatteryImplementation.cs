using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using iPMCloud.Mobile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Maui.Controls;

[assembly: Dependency(typeof(BatteryImplementation))]
namespace iPMCloud.Mobile
{
    public class BatteryImplementation : IBatteryInfo
    {
        public bool CheckIsEnableBatteryOptimizations()
        {

            PowerManager pm = (PowerManager)Android.App.Application.Context.GetSystemService(Context.PowerService);
            //enter you package name of your application
            bool result = pm.IsIgnoringBatteryOptimizations("com.ipmcloud.ipm.mobile");
            return result;
        }

        public void StartSetting()
        {
            var context = Android.App.Application.Context;

            // Fordert direkt den System-Dialog "Akkuoptimierung für <App> zulassen?" an,
            // statt nur die allgemeine Liste aller Apps zu öffnen. Benötigt die Permission
            // REQUEST_IGNORE_BATTERY_OPTIMIZATIONS (bereits im Manifest vorhanden).
            Intent intent = new Intent();
            intent.SetAction(Android.Provider.Settings.ActionRequestIgnoreBatteryOptimizations);
            intent.SetData(Android.Net.Uri.Parse("package:" + context.PackageName));
            intent.AddFlags(ActivityFlags.NewTask);
            context.StartActivity(intent);
        }
    }
}