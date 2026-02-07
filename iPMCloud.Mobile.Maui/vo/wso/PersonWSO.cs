using iPMCloud.Mobile.vo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Maui.Controls;

namespace iPMCloud.Mobile
{
    [Serializable]
    public class PersonWSO
    {
        public Int32 id = 0;
        public Int32 gruppeid = 0;
        public int rolle = 0;
        public String bn = "";
        public String pw = "";
        public String anrede = "";
        public String firma = "";
        public String vorname = "";
        public String name = "";
        public String strasse = "";
        public String hsnr = "";
        public String adresszusatz = "";
        public String plz = "";
        public String ort = "";
        public String land = "";
        public String mobile = "";
        public String telefon = "";
        public String fax = "";
        public String mail = "";
        public String kategorie = "";

        public int allsync = 0;

        public object element;

        public byte[] userIcon;


        //public List<PersonPlanWSO> Plans { get; set; } = new List<PersonPlanWSO>();


        public PersonWSO() { }




        public static StackLayout GetPersonTimesView(List<PersonTime> pts)
        {
            var stack = new StackLayout
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };
            var list = new List<LeistungWSO>();
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
            var w = App.Current.MainPage.Width;
            bool isgleich = pt.start == pt.end;
            bool isMinus = pt.top.Contains("-");
            bool isZero = pt.top.Contains("00:00");
            var day = new Label()
            {
                Text = pt.tag + " " + pt.tagname,
                TextColor = Color.FromHex("#cccccc"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                WidthRequest = w * 0.21,
                LineBreakMode = LineBreakMode.TailTruncation,
                HorizontalTextAlignment = Xamarin.Forms.TextAlignment.Start,
            };
            var times = new Label
            {
                Text = isgleich || (!String.IsNullOrWhiteSpace(pt.grund) && pt.grund != "L" && pt.grund != "G") ? "--:--" : (pt.start + " - " + pt.end),
                TextColor = Color.FromHex("#cccccc"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.TailTruncation,
                HorizontalTextAlignment = Xamarin.Forms.TextAlignment.Start,
                WidthRequest = w * 0.25,
            };
            var pause = new Label
            {
                Text = pt.pause.Contains("00:00") || pt.pause.Contains("--:--") ? "--:--" : ("-" + pt.pause),
                TextColor = pt.pause.Contains("00:00") || pt.pause.Contains("--:--") ? Color.FromHex("#cccccc") : Color.FromHex("#ff7777"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.TailTruncation,
                HorizontalTextAlignment = Xamarin.Forms.TextAlignment.Start,
                WidthRequest = w * 0.18,
            };
            var updown = new Label
            {
                Text = isZero ? "--:--" : ((isMinus ? "" : "+") + pt.top.Replace(" ", "")),
                TextColor = isZero ? Color.FromHex("#cccccc") : (isMinus ? Color.FromHex("#ff7777") : Color.FromHex("#77cc77")),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.TailTruncation,
                HorizontalTextAlignment = Xamarin.Forms.TextAlignment.Start,
                WidthRequest = w * 0.18,
            };
            var summe = new Label
            {
                Text = pt.dauer,
                TextColor = pt.dauer.Contains("-") ? Color.FromHex("#ff7777") : Color.FromHex("#ffffff"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.TailTruncation,
                HorizontalTextAlignment = Xamarin.Forms.TextAlignment.End,
                WidthRequest = w * 0.18,
            };
            var h = new StackLayout()
            {
                Padding = new Thickness(5, 5, 5, 5),
                Margin = new Thickness(0, 0, 0, 1),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromHex("#aa042d53"),
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
            var w = App.Current.MainPage.Width;
            var day = new Label()
            {
                Text = "Tag",
                TextColor = Color.FromHex("#cccccc"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                WidthRequest = w * 0.21,
                LineBreakMode = LineBreakMode.TailTruncation,
                HorizontalTextAlignment = Xamarin.Forms.TextAlignment.Start,
            };
            var times = new Label
            {
                Text = "Zeiten",
                TextColor = Color.FromHex("#cccccc"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.TailTruncation,
                WidthRequest = w * 0.25,
            };
            var pause = new Label
            {
                Text = "Pause",
                TextColor = Color.FromHex("#cccccc"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.TailTruncation,
                WidthRequest = w * 0.18,
            };
            var fahrzeit = new Label
            {
                Text = "+/-",
                TextColor = Color.FromHex("#cccccc"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.TailTruncation,
                WidthRequest = w * 0.18,
            };
            var summe = new Label
            {
                Text = "Gesamt",
                TextColor = Color.FromHex("#cccccc"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.TailTruncation,
                WidthRequest = w * 0.18,
                HorizontalTextAlignment = Xamarin.Forms.TextAlignment.End,
            };
            var h = new StackLayout()
            {
                Padding = new Thickness(5, 5, 5, 5),
                Margin = new Thickness(0, 0, 0, 1),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromHex("#aa042d53"),
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
            var w = App.Current.MainPage.Width;
            var day = new Label()
            {
                Text = pt.monatname + " " + pt.jahr,
                TextColor = Color.FromHex("#cccccc"),
                Margin = new Thickness(5, 0, 5, 0),
                FontSize = 12,
                HorizontalOptions = LayoutOptions.StartAndExpand,
                LineBreakMode = LineBreakMode.TailTruncation,
            };
            var times = new Label
            {
                Text = pt.all,
                TextColor = Color.FromHex("#ffffff"),
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
                BackgroundColor = Color.FromHex("#aa042d53"),
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
                TextColor = Color.FromHex("#cccccc"),
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
                BackgroundColor = Color.FromHex("#aa042d53"),
            };
            h.Children.Add(day);
            return h;
        }












        public static bool SavePerson(AppModel model, PersonWSO person)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, person);
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/user/");
                if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/user/loginuser.ipm");
                File.WriteAllBytes(filePath, ms.ToArray());
                ms.Close();
                ms.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                ms.Close();
                ms.Dispose();
                AppModel.Logger.Error(ex);
                return false;
            }
        }
        public static PersonWSO LoadPerson(AppModel model)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                if (!String.IsNullOrWhiteSpace(model.SettingModel.SettingDTO.CustomerNumber))
                {
                    string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/user/");
                    if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                    string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/user/loginuser.ipm");
                    if (File.Exists(filePath))
                    {
                        byte[] data = File.ReadAllBytes(filePath);
                        BinaryFormatter binForm = new BinaryFormatter();
                        ms.Write(data, 0, data.Length);
                        ms.Seek(0, SeekOrigin.Begin);
                        var person = (Object)binForm.Deserialize(ms) as PersonWSO;
                        ms.Close();
                        ms.Dispose();
                        return person;
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
                AppModel.Logger.Error(ex);
                return null;
            }
        }
        public static string LoadPerson_AsJson()
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                if (!String.IsNullOrWhiteSpace(AppModel.Instance.SettingModel.SettingDTO.CustomerNumber))
                {
                    string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/user/");
                    if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                    string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/user/loginuser.ipm");
                    if (File.Exists(filePath))
                    {
                        byte[] data = File.ReadAllBytes(filePath);
                        BinaryFormatter binForm = new BinaryFormatter();
                        ms.Write(data, 0, data.Length);
                        ms.Seek(0, SeekOrigin.Begin);
                        var person = (Object)binForm.Deserialize(ms) as PersonWSO;
                        ms.Close();
                        ms.Dispose();
                        return JsonConvert.SerializeObject(person);
                    }
                    else
                    {
                        ms.Close();
                        ms.Dispose();
                        return "{File not exist}";
                    }
                }
                else
                {
                    ms.Close();
                    ms.Dispose();
                    return "{Error: CustomerNumber not set}";
                }
            }
            catch (Exception ex)
            {
                ms.Close();
                ms.Dispose();
                AppModel.Logger.Error(ex);
                return "{Error: " + ex.Message + "}";
            }
        }
        public static bool DeletePerson(AppModel model)
        {
            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/user/loginuser.ipm");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error(ex);
                return false;
            }
            return true;
        }


    }
}