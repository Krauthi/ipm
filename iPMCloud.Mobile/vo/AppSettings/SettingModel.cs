using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace iPMCloud.Mobile.vo
{
    [Serializable]
    public class SettingModel
    {
        public const string FIX_SERVER_URL = "http://5.35.250.51/IPMRegistrationService";
        public const string FIX_SERVER_URL_DEV_B = "http://nbkrauthausen/IPMRegistrationService";
        public const string FIX_SERVER_URL_DEV = "http://192.168.2.121:45455";

        public AppModel model { get; set; }

        private SettingDTO _settingDTO = new SettingDTO();
        public SettingDTO SettingDTO { get => _settingDTO; set => _settingDTO = value; }

        public SettingModel() { }

        // Aufruf in MainActivity und AppDelegate
        public void InitializeSettings()
        {
            LoadSettings();
            //if (SettingDTO.SyncTimeHours == 0) { SettingDTO.SyncTimeHours = 12; SaveSettings(); }
            if (IoLoadError)
            {
                IoLoadError = false; SettingDTO = new SettingDTO();
            }
            SettingDTO.RunBackground = false; // FIX false

            if (model != null && model.IsTest)
            {
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
                // SettingDTO.ServerUrl = "https://utermark.ipmcloud.de/";
                //SettingDTO.CustomerNumber = "10099";


                //SettingDTO.LoginName = "svenkilian";
                //SettingDTO.LoginPassword = "heinrichshms";
                //SettingDTO.ServerUrl = "https://heinrichs-hms.ipmcloud.de";
                //SettingDTO.CustomerNumber = "10020";


                SettingDTO.LoginName = "L.Schmitz";
                SettingDTO.LoginPassword = "16071967";
                SettingDTO.ServerUrl = "https://tschoecke.ipmcloud.de";
                SettingDTO.CustomerNumber = "11037";



                SettingDTO.LoginName = "Esposito";
                SettingDTO.LoginPassword = "21081986";
                SettingDTO.CustomerName = "iPD Hensel Immobilien Pflege und Dienstleistungen";
                SettingDTO.CustomerNumber = "1"; // "10074";
                SettingDTO.ServerUrl = "https://test-hensel.ipmcloud.de";


                SaveSettings();
            }

        }



        public bool IoSaveError { get; set; } = false;
        public bool IoLoadError { get; set; } = false;
        public bool IoDeleteError { get; set; } = false;


        public bool SaveSettings()
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                if (!SettingDTO.Autologin)
                {
                    //Passwort nicht speichern
                    //SettingDTO.LoginPassword = "";
                }
                IoSaveError = false;
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, SettingDTO);
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/settings/");
                if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/settings/set.ipm");
                File.WriteAllBytes(filePath, ms.ToArray());
                ms.Close();
                ms.Dispose();
                Company.AddUpdateCompany(model, SettingDTO);
                return true;
            }
            catch (Exception ex)
            {
                ms.Close();
                ms.Dispose();
                Console.WriteLine(ex.Message);
                IoSaveError = true;
                return false;
            }
        }
        public bool LoadSettings()
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                //? WICHTIG!!!!
                // Counter wenn es ein Problem mit den Settings gibt dann läuft es hier im Kreis!
                // Abstellen in dem man zählt wieviel versucher machen darf und dann Abbrechen!
                IoLoadError = false;
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/settings/");
                if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/settings/set.ipm");
                if (File.Exists(filePath))
                {
                    byte[] data = File.ReadAllBytes(filePath);
                    BinaryFormatter binForm = new BinaryFormatter();
                    ms.Write(data, 0, data.Length);
                    ms.Seek(0, SeekOrigin.Begin);
                    var obj = (Object)binForm.Deserialize(ms) as SettingDTO;
                    SettingDTO = obj;
                    ms.Close();
                    ms.Dispose();
                    return true;
                }
                else
                {
                    ms.Close();
                    ms.Dispose();
                    SettingDTO = new SettingDTO();
                    SaveSettings();
                    LoadSettings();
                    return true;
                }
            }
            catch (Exception)
            {
                ms.Close();
                ms.Dispose();
                IoLoadError = true;
                return false;
            }
        }
        public string LoadSettings_AsJson()
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/settings/");
                if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/settings/set.ipm");
                if (File.Exists(filePath))
                {
                    byte[] data = File.ReadAllBytes(filePath);
                    BinaryFormatter binForm = new BinaryFormatter();
                    ms.Write(data, 0, data.Length);
                    ms.Seek(0, SeekOrigin.Begin);
                    var obj = (Object)binForm.Deserialize(ms) as SettingDTO;
                    ms.Close();
                    ms.Dispose();
                    obj.LoginPassword = "";
                    obj.LoginToken = "";
                    return JsonConvert.SerializeObject(obj);
                }
                else
                {
                    ms.Close();
                    ms.Dispose();
                    SettingDTO = new SettingDTO();
                    SaveSettings();
                    LoadSettings();
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

        public bool DeleteSettings()
        {
            try
            {
                IoDeleteError = false;
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/settings/set.ipm");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception)
            {
                IoDeleteError = true;
                return false;
            }
            return true;
        }


        public bool IsCredentialsSettingsReady
        {
            get
            {
                return (!String.IsNullOrWhiteSpace(SettingDTO.ServerUrl) &&
                        !String.IsNullOrWhiteSpace(SettingDTO.CustomerNumber) &&
                        !String.IsNullOrWhiteSpace(SettingDTO.CustomerName));
            }
        }
        public bool IsLoginSettingsReady
        {
            get
            {
                return (!String.IsNullOrWhiteSpace(SettingDTO.LoginName) &&
                        !String.IsNullOrWhiteSpace(SettingDTO.LoginPassword));
            }
        }


    }
}
