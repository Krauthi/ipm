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
    public class ChecksRequest
    {
        public string token = "";
        public Int32 id = 0; // PersonId
        public int view = 10;  // 10 = Fällige und Offene

        public ChecksRequest()
        {
        }
    }

    [Serializable]
    public class CheckRequest
    {
        public string token = "";
        public Int32 id = 0; // CheckId

        public CheckRequest()
        {
        }
    }


    [Serializable]
    public class CheckARequest
    {
        public string token = "";
        public Check checkA = null;

        public CheckARequest()
        {
        }
    }

    [Serializable]
    public class CheckABemImgRequest
    {
        public string token = "";
        public CheckLeistungAntwortBemImg BemImg = null;

        public CheckABemImgRequest()
        {
        }
    }

    [Serializable]
    public class ChecksResponse
    {
        public bool success = false;
        public string message;
        public bool active = true;
        public DateTime? lastCall = null;

        public List<CheckInfo> checks = new List<CheckInfo>();
        public Check check = null;
        public Check checkA = null;

        public ChecksResponse()
        {

        }
    }

    public class CheckClass
    {
        public CheckClass() { }

        public static bool SaveChecksInfo(ChecksResponse response)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                var json = JsonConvert.SerializeObject(response);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, json);
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/"
                    + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/checks/");
                if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/"
                    + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/checks/checksresponse.ipm");
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

        public static ChecksResponse LoadChecksInfo()
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/"
                    + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/checks/checksresponse.ipm");
                if (File.Exists(filePath))
                {
                    byte[] data = File.ReadAllBytes(filePath);
                    BinaryFormatter binForm = new BinaryFormatter();
                    ms.Write(data, 0, data.Length);
                    ms.Seek(0, SeekOrigin.Begin);
                    var json = (Object)binForm.Deserialize(ms) as String;
                    ms.Close();
                    ms.Dispose();
                    return JsonConvert.DeserializeObject<ChecksResponse>(json);
                }
                else
                {
                    ms.Close();
                    ms.Dispose();
                    return new ChecksResponse();
                }
            }
            catch (Exception ex)
            {
                ms.Close();
                ms.Dispose();
                AppModel.Logger.Error(ex);
                return new ChecksResponse();
            }
        }

        public static bool Delete()
        {
            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/"
                    + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/checks/checksresponse.ipm");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error(ex);
                return false;
            }
            return true;
        }



        public static bool SaveCheck(Check c)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                var json = JsonConvert.SerializeObject(c);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, json);
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/"
                    + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/checks/");
                if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/"
                    + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/checks/check_" + c.id + ".ipm");
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

        public static Check LoadCheck(Int32 id)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/"
                    + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/checks/check_" + id + ".ipm");
                if (File.Exists(filePath))
                {
                    byte[] data = File.ReadAllBytes(filePath);
                    BinaryFormatter binForm = new BinaryFormatter();
                    ms.Write(data, 0, data.Length);
                    ms.Seek(0, SeekOrigin.Begin);
                    var json = (Object)binForm.Deserialize(ms) as String;
                    ms.Close();
                    ms.Dispose();
                    return JsonConvert.DeserializeObject<Check>(json);
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

        public static bool DeleteCheck(Int32 id)
        {
            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/"
                    + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/checks/check_" + id + ".ipm");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error(ex);
                return false;
            }
            return true;
        }



        public static bool SaveCheckA(Check c)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                var json = JsonConvert.SerializeObject(c);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, json);
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/"
                    + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/checks/");
                if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/"
                    + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/checks/checka_" + c.id + ".ipm");
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

        public static Check LoadCheckA(Int32 id)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/"
                    + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/checks/checka_" + id + ".ipm");
                if (File.Exists(filePath))
                {
                    byte[] data = File.ReadAllBytes(filePath);
                    BinaryFormatter binForm = new BinaryFormatter();
                    ms.Write(data, 0, data.Length);
                    ms.Seek(0, SeekOrigin.Begin);
                    var json = (Object)binForm.Deserialize(ms) as String;
                    ms.Close();
                    ms.Dispose();
                    return JsonConvert.DeserializeObject<Check>(json);
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

        public static Int32 GiveCheckAToWork()
        {
            var list = AppModel.Instance.ChecksInfoResponse.checks;
            Int32 isWorkedId = -1;
            try
            {
                string directoryPath = Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/checks");
                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath);
                    if (files != null && files.Length > 0)
                    {
                        files.ToList().ForEach(file =>
                        {
                            var f = file.Split(new String[] { "checka_" }, System.StringSplitOptions.RemoveEmptyEntries);
                            if (f != null && f.Length > 1)
                            {
                                Int32 checkaId = Int32.Parse(f[1].Replace(".ipm", ""));
                                var foundCheckA = list.FindAll(_ => _.checkA_id == checkaId);
                                if (foundCheckA != null && foundCheckA.Count > 0 && isWorkedId == -1)
                                {
                                    isWorkedId = checkaId;
                                }
                                else
                                {
                                    DeleteCheckA(checkaId);
                                }
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error(ex);
            }
            return isWorkedId;
        }

        public static bool DeleteCheckA(Int32 id)
        {
            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/"
                    + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/checks/checka_" + id + ".ipm");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error(ex);
                return false;
            }
            return true;
        }




        public static bool ToUploadStack(Check c)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                var json = JsonConvert.SerializeObject(c);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, json);
                string directoryPath = Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber
                    + "/uploadchecka/");
                if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                string filePath = Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber
                    + "/uploadchecka/" + c.id + ".ipm");
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
                AppModel.Logger.Error(ex.Message);
                return false;
            }
        }
        public static int CountFromStack()
        {
            try
            {
                string directoryPath = Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/uploadchecka/");
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
        public static List<Check> LoadAllFromUploadStack()
        {
            List<Check> list = new List<Check>();
            try
            {
                string directoryPath = Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/uploadchecka/");
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

        public static string LoadAllFromUploadStack_AsJson()
        {
            List<Check> list = new List<Check>();
            try
            {
                string directoryPath = Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/uploadchecka/");
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
            return JsonConvert.SerializeObject(list);
        }
        private static Check LoadFromUploadStack(string filename)
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
                    return JsonConvert.DeserializeObject<Check>(json);
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
        public static bool DeleteFromUploadStack(Check c)
        {
            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.MyDocuments), "ipm/" +
                    AppModel.Instance.SettingModel.SettingDTO.CustomerNumber +
                    "/uploadchecka/" + c.id + ".ipm");
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
