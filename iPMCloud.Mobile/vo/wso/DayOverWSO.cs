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
    public class DayOverWSO
    {
        public Int32 gruppeid = 0;
        public Int32 personid = 0;

        public string guid = "";

        public long endticks = 0;
        // GPS
        public string latin = "";
        public string lonin = "";
        public string messagein = "";
        public string latout = "";
        public string lonout = "";
        public string messageout = "";

        public DayOverWSO()
        {
            this.guid = Guid.NewGuid().ToString();
        }



        public static bool Save(AppModel model, DayOverWSO day)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                var json = JsonConvert.SerializeObject(day);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, json);
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/dayover/");
                if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/dayover/" + day.endticks + ".ipm");
                File.WriteAllBytes(filePath, ms.ToArray());
                ms.Close();
                ms.Dispose();
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
        public static DayOverWSO LoadLast(AppModel model)
        {
            List<DayOverWSO> list = new List<DayOverWSO>();
            try
            {
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/dayover/");
                if (Directory.Exists(directoryPath))
                {
                    //Directory.Delete(directoryPath,true);
                    var files = Directory.GetFiles(directoryPath);
                    if (files != null && files.Length > 0)
                    {
                        files.ToList().ForEach(file =>
                        {
                            list.Add(DayOverWSO.Load(model, file));
                        });
                    }
                    list = list.OrderByDescending(d => d.endticks).ToList();
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error(ex);
            }
            if (list.Count > 0)
            {
                return list[0];
            }
            return null;
        }
        public static List<DayOverWSO> LoadAll(AppModel model)
        {
            List<DayOverWSO> list = new List<DayOverWSO>();
            try
            {
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/dayover/");
                if (Directory.Exists(directoryPath))
                {
                    //Directory.Delete(directoryPath,true);
                    var files = Directory.GetFiles(directoryPath);
                    if (files != null && files.Length > 0)
                    {
                        files.ToList().ForEach(file =>
                        {
                            list.Add(DayOverWSO.Load(model, file));
                        });
                    }
                    list = list.OrderByDescending(d => d.endticks).ToList();
                    if (list.Count > 20)
                    {
                        // nur die letzten 10 behalten
                        for (int i = 20; i < list.Count; i++)
                        {
                            Delete(model, list[i]);
                        }
                        list = LoadAll(model).OrderByDescending(d => d.endticks).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error(ex);
            }
            return list;
        }
        private static DayOverWSO Load(AppModel model, string filename)
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
                    return JsonConvert.DeserializeObject<DayOverWSO>(json);
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
        public static bool Delete(AppModel model, DayOverWSO day)
        {
            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/dayover/" + day.endticks + ".ipm");
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


        public static bool ToUploadStack(AppModel model, DayOverWSO day)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                var json = JsonConvert.SerializeObject(day);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, json);
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/dayoverupload/");
                if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/dayoverupload/" + day.endticks + ".ipm");
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
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/dayoverupload/");
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
        public static List<DayOverWSO> LoadAllFromUploadStack(AppModel model)
        {
            List<DayOverWSO> list = new List<DayOverWSO>();
            try
            {
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/dayoverupload/");
                if (Directory.Exists(directoryPath))
                {
                    //Directory.Delete(directoryPath,true);
                    var files = Directory.GetFiles(directoryPath);
                    if (files != null && files.Length > 0)
                    {
                        files.ToList().ForEach(file =>
                        {
                            list.Add(DayOverWSO.LoadFromUploadStack(model, file));
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
        private static DayOverWSO LoadFromUploadStack(AppModel model, string filename)
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
                    return JsonConvert.DeserializeObject<DayOverWSO>(json);
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
        public static bool DeleteFromUploadStack(AppModel model, DayOverWSO day)
        {
            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/dayoverupload/" + day.endticks + ".ipm");
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