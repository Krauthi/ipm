using iPMCloud.Mobile.vo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace iPMCloud.Mobile
{
    #region Request/Response DTOs

    public class ChecksRequest
    {
        public string token { get; set; } = string.Empty;
        public int id { get; set; } = 0; // PersonId
        public int view { get; set; } = 10;  // 10 = Fällige und Offene

        public ChecksRequest() { }

        public ChecksRequest(string token, int personId, int view = 10)
        {
            this.token = token;
            this.id = personId;
            this.view = view;
        }
    }

    public class CheckRequest
    {
        public string token { get; set; } = string.Empty;
        public int id { get; set; } = 0; // CheckId

        public CheckRequest() { }

        public CheckRequest(string token, int checkId)
        {
            this.token = token;
            this.id = checkId;
        }
    }

    public class CheckARequest
    {
        public string token { get; set; } = string.Empty;
        public Check checkA { get; set; } = null;

        public CheckARequest() { }

        public CheckARequest(string token, Check checkA)
        {
            this.token = token;
            this.checkA = checkA;
        }
    }

    public class CheckABemImgRequest
    {
        public string token { get; set; } = string.Empty;
        public CheckLeistungAntwortBemImg BemImg { get; set; } = null;

        public CheckABemImgRequest() { }

        public CheckABemImgRequest(string token, CheckLeistungAntwortBemImg bemImg)
        {
            this.token = token;
            this.BemImg = bemImg;
        }
    }

    public class ChecksResponse
    {
        public bool success { get; set; } = false;
        public string message { get; set; } = string.Empty;
        public bool active { get; set; } = true;
        public DateTime? lastCall { get; set; } = null;
        public List<CheckInfo> checks { get; set; } = new List<CheckInfo>();
        public Check check { get; set; } = null;
        public Check checkA { get; set; } = null;

        public ChecksResponse() { }
    }

    #endregion

    public class CheckClass
    {
        public CheckClass() { }

        #region ChecksInfo Management

        /// <summary>
        /// Speichert die ChecksResponse (Übersicht)
        /// </summary>
        public static bool SaveChecksInfo(ChecksResponse response)
        {
            try
            {
                if (response == null)
                {
                    AppModel.Logger?.Error("SaveChecksInfo: response is null");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    AppModel.Logger?.Error("SaveChecksInfo: CustomerNumber is null");
                    return false;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/checks/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, "checksresponse.ipm");

                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                string jsonString = JsonConvert.SerializeObject(response, jsonSettings);
                File.WriteAllText(filePath, jsonString);

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: SaveChecksInfo()");
                return false;
            }
        }

        /// <summary>
        /// Lädt die ChecksResponse (Übersicht)
        /// </summary>
        public static ChecksResponse LoadChecksInfo()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return new ChecksResponse();
                }

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/checks/checksresponse.ipm"
                );

                if (File.Exists(filePath))
                {
                    string jsonString = File.ReadAllText(filePath);

                    if (string.IsNullOrWhiteSpace(jsonString))
                    {
                        return new ChecksResponse();
                    }

                    var jsonSettings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };

                    return JsonConvert.DeserializeObject<ChecksResponse>(jsonString, jsonSettings) ?? new ChecksResponse();
                }

                return new ChecksResponse();
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: LoadChecksInfo()");
                return new ChecksResponse();
            }
        }

        /// <summary>
        /// Löscht die ChecksResponse
        /// </summary>
        public static bool Delete()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/checks/checksresponse.ipm"
                );

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: Delete()");
                return false;
            }
        }

        #endregion

        #region Check Management (Read-Only)

        /// <summary>
        /// Speichert einen einzelnen Check (Lesemodus)
        /// </summary>
        public static bool SaveCheck(Check c)
        {
            try
            {
                if (c == null)
                {
                    AppModel.Logger?.Error("SaveCheck: check is null");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/checks/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, $"check_{c.id}.ipm");

                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                string jsonString = JsonConvert.SerializeObject(c, jsonSettings);
                File.WriteAllText(filePath, jsonString);

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: SaveCheck()");
                return false;
            }
        }

        /// <summary>
        /// Lädt einen einzelnen Check (Lesemodus)
        /// </summary>
        public static Check LoadCheck(int id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return null;
                }

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/checks/check_" + id + ".ipm"
                );

                if (File.Exists(filePath))
                {
                    string jsonString = File.ReadAllText(filePath);

                    if (string.IsNullOrWhiteSpace(jsonString))
                    {
                        return null;
                    }

                    var jsonSettings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };

                    return JsonConvert.DeserializeObject<Check>(jsonString, jsonSettings);
                }

                return null;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, $"ERROR: LoadCheck({id})");
                return null;
            }
        }

        /// <summary>
        /// Löscht einen einzelnen Check
        /// </summary>
        public static bool DeleteCheck(int id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/checks/check_" + id + ".ipm"
                );

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, $"ERROR: DeleteCheck({id})");
                return false;
            }
        }

        #endregion

        #region CheckA Management (In-Progress/Editing)

        /// <summary>
        /// Speichert einen CheckA (in Bearbeitung)
        /// </summary>
        public static bool SaveCheckA(Check c)
        {
            try
            {
                if (c == null)
                {
                    AppModel.Logger?.Error("SaveCheckA: check is null");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/checks/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, $"checka_{c.id}.ipm");

                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                string jsonString = JsonConvert.SerializeObject(c, jsonSettings);
                File.WriteAllText(filePath, jsonString);

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: SaveCheckA()");
                return false;
            }
        }

        /// <summary>
        /// Lädt einen CheckA (in Bearbeitung)
        /// </summary>
        public static Check LoadCheckA(int id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return null;
                }

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/checks/checka_" + id + ".ipm"
                );

                if (File.Exists(filePath))
                {
                    string jsonString = File.ReadAllText(filePath);

                    if (string.IsNullOrWhiteSpace(jsonString))
                    {
                        return null;
                    }

                    var jsonSettings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };

                    return JsonConvert.DeserializeObject<Check>(jsonString, jsonSettings);
                }

                return null;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, $"ERROR: LoadCheckA({id})");
                return null;
            }
        }

        /// <summary>
        /// Gibt eine CheckA-ID zurück, die noch bearbeitet wird
        /// </summary>
        public static int GiveCheckAToWork()
        {
            var list = AppModel.Instance.ChecksInfoResponse?.checks;
            if (list == null || list.Count == 0)
            {
                return -1;
            }

            int isWorkedId = -1;

            try
            {
                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return -1;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/checks"
                );

                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath, "checka_*.ipm");

                    if (files != null && files.Length > 0)
                    {
                        foreach (var file in files)
                        {
                            var fileName = Path.GetFileName(file);

                            if (fileName.StartsWith("checka_") && fileName.EndsWith(".ipm"))
                            {
                                string idString = fileName.Replace("checka_", "").Replace(".ipm", "");

                                if (int.TryParse(idString, out int checkaId))
                                {
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
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: GiveCheckAToWork()");
            }

            return isWorkedId;
        }

        /// <summary>
        /// Löscht einen CheckA
        /// </summary>
        public static bool DeleteCheckA(int id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/checks/checka_" + id + ".ipm"
                );

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, $"ERROR: DeleteCheckA({id})");
                return false;
            }
        }

        #endregion

        #region Upload Stack Management

        /// <summary>
        /// Fügt einen Check zum Upload-Stack hinzu
        /// </summary>
        public static bool ToUploadStack(Check c)
        {
            try
            {
                if (c == null)
                {
                    AppModel.Logger?.Error("ToUploadStack: check is null");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/uploadchecka/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, $"{c.id}.ipm");

                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                string jsonString = JsonConvert.SerializeObject(c, jsonSettings);
                File.WriteAllText(filePath, jsonString);

                // UI Update
                if (AppModel.Instance.MainPage != null)
                {
                    AppModel.Instance.MainPage.SetAllSyncState();
                }

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: ToUploadStack()");
                return false;
            }
        }

        /// <summary>
        /// Zählt die Anzahl der Checks im Upload-Stack
        /// </summary>
        public static int CountFromStack()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return 0;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/uploadchecka/"
                );

                if (Directory.Exists(directoryPath))
                {
                    return Directory.GetFiles(directoryPath, "*.ipm").Length;
                }

                return 0;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: CountFromStack()");
                return 0;
            }
        }

        /// <summary>
        /// Lädt alle Checks aus dem Upload-Stack
        /// </summary>
        public static List<Check> LoadAllFromUploadStack()
        {
            List<Check> list = new List<Check>();

            try
            {
                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return list;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/uploadchecka/"
                );

                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath, "*.ipm");

                    if (files != null && files.Length > 0)
                    {
                        foreach (var file in files)
                        {
                            var check = LoadFromUploadStack(file);
                            if (check != null)
                            {
                                list.Add(check);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: LoadAllFromUploadStack()");
            }

            return list;
        }

        /// <summary>
        /// Lädt alle Checks aus dem Upload-Stack als JSON
        /// </summary>
        public static string LoadAllFromUploadStack_AsJson()
        {
            try
            {
                var list = LoadAllFromUploadStack();
                return JsonConvert.SerializeObject(list, Formatting.Indented);
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: LoadAllFromUploadStack_AsJson()");
                return "[]";
            }
        }

        /// <summary>
        /// Lädt einen einzelnen Check aus dem Upload-Stack
        /// </summary>
        private static Check LoadFromUploadStack(string filename)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filename) || !File.Exists(filename))
                {
                    return null;
                }

                string jsonString = File.ReadAllText(filename);

                if (string.IsNullOrWhiteSpace(jsonString))
                {
                    return null;
                }

                var jsonSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                return JsonConvert.DeserializeObject<Check>(jsonString, jsonSettings);
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, $"ERROR: LoadFromUploadStack() - {filename}");
                return null;
            }
        }

        /// <summary>
        /// Löscht einen Check aus dem Upload-Stack
        /// </summary>
        public static bool DeleteFromUploadStack(Check c)
        {
            try
            {
                if (c == null)
                {
                    return false;
                }

                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/uploadchecka/" + c.id + ".ipm"
                );

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);

                    // UI Update
                    if (AppModel.Instance.MainPage != null)
                    {
                        AppModel.Instance.MainPage.SetAllSyncState();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: DeleteFromUploadStack()");
                return false;
            }
        }

        /// <summary>
        /// Löscht alle Checks aus dem Upload-Stack
        /// </summary>
        public static bool ClearUploadStack()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/uploadchecka/"
                );

                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath, "*.ipm");

                    foreach (var file in files)
                    {
                        File.Delete(file);
                    }

                    // UI Update
                    if (AppModel.Instance.MainPage != null)
                    {
                        AppModel.Instance.MainPage.SetAllSyncState();
                    }

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: ClearUploadStack()");
                return false;
            }
        }

        #endregion
    }
}