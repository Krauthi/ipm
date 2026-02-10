using iPMCloud.Mobile.vo;
using iPMCloud.Mobile.vo.wso;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;

namespace iPMCloud.Mobile
{
    [Serializable]
    public class PlanRequest
    {
        public string token = "";
        public Int32 id = 0;

        public PlanRequest()
        {
        }
    }

    [Serializable]
    public class PlanResponse
    {
        public bool success = false;
        public string message;
        public bool active = true;
        public DateTime? lastCall = null;

        public PlanPersonMobileWeek planweek = new PlanPersonMobileWeek();
        public List<PersonSmallWSO> persons = new List<PersonSmallWSO>();

        public PersonSmallWSO selectedPerson = null;

        public PlanResponse()
        {

        }
    }




    [Serializable]
    public class SavePlanRequest
    {
        public string token = "";
        public List<ObjektPlanWeekMobile> planweek = new List<ObjektPlanWeekMobile>();

        public SavePlanRequest()
        {
        }
    }

    [Serializable]
    public class SavePlanResponse
    {
        public bool success = false;
        public string message;
        public bool active = true;
        //public List<ObjektPlanWeekMobile> planweek = new List<ObjektPlanWeekMobile>();

        public SavePlanResponse()
        {
        }
    }


    [Serializable]
    public class PlanPersonMobileWeek
    {
        public Int32 kw { get; set; } = 0;
        public List<List<PlanPersonMobile>> days { get; set; }

        public string startweekstamp { get; set; }
        public DateTime startweekdate { get; set; }
        public DateTime endweekdate { get; set; }
        public string datum { get; set; } = null;
    }
    [Serializable]

    public class PlanPersonMobileStdSum
    {
        public Int32 itemid { get; set; } = 0;
        public Int32 personid { get; set; } = 0;
        public List<string> stdDay { get; set; } = new List<string> {
            "0,00","0,00","0,00","0,00","0,00","0,00","0,00"
        };
        public string stdSum { get; set; } = "0,00";

    }

    [Serializable]
    public class ObjektPlanWeekMobile : ICloneable
    {
        //public Int32 id = 0;
        //public int day = 0;
        //public Int32 objektid = 0;
        //public Int32 personid = 0;
        //public Int32 auftragid = 0;
        //public Int32 muelltoid = 0;
        //public String katname = "";
        //public String lastwork = "0";
        //public String lastworker = "";
        //public int haswork = 0;
        //public int kwnum = 0;
        //public String kw = "";
        //public int a = 0;
        //public String b = "";
        //public String c = "";

        //public String guid = "";
        //public bool suscces = false;


        //public List<ObjektPlanWeekMobile> more = null;
        //public bool isSelected = false;

        //public Int32 leiid = 0;

        // public StackLayout view;

        public ObjektPlanWeekMobile() { }


        //public static ObjektPlanWeekMobile CloneIt(ObjektPlanWeekMobile o)
        //{
        //    return new ObjektPlanWeekMobile
        //    {

        //    }
        //}

        public async static void ShowOrderContainer(Object value)
        {
            var stack = ((value as List<Object>)[0] as StackLayout);
            var model = ((value as List<Object>)[1] as AppModel);
            var list = ((value as List<Object>)[2] as BuildingWSO).ArrayOfAuftrag;
            var overlay = ((value as List<Object>)[3] as AbsoluteLayout);
            int w = 0;

            if (stack.Children.Count == 0)
            {
                overlay.IsVisible = true;
                await Task.Delay(1);

                stack.Children.Add(new StackLayout()
                {
                    Padding = new Thickness(0),
                    Margin = new Thickness(0),
                    Spacing = 0,
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Children = {
                        new Label()
                        {
                            Padding = new Thickness(0),
                            Text = "Direkterfassung",
                            TextColor = Color.FromArgb("#ffffff"),
                            Margin = new Thickness(3, 3, 5, 3),
                            FontSize = 14,
                            HorizontalOptions = LayoutOptions.StartAndExpand,
                            LineBreakMode = LineBreakMode.WordWrap,
                        },
                        new Label()
                        {
                            Padding = new Thickness(0),
                            Text = "(123456)",
                            TextColor = Color.FromArgb("#00ff00"),
                            Margin = new Thickness(5, 3, 5, 3),
                            FontSize = 12,
                            WidthRequest = 46,
                            HorizontalTextAlignment = TextAlignment.Center,
                            HorizontalOptions = LayoutOptions.Center,
                            LineBreakMode = LineBreakMode.WordWrap,
                        }
                    }
                });
                stack.IsVisible = true;
            }
            else
            {
                stack.IsVisible = false;
                stack.Children.Clear();
            }
            await Task.Delay(1);
            overlay.IsVisible = false;
        }

        public static StackLayout GetPlanedTodayList(PlanPersonMobile p, ICommand func)
        {
            var b = AppModel.Instance.AllBuildings.Find(o => o.id == p.objektid);

            var pinBtn = new StackLayout
            {
                Padding = new Thickness(1),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.Start,
                Children = {new StackLayout
                        {
                            Padding = new Thickness(2),
                            Margin = new Thickness(0),
                            Spacing = 0,
                            Orientation = StackOrientation.Horizontal,
                            VerticalOptions = LayoutOptions.FillAndExpand,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            Children = {
                                new Image {
                                    Source = p.haswork == 1 ? AppModel.Instance.imagesBase.CheckWhite : AppModel.Instance.imagesBase.Pin,
                                    HeightRequest = 28,
                                    WidthRequest = 28,
                                    VerticalOptions = LayoutOptions.Center,
                                    HorizontalOptions = LayoutOptions.Center,
                                }
                            }
                        }
                }
            };
            if (p.haswork == 0 && b != null)
            {
                pinBtn.GestureRecognizers.Clear();
                var tgr_imgPin = new TapGestureRecognizer();
                tgr_imgPin.Tapped -= (object o, TappedEventArgs ev) => { BuildingWSO.btn_MapTapped(b); };
                tgr_imgPin.Tapped += (object o, TappedEventArgs ev) => { BuildingWSO.btn_MapTapped(b); };
                pinBtn.GestureRecognizers.Add(tgr_imgPin);
            }

            var infoBtn = new StackLayout
            {
                Padding = new Thickness(1),
                Margin = new Thickness(0, 0, 3, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.End,
                IsVisible = b != null && !String.IsNullOrWhiteSpace(b.notiz),
                Children = {new StackLayout
                        {
                            Padding = new Thickness(2),
                            Margin = new Thickness(0),
                            Spacing = 0,
                            Orientation = StackOrientation.Horizontal,
                            VerticalOptions = LayoutOptions.FillAndExpand,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            Children = {
                                new Image {
                                    Source = AppModel._Instance.imagesBase.InfoCircle,
                                    HeightRequest = 26,
                                    WidthRequest = 26,
                                    VerticalOptions = LayoutOptions.Center,
                                    HorizontalOptions = LayoutOptions.Center,
                                }
                            }
                        }
                }
            };
            if (b != null && !String.IsNullOrWhiteSpace(b.notiz))
            {
                infoBtn.GestureRecognizers.Clear();
                var t_btn_objektinfo = new TapGestureRecognizer();
                t_btn_objektinfo.Tapped += (object ooo, TappedEventArgs ev) => { AppModel._Instance.MainPage.OpenObjektInfoDialogB(b.notiz); };
                infoBtn.GestureRecognizers.Add(t_btn_objektinfo);
            }

            var chooseStack = new StackLayout
            {
                Padding = new Thickness(1),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.Start,
                BackgroundColor = Color.FromArgb("#04330d"),
                Children = {new StackLayout
                        {
                            Padding = new Thickness(3),
                            Margin = new Thickness(0),
                            Spacing = 0,
                            Orientation = StackOrientation.Horizontal,
                            VerticalOptions = LayoutOptions.FillAndExpand,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            BackgroundColor = Color.FromArgb("#04532d"),
                            Children = {
                                new Image {
                                    Source = "auswahl.png",
                                    HeightRequest = 30,
                                    WidthRequest = 30,
                                    VerticalOptions = LayoutOptions.Center,
                                    HorizontalOptions = LayoutOptions.Center,
                                }
                            }
                        }
                }
            };
            if (func != null && b != null && AppModel.Instance.AppControll.direktBuchenPos)
            {
                chooseStack.GestureRecognizers.Clear();
                chooseStack.GestureRecognizers.Add(new TapGestureRecognizer() { Command = func, CommandParameter = new IntBoolParam { val = p.objektid, bol = false } });
            }


            var stv = new StackLayout()
            {
                Padding = new Thickness(0, 0, 0, 3),
                Margin = new Thickness(0, 2, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromArgb("#55042d53"),
            };
            var st = new StackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };
            if (AppModel.Instance.AppControll.direktBuchenPos)
            {
                st.Children.Add(chooseStack);
            }
            st.Children.Add(new Label()
            {
                Padding = new Thickness(0),
                Text = b != null ? "" + b.plz + " " + b.ort + " - " + b.strasse + " " + b.hsnr : "Nicht gefunden! (Synchronisieren)",
                TextColor = (b == null ? Color.FromArgb("#ffcc00") : (p.haswork == 1 ? Color.FromArgb("#00ff00") : Color.FromArgb("#ffffff"))),
                Margin = new Thickness(3, 3, 5, 3),
                FontSize = 13,
                HorizontalOptions = LayoutOptions.StartAndExpand,
                LineBreakMode = LineBreakMode.WordWrap,
            });
            st.Children.Add(new Label()
            {
                Padding = new Thickness(0),
                Text = "(" + p.std + ")",
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(5, 3, 5, 3),
                FontSize = 10,
                WidthRequest = 46,
                HorizontalTextAlignment = TextAlignment.Center,
                HorizontalOptions = LayoutOptions.Center,
                LineBreakMode = LineBreakMode.WordWrap,
            });
            st.Children.Add(infoBtn);
            st.Children.Add(pinBtn);
            stv.Children.Add(st);





            if (p.muelltoid > 0)
            {
                var stmuell = new StackLayout()
                {
                    Padding = new Thickness(0),
                    Margin = new Thickness(0, 2, 0, 0),
                    Spacing = 0,
                    IsVisible = p.more != null && p.more.Count > 0,
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.FillAndExpand
                };
                if (p.more != null && p.more.Count > 0)
                {
                    p.more.ForEach(pp =>
                    {
                        string name = "";
                        string col = "";
                        string statem = "";
                        string leiid = "";
                        try
                        {
                            string[] all = pp.info.Split('#');
                            name = all[0];
                            col = "#" + all[1];
                            statem = all[2];
                            leiid = all[3];
                        }
                        catch (Exception) { }

                        var lbMuell = new Label()
                        {
                            Padding = new Thickness(3, 2, 3, 2),
                            Text = name,
                            TextColor = Color.FromArgb("#000000"),
                            Margin = new Thickness(0, 0, 0, 0),
                            FontSize = 12,
                            LineBreakMode = LineBreakMode.TailTruncation,
                            HorizontalOptions = LayoutOptions.Start,
                            BackgroundColor = Color.FromArgb(col)
                        };
                        if (pp.mobil == 0)
                        {
                            Image imgMobilOff = new Image
                            {
                                Source = "mobil_leistung_off.png",
                                Margin = new Thickness(2, 0, 2, 0),
                                HeightRequest = 20,
                                WidthRequest = 20,
                                VerticalOptions = LayoutOptions.Center,
                                HorizontalOptions = LayoutOptions.Start,
                            };
                            stmuell.Children.Add(imgMobilOff);
                        }
                        else
                        {
                            Image imgMuell = new Image
                            {
                                Source = statem == "1" ? "muellOut5.png" : (statem == "2" ? "muellIn5.png" : "muellInOut5.png"),
                                Margin = new Thickness(2, -2, 0, 0),
                                HeightRequest = 20,
                                WidthRequest = 20,
                                VerticalOptions = LayoutOptions.Center,
                                HorizontalOptions = LayoutOptions.Start,
                            };
                            stmuell.Children.Add(imgMuell);
                        }
                        stmuell.Children.Add(lbMuell);
                    });
                }
                //stmuell.Children.Add(imgDirekt);
                stv.Children.Add(stmuell);
            }
            else
            {
                var stpos = new FlexLayout()
                {
                    Padding = new Thickness(0),
                    Margin = new Thickness(0, -2, 0, 0),
                    JustifyContent = FlexJustify.Start,
                    AlignContent = FlexAlignContent.Start,
                    AlignItems = FlexAlignItems.Start,
                    Direction = FlexDirection.Row,
                    Wrap = FlexWrap.Wrap,
                    IsVisible = p.more != null && p.more.Count > 0,
                    HorizontalOptions = LayoutOptions.FillAndExpand
                };
                if (p.more != null && p.more.Count > 0)
                {
                    AuftragWSO auft = null;
                    bool isAufInclude = true;
                    //bool isKatInclude = true;
                    KategorieWSO kategorie = null;
                    Int32 holdAufId = 0;
                    p.more.ForEach(pp =>
                    {
                        if (b != null && b.ArrayOfAuftrag.Count > 0)
                        {
                            isAufInclude = false;
                            auft = b.ArrayOfAuftrag.Find(_ => _.id == pp.auftragid);
                            isAufInclude = (auft != null);
                        }

                        if (auft != null)
                        {
                            kategorie = auft.kategorien.Find(k => k.id == pp.katid);
                            //isKatInclude = (isKatInclude && kategorie != null);
                        }
                        var vertretend = pp.personid != pp.vonpersonid;
                        var text = "";
                        if (b.ArrayOfAuftrag.Count > 1 && auft != null)
                        {
                            if (holdAufId != pp.auftragid) { text = text + "(" + auft.id + ")"; }
                            text = text + (" - " + (pp.katname != null && pp.katname.Length == 0 ? "Alle Kategorien" : pp.katname)) +
                                (vertretend ? " (" + GetPlanedPersonInitialien(pp.vonpersonid) + ")" : "");
                        }
                        else
                        {
                            text = ("- " + (pp.katname != null && pp.katname.Length == 0 ? "Alle Kategorien" : pp.katname)) +
                                (vertretend ? " (" + GetPlanedPersonInitialien(pp.vonpersonid) + ")" : "");
                        }
                        if (auft != null)
                        {
                            if (kategorie == null) { text = "Kategorie nicht gefunden! (Synchronisieren)"; }
                        }
                        else
                        {
                            text = "Auftrag nicht gefunden! (Synchronisieren)";
                            //isKatInclude = false;
                        }
                        if (auft != null)
                        {
                            if (holdAufId != pp.auftragid) { holdAufId = auft.id; }
                        }
                        var lbK = new Label()
                        {
                            Padding = new Thickness(2, 0, 2, 0),
                            Text = text,
                            TextColor = kategorie != null ? (vertretend ? Color.FromArgb("#ffaa00") : Color.FromArgb("#cccccc")) : Color.FromArgb("#ffcc00"),
                            Margin = new Thickness(0, 0, 0, 0),
                            FontSize = 12,
                            LineBreakMode = LineBreakMode.TailTruncation,
                            HorizontalOptions = LayoutOptions.Start,
                        };
                        stpos.Children.Add(lbK);

                    });
                }
                stv.Children.Add(stpos);
            }

            return stv;
        }

        public static StackLayout GetPlanedReadyTodayList(PlanPersonMobile p)
        {
            var b = AppModel.Instance.AllBuildings.Find(o => o.id == p.objektid);
            Image imgPin = new Image
            {
                Source = p.haswork == 1 ? AppModel.Instance.imagesBase.CheckWhite : AppModel.Instance.imagesBase.Pin,
                Margin = new Thickness(5, 0, 5, 0),
                HeightRequest = p.haswork == 1 ? 16 : 22,
                WidthRequest = p.haswork == 1 ? 16 : 22,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End,
            };
            if (p.haswork == 0 && b != null)
            {
                imgPin.GestureRecognizers.Clear();
                var tgr_imgPin = new TapGestureRecognizer();
                tgr_imgPin.Tapped -= (object o, TappedEventArgs ev) => { BuildingWSO.btn_MapTapped(b); };
                tgr_imgPin.Tapped += (object o, TappedEventArgs ev) => { BuildingWSO.btn_MapTapped(b); };
                imgPin.GestureRecognizers.Add(tgr_imgPin);
            }
            var imgInfo = new Image
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End,
                Margin = new Thickness(5, 0, 5, 0),
                HeightRequest = 22,
                WidthRequest = 22,
                Source = AppModel._Instance.imagesBase.InfoCircle,
                IsVisible = !String.IsNullOrWhiteSpace(b.notiz),
            };
            if (!String.IsNullOrWhiteSpace(b.notiz))
            {
                imgInfo.GestureRecognizers.Clear();
                var t_btn_objektinfo = new TapGestureRecognizer();
                t_btn_objektinfo.Tapped += (object ooo, TappedEventArgs ev) => { AppModel._Instance.MainPage.OpenObjektInfoDialogB(b.notiz); };
                imgInfo.GestureRecognizers.Add(t_btn_objektinfo);
            }

            var stv = new StackLayout()
            {
                Padding = new Thickness(1, 2, 1, 3),
                Margin = new Thickness(0, 2, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromArgb("#55042d53"),
            };
            var st = new StackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children = {
                    new Label()
                    {
                        Padding = new Thickness(0),
                        Text = b != null ? "" + b.plz + " " + b.ort + " - " + b.strasse + " " + b.hsnr:"Objekt nicht gefunden! (Synchronisieren)",
                        TextColor = (b == null ? Color.FromArgb("#ffcc00") : (p.haswork == 1 ? Color.FromArgb("#00ff00"):Color.FromArgb("#ffffff"))),
                        Margin = new Thickness(3, 3, 5, 3),
                        FontSize = 13,
                        HorizontalOptions = LayoutOptions.StartAndExpand,
                        LineBreakMode = LineBreakMode.WordWrap,
                    },
                    new Label()
                    {
                        Padding = new Thickness(0),
                        Text = "(" + p.std + ")",
                        TextColor = Color.FromArgb("#cccccc"),
                        Margin = new Thickness(5, 3, 5, 3),
                        FontSize = 10,
                        WidthRequest = 46,
                        HorizontalTextAlignment = TextAlignment.Center,
                        HorizontalOptions = LayoutOptions.Center,
                        LineBreakMode = LineBreakMode.WordWrap,
                    },
                    imgInfo,
                    imgPin
                }
            };
            stv.Children.Add(st);


            if (p.muelltoid > 0)
            {
                var stmuell = new StackLayout()
                {
                    Padding = new Thickness(0),
                    Margin = new Thickness(0),
                    Spacing = 0,
                    IsVisible = p.more != null && p.more.Count > 0,
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.FillAndExpand
                };
                //Image imgDirekt = new Image
                //{
                //    Source = "direkt.png",
                //    Margin = new Thickness(0, -3, 45, 0),
                //    HeightRequest = 28,
                //    WidthRequest = 28,
                //    VerticalOptions = LayoutOptions.Center,
                //    HorizontalOptions = LayoutOptions.Start,
                //};
                if (p.more != null && p.more.Count > 0)
                {
                    p.more.ForEach(pp =>
                    {
                        string name = "";
                        string col = "";
                        string statem = "";
                        string leiid = "";
                        try
                        {
                            string[] all = pp.info.Split('#');
                            name = all[0];
                            col = "#" + all[1];
                            statem = all[2];
                            leiid = all[3];
                        }
                        catch (Exception) { }

                        Image imgMuell = new Image
                        {
                            Source = statem == "1" ? "muellOut5.png" : (statem == "2" ? "muellIn5.png" : "muellInOut5.png"),
                            Margin = new Thickness(2, -2, 0, 0),
                            HeightRequest = 20,
                            WidthRequest = 20,
                            VerticalOptions = LayoutOptions.Center,
                            HorizontalOptions = LayoutOptions.Start,
                        };
                        var lbMuell = new Label()
                        {
                            Padding = new Thickness(3, 2, 3, 2),
                            Text = name,
                            TextColor = Color.FromArgb("#000000"),
                            Margin = new Thickness(0, 0, 0, 0),
                            FontSize = 12,
                            LineBreakMode = LineBreakMode.TailTruncation,
                            HorizontalOptions = LayoutOptions.Start,
                            BackgroundColor = Color.FromArgb(col)
                        };
                        stmuell.Children.Add(imgMuell);
                        stmuell.Children.Add(lbMuell);
                    });
                }
                //stmuell.Children.Add(imgDirekt);
                stv.Children.Add(stmuell);
            }
            else
            {
                var stpos = new FlexLayout()
                {
                    Padding = new Thickness(0),
                    Margin = new Thickness(0, -2, 0, 0),
                    JustifyContent = FlexJustify.Start,
                    AlignContent = FlexAlignContent.Start,
                    AlignItems = FlexAlignItems.Start,
                    Direction = FlexDirection.Row,
                    Wrap = FlexWrap.Wrap,
                    IsVisible = p.more != null && p.more.Count > 0,
                    HorizontalOptions = LayoutOptions.FillAndExpand
                };
                //Image imgDirektx = new Image
                //{
                //    Source = "direkt.png",
                //    Margin = new Thickness(0, -3, 45, 0),
                //    HeightRequest = 28,
                //    WidthRequest = 28,
                //    VerticalOptions = LayoutOptions.Center,
                //    HorizontalOptions = LayoutOptions.Start,
                //};
                if (p.more != null && p.more.Count > 0)
                {
                    p.more.ForEach(pp =>
                    {

                        //var sth = new StackLayout()
                        //{
                        //    Padding = new Thickness(0),
                        //    Margin = new Thickness(0),
                        //    Spacing = 0,
                        //    Orientation = StackOrientation.Horizontal,
                        //    HorizontalOptions = LayoutOptions.FillAndExpand
                        //};
                        //Image imgkat = new Image
                        //{
                        //    Source = AppModel.Instance.imagesBase.KategorieSymbol,
                        //    Margin = new Thickness(2, -2, 0, 0),
                        //    HeightRequest = 14,
                        //    WidthRequest = 14,
                        //    VerticalOptions = LayoutOptions.Center,
                        //    HorizontalOptions = LayoutOptions.Start,
                        //};
                        var vertretend = pp.personid != pp.vonpersonid;
                        var text = ("- " + (pp.katname != null && pp.katname.Length == 0 ? "Alle Kategorien" : pp.katname)) +
                            (vertretend ? " (" + GetPlanedPersonInitialien(pp.vonpersonid) + ")" : "");
                        var lbK = new Label()
                        {
                            Padding = new Thickness(2, 0, 2, 0),
                            Text = text,
                            TextColor = vertretend ? Color.FromArgb("#ffaa00") : Color.FromArgb("#cccccc"),
                            Margin = new Thickness(0, 0, 0, 0),
                            FontSize = 12,
                            LineBreakMode = LineBreakMode.TailTruncation,
                            HorizontalOptions = LayoutOptions.Start,
                        };
                        //sth.Children.Add(imgkat);
                        //sth.Children.Add(lbK);
                        stpos.Children.Add(lbK);
                    });
                }
                stv.Children.Add(stpos);
            }

            return stv;
        }

        public static StackLayout GetPlanedTodayItem(PlanPersonMobile p)
        {
            var b = AppModel.Instance.AllBuildings.Find(o => o.id == p.objektid);

            var stv = new StackLayout()
            {
                Padding = new Thickness(5, 6, 5, 6),
                Margin = new Thickness(0, 2, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromArgb("#55042d53"),
            };
            stv.Children.Add(new Label
            {
                Padding = new Thickness(0),
                Text = b != null ? "" + b.plz + " " + b.ort + " - " + b.strasse + " " + b.hsnr : "Objekt nicht gefunden!",
                TextColor = (b == null ? Color.FromArgb("#ffcc00") : Color.FromArgb("#ffffff")),
                Margin = new Thickness(3, 3, 5, 3),
                FontSize = 13,
                HorizontalOptions = LayoutOptions.StartAndExpand,
                LineBreakMode = LineBreakMode.TailTruncation,
            });
            if (p.muelltoid > 0)
            {
                var stmuell = new StackLayout()
                {
                    Padding = new Thickness(0),
                    Margin = new Thickness(0),
                    Spacing = 0,
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.FillAndExpand
                };
                var name = "";
                var col = "";
                var statem = "";
                var leiid = "";
                try
                {
                    var all = p.info.Split('#');
                    name = all[0];
                    col = "#" + all[1];
                    statem = all[2];
                    leiid = all[3];
                }
                catch (Exception) { }

                Image imgMuell = new Image
                {
                    Source = statem == "1" ? "muellOut5.png" : (statem == "2" ? "muellIn5.png" : "muellInOut5.png"),
                    Margin = new Thickness(3, -2, 3, 0),
                    HeightRequest = 20,
                    WidthRequest = 20,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.End,
                };
                var lbMuell = new Label()
                {
                    Padding = new Thickness(5, 2, 5, 2),
                    Text = name + "+",
                    TextColor = Color.FromArgb("#000000"),
                    Margin = new Thickness(6, 0, 0, 0),
                    FontSize = 12,
                    LineBreakMode = LineBreakMode.TailTruncation,
                    HorizontalOptions = LayoutOptions.Start,
                    BackgroundColor = Color.FromArgb(col)
                };
                stmuell.Children.Add(imgMuell);
                stmuell.Children.Add(lbMuell);
                stv.Children.Add(stmuell);
            }

            if (p.personid != p.vonpersonid)
            {
                stv.Children.Add(new Label()
                {
                    Padding = new Thickness(0),
                    Text = " (" + GetPlanedPersonInitialien(p.vonpersonid) + ")",
                    TextColor = Colors.Orange,
                    Margin = new Thickness(3, 0, 5, 3),
                    FontSize = 12,
                    HorizontalOptions = LayoutOptions.StartAndExpand,
                    LineBreakMode = LineBreakMode.WordWrap,
                });
            }

            return stv;
        }


        public static StackLayout GetOptWinterCheckItemHeadItem(Object value, AuftragWSO order, Border btn, Label lb, ICommand func,
            List<IntBemerkungWSOPair> selectedBemerkungForNoticeList_DirektPos)
        {
            var model = ((value as List<Object>)[0] as AppModel);
            //var list = ((value as List<Object>)[1] as BuildingWSO).ArrayOfAuftrag;
            var obj = ((value as List<Object>)[1] as BuildingWSO);
            List<AuftragWSO> list = null;
            if (obj != null)
            {
                list = obj.ArrayOfAuftrag != null ? obj.ArrayOfAuftrag : new List<AuftragWSO>();
            }
            var overlay = ((value as List<Object>)[2] as AbsoluteLayout);
            var ppm = ((value as List<Object>)[3] as PlanPersonMobile);

            var b = AppModel.Instance.AllBuildings.Find(o => o.id == ppm.objektid);
            Image winter = new Image
            {
                Margin = new Thickness(0, 0, 0, 0),
                HeightRequest = 22,
                WidthRequest = 22,
                Source = "win_26.png",
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
            };

            var stv = new StackLayout()
            {
                Padding = new Thickness(5, 6, 5, 6),
                Margin = new Thickness(0, 2, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromArgb("#55042d53"),
            };
            var st = new StackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children = {
                    winter,
                    new Label()
                    {
                        Padding = new Thickness(0),
                        Text = b != null ? "" + b.plz + " " + b.ort + " - " + b.strasse + " " + b.hsnr:"Nicht gefunden! (Synchronisieren)",
                        TextColor = (b == null ? Color.FromArgb("#ffcc00") : Color.FromArgb("#ffffff")),
                        Margin = new Thickness(3, 3, 5, 3),
                        FontSize = 14,
                        HorizontalOptions = LayoutOptions.StartAndExpand,
                        LineBreakMode = LineBreakMode.WordWrap,
                    },
                    new Label()
                    {
                        Padding = new Thickness(0),
                        Text = "(" + ppm.std + ")",
                        TextColor = Color.FromArgb("#cccccc"),
                        Margin = new Thickness(0, 3, 5, 3),
                        FontSize = 12,
                        WidthRequest = 46,
                        HorizontalTextAlignment = TextAlignment.Center,
                        HorizontalOptions = LayoutOptions.Center
                    }
                }
            };
            stv.Children.Add(st);


            bool isWinterKat = false;
            order.kategorien.ForEach(k =>
            {
                if (k.winterservice > 0 && !isWinterKat)
                {
                    isWinterKat = true;
                }
            });

            if (isWinterKat)
            {
                LeistungWSO onlyOneLei = null;
                int cc = 0;
                int leiCount = 0;
                order.kategorien.ForEach(k =>
                {
                    if (k.winterservice > 0)
                    {
                        k.leistungen.ForEach(l =>
                        {
                            if (l.selected) { cc++; }
                            leiCount++;
                            l.selected = false;
                            onlyOneLei = l;
                        });
                    }
                });
                if (onlyOneLei != null && leiCount == 1)
                {
                    onlyOneLei.selected = true;
                    btn.IsVisible = true;
                    lb.IsVisible = false;
                }
                else
                {
                    btn.IsVisible = (cc > 0);
                    lb.IsVisible = !btn.IsVisible;
                }

                var imageLOrder = new Image
                {
                    Margin = new Thickness(1, 0, 5, 0),
                    HeightRequest = 15,
                    WidthRequest = 15,
                    VerticalOptions = LayoutOptions.Start,
                    HorizontalOptions = LayoutOptions.Start,
                    Source = AppModel.Instance.imagesBase.OrderFolderTools
                };
                var lbOrder = new Label()
                {
                    Text = order.GetMobileText(),
                    TextColor = Color.FromArgb("#999999"),
                    Margin = new Thickness(0, 0, 5, 0),
                    FontSize = 14,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    LineBreakMode = LineBreakMode.WordWrap,
                };
                var hOrder = new StackLayout()
                {
                    Padding = new Thickness(24, 0, 0, 0),
                    Margin = new Thickness(0, 0, 0, 0),
                    Spacing = 0,
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    //BackgroundColor = Color.FromArgb("#aa042d53"),
                };
                hOrder.Children.Add(imageLOrder);
                hOrder.Children.Add(lbOrder);

                stv.Children.Add(hOrder);

                order.kategorien.Where(k => k.winterservice == 1).ToList().ForEach(c =>
                {
                    var imageLCatB = new Image
                    {
                        Margin = new Thickness(0, 0, 5, 0),
                        HeightRequest = 16,
                        WidthRequest = 16,
                        VerticalOptions = LayoutOptions.Start,
                        HorizontalOptions = LayoutOptions.Start,
                        Source = (c.art == "Leistung" ? AppModel.Instance.imagesBase.LeistungSymbol :
                                    (c.art == "Produkt" ? AppModel.Instance.imagesBase.ProduktSymbol :
                                        (c.art == "Texte" ? AppModel.Instance.imagesBase.TextSymbol :
                                        (c.art == "Check" ? AppModel.Instance.imagesBase.CheckWhite :
                                            AppModel.Instance.imagesBase.Quest
                                    ))))
                    };
                    var imageLCat = new Image
                    {
                        Margin = new Thickness(0, 0, 5, 0),
                        HeightRequest = 16,
                        WidthRequest = 16,
                        VerticalOptions = LayoutOptions.Start,
                        HorizontalOptions = LayoutOptions.Start,
                        Source = (c.art == "Leistung" ? AppModel.Instance.imagesBase.KLSymbol :
                            (c.art == "Produkt" ? AppModel.Instance.imagesBase.KPSymbol :
                                (c.art == "Texte" ? AppModel.Instance.imagesBase.KTSymbol :
                                (c.art == "Check" ? AppModel.Instance.imagesBase.KCSymbol :
                                    AppModel.Instance.imagesBase.Quest))))
                    };
                    var lbCat = new Label()
                    {
                        Text = c.GetMobileText(),
                        TextColor = Color.FromArgb("#ffcc00"),
                        Margin = new Thickness(0, 0, 5, 0),
                        FontSize = 14,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        LineBreakMode = LineBreakMode.WordWrap,
                    };
                    var hCat = new StackLayout()
                    {
                        Padding = new Thickness(24, 0, 0, 0),
                        Margin = new Thickness(0, 0, 0, 0),
                        Spacing = 0,
                        Orientation = StackOrientation.Horizontal,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                    };

                    hCat.Children.Add(imageLCatB);
                    hCat.Children.Add(lbCat);
                    stv.Children.Add(hCat);
                    c.leistungen.ForEach(l =>
                    {
                        var cl = GetOptWinterCheckItem(l, ppm, btn, lb, order, func, selectedBemerkungForNoticeList_DirektPos.Find(_ => _.lei.id == l.id));
                        stv.Children.Add(cl);
                    });

                    stv.Children.Add(new StackLayout()
                    {
                        Padding = new Thickness(0),
                        Margin = new Thickness(0),
                        Spacing = 0,
                        HeightRequest = 10,
                        Orientation = StackOrientation.Horizontal,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                    });
                });
            }

            return stv;
        }


        public static StackLayout GetOptWinterCheckItem(LeistungWSO lei, PlanPersonMobile p, Border btn, Label lb, AuftragWSO order, ICommand func, IntBemerkungWSOPair obj)
        {
            CheckBox checkBox = new CheckBox
            {
                Margin = new Thickness(0),
                IsChecked = lei.selected,
                HeightRequest = 40,
                WidthRequest = 40,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
            };
            Label checkLb = new Label
            {
                Padding = new Thickness(0),
                Text = lei.GetMobileText(),
                TextColor = Color.FromArgb("#ffffff"),
                Margin = new Thickness(0, 2, 5, lei.art == "Produkt" ? 10 : 2),
                FontSize = 12,
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.Center,
            };
            var sth = new StackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.Start,
                BackgroundColor = Color.FromArgb("#ff042d53")
            };
            sth.Children.Add(checkBox);
            sth.Children.Add(checkLb);
            var winterBemerkungBadgeCount = new Label
            {
                Text = "0",
                Margin = new Thickness(0),
                Padding = new Thickness(3, 0, 3, 0),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                FontSize = 10,
                TextColor = Color.FromArgb("#ffffff"),
                HorizontalTextAlignment = TextAlignment.Center
            };
            var badgeStack = new StackLayout()
            {
                Margin = new Thickness(-9, -4, 0, 0),
                Padding = new Thickness(0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start,
                IsVisible = false,
                Children = {
                        new Border {
                            BackgroundColor = Color.FromArgb("#007700"),
                             Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
                            HorizontalOptions = LayoutOptions.Start,
                            VerticalOptions = LayoutOptions.Start,
                            HeightRequest = 15,
                            WidthRequest = 28,
                            Margin = new Thickness(0),
                            Padding = new Thickness(1,1,0,0),
                            StrokeShape = new RoundRectangle { CornerRadius = 10 },
                            Content = winterBemerkungBadgeCount
                        },
                    }
            };
            obj.badge = winterBemerkungBadgeCount;
            obj.badgeStack = badgeStack;
            obj.count = 0;

            var noticeStack = new StackLayout
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Orientation = StackOrientation.Vertical,
                Spacing = 0,
                Padding = new Thickness(1),
                Margin = new Thickness(0),
                BackgroundColor = Color.FromArgb("#144d73"),
                Children = {
                    new Image
                    {
                        Margin = new Thickness(2),
                        HeightRequest = 32,
                        WidthRequest = 32,
                        VerticalOptions = LayoutOptions.CenterAndExpand,
                        HorizontalOptions = LayoutOptions.CenterAndExpand,
                        Source = AppModel.Instance.imagesBase.CamMessageWarn,
                    }

                }
            };
            var btnNoticeFrame = new Border()
            {
                Padding = new Thickness(1),
                Margin = new Thickness(2, 2, 2, 0),
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start,
                BackgroundColor = Color.FromArgb("#041d43"),
                Content = noticeStack,
                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
            };
            if (func != null)
            {
                btnNoticeFrame.GestureRecognizers.Clear();
                btnNoticeFrame.GestureRecognizers.Add(new TapGestureRecognizer() { Command = func, CommandParameter = lei });
            }

            sth.Children.Add(btnNoticeFrame);
            sth.Children.Add(badgeStack);

            var stv = new StackLayout()
            {
                Padding = new Thickness(0, 2, 0, 2),
                Margin = new Thickness(-5, 0, -5, 0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand
            };

            stv.Children.Add(sth);


            lei.leiInWork = LeistungInWorkWSO.ConvertLeistungTo(lei);

            if (lei.art == "Produkt")
            {
                var lbanzahl = new Label()
                {
                    Text = "Anzahl (" + Utils.getEinheitStr(lei.einheit) + "):",
                    TextColor = Color.FromArgb("#aaaaaa"),
                    Margin = new Thickness(0, 0, 0, 0),
                    Padding = new Thickness(5, 0, 0, 0),
                    FontSize = 12,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.StartAndExpand,
                };
                var InWorkPosSmallCardEntryAnzahl = new CustomEntry()
                {
                    Margin = new Thickness(0, -10, 0, -5),
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.StartAndExpand,
                    TextColor = Colors.White,
                    FontSize = 16,
                    Keyboard = Keyboard.Numeric,
                    HeightRequest = 40,
                    Text = Utils.formatDEStr(decimal.Parse(lei.leiInWork.anzahl) > 0 ? decimal.Parse(lei.leiInWork.anzahl) : 1),
                    MinimumWidthRequest = 100,
                    HorizontalTextAlignment = TextAlignment.End,
                    BackgroundColor = Colors.Transparent
                };
                InWorkPosSmallCardEntryAnzahl.ReturnCommandParameter = lei.leiInWork;
                InWorkPosSmallCardEntryAnzahl.Unfocused -= LeistungWSO.InWorkAnzahlChange;
                InWorkPosSmallCardEntryAnzahl.Unfocused += LeistungWSO.InWorkAnzahlChange;
                InWorkPosSmallCardEntryAnzahl.TextChanged -= LeistungWSO.InWorkPosSmallCardEntryAnzahlChanged;
                InWorkPosSmallCardEntryAnzahl.TextChanged += LeistungWSO.InWorkPosSmallCardEntryAnzahlChanged;

                var hanzahl = new StackLayout()
                {
                    Padding = new Thickness(5, 2, 5, 0),
                    Margin = new Thickness(0),
                    Spacing = 0,
                    Orientation = StackOrientation.Vertical,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    //BackgroundColor = Color.FromArgb("#144d73"),
                };
                var addBtn = new StackLayout()
                {
                    Padding = new Thickness(5, 2, 5, 2),
                    Margin = new Thickness(3, 0, 0, 0),
                    Spacing = 0,
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.Start,
                    WidthRequest = 50,
                    BackgroundColor = Color.FromArgb("#04532d"),
                    Children = {
                        new Label()
                        {
                            Text = "+",
                            TextColor = Color.FromArgb("#ffffff"),
                            Margin = new Thickness(0),
                            Padding = new Thickness(0),
                            FontSize = 24,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            HorizontalTextAlignment = TextAlignment.Center,
                            VerticalOptions = LayoutOptions.Center,
                        }

                    }
                };
                addBtn.GestureRecognizers.Clear();
                var t_addBtn = new TapGestureRecognizer();
                t_addBtn.Tapped += (object ob, TappedEventArgs ev) => { LeistungWSO.InWorkAddSubAnzahlChange(lei.leiInWork, InWorkPosSmallCardEntryAnzahl, 1); };
                addBtn.GestureRecognizers.Add(t_addBtn);
                var subBtn = new StackLayout()
                {
                    Padding = new Thickness(5, 2, 5, 2),
                    Margin = new Thickness(3, 0, 0, 0),
                    Spacing = 0,
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.Start,
                    WidthRequest = 50,
                    BackgroundColor = Color.FromArgb("#73042d"),
                    Children = {
                        new Label()
                        {
                            Text = "-",
                            TextColor = Color.FromArgb("#ffffff"),
                            Margin = new Thickness(0),
                            Padding = new Thickness(0),
                            FontSize = 24,
                            HorizontalTextAlignment = TextAlignment.Center,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            VerticalOptions = LayoutOptions.Center,
                        }

                    }
                };
                subBtn.GestureRecognizers.Clear();
                var t_subBtn = new TapGestureRecognizer();
                t_subBtn.Tapped += (object ob, TappedEventArgs ev) => { LeistungWSO.InWorkAddSubAnzahlChange(lei.leiInWork, InWorkPosSmallCardEntryAnzahl, -1); };
                subBtn.GestureRecognizers.Add(t_subBtn);
                var ho_anzahl = new StackLayout()
                {
                    Padding = new Thickness(3, 0, 3, 3),
                    Margin = new Thickness(50, 0, 0, 0),
                    Spacing = 0,
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    BackgroundColor = Color.FromArgb("#ff042d53"),
                    Children =
                    {
                        hanzahl,addBtn,subBtn
                    }
                };
                hanzahl.Children.Add(lbanzahl);
                hanzahl.Children.Add(InWorkPosSmallCardEntryAnzahl);

                stv.Children.Add(ho_anzahl);
            }




            checkBox.CheckedChanged -= (object o, CheckedChangedEventArgs ev) => { ChangeObjektPlanWeekMobileWinterDirektC(checkBox, p, btn, lb, lei, order); };
            checkBox.CheckedChanged += (object o, CheckedChangedEventArgs ev) => { ChangeObjektPlanWeekMobileWinterDirektC(checkBox, p, btn, lb, lei, order); };

            sth.GestureRecognizers.Clear();
            var t_quest_direktbuchen_cancel = new TapGestureRecognizer();
            t_quest_direktbuchen_cancel.Tapped -= (object o, TappedEventArgs ev) => { ChangeObjektPlanWeekMobileWinterDirekt(checkBox, p, btn, lb, lei, order); };
            t_quest_direktbuchen_cancel.Tapped += (object o, TappedEventArgs ev) => { ChangeObjektPlanWeekMobileWinterDirekt(checkBox, p, btn, lb, lei, order); };
            sth.GestureRecognizers.Add(t_quest_direktbuchen_cancel);


            return stv;
        }



        public static void ChangeObjektPlanWeekMobileWinterDirekt(CheckBox checkBox, PlanPersonMobile p, Border btn, Label lb, LeistungWSO lei, AuftragWSO order)
        {
            checkBox.IsChecked = !checkBox.IsChecked;
            //            p.isSelected = checkBox.IsChecked;
            lei.selected = checkBox.IsChecked;
            int c = 0;
            order.kategorien.ForEach(k =>
            {
                if (k.winterservice > 0)
                {
                    k.leistungen.ForEach(l =>
                    {
                        if (l.selected) { c++; }
                    });
                }
            });
            btn.IsVisible = (c > 0);
            lb.IsVisible = !btn.IsVisible;
        }

        public static void ChangeObjektPlanWeekMobileWinterDirektC(CheckBox checkBox, PlanPersonMobile p, Border btn, Label lb, LeistungWSO lei, AuftragWSO order)
        {
            //            p.isSelected = checkBox.IsChecked;
            lei.selected = checkBox.IsChecked;
            int c = 0;
            order.kategorien.ForEach(k =>
            {
                if (k.winterservice > 0)
                {
                    k.leistungen.ForEach(l =>
                    {
                        if (l.selected) { c++; }
                    });
                }
            });
            btn.IsVisible = (c > 0);
            lb.IsVisible = !btn.IsVisible;
        }



        public static StackLayout GetPlanedTodayCheckItem(bool sel, PlanPersonMobile p, Border btn, Label lb, PlanPersonMobile mainP, ICommand func, IntBemerkungWSOPair obj)
        {
            var b = AppModel.Instance.AllBuildings.Find(o => o.id == p.objektid);

            CheckBox checkBox = new CheckBox
            {
                Margin = new Thickness(0),
                IsChecked = sel,
                HeightRequest = 40,
                WidthRequest = 40,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start,
            };
            var sth = new StackLayout()
            {
                Padding = new Thickness(5, 6, 5, 6),
                Margin = new Thickness(0, 2, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromArgb("#55042d53"),
            };
            var stv = new StackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromArgb("#55042d53"),
            };
            stv.Children.Add(new Label
            {
                Padding = new Thickness(0),
                Text = b != null ? "" + b.plz + " " + b.ort + " - " + b.strasse + " " + b.hsnr : "Objekt nicht gefunden!",
                TextColor = (b == null ? Color.FromArgb("#ffcc00") : Color.FromArgb("#ffffff")),
                Margin = new Thickness(3, 3, 5, 3),
                FontSize = 14,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                LineBreakMode = LineBreakMode.TailTruncation,
            });
            if (p.muelltoid > 0)
            {
                var stmuell = new StackLayout()
                {
                    Padding = new Thickness(0),
                    Margin = new Thickness(0),
                    Spacing = 0,
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.FillAndExpand
                };
                var name = "";
                var col = "";
                var statem = "";
                var leiid = "";
                try
                {
                    var all = p.info.Split('#');
                    name = all[0];
                    col = "#" + all[1];
                    statem = all[2];
                    leiid = all[3];
                }
                catch (Exception) { }

                Image imgMuell = new Image
                {
                    Source = statem == "1" ? "muellOut5.png" : (statem == "2" ? "muellIn5.png" : "muellInOut5.png"),
                    Margin = new Thickness(3, -2, 3, 0),
                    HeightRequest = 20,
                    WidthRequest = 20,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.End,
                };
                var lbMuell = new Label()
                {
                    Padding = new Thickness(5, 2, 5, 2),
                    Text = name,
                    TextColor = Color.FromArgb("#000000"),
                    Margin = new Thickness(6, 0, 0, 0),
                    FontSize = 12,
                    LineBreakMode = LineBreakMode.TailTruncation,
                    HorizontalOptions = LayoutOptions.Start,
                    BackgroundColor = Color.FromArgb(col)
                };
                stmuell.Children.Add(imgMuell);
                stmuell.Children.Add(lbMuell);
                stv.Children.Add(stmuell);
            }

            if (p.personid != p.vonpersonid)
            {
                stv.Children.Add(new Label()
                {
                    Padding = new Thickness(0),
                    Text = " (" + GetPlanedPersonInitialien(p.vonpersonid) + ")",
                    TextColor = Colors.Orange,
                    Margin = new Thickness(3, 0, 5, 3),
                    FontSize = 12,
                    HorizontalOptions = LayoutOptions.StartAndExpand,
                    LineBreakMode = LineBreakMode.WordWrap,
                });
            }
            sth.Children.Add(checkBox);
            sth.Children.Add(stv);



            if (obj != null)
            {
                var bemBadgeCount = new Label
                {
                    Text = "0",
                    Margin = new Thickness(0),
                    Padding = new Thickness(3, 0, 3, 0),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 10,
                    TextColor = Color.FromArgb("#ffffff"),
                    HorizontalTextAlignment = TextAlignment.Center
                };
                var badgeStack = new StackLayout()
                {
                    Margin = new Thickness(-9, -4, 0, 0),
                    Padding = new Thickness(0),
                    Spacing = 0,
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Start,
                    IsVisible = false,
                    Children = {
                        new Border {
                            BackgroundColor = Color.FromArgb("#007700"),
                             Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
                            HorizontalOptions = LayoutOptions.Start,
                            VerticalOptions = LayoutOptions.Start,
                            HeightRequest = 15,
                            WidthRequest = 28,
                            Margin = new Thickness(0),
                            Padding = new Thickness(1,1,0,0),
                            StrokeShape = new RoundRectangle { CornerRadius = 10 },
                            Content = bemBadgeCount
                        },
                    }
                };
                obj.badge = bemBadgeCount;
                obj.badgeStack = badgeStack;
                obj.count = 0;

                var noticeStack = new StackLayout
                {
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Orientation = StackOrientation.Vertical,
                    Spacing = 0,
                    Padding = new Thickness(1),
                    Margin = new Thickness(0),
                    BackgroundColor = Color.FromArgb("#144d73"),
                    Children = {
                    new Image
                    {
                        Margin = new Thickness(2),
                        HeightRequest = 32,
                        WidthRequest = 32,
                        VerticalOptions = LayoutOptions.CenterAndExpand,
                        HorizontalOptions = LayoutOptions.CenterAndExpand,
                        Source = AppModel.Instance.imagesBase.CamMessageWarn,
                    }

                }
                };
                var btnNoticeFrame = new Border()
                {
                    Padding = new Thickness(1),
                    Margin = new Thickness(2, 2, 2, 0),
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Start,
                    BackgroundColor = Color.FromArgb("#041d43"),
                    Content = noticeStack,
                    Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
                };
                if (func != null)
                {
                    btnNoticeFrame.GestureRecognizers.Clear();
                    btnNoticeFrame.GestureRecognizers.Add(new TapGestureRecognizer() { Command = func, CommandParameter = obj.lei });
                }

                sth.Children.Add(btnNoticeFrame);
                sth.Children.Add(badgeStack);
            }




            checkBox.CheckedChanged -= (object o, CheckedChangedEventArgs ev) => { ChangeObjektPlanWeekMobileDirektC(checkBox, p, btn, lb, mainP); };
            checkBox.CheckedChanged += (object o, CheckedChangedEventArgs ev) => { ChangeObjektPlanWeekMobileDirektC(checkBox, p, btn, lb, mainP); };

            stv.GestureRecognizers.Clear();
            var t_quest_direktbuchen_cancel = new TapGestureRecognizer();
            t_quest_direktbuchen_cancel.Tapped += (object o, TappedEventArgs ev) => { ChangeObjektPlanWeekMobileDirekt(checkBox, p, btn, lb, mainP); };
            stv.GestureRecognizers.Add(t_quest_direktbuchen_cancel);


            return sth;
        }

        public static StackLayout GetPlanedTodayNotMobileItem(PlanPersonMobile p)
        {
            var b = AppModel.Instance.AllBuildings.Find(o => o.id == p.objektid);


            var stv = new StackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromArgb("#55042d53"),
            };
            stv.Children.Add(new Label
            {
                Padding = new Thickness(0),
                Text = b != null ? "" + b.plz + " " + b.ort + " - " + b.strasse + " " + b.hsnr : "Objekt nicht gefunden!",
                TextColor = (b == null ? Color.FromArgb("#ffcc00") : Color.FromArgb("#ffffff")),
                Margin = new Thickness(3, 3, 5, 0),
                FontSize = 14,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                LineBreakMode = LineBreakMode.TailTruncation,
            });

            var sth = new StackLayout()
            {
                Padding = new Thickness(3, 1, 3, 1),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };
            Image imgMobilOff = new Image
            {
                Source = "mobil_leistung_off.png",
                Margin = new Thickness(2, 0, 2, 0),
                HeightRequest = 24,
                WidthRequest = 24,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start,
            };
            sth.Children.Add(imgMobilOff);

            if (p.muelltoid > 0)
            {
                var stmuell = new StackLayout()
                {
                    Padding = new Thickness(0),
                    Margin = new Thickness(0),
                    Spacing = 0,
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.StartAndExpand
                };
                var name = "";
                var col = "";
                var statem = "";
                var leiid = "";
                try
                {
                    var all = p.info.Split('#');
                    name = all[0];
                    col = "#" + all[1];
                    statem = all[2];
                    leiid = all[3];
                }
                catch (Exception) { }

                var lbMuell = new Label()
                {
                    Padding = new Thickness(5, 2, 5, 2),
                    Text = name,
                    TextColor = Color.FromArgb("#000000"),
                    Margin = new Thickness(6, 0, 0, 0),
                    FontSize = 12,
                    LineBreakMode = LineBreakMode.TailTruncation,
                    HorizontalOptions = LayoutOptions.StartAndExpand,
                    VerticalOptions = LayoutOptions.Center,
                    BackgroundColor = Color.FromArgb(col)
                };
                stmuell.Children.Add(lbMuell);
                sth.Children.Add(stmuell);
            }
            stv.Children.Add(sth);
            stv.Children.Add(new Label
            {
                Padding = new Thickness(0),
                Text = "Diese Leistung ist zwar geplant und zugewiesen, jedoch ist die Bearbeitung für Mobile deaktiviert.",
                TextColor = Color.FromArgb("#ffcc00"),
                Margin = new Thickness(8, 0, 0, 5),
                FontSize = 14,
                HorizontalOptions = LayoutOptions.Start,
            });
            return stv;
        }

        public static void ChangeObjektPlanWeekMobileDirekt(CheckBox checkBox, PlanPersonMobile p, Border btn, Label lb, PlanPersonMobile mainP)
        {
            checkBox.IsChecked = !checkBox.IsChecked;
            p.isSelected = checkBox.IsChecked;

            if (mainP != null && mainP.more != null && mainP.more.Count > 0)
            {
                int c = 0;
                mainP.more.ForEach(o =>
                {
                    if (o.isSelected)
                        c++;
                });
                btn.IsVisible = (c > 0);
                lb.IsVisible = !btn.IsVisible;
            }
        }

        public static void ChangeObjektPlanWeekMobileDirektC(CheckBox checkBox, PlanPersonMobile p, Border btn, Label lb, PlanPersonMobile mainP)
        {
            p.isSelected = checkBox.IsChecked;
            if (mainP != null && mainP.more != null && mainP.more.Count > 0)
            {
                int c = 0;
                mainP.more.ForEach(o =>
                {
                    if (o.isSelected)
                        c++;
                });
                btn.IsVisible = (c > 0);
                lb.IsVisible = !btn.IsVisible;
            }
        }


        public static string GetPlanedPerson(Int32 id)
        {
            var pe = AppModel.Instance.PlanResponse.persons.Find(p => p.id == id);
            if (pe != null) { return (String.IsNullOrWhiteSpace(pe.name) ? "" : pe.name) + " " + (String.IsNullOrWhiteSpace(pe.vorname) ? "" : pe.vorname.Substring(0, 1) + "."); }
            return "Pers.Id.: " + id;
        }
        public static string GetPlanedPersonInitialien(Int32 id)
        {
            var pe = AppModel.Instance.PlanResponse.persons.Find(p => p.id == id);
            if (pe != null) { return (String.IsNullOrWhiteSpace(pe.name) ? "" : pe.name.Substring(0, 1)) + "." + (String.IsNullOrWhiteSpace(pe.vorname) ? "" : pe.vorname.Substring(0, 1) + "."); }
            return "Pers.Id.: " + id;
        }

        public static StackLayout GetPlanedOptListWinter(PlanPersonMobile p, Object value, bool showLast)
        {
            var model = ((value as List<Object>)[0] as AppModel);
            //var list = ((value as List<Object>)[1] as BuildingWSO).ArrayOfAuftrag;
            var obj = ((value as List<Object>)[1] as BuildingWSO);
            List<AuftragWSO> list = new List<AuftragWSO>();
            if (obj != null)
            {
                list = obj.ArrayOfAuftrag.FindAll(_ => _.id == p.auftragid);
                //list = obj.ArrayOfAuftrag != null ? obj.ArrayOfAuftrag : new List<AuftragWSO>();
            }
            var overlay = ((value as List<Object>)[2] as AbsoluteLayout);
            var ppm = ((value as List<Object>)[3] as PlanPersonMobile);

            DateTime? dtLast = null;
            if (!String.IsNullOrWhiteSpace(p.lastwork))
            {
                dtLast = Utils.StringDateToDateTime(p.lastwork);
            }
            var isToday = showLast && dtLast != null && dtLast.Value.ToString("ddMMyyyy") == DateTime.Now.ToString("ddMMyyyy");

            var b = AppModel.Instance.AllBuildings.Find(o => o.id == p.objektid);
            Image imgPin = new Image
            {
                Source = AppModel.Instance.imagesBase.Pin,
                Margin = new Thickness(5, 0, 5, 0),
                HeightRequest = 22,
                WidthRequest = 22,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End,
            };
            Image winter = new Image
            {
                Margin = new Thickness(0, 0, 0, 0),
                HeightRequest = 22,
                WidthRequest = 22,
                Source = "win_26.png",
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
            };
            if (b != null)
            {
                imgPin.GestureRecognizers.Clear();
                var tgr_imgPin = new TapGestureRecognizer();
                tgr_imgPin.Tapped -= (object o, TappedEventArgs ev) => { BuildingWSO.btn_MapTapped(b); };
                tgr_imgPin.Tapped += (object o, TappedEventArgs ev) => { BuildingWSO.btn_MapTapped(b); };
                imgPin.GestureRecognizers.Add(tgr_imgPin);
            }
            var imgInfo = new Image
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End,
                Margin = new Thickness(5, 0, 5, 0),
                HeightRequest = 22,
                WidthRequest = 22,
                Source = AppModel._Instance.imagesBase.InfoCircle,
                IsVisible = obj != null && !String.IsNullOrWhiteSpace(obj.notiz),
            };
            if (obj != null && !String.IsNullOrWhiteSpace(obj.notiz))
            {
                imgInfo.GestureRecognizers.Clear();
                var t_btn_objektinfo = new TapGestureRecognizer();
                t_btn_objektinfo.Tapped += (object ooo, TappedEventArgs ev) => { AppModel._Instance.MainPage.OpenObjektInfoDialogB(obj.notiz); };
                imgInfo.GestureRecognizers.Add(t_btn_objektinfo);
            }

            var stv = new StackLayout()
            {
                Padding = new Thickness(5, 6, 5, 6),
                Margin = new Thickness(0, 2, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromArgb(isToday ? "#ff04532d" : "#55042d53"),
            };
            var st = new StackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children = {
                    winter,
                    new Label()
                    {
                        Padding = new Thickness(0),
                        Text = b != null ? "" + b.plz + " " + b.ort + " - " + b.strasse + " " + b.hsnr:"Nicht gefunden! (Synchronisieren)",
                        TextColor = (b == null ? Color.FromArgb("#ffcc00") : Color.FromArgb("#ffffff")),
                        Margin = new Thickness(3, 3, 5, 3),
                        FontSize = 14,
                        HorizontalOptions = LayoutOptions.StartAndExpand,
                        LineBreakMode = LineBreakMode.WordWrap,
                    },
                    new Label()
                    {
                        Padding = new Thickness(0),
                        Text = "(" + p.std + ")",
                        TextColor = Color.FromArgb("#cccccc"),
                        Margin = new Thickness(0, 3, 5, 3),
                        FontSize = 12,
                        WidthRequest = 46,
                        HorizontalTextAlignment = TextAlignment.Center,
                        HorizontalOptions = LayoutOptions.Center
                    },
                    imgInfo,
                    imgPin
                }
            };
            stv.Children.Add(st);


            var oList = new List<AuftragWSO>();
            oList = list;
            oList.ForEach(o =>
            {
                bool isWinterKat = false;
                o.kategorien.ForEach(k =>
                {
                    if (k.winterservice > 0 && !isWinterKat)
                    {
                        isWinterKat = true;
                    }
                });

                if (isWinterKat)
                {

                    var imageLOrder = new Image
                    {
                        Margin = new Thickness(1, 0, 5, 0),
                        HeightRequest = 15,
                        WidthRequest = 15,
                        VerticalOptions = LayoutOptions.Start,
                        HorizontalOptions = LayoutOptions.Start,
                        Source = AppModel.Instance.imagesBase.OrderFolderTools
                    };
                    var lbOrder = new Label()
                    {
                        Text = o.GetMobileText(),
                        TextColor = Color.FromArgb("#999999"),
                        Margin = new Thickness(0, 0, 5, 0),
                        FontSize = 12,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        LineBreakMode = LineBreakMode.WordWrap,
                    };
                    var hOrder = new StackLayout()
                    {
                        Padding = new Thickness(24, 0, 0, 0),
                        Margin = new Thickness(0, 0, 0, 0),
                        Spacing = 0,
                        Orientation = StackOrientation.Horizontal,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        //BackgroundColor = Color.FromArgb("#aa042d53"),
                    };
                    hOrder.Children.Add(imageLOrder);
                    hOrder.Children.Add(lbOrder);

                    stv.Children.Add(hOrder);

                    o.kategorien.Where(k => k.winterservice == 1).ToList().ForEach(c =>
                    {
                        var imageLCatB = new Image
                        {
                            Margin = new Thickness(0, 0, 5, 0),
                            HeightRequest = 16,
                            WidthRequest = 16,
                            VerticalOptions = LayoutOptions.Start,
                            HorizontalOptions = LayoutOptions.Start,
                            Source = (c.art == "Leistung" ? AppModel.Instance.imagesBase.LeistungSymbol :
                                        (c.art == "Produkt" ? AppModel.Instance.imagesBase.ProduktSymbol :
                                            (c.art == "Texte" ? AppModel.Instance.imagesBase.TextSymbol :
                                            (c.art == "Check" ? AppModel.Instance.imagesBase.CheckWhite :
                                                AppModel.Instance.imagesBase.Quest
                                     ))))
                        };
                        var imageLCat = new Image
                        {
                            Margin = new Thickness(0, 0, 5, 0),
                            HeightRequest = 16,
                            WidthRequest = 16,
                            VerticalOptions = LayoutOptions.Start,
                            HorizontalOptions = LayoutOptions.Start,
                            Source = (c.art == "Leistung" ? AppModel.Instance.imagesBase.KLSymbol :
                              (c.art == "Produkt" ? AppModel.Instance.imagesBase.KPSymbol :
                                  (c.art == "Texte" ? AppModel.Instance.imagesBase.KTSymbol :
                                  (c.art == "Check" ? AppModel.Instance.imagesBase.KCSymbol :
                                      AppModel.Instance.imagesBase.Quest))))
                        };
                        var lbCat = new Label()
                        {
                            Text = c.GetMobileText(),
                            TextColor = Color.FromArgb("#cccccc"),
                            Margin = new Thickness(0, 0, 5, 0),
                            FontSize = 12,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            LineBreakMode = LineBreakMode.WordWrap,
                        };
                        var hCat = new StackLayout()
                        {
                            Padding = new Thickness(24, 0, 0, 0),
                            Margin = new Thickness(0, 0, 0, 0),
                            Spacing = 0,
                            Orientation = StackOrientation.Horizontal,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            //BackgroundColor = Color.FromArgb("#cc042d53"),
                        };

                        hCat.Children.Add(imageLCatB);
                        hCat.Children.Add(lbCat);
                        stv.Children.Add(hCat);
                        //c.leistungen.ForEach(l =>
                        //{
                        //    var mainFrame = LeistungWSO.GetPositionWinterCardView(l);
                        //    mainFrame.GestureRecognizers.Clear();
                        //    var tgr = new TapGestureRecognizer();
                        //    tgr.Tapped += (object ob, EventArgs ev) => { var i = 0; };
                        //    mainFrame.GestureRecognizers.Add(tgr);
                        //    (categories.Children[1] as StackLayout).Children.Add(mainFrame);
                        //});
                    });
                }
            });

            if (showLast && !String.IsNullOrWhiteSpace(p.lastwork))
            {
                stv.Children.Add(new Label()
                {
                    Padding = new Thickness(0),
                    Text = "Zuletzt: " + p.lastwork,
                    TextColor = Color.FromArgb("#ffcc00"),
                    Margin = new Thickness(3, 0, 5, 3),
                    FontSize = 12,
                    HorizontalOptions = LayoutOptions.StartAndExpand,
                    LineBreakMode = LineBreakMode.WordWrap,
                });
            }
            if (p.personid != p.vonpersonid)
            {
                stv.Children.Add(new Label()
                {
                    Padding = new Thickness(0),
                    Text = " (" + GetPlanedPersonInitialien(p.vonpersonid) + ")",
                    TextColor = Colors.Orange,
                    Margin = new Thickness(3, 0, 5, 3),
                    FontSize = 12,
                    HorizontalOptions = LayoutOptions.StartAndExpand,
                    LineBreakMode = LineBreakMode.WordWrap,
                });
            }

            return stv;
        }

        public static StackLayout GetPlanedOptList(PlanPersonMobile p, bool showLast)
        {
            var b = AppModel.Instance.AllBuildings.Find(o => o.id == p.objektid);
            Image imgPin = new Image
            {
                Source = AppModel.Instance.imagesBase.Pin,
                Margin = new Thickness(5, 0, 5, 0),
                HeightRequest = p.haswork == 1 ? 16 : 22,
                WidthRequest = p.haswork == 1 ? 16 : 22,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End,
            };
            Image winter = new Image
            {
                Opacity = p.winterservice == 0 ? 0 : 1,
                Margin = new Thickness(0, 0, 0, 0),
                HeightRequest = p.haswork == 1 ? 16 : 22,
                WidthRequest = p.haswork == 1 ? 16 : 22,
                Source = "win_26.png",
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
            };
            if (b != null)
            {
                imgPin.GestureRecognizers.Clear();
                var tgr_imgPin = new TapGestureRecognizer();
                tgr_imgPin.Tapped -= (object o, TappedEventArgs ev) => { BuildingWSO.btn_MapTapped(b); };
                tgr_imgPin.Tapped += (object o, TappedEventArgs ev) => { BuildingWSO.btn_MapTapped(b); };
                imgPin.GestureRecognizers.Add(tgr_imgPin);
            }
            var imgInfo = new Image
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End,
                Margin = new Thickness(5, 0, 5, 0),
                HeightRequest = 22,
                WidthRequest = 22,
                Source = AppModel._Instance.imagesBase.InfoCircle,
                IsVisible = b != null && !String.IsNullOrWhiteSpace(b.notiz),
            };
            if (b != null && !String.IsNullOrWhiteSpace(b.notiz))
            {
                imgInfo.GestureRecognizers.Clear();
                var t_btn_objektinfo = new TapGestureRecognizer();
                t_btn_objektinfo.Tapped += (object ooo, TappedEventArgs ev) => { AppModel._Instance.MainPage.OpenObjektInfoDialogB(b.notiz); };
                imgInfo.GestureRecognizers.Add(t_btn_objektinfo);
            }



            var stv = new StackLayout()
            {
                Padding = new Thickness(5, 6, 5, 6),
                Margin = new Thickness(0, 2, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromArgb("#55042d53"),
            };
            var st = new StackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children = {
                    winter,
                    new Label()
                    {
                        Padding = new Thickness(0),
                        Text = b != null ? "" + b.plz + " " + b.ort + " - " + b.strasse + " " + b.hsnr:"Nicht gefunden! (Synchronisieren)",
                        TextColor = (b == null ? Color.FromArgb("#ffcc00") : Color.FromArgb("#ffffff")),
                        Margin = new Thickness(3, 3, 5, 3),
                        FontSize = 14,
                        HorizontalOptions = LayoutOptions.StartAndExpand,
                        LineBreakMode = LineBreakMode.WordWrap,
                    },
                    new Label()
                    {
                        Padding = new Thickness(0),
                        Text = "(" + p.std + ")",
                        TextColor = Color.FromArgb("#cccccc"),
                        Margin = new Thickness(0, 3, 5, 3),
                        FontSize = 12,
                        WidthRequest = 46,
                        HorizontalTextAlignment = TextAlignment.Center,
                        HorizontalOptions = LayoutOptions.Center
                    },
                    imgInfo,
                    imgPin
                }
            };
            stv.Children.Add(st);

            if (showLast)
            {

                stv.Children.Add(new Label()
                {
                    Padding = new Thickness(0),
                    Text = "Zuletzt: " + p.lastwork,
                    TextColor = Color.FromArgb("#ffcc00"),
                    Margin = new Thickness(3, 0, 5, 3),
                    FontSize = 12,
                    HorizontalOptions = LayoutOptions.StartAndExpand,
                    LineBreakMode = LineBreakMode.WordWrap,
                });
            }
            if (p.personid != p.vonpersonid)
            {
                stv.Children.Add(new Label()
                {
                    Padding = new Thickness(0),
                    Text = " (" + GetPlanedPersonInitialien(p.vonpersonid) + ")",
                    TextColor = Colors.Orange,
                    Margin = new Thickness(3, 0, 5, 3),
                    FontSize = 12,
                    HorizontalOptions = LayoutOptions.StartAndExpand,
                    LineBreakMode = LineBreakMode.WordWrap,
                });
            }

            return stv;
        }

        public static StackLayout GetPlanedLastList(PlanPersonMobile p)
        {
            var b = AppModel.Instance.AllBuildings.Find(o => o.id == p.objektid);
            Image imgPin = new Image
            {
                Source = p.haswork == 1 ? AppModel.Instance.imagesBase.CheckWhite : AppModel.Instance.imagesBase.Pin,
                Margin = new Thickness(5, 0, 5, 0),
                HeightRequest = p.haswork == 1 ? 16 : 22,
                WidthRequest = p.haswork == 1 ? 16 : 22,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End,
            };
            if (p.haswork == 0 && b != null)
            {
                imgPin.GestureRecognizers.Clear();
                var tgr_imgPin = new TapGestureRecognizer();
                tgr_imgPin.Tapped -= (object o, TappedEventArgs ev) => { BuildingWSO.btn_MapTapped(b); };
                tgr_imgPin.Tapped += (object o, TappedEventArgs ev) => { BuildingWSO.btn_MapTapped(b); };
                imgPin.GestureRecognizers.Add(tgr_imgPin);
            }

            var stv = new StackLayout()
            {
                Padding = new Thickness(5, 6, 5, 6),
                Margin = new Thickness(0, 2, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromArgb("#55042d53"),
                Children = {
                    new StackLayout {
                        Padding = new Thickness(0),
                        Margin = new Thickness(0),
                        Spacing = 0,
                        Orientation = StackOrientation.Horizontal,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        Children = {
                            new Label() {
                                Padding = new Thickness(0),
                                Text = b != null ? "" + b.plz + " " + b.ort + " - " + b.strasse + " " + b.hsnr:"Objekt nicht gefunden! (Synchronisieren)",
                                TextColor = (b == null ? Color.FromArgb("#ffcc00") : (p.haswork == 1 ? Color.FromArgb("#00ff00"):Color.FromArgb("#ffffff"))),
                                Margin = new Thickness(3, 3, 5, 3),
                                FontSize = 14,
                                HorizontalOptions = LayoutOptions.StartAndExpand,
                                LineBreakMode = LineBreakMode.WordWrap,
                            },
                            new Label()
                            {
                                Padding = new Thickness(0),
                                Text = "(" + p.std + ")",
                                TextColor = p.haswork == 1 ? Color.FromArgb("#00ff00"):Color.FromArgb("#cccccc"),
                                Margin = new Thickness(5, 3, 5, 3),
                                FontSize = 12,
                                WidthRequest = 46,
                                HorizontalTextAlignment = TextAlignment.Center,
                                HorizontalOptions = LayoutOptions.Center,
                            },
                            imgPin
                        }
                    }
                }
            };
            if (p.muelltoid > 0)
            {
                var name = "";
                var col = "";
                var statem = "";
                var leiid = "";
                try
                {
                    var all = p.info.Split('#');
                    name = all[0];
                    col = "#" + all[1];
                    statem = all[2];
                    leiid = all[3];
                }
                catch (Exception) { }


                var stmuell = new StackLayout()
                {
                    Padding = new Thickness(0),
                    Margin = new Thickness(0),
                    Spacing = 0,
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.FillAndExpand
                };
                Image imgMuell = new Image
                {
                    Source = statem == "1" ? "muellOut5.png" : (statem == "2" ? "muellIn5.png" : "muellInOut5.png"),
                    Margin = new Thickness(3, -2, 5, 0),
                    HeightRequest = 20,
                    WidthRequest = 20,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.End,
                };
                var lbMuell = new Label()
                {
                    Padding = new Thickness(5, 2, 5, 2),
                    Text = name,
                    TextColor = Color.FromArgb("#000000"),
                    Margin = new Thickness(6, 0, 0, 0),
                    FontSize = 12,
                    HorizontalOptions = LayoutOptions.StartAndExpand,
                    BackgroundColor = Color.FromArgb(col)
                };
                //var lbMuellA = new Label()
                //{
                //    Padding = new Thickness(5, 2, 5, 2),
                //    Text = statem == "1" ? "RAUSSTELLEN" : (statem == "2" ? "REINSTELLEN" : "REIN- u. RAUSSTELLEN"),
                //    TextColor = statem == "1" ? Color.FromArgb("#FF0000") : (statem == "2" ? Color.FromArgb("#00FF00") : Color.FromArgb("#FFCC00")),
                //    Margin = new Thickness(6, 0, 0, 0),
                //    FontSize = 12,
                //    HorizontalOptions = LayoutOptions.StartAndExpand,
                //};
                stmuell.Children.Add(imgMuell);
                stmuell.Children.Add(lbMuell);
                //stmuell.Children.Add(lbMuellA);

                stv.Children.Add(stmuell);
            }

            if (p.haswork == 1)
            {
                stv.Children.Add(new Label()
                {
                    Padding = new Thickness(0),
                    Text = "Zuletzt: " + p.lastwork,
                    TextColor = Color.FromArgb("#ffcc00"),
                    Margin = new Thickness(3, 0, 5, 3),
                    FontSize = 12,
                    HorizontalOptions = LayoutOptions.StartAndExpand,
                    LineBreakMode = LineBreakMode.WordWrap,
                });
            }
            if (p.personid != p.vonpersonid)
            {
                stv.Children.Add(new Label()
                {
                    Padding = new Thickness(0),
                    Text = " (" + GetPlanedPersonInitialien(p.vonpersonid) + ")",
                    TextColor = Colors.Orange,
                    Margin = new Thickness(3, 0, 5, 3),
                    FontSize = 12,
                    HorizontalOptions = LayoutOptions.StartAndExpand,
                    LineBreakMode = LineBreakMode.WordWrap,
                });
            }

            return stv;
        }



        /// <summary>
        /// Speichert die PlanResponse
        /// </summary>
        public static bool Save(AppModel model, PlanResponse response)
        {
            try
            {
                if (model == null || response == null)
                {
                    AppModel.Logger?.Error("Save PlanResponse: model or response is null");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    AppModel.Logger?.Error("Save PlanResponse: CustomerNumber is null");
                    return false;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/planperson/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, "planresponse.ipm");

                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                string jsonString = JsonConvert.SerializeObject(response, jsonSettings);
                File.WriteAllText(filePath, jsonString);

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: Save PlanResponse");
                return false;
            }
        }

        /// <summary>
        /// Lädt die PlanResponse
        /// </summary>
        public static PlanResponse Load(AppModel model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return new PlanResponse();
                }

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/planperson/planresponse.ipm"
                );

                if (File.Exists(filePath))
                {
                    try
                    {
                        string jsonString = File.ReadAllText(filePath);

                        if (string.IsNullOrWhiteSpace(jsonString))
                        {
                            return new PlanResponse();
                        }

                        var jsonSettings = new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Include,
                            DefaultValueHandling = DefaultValueHandling.Include,
                            MissingMemberHandling = MissingMemberHandling.Ignore
                        };

                        PlanResponse response = JsonConvert.DeserializeObject<PlanResponse>(jsonString, jsonSettings);
                        return response ?? new PlanResponse();
                    }
                    catch (JsonException jsonEx)
                    {
                        // JSON Fehler - könnte alte BinaryFormatter Datei sein
                        AppModel.Logger?.Warn(jsonEx, "Failed to deserialize PlanResponse JSON, attempting migration");

                        if (TryMigrateLegacyPlanResponse(filePath, out PlanResponse migratedResponse))
                        {
                            // Nach erfolgreicher Migration neu speichern
                            Save(model, migratedResponse);
                            return migratedResponse;
                        }

                        return new PlanResponse();
                    }
                }
                else
                {
                    return new PlanResponse();
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: Load PlanResponse");
                return new PlanResponse();
            }
        }
        /// <summary>
        /// Versucht alte BinaryFormatter-Datei zu migrieren
        /// </summary>
        private static bool TryMigrateLegacyPlanResponse(string filePath, out PlanResponse planResponse)
        {
            planResponse = null;

            try
            {
                // Alte Datei sichern
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupPath = filePath + $".old_binary_{timestamp}";

                if (File.Exists(filePath))
                {
                    File.Copy(filePath, backupPath, true);
                    AppModel.Logger?.Info($"Legacy PlanResponse file backed up to: {backupPath}");
                }

                // In .NET MAUI kann BinaryFormatter nicht mehr verwendet werden
                // Die alte Datei wird gesichert und leere Response zurückgegeben
                planResponse = new PlanResponse();
                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: TryMigrateLegacyPlanResponse()");
                return false;
            }
        }
        /// <summary>
        /// Löscht die PlanResponse
        /// </summary>
        public static bool Delete(AppModel model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/planperson/planresponse.ipm"
                );

                if (File.Exists(filePath))
                {
                    // Optional: Backup vor dem Löschen
                    string backupPath = filePath + $".deleted_{DateTime.Now:yyyyMMdd_HHmmss}";
                    File.Copy(filePath, backupPath, true);

                    File.Delete(filePath);

                    AppModel.Logger?.Info($"PlanResponse deleted. Backup: {backupPath}");
                }

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: Delete PlanResponse");
                return false;
            }
        }

        /// <summary>
        /// Lädt die PlanResponse als JSON-String
        /// </summary>
        public static string Load_AsJson(AppModel model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return "{\"Error\": \"CustomerNumber not set\"}";
                }

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/planperson/planresponse.ipm"
                );

                if (File.Exists(filePath))
                {
                    string jsonString = File.ReadAllText(filePath);

                    if (string.IsNullOrWhiteSpace(jsonString))
                    {
                        return "{\"Error\": \"File is empty\"}";
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
                AppModel.Logger?.Error(ex, "ERROR: Load_AsJson PlanResponse");
                return "{\"Error\": \"" + ex.Message.Replace("\"", "'") + "\"}";
            }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }




}
