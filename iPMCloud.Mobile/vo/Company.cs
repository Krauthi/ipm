using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace iPMCloud.Mobile.vo
{
    [Serializable]
    public class Company : SettingDTO
    {

        public Company() { }

        public static bool AddUpdateCompany(AppModel model, SettingDTO s)
        {
            return AddUpdateCompany(model, ToCompany(s));
        }
        public static bool AddUpdateCompany(AppModel model, Company company)
        {
            if (model.Companies == null)
            {
                model.Companies = new List<Company>();
            }
            if (String.IsNullOrWhiteSpace(company.CustomerNumber) || String.IsNullOrWhiteSpace(company.CustomerName) || String.IsNullOrWhiteSpace(company.ServerUrl)) { return false; }
            var compIndex = model.Companies.FindIndex(c => c.CustomerNumber == company.CustomerNumber);
            if (compIndex > -1)
            {
                model.Companies.RemoveAt(compIndex);
            }
            model.Companies.Add(company);
            SaveCompanies(model.Companies);
            return true;
        }


        private static bool SaveCompanies(List<Company> companies)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                string json = "";
                try
                {
                    json = JsonConvert.SerializeObject(companies);
                }
                catch (Exception e)
                {
                    var ewe = e;
                    AppModel.Logger.Error(e);
                    return false;
                }
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, json);
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/companies/");
                if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/companies/companies.ipm");
                File.WriteAllBytes(filePath, ms.ToArray());
                ms.Close();
                ms.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error(ex);
                ms.Close();
                ms.Dispose();
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public static List<Company> LoadCompanies()
        {
            //string filePathx = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/companies/companies.ipm");
            //if (File.Exists(filePathx))
            //{
            //    File.Delete(filePathx);
            //}
            //return null;
            MemoryStream ms = new MemoryStream();
            try
            {
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/companies/");
                if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/companies/companies.ipm");
                if (File.Exists(filePath))
                {
                    byte[] data = File.ReadAllBytes(filePath);
                    BinaryFormatter binForm = new BinaryFormatter();
                    ms.Write(data, 0, data.Length);
                    ms.Seek(0, SeekOrigin.Begin);
                    var json = (Object)binForm.Deserialize(ms) as String;
                    ms.Close();
                    ms.Dispose();
                    return JsonConvert.DeserializeObject<List<Company>>(json);
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
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public static string LoadCompanies_AsJson()
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/companies/");
                if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/companies/companies.ipm");
                if (File.Exists(filePath))
                {
                    byte[] data = File.ReadAllBytes(filePath);
                    BinaryFormatter binForm = new BinaryFormatter();
                    ms.Write(data, 0, data.Length);
                    ms.Seek(0, SeekOrigin.Begin);
                    var json = (Object)binForm.Deserialize(ms) as String;
                    if (json != null)
                    {
                        var obj = JsonConvert.DeserializeObject<List<Company>>(json);
                        if (obj != null)
                        {
                            obj.ForEach(company =>
                            {
                                company.LoginPassword = "";
                                company.LoginToken = "";
                            });
                        }
                        json = JsonConvert.SerializeObject(obj);
                    }
                    ms.Close();
                    ms.Dispose();
                    return json != null ? json : "";
                }
                else
                {
                    ms.Close();
                    ms.Dispose();
                    return "{File not exist}";
                }
            }
            catch (Exception ex)
            {
                ms.Close();
                ms.Dispose();
                return "{Error: " + ex.Message + "}";
            }
        }


        public static bool DeleteCompany(AppModel model, Company company)
        {
            return DeleteCompany(model, company.CustomerNumber);
        }
        public static bool DeleteCompany(AppModel model, string CustomerNumber)
        {
            try
            {
                var companyIndex = model.Companies.FindIndex(c => c.CustomerNumber == CustomerNumber);
                if (companyIndex > -1)
                {
                    model.Companies.RemoveAt(companyIndex);
                    SaveCompanies(model.Companies);
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error(ex);
                return false;
            }
            return true;
        }



        public static SettingDTO ToSettingDTO(Company c)
        {
            SettingDTO s = new SettingDTO
            {
                ServerUrl = "" + c.ServerUrl,
                CustomerNumber = "" + c.CustomerNumber,
                CustomerName = "" + c.CustomerName,
                LoginName = "" + c.LoginName,
                LoginPassword = "" + c.LoginPassword,
                LoginToken = "" + c.LoginToken,
                LastTokenDateTimeTicks = "" + c.LastTokenDateTimeTicks,
                Autologin = c.Autologin,
                FontSize = "" + c.FontSize,
                LastBuildingIdScanned = c.LastBuildingIdScanned,
                LastBuildingSyncedDateTimeTicks = c.LastBuildingSyncedDateTimeTicks,
                PNToken = c.PNToken,
                RunBackground = false,//c.RunBackground,
                SyncTimeHours = c.SyncTimeHours,
                GPSInfoHasShow = c.GPSInfoHasShow,
            };
            return s;
        }
        public static Company ToCompany(SettingDTO c)
        {
            Company s = new Company
            {
                ServerUrl = "" + c.ServerUrl,
                CustomerNumber = "" + c.CustomerNumber,
                CustomerName = "" + c.CustomerName,
                LoginName = "" + c.LoginName,
                LoginPassword = "" + c.LoginPassword,
                LoginToken = "" + c.LoginToken,
                LastTokenDateTimeTicks = "" + c.LastTokenDateTimeTicks,
                Autologin = c.Autologin,
                FontSize = "" + c.FontSize,
                LastBuildingIdScanned = c.LastBuildingIdScanned,
                LastBuildingSyncedDateTimeTicks = c.LastBuildingSyncedDateTimeTicks,
                PNToken = c.PNToken,
                RunBackground = false,// c.RunBackground,
                GPSInfoHasShow = c.GPSInfoHasShow,
                SyncTimeHours = c.SyncTimeHours,
            };
            return s;
        }
    }
}
