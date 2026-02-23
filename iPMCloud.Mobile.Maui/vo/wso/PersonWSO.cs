using iPMCloud.Mobile.vo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Maui.Controls;

namespace iPMCloud.Mobile
{
    public class PersonWSO
    {
        public Int32 id { get; set; } = 0;
        public Int32 gruppeid { get; set; } = 0;
        public int rolle { get; set; } = 0;
        public string bn { get; set; } = "";
        public string pw { get; set; } = "";
        public string anrede { get; set; } = "";
        public string firma { get; set; } = "";
        public string vorname { get; set; } = "";
        public string name { get; set; } = "";
        public string strasse { get; set; } = "";
        public string hsnr { get; set; } = "";
        public string adresszusatz { get; set; } = "";
        public string plz { get; set; } = "";
        public string ort { get; set; } = "";
        public string land { get; set; } = "";
        public string mobile { get; set; } = "";
        public string telefon { get; set; } = "";
        public string fax { get; set; } = "";
        public string mail { get; set; } = "";
        public string kategorie { get; set; } = "";
        public int allsync { get; set; } = 0;

        [JsonIgnore] // Nicht serialisieren (kann nicht direkt serialisiert werden)
        public object element { get; set; }

        public byte[] userIcon { get; set; }

        //public List<PersonPlanWSO> Plans { get; set; } = new List<PersonPlanWSO>();

        public PersonWSO() { }

        #region UI Helper Methods

        public static StackLayout GetPersonTimesView(List<PersonTime> pts)
        {
            var stack = new StackLayout
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.Fill,
            };

            if (pts != null && pts.Count > 0)
            {
                pts.ForEach(pt =>
                {
                    stack.Children.Add(GetPersonTimesViewItem(pt));
                });
            }
            else
            {
                stack.Children.Add(GetPersonTimesViewEmptyItem());
            }

            return stack;
        }

        public static StackLayout GetPersonTimesViewItem(PersonTime pt)
        {
            var w = App.Current.Windows.FirstOrDefault().Width;
            bool isgleich = pt.start == pt.end;
            bool isMinus = pt.top.Contains("-");
            bool isZero = pt.top.Contains("00:00");

            var day = new Label()
            {
                Text = pt.tag + " " + pt.tagname,
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                WidthRequest = w * 0.21,
                LineBreakMode = LineBreakMode.TailTruncation,
                HorizontalTextAlignment = TextAlignment.Start,
            };

            var times = new Label
            {
                Text = isgleich || (!string.IsNullOrWhiteSpace(pt.grund) && pt.grund != "L" && pt.grund != "G")
                    ? "--:--"
                    : (pt.start + " - " + pt.end),
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.TailTruncation,
                HorizontalTextAlignment = TextAlignment.Start,
                WidthRequest = w * 0.25,
            };

            var pause = new Label
            {
                Text = pt.pause.Contains("00:00") || pt.pause.Contains("--:--") ? "--:--" : ("-" + pt.pause),
                TextColor = pt.pause.Contains("00:00") || pt.pause.Contains("--:--")
                    ? Color.FromArgb("#cccccc")
                    : Color.FromArgb("#ff7777"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.TailTruncation,
                HorizontalTextAlignment = TextAlignment.Start,
                WidthRequest = w * 0.18,
            };

            var updown = new Label
            {
                Text = isZero ? "--:--" : ((isMinus ? "" : "+") + pt.top.Replace(" ", "")),
                TextColor = isZero
                    ? Color.FromArgb("#cccccc")
                    : (isMinus ? Color.FromArgb("#ff7777") : Color.FromArgb("#77cc77")),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.TailTruncation,
                HorizontalTextAlignment = TextAlignment.Start,
                WidthRequest = w * 0.18,
            };

            var summe = new Label
            {
                Text = pt.dauer,
                TextColor = pt.dauer.Contains("-") ? Color.FromArgb("#ff7777") : Color.FromArgb("#ffffff"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.TailTruncation,
                HorizontalTextAlignment = TextAlignment.End,
                WidthRequest = w * 0.18,
            };

            var h = new StackLayout()
            {
                Padding = new Thickness(5, 5, 5, 5),
                Margin = new Thickness(0, 0, 0, 1),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromArgb("#aa042d53"),
            };

            h.Children.Add(day);
            h.Children.Add(times);
            h.Children.Add(pause);
            h.Children.Add(updown);
            h.Children.Add(summe);

            return h;
        }

        public static StackLayout GetPersonTimesViewHeaderItem()
        {
            var w = App.Current.Windows.FirstOrDefault().Width;

            var day = new Label()
            {
                Text = "Tag",
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                WidthRequest = w * 0.21,
                LineBreakMode = LineBreakMode.TailTruncation,
                HorizontalTextAlignment = TextAlignment.Start,
            };

            var times = new Label
            {
                Text = "Zeiten",
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.TailTruncation,
                WidthRequest = w * 0.25,
            };

            var pause = new Label
            {
                Text = "Pause",
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.TailTruncation,
                WidthRequest = w * 0.18,
            };

            var fahrzeit = new Label
            {
                Text = "+/-",
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.TailTruncation,
                WidthRequest = w * 0.18,
            };

            var summe = new Label
            {
                Text = "Gesamt",
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.TailTruncation,
                WidthRequest = w * 0.18,
                HorizontalTextAlignment = TextAlignment.End,
            };

            var h = new StackLayout()
            {
                Padding = new Thickness(5, 5, 5, 5),
                Margin = new Thickness(0, 0, 0, 1),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromArgb("#aa042d53"),
            };

            h.Children.Add(day);
            h.Children.Add(times);
            h.Children.Add(pause);
            h.Children.Add(fahrzeit);
            h.Children.Add(summe);

            return h;
        }

        public static StackLayout GetPersonTimesViewAllItem(PersonTime pt)
        {
            var w = App.Current.Windows.FirstOrDefault().Width;

            var day = new Label()
            {
                Text = pt.monatname + " " + pt.jahr,
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(5, 0, 5, 0),
                FontSize = 12,
                HorizontalOptions = LayoutOptions.StartAndExpand,
                LineBreakMode = LineBreakMode.TailTruncation,
            };

            var times = new Label
            {
                Text = pt.all,
                TextColor = Color.FromArgb("#ffffff"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.TailTruncation,
                HorizontalOptions = LayoutOptions.End,
            };

            var h = new StackLayout()
            {
                Padding = new Thickness(5, 5, 5, 5),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromArgb("#aa042d53"),
            };

            h.Children.Add(day);
            h.Children.Add(times);

            return h;
        }

        public static StackLayout GetPersonTimesViewEmptyItem()
        {
            var day = new Label()
            {
                Text = "Es konnten keine Zeiten abgerufen werden!",
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(5, 0, 5, 1),
                FontSize = 14,
                HorizontalOptions = LayoutOptions.StartAndExpand,
                LineBreakMode = LineBreakMode.WordWrap,
            };

            var h = new StackLayout()
            {
                Padding = new Thickness(5, 5, 5, 5),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromArgb("#aa042d53"),
            };

            h.Children.Add(day);

            return h;
        }

        #endregion

        #region Persistence Methods (JSON statt BinaryFormatter)

        public static bool SavePerson(AppModel model, PersonWSO person)
        {
            try
            {
                if (model == null || person == null)
                {
                    AppModel.Logger?.Error("SavePerson: model or person is null");
                    return false;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/user/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, "loginuser.ipm");

                // JSON Serialisierung mit Newtonsoft.Json
                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                string jsonString = JsonConvert.SerializeObject(person, jsonSettings);
                File.WriteAllText(filePath, jsonString);

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: SavePerson()");
                return false;
            }
        }

        public static PersonWSO LoadPerson(AppModel model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.SettingModel.SettingDTO.CustomerNumber))
                {
                    AppModel.Logger?.Warn("LoadPerson: model is null or CustomerNumber not set");
                    return null;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/user/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, "loginuser.ipm");

                if (File.Exists(filePath))
                {
                    try
                    {
                        // JSON laden
                        string jsonString = File.ReadAllText(filePath);

                        if (string.IsNullOrWhiteSpace(jsonString))
                        {
                            AppModel.Logger?.Warn("LoadPerson: File is empty");
                            return null;
                        }

                        var jsonSettings = new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Include,
                            DefaultValueHandling = DefaultValueHandling.Include,
                            MissingMemberHandling = MissingMemberHandling.Ignore
                        };

                        PersonWSO person = JsonConvert.DeserializeObject<PersonWSO>(jsonString, jsonSettings);
                        return person;
                    }
                    catch (JsonException jsonEx)
                    {
                        // JSON Fehler - könnte alte BinaryFormatter Datei sein
                        AppModel.Logger?.Warn(jsonEx, "Failed to deserialize JSON, attempting migration");

                        if (TryMigrateLegacyPerson(filePath, out PersonWSO migratedPerson))
                        {
                            // Nach erfolgreicher Migration neu speichern
                            SavePerson(model, migratedPerson);
                            return migratedPerson;
                        }

                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: LoadPerson()");
                return null;
            }
        }

        private static bool TryMigrateLegacyPerson(string filePath, out PersonWSO person)
        {
            person = null;

            try
            {
                // Alte Datei sichern
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupPath = filePath + $".old_binary_{timestamp}";

                if (File.Exists(filePath))
                {
                    File.Copy(filePath, backupPath, true);
                    AppModel.Logger?.Info($"Legacy person file backed up to: {backupPath}");
                }

                // In .NET MAUI kann BinaryFormatter nicht mehr verwendet werden
                // Die alte Datei muss manuell konvertiert oder neu erstellt werden
                return false;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: TryMigrateLegacyPerson()");
                return false;
            }
        }

        public static string LoadPerson_AsJson()
        {
            try
            {
                if (AppModel.Instance == null ||
                    string.IsNullOrWhiteSpace(AppModel.Instance.SettingModel.SettingDTO.CustomerNumber))
                {
                    return "{\"Error\": \"CustomerNumber not set\"}";
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/user/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, "loginuser.ipm");

                if (File.Exists(filePath))
                {
                    string jsonString = File.ReadAllText(filePath);

                    if (string.IsNullOrWhiteSpace(jsonString))
                    {
                        return "{\"Error\": \"File is empty\"}";
                    }

                    // JSON bereits vorhanden, einfach zurückgeben
                    // Optional: Sensible Daten entfernen
                    var person = JsonConvert.DeserializeObject<PersonWSO>(jsonString);

                    if (person != null)
                    {
                        // Passwort aus Ausgabe entfernen
                        person.pw = "";
                        return JsonConvert.SerializeObject(person, Formatting.Indented);
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
                AppModel.Logger?.Error(ex, "ERROR: LoadPerson_AsJson()");
                return "{\"Error\": \"" + ex.Message.Replace("\"", "'") + "\"}";
            }
        }

        public static bool DeletePerson(AppModel model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.SettingModel.SettingDTO.CustomerNumber))
                {
                    AppModel.Logger?.Warn("DeletePerson: model is null or CustomerNumber not set");
                    return false;
                }

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/user/loginuser.ipm"
                );

                if (File.Exists(filePath))
                {
                    // Optional: Backup vor dem Löschen
                    string backupPath = filePath + $".deleted_{DateTime.Now:yyyyMMdd_HHmmss}";
                    File.Copy(filePath, backupPath, true);

                    File.Delete(filePath);

                    AppModel.Logger?.Info($"Person deleted. Backup created: {backupPath}");
                }

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: DeletePerson()");
                return false;
            }
        }

        #endregion
    }
}