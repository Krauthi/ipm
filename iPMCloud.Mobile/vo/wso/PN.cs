using iPMCloud.Mobile.vo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace iPMCloud.Mobile
{
    [Serializable]
    public class PN
    {
        public string id;
        public DateTime? datum = null; // Erstell Datum
        public string titel = "";
        public string beschreibung = "";
        public string data = "";
        public string status = "Neu";

        public PN()
        {
        }

        public static bool Save(PN pn)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, pn);
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" +
                    AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/pn/");
                if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" +
                    AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/pn/" + pn.id + ".ipm");
                File.WriteAllBytes(filePath, ms.ToArray());
                ms.Close();
                ms.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                ms.Close();
                ms.Dispose();
                AppModel.Logger.Error("Save PN - " + ex);
                return false;
            }
        }


        public static List<PN> LoadAll()
        {
            List<PN> list = new List<PN>();
            try
            {
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" +
                    AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/pn/");
                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath);
                    if (files != null && files.Length > 0)
                    {
                        files.ToList().ForEach(file =>
                        {
                            file = file.Substring(file.LastIndexOf('/') + 1);
                            var loadedPN = PN.Load((file.Replace(".ipm", "")));
                            if (loadedPN != null)
                            {
                                list.Add(loadedPN);
                            }
                        });
                    }
                    list = list.OrderByDescending(d => d.datum).ToList();
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error("LoadAll PN - " + ex);
            }
            return list;
        }

        public static PN Load(string id)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                if (!String.IsNullOrWhiteSpace(AppModel.Instance.SettingModel.SettingDTO.CustomerNumber))
                {
                    string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" +
                        AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/pn/");
                    if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                    string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" +
                        AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/pn/" + id + ".ipm");
                    if (File.Exists(filePath))
                    {
                        byte[] data = File.ReadAllBytes(filePath);
                        BinaryFormatter binForm = new BinaryFormatter();
                        ms.Write(data, 0, data.Length);
                        ms.Seek(0, SeekOrigin.Begin);
                        var pn = (Object)binForm.Deserialize(ms) as PN;
                        ms.Close();
                        ms.Dispose();
                        return pn;
                    }
                    else
                    {
                        ms.Close();
                        ms.Dispose();
                        return null;
                    }
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
                AppModel.Logger.Error("Load PN - " + ex);
                return null;
            }
        }


        public static bool Delete(string id)
        {
            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" +
                    AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/pn/" + id + ".ipm");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error("Delete PN - " + ex);
                return false;
            }
            return true;
        }



    }


}