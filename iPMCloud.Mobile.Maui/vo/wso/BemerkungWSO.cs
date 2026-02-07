using iPMCloud.Mobile.vo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace iPMCloud.Mobile
{
    public class BemerkungWSO
    {
        public int id { get; set; } = 0;
        public string text { get; set; } = string.Empty;

        /// <summary>
        /// Priorität:
        /// 0 = Standard und für Kunde
        /// 1 = Intern nicht für Kunde
        /// 2 = Störung und für Kunde
        /// 3 = Intern und Störung nicht für Kunde
        /// </summary>
        public int prio { get; set; } = 0;

        public int gruppeid { get; set; } = 0;
        public int objektid { get; set; } = 0;
        public int auftragid { get; set; } = 0;
        public int leistungid { get; set; } = 0;
        public int personid { get; set; } = 0;
        public long datum { get; set; } = 0;
        public bool hasSend { get; set; } = false;
        public string guid { get; set; } = string.Empty;
        public List<BildWSO> photos { get; set; } = new List<BildWSO>();

        public BemerkungWSO()
        {
            this.guid = Guid.NewGuid().ToString();
        }

        #region Save/Load/Delete Methods

        /// <summary>
        /// Speichert eine Bemerkung
        /// </summary>
        public static bool Save(AppModel model, BemerkungWSO notice)
        {
            try
            {
                if (model == null || notice == null)
                {
                    AppModel.Logger?.Error("Save BemerkungWSO: model or notice is null");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    AppModel.Logger?.Error("Save BemerkungWSO: CustomerNumber is null");
                    return false;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/objectnotice/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, $"{notice.datum}.ipm");

                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                string jsonString = JsonConvert.SerializeObject(notice, jsonSettings);
                File.WriteAllText(filePath, jsonString);

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: Save BemerkungWSO");
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Lädt alle Bemerkungen
        /// </summary>
        public static List<BemerkungWSO> LoadAll(AppModel model)
        {
            List<BemerkungWSO> list = new List<BemerkungWSO>();

            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return list;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/objectnotice/"
                );

                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath, "*.ipm");

                    if (files != null && files.Length > 0)
                    {
                        foreach (var file in files)
                        {
                            var notice = Load(model, file);
                            if (notice != null)
                            {
                                list.Add(notice);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: LoadAll BemerkungWSO");
            }

            return list;
        }

        /// <summary>
        /// Lädt alle Bemerkungen als JSON
        /// </summary>
        public static string LoadAll_AsJson()
        {
            try
            {
                var list = LoadAll(AppModel.Instance);
                return JsonConvert.SerializeObject(list, Formatting.Indented);
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: LoadAll_AsJson BemerkungWSO");
                return "[]";
            }
        }

        /// <summary>
        /// Lädt eine einzelne Bemerkung
        /// </summary>
        public static BemerkungWSO Load(AppModel model, string filename)
        {
            try
            {
                if (!File.Exists(filename))
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

                return JsonConvert.DeserializeObject<BemerkungWSO>(jsonString, jsonSettings);
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, $"ERROR: Load BemerkungWSO - {filename}");
                return null;
            }
        }

        /// <summary>
        /// Löscht alle gesendeten Bemerkungen
        /// </summary>
        public static bool DeleteHasSend(AppModel model, List<BemerkungWSO> list)
        {
            try
            {
                if (list == null || list.Count == 0)
                {
                    return true;
                }

                if (string.IsNullOrWhiteSpace(model?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                foreach (var bem in list)
                {
                    if (bem.hasSend)
                    {
                        string filePath = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                            "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/objectnotice/" + bem.datum + ".ipm"
                        );

                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: DeleteHasSend BemerkungWSO");
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Löscht eine einzelne Bemerkung
        /// </summary>
        public static bool Delete(AppModel model, BemerkungWSO bem)
        {
            try
            {
                if (bem == null || string.IsNullOrWhiteSpace(model?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/objectnotice/" + bem.datum + ".ipm"
                );

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: Delete BemerkungWSO");
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        #endregion

        #region Upload Stack Management

        /// <summary>
        /// Fügt eine Bemerkung zum Upload-Stack hinzu
        /// </summary>
        public static bool ToUploadStack(AppModel model, BemerkungWSO bem)
        {
            try
            {
                if (model == null || bem == null)
                {
                    AppModel.Logger?.Error("ToUploadStack BemerkungWSO: model or bem is null");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/objectnoticeupload/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, $"{bem.datum}.ipm");

                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                string jsonString = JsonConvert.SerializeObject(bem, jsonSettings);
                File.WriteAllText(filePath, jsonString);

                // UI Update
                if (AppModel.Instance?.MainPage != null)
                {
                    AppModel.Instance.MainPage.SetAllSyncState();
                }

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: ToUploadStack BemerkungWSO");
                return false;
            }
        }

        /// <summary>
        /// Zählt die Anzahl der Bemerkungen im Upload-Stack
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
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/objectnoticeupload/"
                );

                if (Directory.Exists(directoryPath))
                {
                    return Directory.GetFiles(directoryPath, "*.ipm").Length;
                }

                return 0;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: CountFromStack BemerkungWSO");
                return 0;
            }
        }

        /// <summary>
        /// Lädt alle Bemerkungen aus dem Upload-Stack
        /// </summary>
        public static List<BemerkungWSO> LoadAllFromUploadStack(AppModel model)
        {
            List<BemerkungWSO> list = new List<BemerkungWSO>();

            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return list;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/objectnoticeupload/"
                );

                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath, "*.ipm");

                    if (files != null && files.Length > 0)
                    {
                        foreach (var file in files)
                        {
                            var notice = LoadFromUploadStack(file);
                            if (notice != null)
                            {
                                list.Add(notice);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: LoadAllFromUploadStack BemerkungWSO");
            }

            return list;
        }

        /// <summary>
        /// Lädt alle Bemerkungen aus dem Upload-Stack als JSON
        /// </summary>
        public static string LoadAllFromUploadStack_AsJson()
        {
            try
            {
                var list = LoadAllFromUploadStack(AppModel.Instance);
                return JsonConvert.SerializeObject(list, Formatting.Indented);
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: LoadAllFromUploadStack_AsJson BemerkungWSO");
                return "[]";
            }
        }

        /// <summary>
        /// Lädt eine einzelne Bemerkung aus dem Upload-Stack
        /// </summary>
        private static BemerkungWSO LoadFromUploadStack(string filename)
        {
            try
            {
                if (!File.Exists(filename))
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

                return JsonConvert.DeserializeObject<BemerkungWSO>(jsonString, jsonSettings);
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, $"ERROR: LoadFromUploadStack BemerkungWSO - {filename}");
                return null;
            }
        }

        /// <summary>
        /// Löscht eine Bemerkung aus dem Upload-Stack
        /// </summary>
        public static bool DeleteFromUploadStack(AppModel model, BemerkungWSO bem)
        {
            try
            {
                if (bem == null || model == null || string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/objectnoticeupload/" + bem.datum + ".ipm"
                );

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);

                    // UI Update
                    if (AppModel.Instance?.MainPage != null)
                    {
                        AppModel.Instance.MainPage.SetAllSyncState();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: DeleteFromUploadStack BemerkungWSO");
                return false;
            }
        }

        /// <summary>
        /// Löscht alle Bemerkungen aus dem Upload-Stack
        /// </summary>
        public static bool ClearUploadStack(AppModel model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/objectnoticeupload/"
                );

                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath, "*.ipm");

                    foreach (var file in files)
                    {
                        File.Delete(file);
                    }

                    // UI Update
                    if (AppModel.Instance?.MainPage != null)
                    {
                        AppModel.Instance.MainPage.SetAllSyncState();
                    }

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: ClearUploadStack BemerkungWSO");
                return false;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Prüft ob die Bemerkung gültig ist
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(text) &&
                   !string.IsNullOrWhiteSpace(guid) &&
                   datum > 0;
        }

        /// <summary>
        /// Gibt die Priorität als Text zurück
        /// </summary>
        public string GetPrioText()
        {
            return prio switch
            {
                0 => "Standard (für Kunde)",
                1 => "Intern (nicht für Kunde)",
                2 => "Störung (für Kunde)",
                3 => "Intern & Störung (nicht für Kunde)",
                _ => "Unbekannt"
            };
        }

        /// <summary>
        /// Prüft ob die Bemerkung für den Kunden sichtbar ist
        /// </summary>
        public bool IsVisibleForCustomer()
        {
            return prio == 0 || prio == 2;
        }

        /// <summary>
        /// Prüft ob die Bemerkung eine Störung ist
        /// </summary>
        public bool IsDisruption()
        {
            return prio == 2 || prio == 3;
        }

        /// <summary>
        /// Gibt das Datum als DateTime zurück
        /// </summary>
        public DateTime GetDateTime()
        {
            return new DateTime(datum);
        }

        /// <summary>
        /// Setzt das Datum von einem DateTime
        /// </summary>
        public void SetDateTime(DateTime dateTime)
        {
            datum = dateTime.Ticks;
        }

        #endregion
    }
}