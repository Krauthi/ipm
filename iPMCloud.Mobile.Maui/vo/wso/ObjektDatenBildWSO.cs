using iPMCloud.Mobile.vo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace iPMCloud.Mobile
{
    /// <summary>
    /// Repräsentiert ein Bild für Objektdaten (z.B. Zählerstände)
    /// </summary>
    public class ObjektDatenBildWSO
    {
        public int id { get; set; } = 0;
        public int meterid { get; set; } = 0;
        public int standid { get; set; } = 0;
        public string filename { get; set; } = string.Empty;
        public string lastchange { get; set; } = "0";
        public int del { get; set; } = 0;
        public string bemerkung { get; set; } = string.Empty;
        public byte[] bytes { get; set; }
        public string guid { get; set; } = string.Empty;

        public ObjektDatenBildWSO()
        {
            this.guid = Guid.NewGuid().ToString();
        }

        public ObjektDatenBildWSO(int meterid, byte[] imageBytes, string bemerkung = "")
        {
            this.guid = Guid.NewGuid().ToString();
            this.meterid = meterid;
            this.bytes = imageBytes;
            this.bemerkung = bemerkung;
            this.lastchange = DateTime.Now.Ticks.ToString();
        }

        #region Upload Stack Management

        /// <summary>
        /// Fügt ein Objektdaten-Bild zum Upload-Stack hinzu
        /// </summary>
        public static bool ToUploadStack(AppModel model, ObjektDatenBildWSO od)
        {
            try
            {
                if (model == null || od == null)
                {
                    AppModel.Logger?.Error("ToUploadStack ObjektDatenBildWSO: model or od is null");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    AppModel.Logger?.Error("ToUploadStack ObjektDatenBildWSO: CustomerNumber is null");
                    return false;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/objectvaluebildupload/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, $"{od.guid}.ipm");

                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include
                };

                string jsonString = JsonConvert.SerializeObject(od, jsonSettings);
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
                AppModel.Logger?.Error(ex, "ERROR: ToUploadStack ObjektDatenBildWSO");
                return false;
            }
        }

        /// <summary>
        /// Zählt die Anzahl der Objektdaten-Bilder im Upload-Stack
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
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/objectvaluebildupload/"
                );

                if (Directory.Exists(directoryPath))
                {
                    return Directory.GetFiles(directoryPath, "*.ipm").Length;
                }

                return 0;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: CountFromStack ObjektDatenBildWSO");
                return 0;
            }
        }

        /// <summary>
        /// Lädt alle Objektdaten-Bilder aus dem Upload-Stack
        /// </summary>
        public static List<ObjektDatenBildWSO> LoadAllFromUploadStack(AppModel model)
        {
            List<ObjektDatenBildWSO> list = new List<ObjektDatenBildWSO>();

            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return list;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/objectvaluebildupload/"
                );

                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath, "*.ipm");

                    if (files != null && files.Length > 0)
                    {
                        foreach (var file in files)
                        {
                            var od = LoadFromUploadStack(file);
                            if (od != null)
                            {
                                list.Add(od);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: LoadAllFromUploadStack ObjektDatenBildWSO");
            }

            return list;
        }

        /// <summary>
        /// Lädt ein einzelnes Objektdaten-Bild aus dem Upload-Stack
        /// </summary>
        private static ObjektDatenBildWSO LoadFromUploadStack(string filename)
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

                return JsonConvert.DeserializeObject<ObjektDatenBildWSO>(jsonString, jsonSettings);
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, $"ERROR: LoadFromUploadStack ObjektDatenBildWSO - {filename}");
                return null;
            }
        }

        /// <summary>
        /// Löscht ein Objektdaten-Bild aus dem Upload-Stack
        /// </summary>
        public static bool DeleteFromUploadStack(AppModel model, ObjektDatenBildWSO od)
        {
            try
            {
                if (od == null || model == null || string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/objectvaluebildupload/" + od.guid + ".ipm"
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
                AppModel.Logger?.Error(ex, "ERROR: DeleteFromUploadStack ObjektDatenBildWSO");
                return false;
            }
        }

        /// <summary>
        /// Löscht alle Objektdaten-Bilder aus dem Upload-Stack
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
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/objectvaluebildupload/"
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
                AppModel.Logger?.Error(ex, "ERROR: ClearUploadStack ObjektDatenBildWSO");
                return false;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Prüft ob das Objektdaten-Bild gültig ist
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(guid) &&
                   bytes != null &&
                   bytes.Length > 0;
        }

        /// <summary>
        /// Gibt die Größe des Bildes in Bytes zurück
        /// </summary>
        public int GetImageSize()
        {
            return bytes?.Length ?? 0;
        }

        /// <summary>
        /// Gibt die Größe des Bildes in KB zurück
        /// </summary>
        public double GetImageSizeInKB()
        {
            return GetImageSize() / 1024.0;
        }

        /// <summary>
        /// Gibt die Größe des Bildes in MB zurück
        /// </summary>
        public double GetImageSizeInMB()
        {
            return GetImageSizeInKB() / 1024.0;
        }

        /// <summary>
        /// Prüft ob das Bild als gelöscht markiert ist
        /// </summary>
        public bool IsDeleted()
        {
            return del > 0;
        }

        /// <summary>
        /// Markiert das Bild als gelöscht
        /// </summary>
        public void MarkAsDeleted()
        {
            del = 1;
            lastchange = DateTime.Now.Ticks.ToString();
        }

        /// <summary>
        /// Gibt das LastChange-Datum als DateTime zurück
        /// </summary>
        public DateTime? GetLastChangeDateTime()
        {
            if (long.TryParse(lastchange, out long ticks) && ticks > 0)
            {
                return new DateTime(ticks);
            }
            return null;
        }

        /// <summary>
        /// Aktualisiert den LastChange-Timestamp
        /// </summary>
        public void UpdateLastChange()
        {
            lastchange = DateTime.Now.Ticks.ToString();
        }

        /// <summary>
        /// Prüft ob ein Filename gesetzt ist
        /// </summary>
        public bool HasFilename()
        {
            return !string.IsNullOrWhiteSpace(filename);
        }

        /// <summary>
        /// Prüft ob eine Bemerkung vorhanden ist
        /// </summary>
        public bool HasBemerkung()
        {
            return !string.IsNullOrWhiteSpace(bemerkung);
        }

        /// <summary>
        /// Erstellt eine Kopie des Objekts (ohne Bilddaten für Performance)
        /// </summary>
        public ObjektDatenBildWSO CloneWithoutBytes()
        {
            return new ObjektDatenBildWSO
            {
                id = this.id,
                meterid = this.meterid,
                standid = this.standid,
                filename = this.filename,
                lastchange = this.lastchange,
                del = this.del,
                bemerkung = this.bemerkung,
                guid = this.guid,
                bytes = null // Keine Bilddaten kopieren
            };
        }

        /// <summary>
        /// Gibt eine Beschreibung des Objekts zurück
        /// </summary>
        public override string ToString()
        {
            var size = GetImageSizeInKB();
            return $"ObjektDatenBild [Meter: {meterid}, GUID: {guid}, Size: {size:F2} KB]";
        }

        #endregion
    }
}