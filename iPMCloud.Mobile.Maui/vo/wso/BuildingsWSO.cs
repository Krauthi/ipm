using iPMCloud.Mobile.vo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Devices;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace iPMCloud.Mobile
{
    [Serializable]
    public class BuildingWSO : ICloneable
    {
        public Int32 id;
        public string objektnr;
        public Int32 gruppeid;
        public Int32 personid;
        //public Int32 aspid;
        //public Int32 bankid;
        public string objektname;
        //public string type 
        //public string status 
        public string strasse;
        public string hsnr;
        public string plz;
        public string ort;
        //public string adresszusatz;
        public String notiz;
        public string land;
        //public string xa;
        //public string xb;
        //public string xc;
        //public string xd;
        public int del;
        //public string lastchange 
        //public string bild;
        //public string qrbild;
        public string handwerkers;
        //public Int32 refid;


        // ? public List<BemerkungWSO> bemerkungen;

        public List<PersonWSO> ArrayOfHandwerker = new List<PersonWSO>();
        public List<ObjektDataWSO> ArrayOfObjektdata = new List<ObjektDataWSO>();
        public List<AuftragWSO> ArrayOfAuftrag = new List<AuftragWSO>();

        public List<Check> ArrayOfChecks = null;

        public String longname = "";
        public String kundename = "";

        public Boolean personinplan = false;


        public bool isInTodoVisible = true;
        public double prio = 1000000000;
        public double prioMax = 1000000000;

        public BuildingWSO() { }



        public static List<BuildingWSO> GetAllBuildings(AppModel model, bool sorted = false)
        {
            List<BuildingWSO> list = new List<BuildingWSO>();
            try
            {
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/buildings/");
                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath);
                    if (files != null && files.Length > 0)
                    {
                        files.ToList().ForEach(building =>
                        {
                            building = building.Replace(directoryPath, "").Replace("b_", "").Replace(".ipm", "");
                            var b = BuildingWSO.LoadBuilding(model, int.Parse(building));
                            if (b.del == 0)
                            {
                                list.Add(BuildingWSO.LoadBuilding(model, int.Parse(building)));
                            }
                        });
                    }
                    if (sorted) { list = list.OrderBy(o => o.id).ToList(); }
                }
            }
            catch (Exception e)
            {
                var a = 0;
            }
            return list;

        }

        public static string GetAllBuildings_ASJSON()
        {
            List<BuildingWSO> list = new List<BuildingWSO>();
            try
            {
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/buildings/");
                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath);
                    if (files != null && files.Length > 0)
                    {
                        files.ToList().ForEach(building =>
                        {
                            building = building.Replace(directoryPath, "").Replace("b_", "").Replace(".ipm", "");
                            var b = BuildingWSO.LoadBuilding(AppModel.Instance, int.Parse(building));
                            if (b.del == 0)
                            {
                                list.Add(BuildingWSO.LoadBuilding(AppModel.Instance, int.Parse(building)));
                            }
                        });
                    }
                }
            }
            catch (Exception e)
            {
                var a = 0;
            }
            return JsonConvert.SerializeObject(list); ;

        }

        public static List<PersonWSO> GetAllWorkers(List<BuildingWSO> buildings)
        {
            List<PersonWSO> workers = new List<PersonWSO>();
            if (buildings != null)
            {
                buildings.ForEach(bu =>
                {
                    if (bu.del == 0 && bu.ArrayOfHandwerker != null && bu.ArrayOfHandwerker.Count > 0)
                    {
                        bu.ArrayOfHandwerker.ForEach(ha =>
                        {
                            if (workers.Find(h => h.id == ha.id) == null)
                            {
                                workers.Add(ha);
                            }
                        });
                    }
                });
                workers = workers.OrderBy(o => o.kategorie).ToList();
            }
            return workers;
        }




        public static StackLayout GetObjektNotScanListView(AppModel model, ICommand func, string s)
        {
            var stack = new StackLayout
            {
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            var oList = model.AllBuildings;
            oList = oList.OrderBy(b => b.plz).ToList();
            int i = 0;
            if (!String.IsNullOrWhiteSpace(s))
            {
                oList.ForEach(b =>
                {
                    string term = "" + b.plz + "##" + b.ort + "##" + b.strasse + "##" + b.hsnr + "##" + b.objektname + "##" + b.objektnr;
                    if (i < 30 && b != null && b.ArrayOfAuftrag.Count > 0 && term.ToLower().Contains(s.ToLower()))
                    {
                        stack.Children.Add(GetBuildingInfoNotScanElement(b, func));
                        i++;
                    }
                });
            }
            return stack;
        }
        public static StackLayout GetBuildingInfoNotScanElement(BuildingWSO obj, ICommand func)
        {
            var land = String.IsNullOrWhiteSpace(obj.land) ? "" : (obj.land.Length > 3 ? ("(" + obj.land.Substring(0, 3) + ") ") : obj.land);

            var imgInfo = new Image
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End,
                Margin = new Thickness(5, 0, 20, 0),
                HeightRequest = 24,
                WidthRequest = 24,
                Source = AppModel._Instance.imagesBase.InfoCircle,
                IsVisible = !String.IsNullOrWhiteSpace(obj.notiz),
            };
            if (!String.IsNullOrWhiteSpace(obj.notiz))
            {
                imgInfo.GestureRecognizers.Clear();
                var t_btn_objektinfo = new TapGestureRecognizer();
                t_btn_objektinfo.Tapped += (object ooo, TappedEventArgs ev) => { AppModel._Instance.MainPage.OpenObjektInfoDialogB(obj.notiz); };
                imgInfo.GestureRecognizers.Add(t_btn_objektinfo);
            }

            var chooseStack = new StackLayout
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.End,
                BackgroundColor = Color.FromArgb("#04532d"),
                WidthRequest = 100,
                Children = {
                    new Label {
                        Text = "Auswählen",
                        VerticalOptions = LayoutOptions.FillAndExpand,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        VerticalTextAlignment = TextAlignment.Center,
                        FontSize = 14,
                        TextColor = Colors.White,
                        HorizontalTextAlignment = TextAlignment.Center,
                        Margin = new Thickness(0),
                        Padding = new Thickness(5)
                    },
                }
            };

            var stack = new StackLayout
            {
                Padding = new Thickness(5, 2, 0, 2),
                Margin = new Thickness(0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromArgb("#77042d53"),//#90144d73"),
                Children = {
                    new StackLayout
                    {
                        Padding = new Thickness(0),
                        Margin = new Thickness(0),
                        Spacing = 0,
                        Orientation = StackOrientation.Horizontal,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        Children = {
                            new Label {
                                Text = obj.strasse + " " + obj.hsnr + "\n" + land + obj.plz  + " " + obj.ort,
                                VerticalOptions = LayoutOptions.StartAndExpand,
                                HorizontalOptions = LayoutOptions.FillAndExpand,
                                FontSize = 16,
                                TextColor = Colors.White,
                                HorizontalTextAlignment = TextAlignment.Start,
                                Margin = new Thickness(10,0,0,0),
                                Padding = new Thickness(0)
                            },
                            imgInfo,
                            chooseStack
                        }
                    }
                }
            };

            if (func != null)
            {
                chooseStack.GestureRecognizers.Clear();
                chooseStack.GestureRecognizers.Add(new TapGestureRecognizer() { Command = func, CommandParameter = new IntBoolParam { val = obj.id, bol = true } });
            }

            var mainStack = new StackLayout
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0, 0, 0, 15),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children = { stack }
            };
            return mainStack;
        }




        public static StackLayout GetBuildingInformation(AppModel model, DateTime lastsyncdate)
        {
            return new StackLayout
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Margin = new Thickness(0, 0, 0, 0),
                Padding = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                Children =
                {
                    new Image {
                        Source = model.imagesBase.InfoImage,
                        Margin = new Thickness(0, 0, 5, 0),
                        HeightRequest = 18,
                        WidthRequest = 18,
                        VerticalOptions = LayoutOptions.Center,
                    },
                    new Label
                    {
                        FontSize = 14,
                        Text = "Objekte: ",
                        HorizontalOptions = LayoutOptions.Start, HorizontalTextAlignment = TextAlignment.Start,
                        TextColor = Color.FromArgb("#999999"),
                        Margin = new Thickness(0, 0, 0, 0),
                        Padding = new Thickness(0, 0, 0, 0),
                    },
                    new Label
                    {
                        FontSize = 14,
                        Text = ""+model.AllBuildings.Count,
                        HorizontalOptions = LayoutOptions.Start, HorizontalTextAlignment = TextAlignment.Start,
                        TextColor = Color.FromArgb("#cccccc"),
                        Margin = new Thickness(0, 0, 0, 0),
                        Padding = new Thickness(0, 0, 0, 0),
                    },
                    new Label
                    {
                        FontSize = 14,
                        Text = " - Stand vom: ",
                        HorizontalOptions = LayoutOptions.Start, HorizontalTextAlignment = TextAlignment.Start,
                        TextColor = Color.FromArgb("#999999"),
                        Margin = new Thickness(0, 0, 0, 0),
                        Padding = new Thickness(0, 0, 0, 0),
                    },
                    new Label
                    {
                        FontSize = 14,
                        Text = lastsyncdate.ToString("dd.MM.yyyy - HH:mm"),
                        HorizontalOptions = LayoutOptions.StartAndExpand, HorizontalTextAlignment = TextAlignment.Start,
                        TextColor = Color.FromArgb("#cccccc"),
                        Margin = new Thickness(0, 0, 0, 0),
                        Padding = new Thickness(0, 0, 0, 0),
                    }
                }
            };
        }

        public static StackLayout GetBuildingInfoElement(BuildingWSO obj, AppModel model, double prio = 100000000)
        {
            var land = String.IsNullOrWhiteSpace(obj.land) ? "" : (obj.land.Length > 3 ? ("(" + obj.land.Substring(0, 3) + ") ") : obj.land);
            var stack = new StackLayout
            {
                Padding = new Thickness(10, 5, 10, 5),
                Margin = new Thickness(0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromArgb("#55042d53"),//#90144d73"),
                Children = {
                    new StackLayout
                    {
                        Padding = new Thickness(0),
                        Margin = new Thickness(0),
                        Spacing = 0,
                        Orientation = StackOrientation.Horizontal,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        Children = {
                            new Label {
                                Text = (String.IsNullOrWhiteSpace(obj.objektname) ? "": (obj.objektname + "\n"))
                                + obj.strasse + " " + obj.hsnr + "\n"
                                + land + obj.plz  + " " + obj.ort,
                                VerticalOptions = LayoutOptions.StartAndExpand,
                                HorizontalOptions = LayoutOptions.FillAndExpand,
                                FontSize = 14,
                                TextColor = Colors.White,
                                HorizontalTextAlignment = TextAlignment.Start,
                                Margin = new Thickness(0),
                                Padding = new Thickness(0)
                            }
                        }
                    }
                }
            };
            if (prio < 1360) // kleiner 4 Jahre
            {
                var warn = AuftragWSO.GetOrderWarning(prio, model);
                warn.Margin = new Thickness(0, 0, 0, 0);
                stack.Children.Add(warn);
            }
            return stack;
        }



        public static async void btn_MapTapped(object obj)
        {
            BuildingWSO b = (BuildingWSO)obj;

            AppModel.Instance.MainPageOverlay.IsVisible = true;
            await Task.Delay(1);

            var result = await Geolocation.
                    GetLocationAsync(new GeolocationRequest(
                        GeolocationAccuracy.Default, TimeSpan.FromSeconds(50)));

            string routefrom = ("@" + result.Latitude + "#" + result.Longitude).Replace(",", ".").Replace("#", ",");
            string routeto = "" + b.strasse.Replace(" ", "+") + "+" + b.hsnr.Replace(" ", "+") + "," + b.ort;
            string route = "http://maps.google.com/?daddr=" + routeto + "&saddr=" + routefrom;
            string routeApple = "http://maps.apple.com/?daddr=" + routeto + "&saddr=" + routefrom;


            AppModel.Instance.UseExternHardware = true;

            if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                // https://developer.apple.com/library/ios/featuredarticles/iPhoneURLScheme_Reference/MapLinks/MapLinks.html
                await Launcher.OpenAsync(new Uri(routeApple));
            }
            else if (Device.RuntimePlatform == Device.Android)
            {
                //await Launcher.OpenAsync(new Uri("https://www.google.com/maps/dir/" + routefrom));
                await Launcher.OpenAsync(new Uri(route));
            }

            //AppModel.Instance.UseOutSideHardware = false;
            AppModel.Instance.MainPageOverlay.IsVisible = false;
            await Task.Delay(1);
        }

        public static StackLayout GetBuildingInfoTodoElement(BuildingWSO obj, AppModel model, AbsoluteLayout overlay = null)
        {
            var land = String.IsNullOrWhiteSpace(obj.land) ? "" : (obj.land.Length > 3 ? ("(" + obj.land.Substring(0, 3) + ") ") : obj.land);

            Image imgPin = new Image
            {
                Source = model.imagesBase.Pin,
                Margin = new Thickness(5, 0, 5, 0),
                HeightRequest = 24,
                WidthRequest = 24,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End,
            };
            imgPin.GestureRecognizers.Clear();
            imgPin.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command<Object>(btn_MapTapped),
                CommandParameter = obj
            });

            var imgInfo = new Image
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End,
                Margin = new Thickness(5, 0, 5, 0),
                HeightRequest = 24,
                WidthRequest = 24,
                Source = AppModel._Instance.imagesBase.InfoCircle,
                IsVisible = !String.IsNullOrWhiteSpace(obj.notiz),
            };
            if (!String.IsNullOrWhiteSpace(obj.notiz))
            {
                imgInfo.GestureRecognizers.Clear();
                var t_btn_objektinfo = new TapGestureRecognizer();
                t_btn_objektinfo.Tapped += (object ooo, TappedEventArgs ev) => { AppModel._Instance.MainPage.OpenObjektInfoDialogB(obj.notiz); };
                imgInfo.GestureRecognizers.Add(t_btn_objektinfo);
            }

            Border warn = new Border { IsVisible = false };
            if (obj.prio < 1360) // kleiner 4 Jahre
            {
                warn = AuftragWSO.GetTodoCountWarningSmall(obj.prio);
                warn.Margin = new Thickness(0, 0, 0, 0);
            }


            var stack = new StackLayout
            {
                Padding = new Thickness(10, 2, 10, 2),
                Margin = new Thickness(0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromArgb("#77042d53"),//#90144d73"),
                Children = {
                    new StackLayout
                    {
                        Padding = new Thickness(0),
                        Margin = new Thickness(0),
                        Spacing = 0,
                        Orientation = StackOrientation.Horizontal,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        Children = {
                            warn,
                            new Label {
                                Text = obj.strasse + " " + obj.hsnr + "\n" + land + obj.plz  + " " + obj.ort,
                                VerticalOptions = LayoutOptions.StartAndExpand,
                                HorizontalOptions = LayoutOptions.FillAndExpand,
                                FontSize = 16,
                                TextColor = Colors.White,
                                HorizontalTextAlignment = TextAlignment.Start,
                                Margin = new Thickness(10,0,0,0),
                                Padding = new Thickness(0)
                            },
                            imgInfo,
                            imgPin,
                        }
                    }
                }
            };


            var container = new StackLayout
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                IsVisible = false
            };

            var o = new List<Object>(){
                container,
                model,
                obj,
                overlay,
                null
            };
            stack.GestureRecognizers.Clear();
            stack.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command<Object>(ShowOrderContainer), CommandParameter = o });


            var mainStack = new StackLayout
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0, 0, 0, 15),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children = { stack, container },
                ClassId = "" + obj.plz + "##" + obj.ort + "##" + obj.strasse + "##" + obj.hsnr + "##" + obj.objektname + "##" + obj.objektnr,
            };
            return mainStack;
        }
        public async static void ShowOrderContainerOnlyKat(Object value)
        {
            var stack = ((value as List<Object>)[0] as StackLayout);
            var model = ((value as List<Object>)[1] as AppModel);
            //var list = ((value as List<Object>)[2] as BuildingWSO).ArrayOfAuftrag;
            var obj = ((value as List<Object>)[2] as BuildingWSO);
            PlanPersonMobile pp = (value as List<Object>).Count > 4 ? ((value as List<Object>)[4] as PlanPersonMobile) : null;
            List<AuftragWSO> list = new List<AuftragWSO>();
            if (obj != null)
            {
                list = obj.ArrayOfAuftrag != null ? obj.ArrayOfAuftrag : new List<AuftragWSO>();
            }
            var overlay = ((value as List<Object>)[3] as AbsoluteLayout);
            int w = 0;
            if (stack.Children.Count == 0)
            {
                w = 200;
                overlay.IsVisible = true;
                await Task.Delay(1);
                var oList = new List<AuftragWSO>();
                //list.ForEach(b =>
                //{
                //    b.prio = CalcOverdue(b, model);
                //});
                oList = list.OrderBy(o => o.prio).ToList();
                oList.ForEach(o =>
                {
                    bool onlyText = true;
                    o.kategorien.ForEach(k =>
                    {
                        if (k.art != "Texte" && onlyText)
                        {
                            onlyText = false;
                        }
                    });

                    var prioOrder = CalcOverdue(o, model);
                    //if (prioOrder < 8)  // vorschau 1 Woche
                    //{
                    if (o.isInTodoVisible || onlyText)
                    {
                        //var order = AuftragWSO.GetOrderTodoCardViewOnlyKat(o, model, prioOrder, onlyText);// prioOrder);
                        //stack.Children.Add(order);
                        o.kategorien.ForEach(c =>
                        {
                            var foundKat = pp?.more.Find(p => p.katid == c.id);
                            if (foundKat != null || onlyText)
                            {
                                var prioCat = AuftragWSO.CalcKategorieOverdue(c, model);
                                if (c.isInTodoVisible || onlyText)
                                {
                                    //if (prioCat < 8) // vorschau 1 Woche
                                    //{
                                    var categories = KategorieWSO.GetCategoryTodoCardView(c, model, prioCat, onlyText);
                                    //(order.Children[1] as StackLayout).Children.Add(categories);
                                    stack.Children.Add(categories);
                                    c.leistungen.ForEach(l =>
                                    {
                                        var prioPos = AuftragWSO.CalcLeistungOverdueTodo(l, model);
                                        if (l.isInTodoVisible || onlyText)
                                        {
                                            //if (prioPos < 8)  // vorschau 1 Woche
                                            //{
                                            (categories.Children[1] as StackLayout).Children.Add(LeistungWSO.GetPositionTodoCardView(l, model, onlyText));
                                            //}
                                        }
                                    });
                                    //}
                                }
                            }
                        });
                    }
                    //}
                });
            }
            //await Task.Delay(w);
            stack.IsVisible = !stack.IsVisible;
            overlay.IsVisible = false;
        }

        public async static void ShowOrderContainer(Object value)
        {
            PlanPersonMobile pp = (value as List<Object>).Count > 4 ? ((value as List<Object>)[4] as PlanPersonMobile) : null;
            if (AppModel.Instance.AppControll.filterKategories && !AppModel.Instance.AppControll.ignoreKategorieFilterByPerson && pp != null)
            {
                BuildingWSO.ShowOrderContainerOnlyKat(value);
                return;
            }
            var stack = ((value as List<Object>)[0] as StackLayout);
            var model = ((value as List<Object>)[1] as AppModel);
            //var list = ((value as List<Object>)[2] as BuildingWSO).ArrayOfAuftrag;
            var obj = ((value as List<Object>)[2] as BuildingWSO);
            List<AuftragWSO> list = new List<AuftragWSO>();
            if (obj != null)
            {
                list = obj.ArrayOfAuftrag != null ? obj.ArrayOfAuftrag : new List<AuftragWSO>();
            }
            var overlay = ((value as List<Object>)[3] as AbsoluteLayout);
            int w = 0;
            if (stack.Children.Count == 0)
            {
                w = 200;
                overlay.IsVisible = true;
                await Task.Delay(1);
                var oList = new List<AuftragWSO>();
                //list.ForEach(b =>
                //{
                //    b.prio = CalcOverdue(b, model);
                //});
                oList = list.OrderBy(o => o.prio).ToList();
                oList.ForEach(o =>
                {
                    bool onlyText = true;
                    o.kategorien.ForEach(k =>
                    {
                        if (k.art != "Texte" && onlyText)
                        {
                            onlyText = false;
                        }
                    });

                    var prioOrder = CalcOverdue(o, model);
                    //if (prioOrder < 8)  // vorschau 1 Woche
                    //{
                    if (o.isInTodoVisible || onlyText)
                    {
                        var order = AuftragWSO.GetOrderTodoCardView(o, model, prioOrder, onlyText);// prioOrder);
                        stack.Children.Add(order);
                        o.kategorien.ForEach(c =>
                        {
                            var prioCat = AuftragWSO.CalcKategorieOverdue(c, model);
                            if (c.isInTodoVisible || onlyText)
                            {
                                //if (prioCat < 8) // vorschau 1 Woche
                                //{
                                var categories = KategorieWSO.GetCategoryTodoCardView(c, model, prioCat, onlyText);
                                (order.Children[1] as StackLayout).Children.Add(categories);
                                c.leistungen.ForEach(l =>
                                {
                                    var prioPos = AuftragWSO.CalcLeistungOverdueTodo(l, model);
                                    if (l.isInTodoVisible || onlyText)
                                    {
                                        //if (prioPos < 8)  // vorschau 1 Woche
                                        //{
                                        (categories.Children[1] as StackLayout).Children.Add(LeistungWSO.GetPositionTodoCardView(l, model, onlyText));
                                        //}
                                    }
                                });
                                //}
                            }
                        });
                    }
                    //}
                });
            }
            //await Task.Delay(w);
            stack.IsVisible = !stack.IsVisible;
            overlay.IsVisible = false;
        }

        public static double CalcOverdue(AuftragWSO order, AppModel model)
        {
            double _prio = 100000000;
            order.kategorien.ForEach(c =>
            {
                c.leistungen.ForEach(l =>
                {
                    if (l.nichtpauschal == 0 && l.timevaldays > 0 && l.isInTodoVisible)//long.Parse(l.lastwork) > 1 && l.timevaldays > 0)
                    {
                        l.prio = Prio.GetLeistungPrio(l, model);
                        _prio = Math.Min(_prio, l.prio.days);
                    }
                });
            });
            return _prio;
        }


        public static LeistungWSO FindLeistung(Int32 id)
        {
            LeistungWSO lei = null;
            AppModel.Instance.AllBuildings.ForEach(b =>
            {
                if (lei == null)
                {
                    b.ArrayOfAuftrag.ForEach(a =>
                    {
                        a.kategorien.ForEach(c =>
                        {
                            c.leistungen.ForEach(l =>
                            {
                                if (l.id == id) { lei = l; }
                            });
                        });
                    });
                }
            });
            return lei;
        }

        public static string FindKategorieName(Int32 katid)
        {
            string kat = null;
            AppModel.Instance.AllBuildings.ForEach(b =>
            {
                if (kat == null)
                {
                    b.ArrayOfAuftrag.ForEach(a =>
                    {
                        a.kategorien.ForEach(c =>
                        {
                            if (kat == null && c.id == katid) { kat = c.GetMobileText(); }
                        });
                    });
                }
            });
            return kat;
        }


        public static bool Save(AppModel model, BuildingWSO building)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                var json = JsonConvert.SerializeObject(building);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, json);
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/"
                    + model.SettingModel.SettingDTO.CustomerNumber + "/buildings/");
                if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/"
                    + model.SettingModel.SettingDTO.CustomerNumber + "/buildings/b_" + building.id + ".ipm");
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
        public static BuildingWSO LoadBuilding(AppModel model, Int32 id)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/"
                    + model.SettingModel.SettingDTO.CustomerNumber + "/buildings/b_" + id + ".ipm");
                if (File.Exists(filePath))
                {
                    byte[] data = File.ReadAllBytes(filePath);
                    BinaryFormatter binForm = new BinaryFormatter();
                    ms.Write(data, 0, data.Length);
                    ms.Seek(0, SeekOrigin.Begin);
                    var json = (Object)binForm.Deserialize(ms) as String;
                    ms.Close();
                    ms.Dispose();
                    return JsonConvert.DeserializeObject<BuildingWSO>(json);
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
        public static bool DeleteBuilding(Int32 id)
        {
            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/"
                    + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/buildings/b_" + id + ".ipm");
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
        public static bool DeleteBuildings(List<Int32> buildingIds)
        {
            try
            {
                if (buildingIds != null && buildingIds.Count > 0)
                {
                    buildingIds.ForEach(id => { DeleteBuilding(id); });
                }
                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error(ex);
                return false;
            }
        }
        public static bool DeleteAllBuildings(AppModel model)
        {
            try
            {
                List<BuildingWSO> list = new List<BuildingWSO>();
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/buildings/");
                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath);
                    if (files != null && files.Length > 0)
                    {
                        files.ToList().ForEach(filePath =>
                        {
                            if (File.Exists(filePath))
                            {
                                File.Delete(filePath);
                            }
                        });
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error(ex);
                return false;
            }
        }


        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }
    }




}
