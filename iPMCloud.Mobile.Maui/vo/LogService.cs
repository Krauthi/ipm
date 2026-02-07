using System;
using System.IO;
using System.Reflection;
using NLog;
using NLog.Config;
using Xamarin.Forms;

namespace iPMCloud.Mobile
{
    public class LogService : ILogService
    {
        public void Initialize(Assembly assembly, string assemblyName)
        {
            string resourcePrefix;

            if (Device.RuntimePlatform == Device.iOS)
                resourcePrefix = "iPMCloud.Mobile.iOS";
            else if (Device.RuntimePlatform == Device.Android)
                resourcePrefix = "iPMCloud.Mobile.Droid";
            else
                throw new Exception("Could not initialize Logger: Unknonw Platform");

            //var location = $"{assemblyName}.NLog.config";

            string location = $"{resourcePrefix}.NLog.config";
            Stream stream = assembly.GetManifestResourceStream(location);
            if (stream == null)
                throw new Exception($"The resource '{location}' was not loaded properly.");

            LogManager.Configuration = new XmlLoggingConfiguration(System.Xml.XmlReader.Create(stream), null);
        }
    }
}