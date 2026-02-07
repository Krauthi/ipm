using iPMCloud.Mobile.vo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace iPMCloud.Mobile
{
    [Serializable]
    public class TicketPerson
    {
        public enum PersonStatus
        {
            Abgelehnt,
            Angenommen,
            Erldeigt,
            Weitergeleitet,
            InArbeit,
            Gesehen,
            Neu,
            Kein,
        }

        public Int32 besitzerId = 0;
        public string besitzerName = "";
        public DateTime? aenderungsDatum = null;
        public DateTime? zugewiesenDatum = null;
        public PersonStatus status = PersonStatus.Kein;
    }

    [Serializable]
    public class Ticket
    {
        public enum TicketStatus
        {
            Abgelehnt,
            Angenommen,
            Erldeigt,
            Weitergeleitet,
            InArbeit,
            Gesehen,
            Neu,
            Kein,
        }

        public Int32 id;
        public DateTime? letztesAenderungsDatum = null;
        public DateTime? datum = null; // Erstell Datum
        public string titel = "";
        public string beschreibung = "";
        public Int32 erstellerId = 0;
        public string erstellerName = "";
        public Int32 besitzerId = 0;
        public string besitzerName = "";
        public List<TicketPerson> histPersons = new List<TicketPerson>();

        public Ticket()
        {
        }

        public static bool Save(Ticket t)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, t);
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" +
                    AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/ticket/");
                if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" +
                    AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/ticket/" + t.id + ".ipm");
                File.WriteAllBytes(filePath, ms.ToArray());
                ms.Close();
                ms.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                ms.Close();
                ms.Dispose();
                AppModel.Logger.Error("Save Ticket - " + ex);
                return false;
            }
        }


        public static List<Ticket> LoadAll()
        {
            List<Ticket> list = new List<Ticket>();
            try
            {
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" +
                    AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/ticket/");
                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath);
                    if (files != null && files.Length > 0)
                    {
                        files.ToList().ForEach(file =>
                        {
                            var loadedTicket = Ticket.Load(Int32.Parse(file.Replace(".ipm", "")));
                            if (loadedTicket != null)
                            {
                                list.Add(loadedTicket);
                            }
                        });
                    }
                    list = list.OrderByDescending(d => d.letztesAenderungsDatum).ToList();
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error("LoadAll Ticket - " + ex);
            }
            return list;
        }

        public static Ticket Load(Int32 id)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                if (!String.IsNullOrWhiteSpace(AppModel.Instance.SettingModel.SettingDTO.CustomerNumber))
                {
                    string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" +
                        AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/ticket/");
                    if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                    string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" +
                        AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/ticket/" + id + ".ipm");
                    if (File.Exists(filePath))
                    {
                        byte[] data = File.ReadAllBytes(filePath);
                        BinaryFormatter binForm = new BinaryFormatter();
                        ms.Write(data, 0, data.Length);
                        ms.Seek(0, SeekOrigin.Begin);
                        var pn = (Object)binForm.Deserialize(ms) as Ticket;
                        ms.Close();
                        ms.Dispose();
                        return pn;
                    }
                    else
                    {
                        ms.Close();
                        ms.Dispose();
                        return null;
                    }
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
                AppModel.Logger.Error("Load Ticket - " + ex);
                return null;
            }
        }


        public static bool Delete(Int32 id)
        {
            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" +
                    AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/ticket/" + id + ".ipm");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error("Delete Ticket - " + ex);
                return false;
            }
            return true;
        }



    }


}