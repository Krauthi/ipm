using iPMCloud.Mobile.vo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace iPMCloud.Mobile
{
    [Serializable]
    public class AllTransSignRequest
    {
        public string token = "";
        public string guid = "";
        public long ticks = 0;
        public long allTransSign = 0;
        public Int32 personid = 0;

        public AllTransSignRequest()
        {
        }
    }

    [Serializable]
    public class AllTransSignResponse
    {
        public bool success = false;
        public string message;
        public string guid = "";
        public long ticks = 0;
        public long allTransSign = 0;
        public Int32 personid = 0;

        public AllTransSignResponse()
        {
        }
    }

    [Serializable]
    public class AllTransSign
    {
        public AllTransSign()
        {
        }

        public static bool ToUploadStack(AllTransSignRequest pack)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                var json = JsonConvert.SerializeObject(pack);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, json);
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/trans/");
                if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/trans/t" + pack.ticks + ".ipm");
                File.WriteAllBytes(filePath, ms.ToArray());
                ms.Close();
                ms.Dispose();
                if (AppModel.Instance.MainPage != null) { AppModel.Instance.MainPage.SetAllSyncState(); }
                return true;
            }
            catch (Exception ex)
            {
                ms.Close();
                ms.Dispose();
                AppModel.Logger.Error(ex);
                return false;
            }
        }
        public static int CountFromStack()
        {
            try
            {
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/trans/");
                if (Directory.Exists(directoryPath))
                {
                    return Directory.GetFiles(directoryPath).Length;
                }
            }
            catch (Exception)
            {
                return 0;
            }
            return 0;
        }
        public static List<AllTransSignRequest> LoadAllFromUploadStack()
        {
            List<AllTransSignRequest> list = new List<AllTransSignRequest>();
            try
            {
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/trans/");
                if (Directory.Exists(directoryPath))
                {
                    //Directory.Delete(directoryPath,true);
                    var files = Directory.GetFiles(directoryPath);
                    if (files != null && files.Length > 0)
                    {
                        files.ToList().ForEach(file =>
                        {
                            list.Add(LoadFromUploadStack(file));
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error(ex);
            }
            return list;
        }
        private static AllTransSignRequest LoadFromUploadStack(string filename)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                string filePath = Path.Combine(filename);
                if (File.Exists(filePath))
                {
                    byte[] data = File.ReadAllBytes(filePath);
                    BinaryFormatter binForm = new BinaryFormatter();
                    ms.Write(data, 0, data.Length);
                    ms.Seek(0, SeekOrigin.Begin);
                    var json = (Object)binForm.Deserialize(ms) as String;
                    ms.Close();
                    ms.Dispose();
                    return JsonConvert.DeserializeObject<AllTransSignRequest>(json);
                }
                else
                {
                    ms.Close();
                    ms.Dispose();
                    return null;
                }
            }
            catch (Exception ex)
            {
                ms.Close();
                ms.Dispose();
                AppModel.Logger.Error(ex);
                return null;
            }
        }
        public static bool DeleteFromUploadStack(AllTransSignRequest pack)
        {
            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/trans/t" + pack.ticks + ".ipm");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    if (AppModel.Instance.MainPage != null) { AppModel.Instance.MainPage.SetAllSyncState(); }
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error(ex);
                return false;
            }
            return true;
        }



    }
}