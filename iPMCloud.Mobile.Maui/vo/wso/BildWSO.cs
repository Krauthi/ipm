using iPMCloud.Mobile.vo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace iPMCloud.Mobile
{
    [Serializable]
    public class BildWSO
    {
        public String name = "";
        public byte[] bytes;
        public string guid = "";
        public string mainguid = "";
        public Int32 bemId = -1;


        public string filename { get; set; } = "";

        [NonSerialized]
        public StackLayout stack;

        public BildWSO(string mainguid)
        {
            this.mainguid = mainguid;
            this.guid = Guid.NewGuid().ToString();
        }



        public static StackLayout GetAttachmentForNoticeElement(ImageSource source, string text, ICommand func = null)
        {
            return new StackLayout
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Orientation = StackOrientation.Vertical,
                Spacing = 0,
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Children = {
                    new StackLayout
                    {
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        Orientation = StackOrientation.Horizontal,
                        Spacing = 10,
                        Padding = new Thickness(0),
                        Margin = new Thickness(5, 2, 5, 2),
                        Children = {
                            new Image
                            {
                                Margin = new Thickness(0,0,20,0), HeightRequest = 60,
                                HorizontalOptions = LayoutOptions.Start,
                                VerticalOptions = LayoutOptions.CenterAndExpand,
                                Source = source,
                            },
                            new Label
                            {
                                Text = text,
                                Padding = new Thickness(0),
                                Margin = new Thickness(0),
                                HorizontalOptions = LayoutOptions.StartAndExpand,
                                VerticalOptions = LayoutOptions.CenterAndExpand,
                                FontSize = 14, TextColor = Colors.White, LineBreakMode = LineBreakMode.WordWrap
                            },
                            new Frame
                            {
                                BackgroundColor = Color.FromArgb("#041d43"),
                                 Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
                                HorizontalOptions = LayoutOptions.End,
                                VerticalOptions = LayoutOptions.CenterAndExpand,
                                Padding = new Thickness(1),
                                Margin = new Thickness(3,0,3,0),
                                Content = new StackLayout
                                {
                                    BackgroundColor = Color.FromArgb("#042d53"),
                                    HeightRequest = 40, WidthRequest = 40,
                                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                                    VerticalOptions = LayoutOptions.CenterAndExpand,
                                    Padding = new Thickness(0),
                                    Margin = new Thickness(0),
                                    Spacing = 0,
                                    Children =
                                    {
                                        new Image
                                        {
                                            Margin = new Thickness(0), HeightRequest = 30, WidthRequest = 30,
                                            HorizontalOptions = LayoutOptions.CenterAndExpand,
                                            VerticalOptions = LayoutOptions.CenterAndExpand,
                                            Source = AppModel.Instance.imagesBase.Trash,
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new BoxView
                    {
                        BackgroundColor = Colors.Gray,
                        HeightRequest = 1,
                        VerticalOptions = LayoutOptions.Start,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        Margin = new Thickness(0)
                    }
                }
            };
        }





        public static bool Save(AppModel model, BildWSO b)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                var json = JsonConvert.SerializeObject(b);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, json);
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/nbi/");
                if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/nbi/_" + b.guid + "_" + b.mainguid + ".ipm");
                File.WriteAllBytes(filePath, ms.ToArray());
                ms.Close();
                ms.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                ms.Close();
                ms.Dispose();
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public static List<BildWSO> LoadFromGuid(AppModel model, string guid)
        {
            List<BildWSO> list = new List<BildWSO>();
            try
            {
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/nbi/");
                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath);
                    if (files != null && files.Length > 0)
                    {
                        files.ToList().ForEach(file =>
                        {
                            if (file.Contains(guid))
                            {
                                list.Add(BildWSO.Load(model, file));
                            }
                        });
                    }
                }
            }
            catch (Exception)
            {
            }
            return list;

        }
        public static BildWSO Load(AppModel model, string filename)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                string filePath = Path.Combine(filename);
                if (File.Exists(filePath))
                {
                    byte[] data = File.ReadAllBytes(filePath);
                    BinaryFormatter binForm = new BinaryFormatter();
                    ms.Write(data, 0, data.Length);
                    ms.Seek(0, SeekOrigin.Begin);
                    var json = (Object)binForm.Deserialize(ms) as String;
                    ms.Close();
                    ms.Dispose();
                    var bildwso = JsonConvert.DeserializeObject<BildWSO>(json);
                    bildwso.filename = filename;
                    return bildwso;
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
        public static bool Delete(AppModel model, BildWSO b)
        {
            try
            {
                //string filePath = Path.Combine(b.filename);
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/nbi/_" + b.guid + "_" + b.mainguid + ".ipm");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }
        public static bool DeleteAllTemp()
        {
            try
            {
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/nbi/");
                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath);
                    if (files != null && files.Length > 0)
                    {
                        files.ToList().ForEach(file =>
                        {
                            if (File.Exists(file))
                            {
                                File.Delete(file);
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }



        public static bool SaveToStack(AppModel model, BildWSO b)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                var json = JsonConvert.SerializeObject(b);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, json);
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/nbis/");
                if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/nbis/_" + b.guid + ".ipm");
                File.WriteAllBytes(filePath, ms.ToArray());
                ms.Close();
                ms.Dispose();
                if (AppModel.Instance.MainPage != null) { AppModel.Instance.MainPage.SetAllSyncState(); }
                return true;
            }
            catch (Exception ex)
            {
                ms.Close();
                ms.Dispose();
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public static List<BildWSO> LoadAllFromStack()
        {
            List<BildWSO> list = new List<BildWSO>();
            try
            {
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/nbis/");
                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath);
                    if (files != null && files.Length > 0)
                    {
                        files.ToList().ForEach(file =>
                        {
                            list.Add(BildWSO.Load(AppModel.Instance, file));
                        });
                    }
                }
            }
            catch (Exception)
            {
            }
            return list;
        }
        public static int CountFromStack()
        {
            try
            {
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/nbis/");
                if (Directory.Exists(directoryPath))
                {
                    return Directory.GetFiles(directoryPath).Length;
                }
            }
            catch (Exception)
            {
                return 0;
            }
            return 0;
        }
        public static bool DeleteFromStack(BildWSO pic)
        {
            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/nbis/_" + pic.guid + ".ipm");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    if (AppModel.Instance.MainPage != null) { AppModel.Instance.MainPage.SetAllSyncState(); }
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