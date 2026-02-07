using iPMCloud.Mobile.vo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace iPMCloud.Mobile
{
    #region Request/Response DTOs

    public class AllTransSignRequest
    {
        public string token { get; set; } = string.Empty;
        public string guid { get; set; } = string.Empty;
        public long ticks { get; set; } = 0;
        public long allTransSign { get; set; } = 0;
        public int personid { get; set; } = 0;

        public AllTransSignRequest() { }

        public AllTransSignRequest(string token, string guid, long ticks, long allTransSign, int personid)
        {
            this.token = token;
            this.guid = guid;
            this.ticks = ticks;
            this.allTransSign = allTransSign;
            this.personid = personid;
        }
    }

    public class AllTransSignResponse
    {
        public bool success { get; set; } = false;
        public string message { get; set; } = string.Empty;
        public string guid { get; set; } = string.Empty;
        public long ticks { get; set; } = 0;
        public long allTransSign { get; set; } = 0;
        public int personid { get; set; } = 0;

        public AllTransSignResponse() { }

        public AllTransSignResponse(bool success, string message)
        {
            this.success = success;
            this.message = message;
        }
    }

    #endregion

    public class AllTransSign
    {
        public AllTransSign() { }

        #region Upload Stack Management

        /// <summary>
        /// Fügt ein AllTransSignRequest zum Upload-Stack hinzu
        /// </summary>
        public static bool ToUploadStack(AllTransSignRequest pack)
        {
            try
            {
                if (pack == null)
                {
                    AppModel.Logger?.Error("ToUploadStack: pack is null");
                    return false;
                }

                if (AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber == null)
                {
                    AppModel.Logger?.Error("ToUploadStack: CustomerNumber is null");
                    return false;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/trans/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, $"t{pack.ticks}.ipm");

                // JSON Serialisierung
                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include
                };

                string jsonString = JsonConvert.SerializeObject(pack, jsonSettings);
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
        /// Zählt die Anzahl der Elemente im Upload-Stack
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
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/trans/"
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
        /// Lädt alle Elemente aus dem Upload-Stack
        /// </summary>
        public static List<AllTransSignRequest> LoadAllFromUploadStack()
        {
            List<AllTransSignRequest> list = new List<AllTransSignRequest>();

            try
            {
                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    AppModel.Logger?.Warn("LoadAllFromUploadStack: CustomerNumber is null");
                    return list;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/trans/"
                );

                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath, "*.ipm");

                    if (files != null && files.Length > 0)
                    {
                        foreach (var file in files)
                        {
                            var item = LoadFromUploadStack(file);
                            if (item != null)
                            {
                                list.Add(item);
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
        /// Lädt ein einzelnes Element aus dem Upload-Stack
        /// </summary>
        private static AllTransSignRequest LoadFromUploadStack(string filename)
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
                    AppModel.Logger?.Warn($"LoadFromUploadStack: File is empty - {filename}");
                    return null;
                }

                var jsonSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                AllTransSignRequest request = JsonConvert.DeserializeObject<AllTransSignRequest>(jsonString, jsonSettings);
                return request;
            }
            catch (JsonException jsonEx)
            {
                AppModel.Logger?.Warn(jsonEx, $"Failed to deserialize JSON - {filename}");

                // Versuchen alte BinaryFormatter Datei zu migrieren
                if (TryMigrateLegacyFile(filename, out AllTransSignRequest migratedRequest))
                {
                    return migratedRequest;
                }

                return null;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, $"ERROR: LoadFromUploadStack() - {filename}");
                return null;
            }
        }

        private static bool TryMigrateLegacyFile(string filePath, out AllTransSignRequest request)
        {
            request = null;

            try
            {
                // Alte Datei sichern
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupPath = filePath + $".old_binary_{timestamp}";

                if (File.Exists(filePath))
                {
                    File.Copy(filePath, backupPath, true);
                    AppModel.Logger?.Info($"Legacy trans file backed up to: {backupPath}");
                }

                // In .NET MAUI kann BinaryFormatter nicht mehr verwendet werden
                // Die alte Datei wird gesichert und muss manuell konvertiert werden
                return false;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: TryMigrateLegacyFile()");
                return false;
            }
        }

        /// <summary>
        /// Löscht ein Element aus dem Upload-Stack
        /// </summary>
        public static bool DeleteFromUploadStack(AllTransSignRequest pack)
        {
            try
            {
                if (pack == null)
                {
                    AppModel.Logger?.Error("DeleteFromUploadStack: pack is null");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    AppModel.Logger?.Error("DeleteFromUploadStack: CustomerNumber is null");
                    return false;
                }

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/trans/t" + pack.ticks + ".ipm"
                );

                if (File.Exists(filePath))
                {
                    // Optional: Backup vor dem Löschen
                    // string backupPath = filePath + $".deleted_{DateTime.Now:yyyyMMdd_HHmmss}";
                    // File.Copy(filePath, backupPath, true);

                    File.Delete(filePath);

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
                AppModel.Logger?.Error(ex, "ERROR: DeleteFromUploadStack()");
                return false;
            }
        }

        /// <summary>
        /// Löscht alle Elemente aus dem Upload-Stack
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
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/trans/"
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

                    AppModel.Logger?.Info($"Upload stack cleared: {files.Length} files deleted");
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

        /// <summary>
        /// Prüft ob der Upload-Stack leer ist
        /// </summary>
        public static bool IsUploadStackEmpty()
        {
            return CountFromStack() == 0;
        }

        /// <summary>
        /// Holt das älteste Element aus dem Stack (ohne es zu löschen)
        /// </summary>
        public static AllTransSignRequest GetOldestFromStack()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return null;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/trans/"
                );

                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath, "*.ipm")
                        .OrderBy(f => File.GetCreationTime(f))
                        .ToList();

                    if (files.Any())
                    {
                        return LoadFromUploadStack(files.First());
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: GetOldestFromStack()");
                return null;
            }
        }

        #endregion
    }

    public class TransSignUploadService
    {
        private readonly HttpClient _httpClient;

        public TransSignUploadService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<bool> ProcessUploadStackAsync()
        {
            try
            {
                var requests = AllTransSign.LoadAllFromUploadStack();

                if (requests == null || requests.Count == 0)
                {
                    return true;
                }

                int successCount = 0;

                foreach (var request in requests)
                {
                    var response = await UploadTransSignAsync(request);

                    if (response?.success == true)
                    {
                        AllTransSign.DeleteFromUploadStack(request);
                        successCount++;
                    }
                }

                AppModel.Logger?.Info($"Upload stack processed: {successCount}/{requests.Count} successful");
                return successCount == requests.Count;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: ProcessUploadStackAsync()");
                return false;
            }
        }

        private async Task<AllTransSignResponse> UploadTransSignAsync(AllTransSignRequest request)
        {
            try
            {
                string jsonRequest = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");

                var httpResponse = await _httpClient.PostAsync(
                    "https://api.example.com/transsign",
                    content
                );

                if (httpResponse.IsSuccessStatusCode)
                {
                    string jsonResponse = await httpResponse.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<AllTransSignResponse>(jsonResponse);
                }

                return new AllTransSignResponse(false, $"HTTP Error: {httpResponse.StatusCode}");
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: UploadTransSignAsync()");
                return new AllTransSignResponse(false, ex.Message);
            }
        }
    }

}