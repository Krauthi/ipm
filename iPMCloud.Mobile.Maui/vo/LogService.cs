using Microsoft.Maui.Controls;
using NLog;
using NLog.Config;
using System;
using System.IO;
using System.Reflection;
using System.Xml;

namespace iPMCloud.Mobile
{
    public class LogService : ILogService
    {
        public void Initialize_Old(Assembly assembly, string assemblyName)
        {
            //string resourcePrefix;

            //if (DeviceInfo.Platform == DevicePlatform.iOS)
            //    resourcePrefix = "iPMCloud.Mobile.iOS";
            //else if (DeviceInfo.Platform == DevicePlatform.Android)
            //    resourcePrefix = "iPMCloud.Mobile.Droid";
            //else
            //    throw new Exception("Could not initialize Logger: Unknonw Platform");

            ////var location = $"{assemblyName}.NLog.config";

            //string location = $"{resourcePrefix}.NLog.config";
            //Stream stream = assembly.GetManifestResourceStream(location);
            //if (stream == null)
            //    throw new Exception($"The resource '{location}' was not loaded properly.");

            //LogManager.Configuration = new XmlLoggingConfiguration(System.Xml.XmlReader.Create(stream), null);
        }
    

     public void Initialize(Assembly assembly, string assemblyName)
        {
            string resourcePrefix =
                DeviceInfo.Platform == DevicePlatform.iOS ? "iPMCloud.Mobile.iOS" :
                DeviceInfo.Platform == DevicePlatform.Android ? "iPMCloud.Mobile.Droid" :
                throw new Exception("Could not initialize Logger: Unknown Platform");

            var location = $"{resourcePrefix}.NLog.config";

            using var stream = assembly.GetManifestResourceStream(location);
            if (stream == null)
                throw new Exception($"The resource '{location}' was not loaded properly.");

            using var streamReader = new StreamReader(stream);
            string xmlContent = streamReader.ReadToEnd();

            // NLog 5+ (empfohlen in .NET MAUI)
            LogManager
                .Setup()
                .LoadConfigurationFromXml(xmlContent);
        }
    }
}