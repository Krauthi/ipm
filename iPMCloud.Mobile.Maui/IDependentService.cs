using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace iPMCloud.Mobile
{
    public interface IDependentService
    {
        void Start();
        void Stop();

        StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId); 

    }
}