using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace iPMCloud.Mobile.vo
{
    public class Company : SettingDTO
    {
        public Company() { }

        #region Add/Update Company

        public static bool AddUpdateCompany(AppModel model, SettingDTO s)
        {
            return AddUpdateCompany(model, ToCompany(s));
        }

        public static bool AddUpdateCompany(AppModel model, Company company)
        {
            try
            {
                if (model.Companies == null)
                {
                    model.Companies = new List<Company>();
                }

                if (string.IsNullOrWhiteSpace(company.CustomerNumber) ||
                    string.IsNullOrWhiteSpace(company.CustomerName) ||
                    string.IsNullOrWhiteSpace(company.ServerUrl))
                {
                    AppModel.Logger?.Warn("AddUpdateCompany: Missing required fields");
                    return false;
                }

                var compIndex = model.Companies.FindIndex(c => c.CustomerNumber == company.CustomerNumber);
                if (compIndex > -1)
                {
                    model.Companies.RemoveAt(compIndex);
                }

                model.Companies.Add(company);
                SaveCompanies(model.Companies);

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: AddUpdateCompany()");
                return false;
            }
        }

        #endregion

        #region Save Companies

        private static bool SaveCompanies(List<Company> companies)
        {
            try
            {
                if (companies == null)
                {
                    AppModel.Logger?.Warn("SaveCompanies: companies list is null");
                    return false;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/companies/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, "companies.ipm");

                // JSON Serialisierung mit Newtonsoft.Json
                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                string jsonString = JsonConvert.SerializeObject(companies, jsonSettings);
                File.WriteAllText(filePath, jsonString);

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: SaveCompanies()");
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        #endregion

        #region Load Companies

        public static List<Company> LoadCompanies()
        {
            try
            {
                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/companies/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, "companies.ipm");

                if (File.Exists(filePath))
                {
                    try
                    {
                        // JSON laden
                        string jsonString = File.ReadAllText(filePath);

                        if (string.IsNullOrWhiteSpace(jsonString))
                        {
                            AppModel.Logger?.Warn("LoadCompanies: File is empty");
                            return new List<Company>();
                        }

                        var jsonSettings = new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Include,
                            DefaultValueHandling = DefaultValueHandling.Include,
                            MissingMemberHandling = MissingMemberHandling.Ignore
                        };

                        List<Company> companies = JsonConvert.DeserializeObject<List<Company>>(jsonString, jsonSettings);
                        return companies ?? new List<Company>();
                    }
                    catch (JsonException jsonEx)
                    {
                        // JSON Fehler - könnte alte BinaryFormatter Datei sein
                        AppModel.Logger?.Warn(jsonEx, "Failed to deserialize JSON, attempting migration");

                        if (TryMigrateLegacyCompanies(filePath, out List<Company> migratedCompanies))
                        {
                            // Nach erfolgreicher Migration neu speichern
                            SaveCompanies(migratedCompanies);
                            return migratedCompanies;
                        }

                        return new List<Company>();
                    }
                }
                else
                {
                    return new List<Company>();
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: LoadCompanies()");
                Console.WriteLine(ex.Message);
                return new List<Company>();
            }
        }

        private static bool TryMigrateLegacyCompanies(string filePath, out List<Company> companies)
        {
            companies = new List<Company>();

            try
            {
                // Alte Datei sichern
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupPath = filePath + $".old_binary_{timestamp}";

                if (File.Exists(filePath))
                {
                    File.Copy(filePath, backupPath, true);
                    AppModel.Logger?.Info($"Legacy companies file backed up to: {backupPath}");
                }

                // In .NET MAUI kann BinaryFormatter nicht mehr verwendet werden
                // Die alte Datei muss manuell konvertiert oder neu erstellt werden
                return false;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: TryMigrateLegacyCompanies()");
                return false;
            }
        }

        #endregion

        #region Load Companies As JSON

        public static string LoadCompanies_AsJson()
        {
            try
            {
                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/companies/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, "companies.ipm");

                if (File.Exists(filePath))
                {
                    string jsonString = File.ReadAllText(filePath);

                    if (string.IsNullOrWhiteSpace(jsonString))
                    {
                        return "{\"Error\": \"File is empty\"}";
                    }

                    // JSON parsen und sensible Daten entfernen
                    var companies = JsonConvert.DeserializeObject<List<Company>>(jsonString);

                    if (companies != null && companies.Count > 0)
                    {
                        // Sensible Daten entfernen
                        companies.ForEach(company =>
                        {
                            company.LoginPassword = "";
                            company.LoginToken = "";
                        });

                        return JsonConvert.SerializeObject(companies, Formatting.Indented);
                    }

                    return jsonString;
                }
                else
                {
                    return "{\"Info\": \"File not exist\"}";
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: LoadCompanies_AsJson()");
                return "{\"Error\": \"" + ex.Message.Replace("\"", "'") + "\"}";
            }
        }

        #endregion

        #region Delete Company

        public static bool DeleteCompany(AppModel model, Company company)
        {
            return DeleteCompany(model, company.CustomerNumber);
        }

        public static bool DeleteCompany(AppModel model, string customerNumber)
        {
            try
            {
                if (model.Companies == null || string.IsNullOrWhiteSpace(customerNumber))
                {
                    return false;
                }

                var companyIndex = model.Companies.FindIndex(c => c.CustomerNumber == customerNumber);

                if (companyIndex > -1)
                {
                    model.Companies.RemoveAt(companyIndex);
                    SaveCompanies(model.Companies);

                    AppModel.Logger?.Info($"Company deleted: {customerNumber}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: DeleteCompany()");
                return false;
            }
        }

        public static bool DeleteAllCompanies()
        {
            try
            {
                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/companies/companies.ipm"
                );

                if (File.Exists(filePath))
                {
                    // Backup erstellen
                    string backupPath = filePath + $".deleted_{DateTime.Now:yyyyMMdd_HHmmss}";
                    File.Copy(filePath, backupPath, true);
                    File.Delete(filePath);

                    AppModel.Logger?.Info($"All companies deleted. Backup: {backupPath}");
                }

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: DeleteAllCompanies()");
                return false;
            }
        }

        #endregion

        #region Conversion Methods

        public static SettingDTO ToSettingDTO(Company c)
        {
            if (c == null)
            {
                return new SettingDTO();
            }

            SettingDTO s = new SettingDTO
            {
                ServerUrl = c.ServerUrl ?? string.Empty,
                CustomerNumber = c.CustomerNumber ?? string.Empty,
                CustomerName = c.CustomerName ?? string.Empty,
                LoginName = c.LoginName ?? string.Empty,
                LoginPassword = c.LoginPassword ?? string.Empty,
                LoginToken = c.LoginToken ?? string.Empty,
                LastTokenDateTimeTicks = c.LastTokenDateTimeTicks ?? string.Empty,
                Autologin = c.Autologin,
                FontSize = c.FontSize ?? string.Empty,
                LastBuildingIdScanned = c.LastBuildingIdScanned,
                LastBuildingSyncedDateTimeTicks = c.LastBuildingSyncedDateTimeTicks,
                PNToken = c.PNToken ?? string.Empty,
                RunBackground = false, // Always false
                SyncTimeHours = c.SyncTimeHours,
                GPSInfoHasShow = c.GPSInfoHasShow,
            };

            return s;
        }

        public static Company ToCompany(SettingDTO c)
        {
            if (c == null)
            {
                return new Company();
            }

            Company s = new Company
            {
                ServerUrl = c.ServerUrl ?? string.Empty,
                CustomerNumber = c.CustomerNumber ?? string.Empty,
                CustomerName = c.CustomerName ?? string.Empty,
                LoginName = c.LoginName ?? string.Empty,
                LoginPassword = c.LoginPassword ?? string.Empty,
                LoginToken = c.LoginToken ?? string.Empty,
                LastTokenDateTimeTicks = c.LastTokenDateTimeTicks ?? string.Empty,
                Autologin = c.Autologin,
                FontSize = c.FontSize ?? string.Empty,
                LastBuildingIdScanned = c.LastBuildingIdScanned,
                LastBuildingSyncedDateTimeTicks = c.LastBuildingSyncedDateTimeTicks,
                PNToken = c.PNToken ?? string.Empty,
                RunBackground = false, // Always false
                GPSInfoHasShow = c.GPSInfoHasShow,
                SyncTimeHours = c.SyncTimeHours,
            };

            return s;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Prüft ob eine Company mit der CustomerNumber bereits existiert
        /// </summary>
        public static bool CompanyExists(AppModel model, string customerNumber)
        {
            if (model?.Companies == null || string.IsNullOrWhiteSpace(customerNumber))
            {
                return false;
            }

            return model.Companies.Any(c => c.CustomerNumber == customerNumber);
        }

        /// <summary>
        /// Holt eine Company anhand der CustomerNumber
        /// </summary>
        public static Company GetCompany(AppModel model, string customerNumber)
        {
            if (model?.Companies == null || string.IsNullOrWhiteSpace(customerNumber))
            {
                return null;
            }

            return model.Companies.FirstOrDefault(c => c.CustomerNumber == customerNumber);
        }

        #endregion
    }
}