using iPMCloud.Mobile.vo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace iPMCloud.Mobile
{
    public class CheckLeistungAntwortBemImg
    {
        public Int32 bem_id = 0;
        public string bem_guid = null;
        public string guid = null;
        public string url = "";

        public string filename { get; set; } = "";







        public static bool Save(CheckLeistungAntwortBemImg b)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                var json = JsonConvert.SerializeObject(b);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, json);
                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" +
                    AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/check_nbi/");
                if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" +
                    AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/check_nbi/_" + b.guid + "_" + b.bem_guid + ".ipm");
                File.WriteAllBytes(filePath, ms.ToArray());
                ms.Close();
                ms.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                ms.Close();
                ms.Dispose();
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public static List<CheckLeistungAntwortBemImg> LoadFromGuid(string guid)
        {
            List<CheckLeistungAntwortBemImg> list = new List<CheckLeistungAntwortBemImg>();
            try
            {
                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" +
                    AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/check_nbi/");
                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath);
                    if (files != null && files.Length > 0)
                    {
                        files.ToList().ForEach(file =>
                        {
                            if (file.Contains(guid))
                            {
                                list.Add(CheckLeistungAntwortBemImg.Load(file));
                            }
                        });
                    }
                }
            }
            catch (Exception)
            {
            }
            return list;

        }
        public static CheckLeistungAntwortBemImg Load(string filename)
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
                    var imgWso = JsonConvert.DeserializeObject<CheckLeistungAntwortBemImg>(json);
                    imgWso.filename = filename;
                    return imgWso;
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
        public static bool Delete(CheckLeistungAntwortBemImg b)
        {
            try
            {
                //string filePath = Path.Combine(b.filename);
                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" +
                    AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/check_nbi/_" + b.guid + "_" + b.bem_guid + ".ipm");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }
        public static bool DeleteAllTemp()
        {
            try
            {
                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" +
                    AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/check_nbi/");
                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath);
                    if (files != null && files.Length > 0)
                    {
                        files.ToList().ForEach(file =>
                        {
                            if (File.Exists(file))
                            {
                                File.Delete(file);
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }



        public static bool SaveToStack(CheckLeistungAntwortBemImg b)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                var json = JsonConvert.SerializeObject(b);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, json);
                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" +
                    AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/check_nbis/");
                if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" +
                    AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/check_nbis/_" + b.guid + ".ipm");
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
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public static List<CheckLeistungAntwortBemImg> LoadAllFromStack()
        {
            List<CheckLeistungAntwortBemImg> list = new List<CheckLeistungAntwortBemImg>();
            try
            {
                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" +
                    AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/check_nbis/");
                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath);
                    if (files != null && files.Length > 0)
                    {
                        files.ToList().ForEach(file =>
                        {
                            list.Add(CheckLeistungAntwortBemImg.Load(file));
                        });
                    }
                }
            }
            catch (Exception)
            {
            }
            return list;
        }
        public static int CountFromStack()
        {
            try
            {
                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" +
                    AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/check_nbis/");
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
        public static bool DeleteFromStack(CheckLeistungAntwortBemImg pic)
        {
            try
            {
                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" +
                    AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/check_nbis/_" + pic.guid + ".ipm");
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