using iPMCloud.Mobile.vo;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace iPMCloud.Mobile
{
    public class ObservableLangItemCollection<K, T> : ObservableCollection<T>
    {
        private readonly K _key;

        public ObservableLangItemCollection(IGrouping<K, T> group)
            : base(group)
        {
            _key = group.Key;
        }

        public K Key
        {
            get { return _key; }
        }
    }

    [Serializable]
    public class Lang
    {
        public string text { get; set; } = "Deutsch (Standard)";
        public string lang { get; set; } = "de";
        public string last { get; set; } = "0";

        public Lang()
        {
        }

        public static bool Save(Lang pn)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, pn);
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" +
                    AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/lang/");
                if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" +
                    AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/lang/lang.ipm");
                File.WriteAllBytes(filePath, ms.ToArray());
                ms.Close();
                ms.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                ms.Close();
                ms.Dispose();
                AppModel.Logger.Error("Save Lang - " + ex);
                return false;
            }
        }

        public static Lang Load()
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                if (!String.IsNullOrWhiteSpace(AppModel.Instance.SettingModel.SettingDTO.CustomerNumber))
                {
                    string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" +
                        AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/lang/");
                    if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                    string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" +
                        AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/lang/lang.ipm");
                    if (File.Exists(filePath))
                    {
                        byte[] data = File.ReadAllBytes(filePath);
                        BinaryFormatter binForm = new BinaryFormatter();
                        ms.Write(data, 0, data.Length);
                        ms.Seek(0, SeekOrigin.Begin);
                        var pn = (Object)binForm.Deserialize(ms) as Lang;
                        ms.Close();
                        ms.Dispose();
                        return pn;
                    }
                    else
                    {
                        ms.Close();
                        ms.Dispose();
                        return new Lang();
                    }
                }
                else
                {
                    ms.Close();
                    ms.Dispose();
                    return new Lang();
                }
            }
            catch (Exception ex)
            {
                ms.Close();
                ms.Dispose();
                AppModel.Logger.Error("Load Lang - " + ex);
                return new Lang();
            }
        }


        public static bool Delete(string id)
        {
            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" +
                    AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/lang/lang.ipm");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error("Delete Lang - " + ex);
                return false;
            }
            return true;
        }



    }


}