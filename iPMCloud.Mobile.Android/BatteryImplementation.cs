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
using Xamarin.Forms;

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
            Intent intent = new Intent();

            intent.SetAction(Android.Provider.Settings.ActionIgnoreBatteryOptimizationSettings);
            Forms.Context.StartActivity(intent);
            // StartActivity(intent);
        }
    }
}