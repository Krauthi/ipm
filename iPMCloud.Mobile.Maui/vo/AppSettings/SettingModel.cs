using Newtonsoft.Json;
using System;
using System.IO;

namespace iPMCloud.Mobile.vo
{
    public class SettingModel
    {
        public const string FIX_SERVER_URL = "http://5.35.250.51/IPMRegistrationService";
        public const string FIX_SERVER_URL_DEV_B = "http://nbkrauthausen/IPMRegistrationService";
        public const string FIX_SERVER_URL_DEV = "http://192.168.2.121:45455";

        public AppModel model { get; set; }

        private SettingDTO _settingDTO = new SettingDTO();
        public SettingDTO SettingDTO { get => _settingDTO; set => _settingDTO = value; }

        // Counter gegen Endlosschleife
        private int loadAttempts = 0;
        private const int MaxLoadAttempts = 3;

        public bool IoSaveError { get; set; } = false;
        public bool IoLoadError { get; set; } = false;
        public bool IoDeleteError { get; set; } = false;

        public SettingModel() { }

        // Aufruf in MainActivity und AppDelegate
        public void InitializeSettings()
        {
            LoadSettings();

            if (IoLoadError)
            {
                IoLoadError = false;
                SettingDTO = new SettingDTO();
            }

            SettingDTO.RunBackground = false; // FIX false

            if (model != null && model.IsTest)
            {
                // Test-Konfigurationen
                //SettingDTO.LoginName = "mitarbeiterx";
                //SettingDTO.LoginPassword = "mitarbeiter";

                SettingDTO.LoginName = "Esposito";
                SettingDTO.LoginPassword = "21081986";

                // SUB MAB
                //SettingDTO.LoginName = "dihensel";
                //SettingDTO.LoginPassword = "rheingauer";

                //SettingDTO.LoginName = "olliballiel";
                //SettingDTO.LoginPassword = "08021960";
                //SettingDTO.LoginName = "erwinwillecke";
                //SettingDTO.LoginPassword = "miriam01";

                //SettingDTO.Autologin = false;
                SettingDTO.CustomerName = "iPD Hensel Immobilien Pflege und Dienstleistungen";
                SettingDTO.CustomerNumber = "1"; // "10074";

                SettingDTO.ServerUrl = "http://localhost:52222";
                SettingDTO.ServerUrl = "http://nbkrauthausen/mservice";
                //SettingDTO.ServerUrl = "http://192.168.178.23:52222";

                //SettingDTO.ServerUrl = "https://qsportaltestdecker.ipmcloud.de";
                SettingDTO.ServerUrl = "https://test-hensel.ipmcloud.de";

                //SettingDTO.ServerUrl = "http://localhost:52255";

                //SettingDTO.ServerUrl = "https://ipd-hensel.ipmcloud.de";

                //SettingDTO.ServerUrl = "http://192.168.178.23:45455";
                //SettingDTO.ServerUrl = "http://169.254.26.146:45455";
                //SettingDTO.LoginName = "dihensel";
                //SettingDTO.LoginPassword = "rheingauer";

                //SettingDTO.LoginName = "ChefMarcel";
                //SettingDTO.LoginPassword = "LucaLeon22";
                //SettingDTO.CustomerName = "HGS Manig";
                //SettingDTO.CustomerNumber = "10065";

                //SettingDTO.CustomerName = "HGS MANIG DEVELOPMENT";
                //SettingDTO.CustomerNumber = "10065";
                //SettingDTO.LoginName = "HGs001Schinn";
                //SettingDTO.LoginPassword = "Sense58c";

                //SettingDTO.ServerUrl = "http://192.168.178.23:45455";

                //SettingDTO.LoginName = "SamirArbeit";
                //SettingDTO.LoginPassword = "31313131";

                //SettingDTO.LoginName = "TimoStraub";
                //SettingDTO.LoginPassword = "33333333";
                //SettingDTO.LoginName = "YonasMisigna";
                //SettingDTO.LoginPassword = "12121212";
                //SettingDTO.LoginName = "PatrickKuchler";
                //SettingDTO.LoginPassword = "11111111";
                //SettingDTO.ServerUrl = "https://hms-muminovic.ipmcloud.de";
                //SettingDTO.CustomerNumber = "10013";

                SettingDTO.LoginName = "Fuchs2023";
                SettingDTO.LoginPassword = "Fuchs2023";

                SettingDTO.ServerUrl = "https://bauchinger.ipmcloud.de";
                SettingDTO.CustomerNumber = "10046";

                //SettingDTO.LoginName = "ReneUtermark";
                //SettingDTO.LoginPassword = "Movinghead30";
                //SettingDTO.ServerUrl = "https://utermark.ipmcloud.de/";
                //SettingDTO.CustomerNumber = "10099";

                //SettingDTO.LoginName = "svenkilian";
                //SettingDTO.LoginPassword = "heinrichshms";
                //SettingDTO.ServerUrl = "https://heinrichs-hms.ipmcloud.de";
                //SettingDTO.CustomerNumber = "10020";

                SettingDTO.LoginName = "L.Schmitz";
                SettingDTO.LoginPassword = "16071967";
                SettingDTO.ServerUrl = "https://tschoecke.ipmcloud.de";
                SettingDTO.CustomerNumber = "11037";

                SettingDTO.LoginName = "JoBaum111";
                SettingDTO.LoginPassword = "16101964";
                SettingDTO.CustomerName = "iPD Hensel Immobilien Pflege und Dienstleistungen";
                SettingDTO.CustomerNumber = "1"; // "10074";
                SettingDTO.ServerUrl = "https://test-hensel.ipmcloud.de";

                SaveSettings();
            }
        }

        public bool SaveSettings()
        {
            try
            {
                if (!SettingDTO.Autologin)
                {
                    // Passwort nicht speichern (optional)
                    // SettingDTO.LoginPassword = "";
                }

                IoSaveError = false;

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/settings/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, "set.ipm");

                // JSON Serialisierung mit Newtonsoft.Json
                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include
                };

                string jsonString = JsonConvert.SerializeObject(SettingDTO, jsonSettings);
                File.WriteAllText(filePath, jsonString);

                Company.AddUpdateCompany(model, SettingDTO);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                AppModel.Logger?.Error(ex, "ERROR: SaveSettings()");
                IoSaveError = true;
                return false;
            }
        }

        public bool LoadSettings()
        {
            try
            {
                // WICHTIG: Counter gegen Endlosschleife
                if (loadAttempts >= MaxLoadAttempts)
                {
                    AppModel.Logger?.Error("ERROR: LoadSettings() - Max attempts reached");
                    IoLoadError = true;
                    loadAttempts = 0; // Reset für nächsten Versuch
                    return false;
                }
                loadAttempts++;

                IoLoadError = false;

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/settings/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, "set.ipm");

                if (File.Exists(filePath))
                {
                    try
                    {
                        // JSON laden
                        string jsonString = File.ReadAllText(filePath);

                        if (string.IsNullOrWhiteSpace(jsonString))
                        {
                            throw new Exception("Settings file is empty");
                        }

                        var jsonSettings = new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Include,
                            DefaultValueHandling = DefaultValueHandling.Include,
                            MissingMemberHandling = MissingMemberHandling.Ignore
                        };

                        SettingDTO = JsonConvert.DeserializeObject<SettingDTO>(jsonString, jsonSettings);

                        if (SettingDTO != null)
                        {
                            loadAttempts = 0; // Reset counter bei Erfolg
                            return true;
                        }
                        else
                        {
                            throw new Exception("Deserialized SettingDTO is null");
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        // JSON Fehler - könnte alte BinaryFormatter Datei sein
                        AppModel.Logger?.Warn(jsonEx, "Failed to deserialize JSON, attempting migration");

                        if (TryMigrateLegacySettings(filePath))
                        {
                            loadAttempts = 0;
                            return true;
                        }

                        // Falls Migration fehlschlägt, neue Settings erstellen
                        SettingDTO = new SettingDTO();
                        SaveSettings();
                        loadAttempts = 0;
                        return true;
                    }
                }
                else
                {
                    // Datei existiert nicht - neue Settings erstellen (OHNE rekursiven Aufruf!)
                    SettingDTO = new SettingDTO();
                    SaveSettings();
                    loadAttempts = 0;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                AppModel.Logger?.Error(ex, "ERROR: LoadSettings()");
                IoLoadError = true;
                loadAttempts = 0; // Reset counter
                return false;
            }
        }

        private bool TryMigrateLegacySettings(string filePath)
        {
            try
            {
                // Alte Datei sichern
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupPath = filePath + $".old_binary_{timestamp}";

                if (File.Exists(filePath))
                {
                    File.Copy(filePath, backupPath, true);
                    File.Delete(filePath);

                    AppModel.Logger?.Info($"Legacy settings file backed up to: {backupPath}");
                }

                // Neue leere Settings erstellen
                SettingDTO = new SettingDTO();
                SaveSettings();

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: TryMigrateLegacySettings()");
                return false;
            }
        }

        public string LoadSettings_AsJson()
        {
            try
            {
                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/settings/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, "set.ipm");

                if (File.Exists(filePath))
                {
                    string jsonString = File.ReadAllText(filePath);

                    if (string.IsNullOrWhiteSpace(jsonString))
                    {
                        return "{\"Error\": \"Settings file is empty\"}";
                    }

                    // JSON parsen und sensible Daten entfernen
                    var settings = JsonConvert.DeserializeObject<SettingDTO>(jsonString);

                    if (settings != null)
                    {
                        settings.LoginPassword = "";
                        settings.LoginToken = "";

                        return JsonConvert.SerializeObject(settings, Formatting.Indented);
                    }
                    else
                    {
                        return "{\"Error\": \"Failed to deserialize settings\"}";
                    }
                }
                else
                {
                    return "{\"Info\": \"File not exist\"}";
                }
            }
            catch (Exception ex)
            {
                return "{\"Error\": \"" + ex.Message.Replace("\"", "'") + "\"}";
            }
        }

        public bool DeleteSettings()
        {
            try
            {
                IoDeleteError = false;

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/settings/set.ipm"
                );

                if (File.Exists(filePath))
                {
                    // Optional: Backup vor dem Löschen
                    string backupPath = filePath + $".deleted_{DateTime.Now:yyyyMMdd_HHmmss}";
                    File.Copy(filePath, backupPath, true);

                    File.Delete(filePath);

                    AppModel.Logger?.Info($"Settings deleted. Backup created: {backupPath}");
                }

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: DeleteSettings()");
                IoDeleteError = true;
                return false;
            }
        }

        public bool ResetSettings()
        {
            try
            {
                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/settings/set.ipm"
                );

                if (File.Exists(filePath))
                {
                    string backupPath = filePath + $".backup_{DateTime.Now:yyyyMMdd_HHmmss}";
                    File.Copy(filePath, backupPath, true);
                    File.Delete(filePath);
                }

                SettingDTO = new SettingDTO();
                SaveSettings();

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: ResetSettings()");
                return false;
            }
        }

        public bool IsCredentialsSettingsReady
        {
            get
            {
                return (!string.IsNullOrWhiteSpace(SettingDTO.ServerUrl) &&
                        !string.IsNullOrWhiteSpace(SettingDTO.CustomerNumber) &&
                        !string.IsNullOrWhiteSpace(SettingDTO.CustomerName));
            }
        }

        public bool IsLoginSettingsReady
        {
            get
            {
                return (!string.IsNullOrWhiteSpace(SettingDTO.LoginName) &&
                        !string.IsNullOrWhiteSpace(SettingDTO.LoginPassword));
            }
        }
    }
}