using iPMCloud.Mobile.vo;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace iPMCloud.Mobile
{
    /// <summary>
    /// Repräsentiert Push-Notification Token für Web Service
    /// </summary>
    public class PNWSO
    {
        public int personid { get; set; } = 0;
        public string token { get; set; } = string.Empty;

        public PNWSO() { }

        public PNWSO(int personid, string token)
        {
            this.personid = personid;
            this.token = token;
        }

        #region Upload Stack Management

        /// <summary>
        /// Fügt Push-Notification Token zum Upload-Stack hinzu
        /// Hinweis: Es wird immer nur EIN Token gespeichert (pn.ipm wird überschrieben)
        /// </summary>
        public static bool ToUploadStack(PNWSO pn)
        {
            try
            {
                if (pn == null)
                {
                    AppModel.Logger?.Error("ToUploadStack PNWSO: pn is null");
                    return false;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/pnupload/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, "pn.ipm");

                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include
                };

                string jsonString = JsonConvert.SerializeObject(pn, jsonSettings);
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
                AppModel.Logger?.Error(ex, "ERROR: ToUploadStack PNWSO");
                return false;
            }
        }

        /// <summary>
        /// Zählt die Anzahl der PN-Token im Upload-Stack
        /// Hinweis: Maximal 1 Datei (pn.ipm)
        /// </summary>
        public static int CountFromStack()
        {
            try
            {
                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/pnupload/"
                );

                if (Directory.Exists(directoryPath))
                {
                    return Directory.GetFiles(directoryPath, "*.ipm").Length;
                }

                return 0;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: CountFromStack PNWSO");
                return 0;
            }
        }

        /// <summary>
        /// Lädt den Push-Notification Token aus dem Upload-Stack
        /// </summary>
        public static PNWSO LoadFromUploadStack()
        {
            PNWSO pn = null;

            try
            {
                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/pnupload/"
                );

                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath, "*.ipm");

                    if (files != null && files.Length > 0)
                    {
                        // Nimmt die erste Datei (sollte nur pn.ipm sein)
                        pn = LoadFromFile(files[0]);
                    }
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: LoadFromUploadStack PNWSO");
            }

            return pn;
        }

        /// <summary>
        /// Alias für LoadFromUploadStack (für Kompatibilität)
        /// </summary>
        [Obsolete("Use LoadFromUploadStack() instead")]
        public static PNWSO LoadAllFromUploadStack()
        {
            return LoadFromUploadStack();
        }

        /// <summary>
        /// Lädt eine PN aus einer spezifischen Datei
        /// </summary>
        private static PNWSO LoadFromFile(string filename)
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

                return JsonConvert.DeserializeObject<PNWSO>(jsonString, jsonSettings);
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, $"ERROR: LoadFromFile PNWSO - {filename}");
                return null;
            }
        }

        /// <summary>
        /// Löscht den Push-Notification Token aus dem Upload-Stack
        /// </summary>
        public static bool DeleteFromUploadStack()
        {
            try
            {
                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/pnupload/pn.ipm"
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
                AppModel.Logger?.Error(ex, "ERROR: DeleteFromUploadStack PNWSO");
                return false;
            }
        }

        /// <summary>
        /// Prüft ob ein Token im Upload-Stack vorhanden ist
        /// </summary>
        public static bool HasPendingToken()
        {
            return CountFromStack() > 0;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Prüft ob das PNWSO-Objekt gültig ist
        /// </summary>
        public bool IsValid()
        {
            return personid > 0 && !string.IsNullOrWhiteSpace(token);
        }

        /// <summary>
        /// Gibt eine Beschreibung des Objekts zurück
        /// </summary>
        public override string ToString()
        {
            return $"PNWSO [Person: {personid}, Token: {(string.IsNullOrWhiteSpace(token) ? "leer" : "***")}]";
        }

        /// <summary>
        /// Erstellt eine Kopie des Objekts
        /// </summary>
        public PNWSO Clone()
        {
            return new PNWSO
            {
                personid = this.personid,
                token = this.token
            };
        }

        #endregion

        #region Static Helper Methods

        /// <summary>
        /// Erstellt und speichert einen neuen Push-Notification Token
        /// </summary>
        public static bool RegisterToken(int personid, string token)
        {
            try
            {
                if (personid <= 0 || string.IsNullOrWhiteSpace(token))
                {
                    AppModel.Logger?.Error("RegisterToken: Invalid parameters");
                    return false;
                }

                var pnwso = new PNWSO(personid, token);

                if (!pnwso.IsValid())
                {
                    AppModel.Logger?.Error("RegisterToken: PNWSO is not valid");
                    return false;
                }

                bool saved = ToUploadStack(pnwso);

                if (saved)
                {
                    AppModel.Logger?.Info($"Push Notification Token registered for Person {personid}");
                }

                return saved;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: RegisterToken");
                return false;
            }
        }

        /// <summary>
        /// Deregistriert den Push-Notification Token
        /// </summary>
        public static bool UnregisterToken()
        {
            try
            {
                bool deleted = DeleteFromUploadStack();

                if (deleted)
                {
                    AppModel.Logger?.Info("Push Notification Token unregistered");
                }

                return deleted;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: UnregisterToken");
                return false;
            }
        }

        /// <summary>
        /// Holt den aktuellen Token (falls vorhanden)
        /// </summary>
        public static string GetCurrentToken()
        {
            var pnwso = LoadFromUploadStack();
            return pnwso?.token ?? string.Empty;
        }

        /// <summary>
        /// Holt die aktuelle Person-ID (falls vorhanden)
        /// </summary>
        public static int GetCurrentPersonId()
        {
            var pnwso = LoadFromUploadStack();
            return pnwso?.personid ?? 0;
        }

        #endregion
    }
}