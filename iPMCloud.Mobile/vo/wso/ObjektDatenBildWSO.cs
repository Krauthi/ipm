using iPMCloud.Mobile.vo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Input;
using Xamarin.Forms;

namespace iPMCloud.Mobile

{
    [Serializable]
    public class ObjektDatenBildWSO
    {
        public Int32 id = 0;
        public Int32 meterid = 0;
        public Int32 standid = 0;
        public String filename = "";
        public string lastchange = "0";
        public int del = 0;
        public String bemerkung = "";        
        
        
        public byte[] bytes;
        public string guid = "";

        public ObjektDatenBildWSO() 
        {
            this.guid = Guid.NewGuid().ToString();
        }


        public static bool ToUploadStack(AppModel model, ObjektDatenBildWSO od)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                var json = JsonConvert.SerializeObject(od);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, json);
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/objectvaluebildupload/");
                if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/objectvaluebildupload/" + od.guid + ".ipm");
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
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/objectvaluebildupload/");
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
        public static List<ObjektDatenBildWSO> LoadAllFromUploadStack(AppModel model)
        {
            List<ObjektDatenBildWSO> list = new List<ObjektDatenBildWSO>();
            try
            {
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/objectvaluebildupload/");
                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath);
                    if (files != null && files.Length > 0)
                    {
                        files.ToList().ForEach(file =>
                        {
                            list.Add(ObjektDatenBildWSO.LoadFromUploadStack(file));
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

        private static ObjektDatenBildWSO LoadFromUploadStack(string filename)
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
                    return JsonConvert.DeserializeObject<ObjektDatenBildWSO>(json);
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

        public static bool DeleteFromUploadStack(AppModel model, ObjektDatenBildWSO od)
        {
            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/objectvaluebildupload/" + od.guid + ".ipm");
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