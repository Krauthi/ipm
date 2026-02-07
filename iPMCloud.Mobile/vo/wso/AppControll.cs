using iPMCloud.Mobile.vo;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace iPMCloud.Mobile
{
    [Serializable]
    public class AppControll
    {

        public Int32 id;
        public Int32 gruppeid;
        public bool scanbmp = true;
        public bool cleansaves = false;
        public bool scanbmpview = true;
        public bool scanbmpviewinperson = false;
        public bool scanbmpviewprotokoll = true;
        public bool showPersonTimes = false;
        public bool showObjektPlans = false;
        public bool showTickets = false;
        public bool showChecks = false;
        public bool filterKategories = false;
        public bool translation = false;
        public string lang = "de";

        public Int32 version = 2001050; //2.1.43 [--2],[--1],[-43]

        public bool direktBuchenMuell = true;
        public bool direktBuchenWinter = true;
        public bool direktBuchenPos = false;

        public bool imageIncludeLocation = false;  // Standort ins Bild 


        // PersonWSO Extentions
        public bool ignoreKategorieFilterByPerson = false;


        public AppControll()
        {
        }

        public static bool Save(AppModel model, AppControll ac)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, ac);
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/appcontroll/");
                if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/appcontroll/appcontroll.ipm");
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
        public static AppControll Load(AppModel model)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                if (!String.IsNullOrWhiteSpace(model.SettingModel.SettingDTO.CustomerNumber))
                {
                    string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/appcontroll/");
                    if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                    string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/appcontroll/appcontroll.ipm");
                    if (File.Exists(filePath))
                    {
                        byte[] data = File.ReadAllBytes(filePath);
                        BinaryFormatter binForm = new BinaryFormatter();
                        ms.Write(data, 0, data.Length);
                        ms.Seek(0, SeekOrigin.Begin);
                        var ac = (Object)binForm.Deserialize(ms) as AppControll;
                        ms.Close();
                        ms.Dispose();
                        return ac;
                    }
                    else
                    {
                        ms.Close();
                        ms.Dispose();
                        return new AppControll();
                    }
                }
                else
                {
                    ms.Close();
                    ms.Dispose();
                    return new AppControll();
                }
            }
            catch (Exception ex)
            {
                ms.Close();
                ms.Dispose();
                AppModel.Logger.Error("LoadAppControll" + ex);
                return new AppControll();
            }
        }


        public static string Load_AsJson()
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                if (!String.IsNullOrWhiteSpace(AppModel.Instance.SettingModel.SettingDTO.CustomerNumber))
                {
                    string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/appcontroll/");
                    if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                    string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/appcontroll/appcontroll.ipm");
                    if (File.Exists(filePath))
                    {
                        byte[] data = File.ReadAllBytes(filePath);
                        BinaryFormatter binForm = new BinaryFormatter();
                        ms.Write(data, 0, data.Length);
                        ms.Seek(0, SeekOrigin.Begin);
                        var ac = (Object)binForm.Deserialize(ms) as AppControll;
                        ms.Close();
                        ms.Dispose();
                        return JsonConvert.SerializeObject(ac);
                    }
                    else
                    {
                        ms.Close();
                        ms.Dispose();
                        return "{File not exist}";
                    }
                }
                else
                {
                    ms.Close();
                    ms.Dispose();
                    return "{CustomerNumber not set}";
                }
            }
            catch (Exception ex)
            {
                ms.Close();
                ms.Dispose();
                AppModel.Logger.Error("LoadAppControll" + ex);
                return "{Error: " + ex.Message + "}";
            }
        }
        public static bool Delete(AppModel model)
        {
            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/appcontroll/appcontroll.ipm");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error("Delete AppControll" + ex);
                return false;
            }
            return true;
        }



    }


}