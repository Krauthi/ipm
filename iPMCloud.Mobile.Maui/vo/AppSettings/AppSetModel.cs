using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Devices;
using Microsoft.Maui.ApplicationModel;

namespace iPMCloud.Mobile.vo
{
    [Serializable]
    public class AppSetModel
    {
        //Letzter Stand für die Anzeige im Plan nur 0 = Beides oder 1 = Plan oder 2 = Müll
        private int _ViewOnlyMuell = 0;
        public int ViewOnlyMuell
        {
            get { return _ViewOnlyMuell; }
            set {
                _ViewOnlyMuell = value;
                AppSet.Save();
            }
        }

        public AppSetModel()
        {
        }
    }


    public class AppSet
    {
        public static bool Save()
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, AppModel.Instance.AppSetModel);
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/appset/");
                if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/appset/set.ipm");
                File.WriteAllBytes(filePath, ms.ToArray());
                ms.Close();
                ms.Dispose();
                return true;
            }
            catch (Exception)
            {
                ms.Close();
                ms.Dispose();
                return false;
            }
        }

        public static void Load()
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                AppSetModel obj;
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/appset/");
                if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/appset/set.ipm");
                if (File.Exists(filePath))
                {
                    byte[] data = File.ReadAllBytes(filePath);
                    BinaryFormatter binForm = new BinaryFormatter();
                    ms.Write(data, 0, data.Length);
                    ms.Seek(0, SeekOrigin.Begin);
                    obj = (Object)binForm.Deserialize(ms) as AppSetModel;
                    if (obj != null) { AppModel.Instance.AppSetModel = obj; }
                }
                ms.Close();
                ms.Dispose();
            }
            catch (Exception)
            {
                ms.Close();
                ms.Dispose();
            }
        }
        public static string Load_AsJson()
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                AppSetModel obj = null; 
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/appset/");
                if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/appset/set.ipm");
                if (File.Exists(filePath))
                {
                    byte[] data = File.ReadAllBytes(filePath);
                    BinaryFormatter binForm = new BinaryFormatter();
                    ms.Write(data, 0, data.Length);
                    ms.Seek(0, SeekOrigin.Begin);
                    obj = (Object)binForm.Deserialize(ms) as AppSetModel;
                }
                ms.Close();
                ms.Dispose();
                return obj != null ? JsonConvert.SerializeObject(obj) : "{null}";
            }
            catch (Exception)
            {
                ms.Close();
                ms.Dispose();
                return "{error}";
            }
        }

        public static bool Delete()
        {
            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/appset/set.ipm");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
