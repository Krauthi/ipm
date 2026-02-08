using Android.OS;
using iPMCloud.Mobile.Controls;
using iPMCloud.Mobile.vo;
using Microsoft.Maui.Controls;
// TODO: SignaturePad not MAUI-compatible - needs replacement
// using SignaturePad.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace iPMCloud.Mobile
{
    public class Check : ICloneable
    {
        public Int32 id;
        public Int32 gruppeid;
        public Int32 objektid;
        public Int32 personid;
        public String bezeichnung;
        public String verguetung = "0,00";
        public String typ;
        public String status;
        public String datum;
        public String enddatum;
        public int del;//TINYINT(4)
        public String lastchange;//varchar(60)
        public String p1;
        public String p2;
        public String p3;
        public String p4;
        public String berechnung = ""; // Angebotsberechnung / Pauschal  oder Standard
        public int berechnunginterval = 1; // Angebotsberechnungintervall / 1w(52), 14t(26), monatlich(12), viertj.(4), halbj.(2), jährl.(1)
        public Int32 refid = 0; // Tage die auf MahnErinnerung in der Rechnung dazu addiert wird 
        public String refname = "";
        public String usetime = ""; // für checklisten ein Arbeitszeit definieren der Abarbeitung dieser Liste die dann als Leistung ins Protokoll eingetragen wird und somit auch in die Zeiterfassng
        public int gesperrt = 0;//TINYINT(4)
        public String gesperrtgrund = "";
        public Int32 bindingid = 0;
        public Int32 adressid = 0;
        public String gueltigbis = "";
        public String ausfuehrungam = "";
        public String berechnungzum = "Zum Ende eines Monats"; // Berechung Zum ...
        public int berechnungvorher = 5; // Berechung Zum ...
        public int zahlung = 1; // Berechung Zum ...

        public int nichtberechnen = 0;
        public int wirtschaftsjahr = 0;
        public int inraten = 0;
        public int rgpermail = 0;
        public string spaeter = "0";
        public string saison = null;

        public long start = 0;
        public long end = 0;

        public String verguetungBezahlt = "0,00"; //nicht in DB


        //public PersonAdress adress;
        //public List<PersonAdress> ArrayOfAdress;
        public int countOfCheck_a = 0;
        public int countOfAktivCheck_a = 0;
        public string lastDateOfCheck_a = "0";
        public string lastDateOfCheck_aToReady = "0";
        public string lastStateOfCheck_a = "-";
        public string lastPersonOfCheck_a = "";
        public List<CheckLeistungAntwort> antworten { get; set; } = new List<CheckLeistungAntwort>();

        //public List<ObjektMABS> zustaendige = new List<ObjektMABS>();
        public List<CheckKategorie> kategorien = new List<CheckKategorie>();
        public BuildingWSO objekt;
        public PersonWSO person;
        public Boolean sessionstatus;
        public Boolean faelligeLeistungen = false;
        //public List<ObjektGeo> objektGeos;
        //public List<ObjektGeoSortList> objektGeoSortList;
        public List<BemerkungWSO> bemerkungen { get; set; }

        public int checklisteSenden = 0;
        public String checklisteletzteausfuehrung = "0";

        public int naeststeFaelligkeitDate = 0;
        public string naeststeFaelligkeitDateStr = "";
        public string naeststeFaelligkeitDateStamp = "0";


        public string fileurl = null;
        public string kundename = "";

        public string guid { get; set; } = "";
        public decimal prozent = 0;


        public CheckKategorie leereKategorie;
        public CheckLeistung leereLeistung;
        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }



        public static Check ConvertToCheckARequest(Check c)
        {
            AppModel.Instance.selectedCheckA.guid = Guid.NewGuid().ToString();
            AppModel.Instance.selectedCheckA.antworten.ForEach(_ => _.ClearGui());

            foreach (var item in AppModel.Instance.selectedCheckA.antworten)
            {
                if (item.type != "4")
                {
                    item.f4 = "";
                    item.f5 = "";
                }
                switch (item.type)
                {
                    case "0":// Ja / Nein / Keine
                        break;
                    case "1":// Text
                        break;
                    case "2":// Wert
                        break;
                    case "3":// Bild
                        break;
                    case "4":// Multi
                        if (item.multi == 1)
                        {
                            //4a
                        }
                        else
                        {
                            //4b
                        }
                        if (!string.IsNullOrWhiteSpace(item.a4))
                        {
                            List<int> listA = new List<int>();
                            var ants = item.a4.Split(new String[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            foreach (var an in ants)
                            {
                                if (!string.IsNullOrWhiteSpace(an))
                                {
                                    listA.Add((int.Parse(an) + 1));
                                }
                            };
                            item.a4 = string.Join(";", listA.Select(n => n.ToString()).ToArray());
                        }
                        break;
                    case "7":// Unterschrift
                        item.a1 = "" + item.bemWSO.text;
                        break;
                }

                item.bem = ConvertBemWSOToBem(item);
                item.bemWSO = null;
            }

            return c;
        }


        public static CheckLeistungAntwortBem ConvertBemWSOToBem(CheckLeistungAntwort cla)
        {
            if (cla.bemWSO == null || (cla.bemWSO != null && cla.bemWSO.photos != null && (String.IsNullOrWhiteSpace(cla.bemWSO.text) && cla.bemWSO.photos.Count == 0))) { return null; }
            var bem = new CheckLeistungAntwortBem();
            bem.text = "" + cla.bemWSO.text;
            bem.datum = "" + cla.bemWSO.datum;
            bem.a_id = cla.id;
            foreach (var p in cla.bemWSO.photos)
            {
                bem.imgs.Add(new CheckLeistungAntwortBemImg
                {
                    bem_id = -1,
                    url = System.Convert.ToBase64String(p.bytes)
                });
            }
            return bem;
        }


        public static StackLayout GetOffeneList(List<CheckInfo> checks, double width, ICommand func, ICommand funcEdit = null)
        {
            var main = new StackLayout
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };

            var pinBtn = new StackLayout
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                HeightRequest = 0,
                WidthRequest = 0,
            };
            var infoBtn = new StackLayout
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                HeightRequest = 0,
                WidthRequest = 0,
            };

            if (checks != null && checks.Count > 0)
            {
                foreach (var checkInfo in checks)
                {
                    var stv = new StackLayout()
                    {
                        Padding = new Thickness(0, 0, 0, 3),
                        Margin = new Thickness(0, 2, 0, 0),
                        Spacing = 0,
                        Orientation = StackOrientation.Vertical,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        WidthRequest = width,
                    };


                    var sth_B = new StackLayout()
                    {
                        Padding = new Thickness(0),
                        Margin = new Thickness(0),
                        Spacing = 0,
                        Orientation = StackOrientation.Horizontal,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        WidthRequest = width,
                    };

                    if (checkInfo.berechnunginterval > 0)
                    {
                        sth_B.Children.Add(GetBadgeFrame(checkInfo.naeststeFaelligkeitDate));
                    }
                    else
                    {
                        sth_B.Children.Add(GetBadgeFrameForNachBedarf());
                    }
                    var sth_btn = new StackLayout()
                    {
                        Padding = new Thickness(8, 5, 8, 5),
                        Margin = new Thickness(0, 0, 3, 0),
                        Spacing = 0,
                        Orientation = StackOrientation.Horizontal,
                        HorizontalOptions = LayoutOptions.End,
                        VerticalOptions = LayoutOptions.End,
                        WidthRequest = checkInfo.lastStateOfCheck_a == "Offen" ? 76 : 56,
                        MinimumWidthRequest = checkInfo.lastStateOfCheck_a == "Offen" ? 76 : 56,
                        BackgroundColor = checkInfo.lastStateOfCheck_a == "Offen" ? Color.FromArgb("#73042d") : Color.FromArgb("#04732d"),
                    };
                    sth_btn.Children.Add(new Label()
                    {
                        Padding = new Thickness(0),
                        Text = checkInfo.lastStateOfCheck_a == "Offen" ? "BEARBEITEN" : "STARTEN",
                        TextColor = Color.FromArgb("#ffffff"),
                        Margin = new Thickness(0),
                        FontSize = 13,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        LineBreakMode = LineBreakMode.NoWrap,
                    });


                    var b = AppModel.Instance.AllBuildings.Find(o => o.id == checkInfo.objektid);
                    if (b != null)
                    {
                        var sth_objekt = new StackLayout()
                        {
                            Padding = new Thickness(0),
                            Margin = new Thickness(0),
                            Spacing = 0,
                            Orientation = StackOrientation.Horizontal,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                        };


                        sth_objekt.Children.Add(new Label()
                        {
                            Padding = new Thickness(0),
                            Text = b.plz + " " + b.ort + " - " + b.strasse + " " + b.hsnr,
                            TextColor = Color.FromArgb("#ffffff"),
                            Margin = new Thickness(3, 3, 5, 3),
                            FontSize = 13,
                            HorizontalOptions = LayoutOptions.StartAndExpand,
                            VerticalOptions = LayoutOptions.Start,
                            LineBreakMode = LineBreakMode.WordWrap,
                        });

                        pinBtn = new StackLayout
                        {
                            Padding = new Thickness(1),
                            Margin = new Thickness(0, 0, 0, 0),
                            Spacing = 0,
                            Orientation = StackOrientation.Horizontal,
                            VerticalOptions = LayoutOptions.Start,
                            HorizontalOptions = LayoutOptions.Start,
                            Children = {
                                new StackLayout {
                                    Padding = new Thickness(2),
                                    Margin = new Thickness(0),
                                    Spacing = 0,
                                    Orientation = StackOrientation.Horizontal,
                                    VerticalOptions = LayoutOptions.FillAndExpand,
                                    HorizontalOptions = LayoutOptions.FillAndExpand,
                                    Children = {
                                        new Image {
                                            Source = AppModel.Instance.imagesBase.Pin,
                                            HeightRequest = 28,
                                            WidthRequest = 28,
                                            VerticalOptions = LayoutOptions.Center,
                                            HorizontalOptions = LayoutOptions.Center,
                                        }
                                    }
                                }
                            }
                        };
                        pinBtn.GestureRecognizers.Clear();
                        var tgr_imgPin = new TapGestureRecognizer();
                        tgr_imgPin.Tapped -= (object o, TappedEventArgs ev) => { BuildingWSO.btn_MapTapped(b); };
                        tgr_imgPin.Tapped += (object o, TappedEventArgs ev) => { BuildingWSO.btn_MapTapped(b); };
                        pinBtn.GestureRecognizers.Add(tgr_imgPin);

                        //b.notiz = "ksjkskl sdkljsdkjlsd sdklj sd kl";
                        infoBtn = new StackLayout
                        {
                            Padding = new Thickness(1),
                            Margin = new Thickness(0, 0, 3, 0),
                            Spacing = 0,
                            Orientation = StackOrientation.Horizontal,
                            VerticalOptions = LayoutOptions.Start,
                            HorizontalOptions = LayoutOptions.End,
                            IsVisible = !String.IsNullOrWhiteSpace(b.notiz),
                            Children = {
                                new StackLayout {
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
                        if (!String.IsNullOrWhiteSpace(b.notiz))
                        {
                            infoBtn.GestureRecognizers.Clear();
                            var t_btn_objektinfo = new TapGestureRecognizer();
                            t_btn_objektinfo.Tapped += (object ooo, TappedEventArgs ev) => { AppModel._Instance.MainPage.OpenObjektInfoDialogB(b.notiz); };
                            infoBtn.GestureRecognizers.Add(t_btn_objektinfo);
                        }

                        //sth.Children.Add(infoBtn);
                        //sth.Children.Add(pinBtn);

                        stv.Children.Add(sth_objekt);

                    }
                    else
                    {
                        // Nicht alle Objekte sind synchronisiert!
                        stv.Children.Add(new Label()
                        {
                            Padding = new Thickness(0),
                            Text = "Nicht gefunden! (" + checkInfo.objektid + ")(Synchronisieren)",
                            TextColor = Color.FromArgb("#ffcc00"),
                            Margin = new Thickness(3),
                            FontSize = 13,
                            HorizontalOptions = LayoutOptions.Start,
                            LineBreakMode = LineBreakMode.WordWrap,
                        });
                    }

                    // Check Bezeichnung (Lable)
                    sth_B.Children.Add(new Label()
                    {
                        Padding = new Thickness(0),
                        Text = checkInfo.bezeichnung,
                        TextColor = Color.FromArgb("#ffffff"),
                        Margin = new Thickness(3),
                        FontSize = 13,
                        HorizontalOptions = LayoutOptions.StartAndExpand,
                        VerticalOptions = LayoutOptions.Center,
                        LineBreakMode = LineBreakMode.WordWrap,
                    });

                    sth_B.Children.Add(sth_btn);


                    stv.Children.Add(sth_B);

                    if (func != null)
                    {
                        sth_btn.GestureRecognizers.Clear();
                        sth_btn.GestureRecognizers.Add(new TapGestureRecognizer()
                        {
                            Command = func,
                            CommandParameter = new IntBoolParam { val = checkInfo.id, bol = checkInfo.lastStateOfCheck_a == "Offen" }
                        });
                    }

                    var mainH = new StackLayout
                    {
                        Padding = new Thickness(0),
                        Margin = new Thickness(0, 0, 0, 3),
                        Spacing = 0,
                        Orientation = StackOrientation.Horizontal,
                        //VerticalOptions = LayoutOptions.FillAndExpand,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        WidthRequest = width,
                        BackgroundColor = Color.FromArgb("#55042d53"),
                    };
                    mainH.Children.Add(stv);

                    mainH.Children.Add(infoBtn);
                    mainH.Children.Add(pinBtn);


                    // Status 

                    //var mainHState = new Frame
                    //{
                    //    Padding = new Thickness(0, 2),
                    //    Margin = new Thickness(10, 2),
                    //    VerticalOptions = LayoutOptions.FillAndExpand,
                    //    HorizontalOptions = LayoutOptions.FillAndExpand,
                    //    WidthRequest = width,
                    //    BackgroundColor = Color.FromArgb("#55fff0000"),
                    //    BorderColor = Color.FromArgb("#55ffffff"),
                    //    CornerRadius = 10,
                    //    Content = new Label
                    //    {
                    //        Padding = new Thickness(0),
                    //        Margin = new Thickness(0),
                    //        VerticalOptions = LayoutOptions.CenterAndExpand,
                    //        HorizontalOptions = LayoutOptions.CenterAndExpand,
                    //        Text = " %"
                    //    }
                    //};

                    main.Children.Add(mainH);
                    //main.Children.Add(mainHState);
                }
            }

            return main;
        }



        public static Frame GetBadgeFrame(int value)
        {

            return new Frame
            {
                BackgroundColor = Color.FromArgb(value < 0 ? "#ff0000" : (value < 1 ? "#ffcc00" : "#009900")),
                IsClippedToBounds = true,
                HasShadow = true,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(3, 0, 3, 0),
                Padding = new Thickness(4, 2, 4, 2),
                CornerRadius = 5,
                Content = new Label
                {
                    Text = Int32.Parse("" + value).ToString(),
                    Margin = new Thickness(0, 0, 0, 0),
                    Padding = new Thickness(0, 0, 0, 0),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 11,
                    TextColor = Colors.White,
                    FontAttributes = FontAttributes.Bold,
                    MinimumWidthRequest = 50,
                    LineBreakMode = LineBreakMode.NoWrap,
                    HorizontalTextAlignment = TextAlignment.Center
                }
            };
        }
        public static Frame GetBadgeFrameForNachBedarf()
        {

            return new Frame
            {
                BackgroundColor = Color.FromArgb("#338dca"),
                IsClippedToBounds = true,
                HasShadow = true,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(3, 0, 3, 0),
                Padding = new Thickness(4, 2, 4, 2),
                CornerRadius = 5,
                Content = new Label
                {
                    Text = "-",
                    Margin = new Thickness(0, 0, 0, 0),
                    Padding = new Thickness(0, 0, 0, 0),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 11,
                    TextColor = Colors.White,
                    FontAttributes = FontAttributes.Bold,
                    MinimumWidthRequest = 50,
                    LineBreakMode = LineBreakMode.NoWrap,
                    HorizontalTextAlignment = TextAlignment.Center
                }
            };
        }


        public static Frame GetBadgeRoundFrame(int value, bool isRed, bool isGray)
        {

            return new Frame
            {
                BackgroundColor = Color.FromArgb(isRed ? "#ff0000" : (isGray ? "#999999" : "#009900")),
                IsClippedToBounds = true,
                HasShadow = true,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(3, 0),
                Padding = new Thickness(4, 2),
                CornerRadius = 10,
                Content = new Label
                {
                    Text = Int32.Parse("" + value).ToString(),
                    Margin = new Thickness(0),
                    Padding = new Thickness(0),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 11,
                    TextColor = Colors.White,
                    FontAttributes = FontAttributes.Bold,
                    MinimumWidthRequest = 50,
                    LineBreakMode = LineBreakMode.NoWrap,
                    HorizontalTextAlignment = TextAlignment.Center
                }
            };
        }

        public static Frame GetQuestKategorieHeader(string title)
        {
            return new Frame
            {
                Padding = new Thickness(0),
                Margin = new Thickness(1, 30, 1, 0),
                BackgroundColor = Color.FromArgb("#66042d53"),
                HasShadow = true,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                CornerRadius = 10,
                Content = new StackLayout
                {
                    Padding = new Thickness(5),
                    Margin = new Thickness(0),
                    Spacing = 0,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Orientation = StackOrientation.Vertical,
                    Children = {
                        new Label {
                            Text = title,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            FontSize = 16,
                            TextColor = Color.FromArgb("#ffffff"),
                            HorizontalTextAlignment = TextAlignment.Start,
                            Margin = new Thickness(0),
                            Padding = new Thickness(10,0,10,0)
                        }
                    }
                }
            };
        }

        // Ja / Nein / Keine
        public static Frame GetQuestMain_0(CheckLeistungAntwort quest)
        {
            var tapYes = new TapGestureRecognizer();
            tapYes.Tapped -= (object o, TappedEventArgs ev) => { quest.Tap_a0_Yes(); };
            tapYes.Tapped += (object o, TappedEventArgs ev) => { quest.Tap_a0_Yes(); };
            var tapNo = new TapGestureRecognizer();
            tapNo.Tapped -= (object o, TappedEventArgs ev) => { quest.Tap_a0_No(); };
            tapNo.Tapped += (object o, TappedEventArgs ev) => { quest.Tap_a0_No(); };
            var tapNone = new TapGestureRecognizer();
            tapNone.Tapped -= (object o, TappedEventArgs ev) => { quest.Tap_a0_None(); };
            tapNone.Tapped += (object o, TappedEventArgs ev) => { quest.Tap_a0_None(); };
            var tapReset = new TapGestureRecognizer();
            tapReset.Tapped -= (object o, TappedEventArgs ev) => { quest.Tap_a0_Reset(); };
            tapReset.Tapped += (object o, TappedEventArgs ev) => { quest.Tap_a0_Reset(); };
            var tapBem = new TapGestureRecognizer();
            tapBem.Tapped -= (object o, TappedEventArgs ev) => { quest.Tap_a_Bem(); };
            tapBem.Tapped += (object o, TappedEventArgs ev) => { quest.Tap_a_Bem(); };

            quest.frame_Bem = new Frame
            {
                Padding = new Thickness(5),
                Margin = new Thickness(0, 5, 0, 0),
                BackgroundColor = Color.FromArgb("#042d53"),
                HasShadow = true,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Content = new Image
                {
                    Margin = new Thickness(0),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Source = AppModel.Instance.imagesBase.CamMessage,
                    HeightRequest = 24,
                    WidthRequest = 24,
                },
                GestureRecognizers = { tapBem },
            };
            quest.stack_Bem_Badge = new StackLayout
            {
                Padding = new Thickness(0),
                Margin = new Thickness(-16, -8, 0, 0),
                Spacing = 0,
                HorizontalOptions = LayoutOptions.End,
                Orientation = StackOrientation.Horizontal,
                Children = {
                    quest.bemWSO != null && quest.bemWSO.photos != null ? GetBadgeRoundFrame(
                        quest.bemWSO.photos.Count() > 0
                            ? quest.bemWSO.photos.Count()
                            : (String.IsNullOrWhiteSpace(quest.bemWSO.text) ? 0 : 1),
                        false,
                        quest.bemWSO.photos.Count() == 0)
                    : GetBadgeRoundFrame(0, false, true) }
            };
            quest.frame_No = new Frame
            {
                Padding = new Thickness(10, 5),
                Margin = new Thickness(0),
                Opacity = quest.isReady && quest.a0 != 2 ? 0.5 : 1,
                BackgroundColor = quest.isReady && quest.a0 != 2 ? Color.FromArgb("#666666") : Color.FromArgb("#73042d"),// #666666
                HasShadow = true,
                HorizontalOptions = quest.required != 1 ? LayoutOptions.End : LayoutOptions.EndAndExpand,
                VerticalOptions = LayoutOptions.Center,
                BorderColor = quest.isReady && quest.a0 != 2 ? Colors.White : Colors.Transparent,
                Content = new Label
                {
                    Text = "NEIN",
                    FontSize = 16,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Color.FromArgb("#ffffff"),
                    Margin = new Thickness(0),
                    Padding = new Thickness(0)
                },
                GestureRecognizers = { tapNo },
            };
            quest.frame_Yes = new Frame
            {
                Padding = new Thickness(10, 5),
                Margin = new Thickness(0),
                Opacity = quest.isReady && quest.a0 != 1 ? 0.5 : 1,
                BackgroundColor = quest.isReady && quest.a0 != 1 ? Color.FromArgb("#666666") : Color.FromArgb("#04732d"),// #666666
                HasShadow = true,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center,
                BorderColor = quest.isReady && quest.a0 != 1 ? Colors.White : Colors.Transparent,
                Content = new Label
                {
                    Text = "JA",
                    FontSize = 16,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Color.FromArgb("#ffffff"),
                    Margin = new Thickness(0),
                    Padding = new Thickness(0)
                },
                GestureRecognizers = { tapYes },
            };
            quest.frame_None = new Frame
            {
                IsVisible = quest.required != 1,
                Padding = new Thickness(5, 5),
                Margin = new Thickness(0),
                Opacity = quest.isReady && quest.a0 != 0 ? 0.5 : 1,
                BackgroundColor = quest.isReady && quest.a0 != 0 ? Color.FromArgb("#666666") : Color.FromArgb("#938302"),// #666666
                HasShadow = true,
                HorizontalOptions = LayoutOptions.EndAndExpand,
                VerticalOptions = LayoutOptions.Center,
                BorderColor = quest.isReady && quest.a0 != 0 ? Colors.White : Colors.Transparent,
                Content = new Label
                {
                    Text = "KEINE",
                    FontSize = 16,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Color.FromArgb("#ffffff"),
                    Margin = new Thickness(0),
                    Padding = new Thickness(0)
                },
                GestureRecognizers = { tapNone },
            };
            quest.frame_Reset = new Frame
            {
                IsVisible = quest.isReady,
                Padding = new Thickness(5),
                Margin = new Thickness(0, 5, 0, 0),
                BackgroundColor = Color.FromArgb("#042d53"),
                HasShadow = true,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Content = new Image
                {
                    Margin = new Thickness(0),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Source = AppModel.Instance.imagesBase.Undo,
                    HeightRequest = 24,
                    WidthRequest = 24,
                },
                GestureRecognizers = { tapReset },
            };
            quest.lb_quest = new Label
            {
                Text = quest.frage,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                FontSize = 16,
                TextColor = Colors.White,
                Margin = new Thickness(0),
                Padding = new Thickness(10, (quest.required == 1 ? 0 : 5), 10, 0),
                LineBreakMode = quest.isReady ? LineBreakMode.TailTruncation : LineBreakMode.WordWrap,
            };
            quest.lb_notiz = new Label
            {
                Text = quest.notiz,
                IsVisible = !String.IsNullOrWhiteSpace(quest.notiz),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                FontSize = 13,
                FontAttributes = FontAttributes.Italic,
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(0, 5),
                Padding = new Thickness(10, 0),
                LineBreakMode = quest.isReady ? LineBreakMode.TailTruncation : LineBreakMode.WordWrap,
            };
            quest.lb_required = new Label
            {
                IsVisible = quest.required == 1 && !quest.isReady,
                Text = "*PFLICHTANTWORT",
                HorizontalOptions = LayoutOptions.FillAndExpand,
                FontSize = 12,
                TextColor = quest.required == 1 ? Color.FromArgb("#ffcc00") : Colors.Transparent,
                HorizontalTextAlignment = TextAlignment.End,
                Margin = new Thickness(0),
                Padding = new Thickness(0, -3, 0, 0)
            };
            quest.img_ready = new Image
            {
                IsVisible = quest.isReady,
                Margin = new Thickness(0, 0, 0, -20),
                Source = AppModel.Instance.imagesBase.CheckGreen,
                HeightRequest = 20,
                WidthRequest = 20,
                HorizontalOptions = LayoutOptions.EndAndExpand
            };

            var frame = new Frame
            {
                Padding = new Thickness(0),
                Margin = new Thickness(15, 8, 15, 0),
                BackgroundColor = Color.FromArgb("#99042d53"),
                HasShadow = true,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                CornerRadius = 10,
                Content = new StackLayout
                {
                    Padding = new Thickness(5),
                    Margin = new Thickness(0),
                    Spacing = 0,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Orientation = StackOrientation.Vertical,
                    Children = {
                        quest.lb_required,
                        quest.img_ready,
                        quest.lb_quest,
                        quest.lb_notiz,
                        new StackLayout
                        {
                            Padding = new Thickness(0),
                            Margin = new Thickness(10,5,10,5),
                            Spacing = 15,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            Orientation = StackOrientation.Horizontal,
                            Children = {
                                quest.frame_Reset,
                                quest.frame_Bem,
                                quest.stack_Bem_Badge,
                                quest.frame_None,
                                quest.frame_No,
                                quest.frame_Yes,
                            }
                        }
                    }
                }
            };
            quest.mainFrame = frame;
            quest.CheckIsReadyAndSet_a0();
            return frame;
        }


        // Textantwort
        public static Frame GetQuestMain_1(CheckLeistungAntwort quest)
        {
            var tapReset = new TapGestureRecognizer();
            tapReset.Tapped -= (object o, TappedEventArgs ev) => { quest.Tap_a1_Reset(); };
            tapReset.Tapped += (object o, TappedEventArgs ev) => { quest.Tap_a1_Reset(); };

            var tapNone = new TapGestureRecognizer();
            tapNone.Tapped -= (object o, TappedEventArgs ev) => { quest.Tap_a1_None(); };
            tapNone.Tapped += (object o, TappedEventArgs ev) => { quest.Tap_a1_None(); };

            var tapBem = new TapGestureRecognizer();
            tapBem.Tapped -= (object o, TappedEventArgs ev) => { quest.Tap_a_Bem(); };
            tapBem.Tapped += (object o, TappedEventArgs ev) => { quest.Tap_a_Bem(); };

            quest.frame_Bem = new Frame
            {
                Padding = new Thickness(5),
                Margin = new Thickness(0, 5, 0, 0),
                BackgroundColor = Color.FromArgb("#042d53"),
                HasShadow = true,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Content = new Image
                {
                    Margin = new Thickness(0),
                    Source = AppModel.Instance.imagesBase.CamMessage,
                    HeightRequest = 24,
                    WidthRequest = 24,
                },
                GestureRecognizers = { tapBem },
            };
            quest.stack_Bem_Badge = new StackLayout
            {
                Padding = new Thickness(0),
                Margin = new Thickness(-16, -8, 0, 0),
                Spacing = 0,
                HorizontalOptions = LayoutOptions.End,
                Orientation = StackOrientation.Horizontal,
                Children = {
                    quest.bemWSO != null && quest.bemWSO.photos != null ? GetBadgeRoundFrame(
                        quest.bemWSO.photos.Count() > 0
                            ? quest.bemWSO.photos.Count()
                            : (String.IsNullOrWhiteSpace(quest.bemWSO.text) ? 0 : 1),
                        false,
                        quest.bemWSO.photos.Count() == 0)
                    : GetBadgeRoundFrame(0, false, true) }
            };
            quest.textEditor = new Editor
            {
                Text = quest.a1,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Margin = new Thickness(0),
                BackgroundColor = Color.FromArgb("#ffffff"),
                FontSize = 12,
                TextColor = Colors.Black,
                MaxLength = 499,
                HeightRequest = quest.isReady ? 40 : -1,
                AutoSize = EditorAutoSizeOption.TextChanges,
            };
            quest.textEditor.TextChanged -= (object sender, TextChangedEventArgs e) => { quest.Text_a1_Changed(); };
            quest.textEditor.TextChanged += (object sender, TextChangedEventArgs e) => { quest.Text_a1_Changed(); };
            quest.textEditor.Completed -= (object sender, EventArgs e) => { quest.Text_a1_Completed(); };
            quest.textEditor.Completed += (object sender, EventArgs e) => { quest.Text_a1_Completed(); };
            quest.textEditor.Focused -= (object sender, FocusEventArgs e) => { quest.Text_a1_Focused(); };
            quest.textEditor.Focused += (object sender, FocusEventArgs e) => { quest.Text_a1_Focused(); };

            quest.frame_None = new Frame
            {
                IsVisible = quest.required != 1,
                Padding = new Thickness(8, 5),
                Margin = new Thickness(0, 5, 10, 0),
                Opacity = quest.none ? 0.5 : 1,
                BackgroundColor = !quest.none ? Color.FromArgb("#666666") : Color.FromArgb("#938302"),// #666666
                HasShadow = true,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center,
                BorderColor = quest.none ? Colors.White : Colors.Transparent,
                Content = new Label
                {
                    Text = "KEINE",
                    FontSize = 16,
                    TextColor = Color.FromArgb("#ffffff"),
                    Margin = new Thickness(0),
                    Padding = new Thickness(0)
                },
                GestureRecognizers = { tapNone },
            };
            quest.frame_Reset = new Frame
            {
                IsVisible = quest.isReady,
                Padding = new Thickness(5),
                Margin = new Thickness(0, 5, 0, 0),
                BackgroundColor = Color.FromArgb("#042d53"),
                HasShadow = true,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Content = new Image
                {
                    Margin = new Thickness(0),
                    Source = AppModel.Instance.imagesBase.Undo,
                    HeightRequest = 24,
                    WidthRequest = 24,
                },
                GestureRecognizers = { tapReset },
            };
            quest.lb_quest = new Label
            {
                Text = quest.frage,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                FontSize = 16,
                TextColor = Colors.White,
                Margin = new Thickness(0),
                Padding = new Thickness(10, (quest.required == 1 ? 0 : 5), 10, 0),
                LineBreakMode = quest.isReady ? LineBreakMode.TailTruncation : LineBreakMode.WordWrap,
            };
            quest.lb_notiz = new Label
            {
                Text = quest.notiz,
                IsVisible = !String.IsNullOrWhiteSpace(quest.notiz),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                FontSize = 13,
                FontAttributes = FontAttributes.Italic,
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(0, 5),
                Padding = new Thickness(10, 0),
                LineBreakMode = quest.isReady ? LineBreakMode.TailTruncation : LineBreakMode.WordWrap,
            };
            quest.lb_required = new Label
            {
                IsVisible = quest.required == 1 && !quest.isReady,
                Text = "*PFLICHTANTWORT",
                HorizontalOptions = LayoutOptions.FillAndExpand,
                FontSize = 12,
                TextColor = quest.required == 1 ? Color.FromArgb("#ffcc00") : Colors.Transparent,
                HorizontalTextAlignment = TextAlignment.End,
                Margin = new Thickness(0),
                Padding = new Thickness(0, -3, 0, 0)
            };
            quest.img_ready = new Image
            {
                IsVisible = quest.isReady,
                Margin = new Thickness(0, 0, 0, -20),
                Source = AppModel.Instance.imagesBase.CheckGreen,
                HeightRequest = 20,
                WidthRequest = 20,
                HorizontalOptions = LayoutOptions.EndAndExpand
            };

            var frame = new Frame
            {
                Padding = new Thickness(0),
                Margin = new Thickness(15, 8, 15, 0),
                BackgroundColor = Color.FromArgb("#99042d53"),
                HasShadow = true,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                CornerRadius = 10,
                Content = new StackLayout
                {
                    Padding = new Thickness(5),
                    Margin = new Thickness(0),
                    Spacing = 0,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Orientation = StackOrientation.Vertical,
                    Children = {
                        quest.lb_required,
                        quest.img_ready,
                        quest.lb_quest,
                        quest.lb_notiz,
                        new StackLayout
                        {
                            Padding = new Thickness(0),
                            Margin = new Thickness(10,5),
                            Spacing = 0,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            Orientation = StackOrientation.Horizontal,
                            Children = {
                                new Frame
                                {
                                    Padding = new Thickness(0),
                                    Margin = new Thickness(0),
                                    BackgroundColor = Color.FromArgb("#99ffffff"),
                                    HasShadow = true,
                                    HorizontalOptions = LayoutOptions.FillAndExpand,
                                    CornerRadius = 10,
                                    Content = quest.textEditor,
                                }
                            }
                        },
                        new StackLayout
                        {
                            Padding = new Thickness(0),
                            Margin = new Thickness(10,5),
                            Spacing = 15,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            Orientation = StackOrientation.Horizontal,
                            Children = {
                                quest.frame_Reset,
                                quest.frame_Bem,
                                quest.stack_Bem_Badge,
                                quest.frame_None
                            }
                        }
                    }
                }
            };
            quest.mainFrame = frame;
            quest.CheckIsReadyAndSet_a1();
            return frame;
        }

        // Werantwort
        public static Frame GetQuestMain_2(CheckLeistungAntwort quest)
        {
            var tapReset = new TapGestureRecognizer();
            tapReset.Tapped -= (object o, TappedEventArgs ev) => { quest.Tap_a2_Reset(); };
            tapReset.Tapped += (object o, TappedEventArgs ev) => { quest.Tap_a2_Reset(); };

            var tapNone = new TapGestureRecognizer();
            tapNone.Tapped -= (object o, TappedEventArgs ev) => { quest.Tap_a2_None(); };
            tapNone.Tapped += (object o, TappedEventArgs ev) => { quest.Tap_a2_None(); };
            var tapBem = new TapGestureRecognizer();
            tapBem.Tapped -= (object o, TappedEventArgs ev) => { quest.Tap_a_Bem(); };
            tapBem.Tapped += (object o, TappedEventArgs ev) => { quest.Tap_a_Bem(); };

            quest.frame_Bem = new Frame
            {
                Padding = new Thickness(5),
                Margin = new Thickness(0, 5, 0, 0),
                BackgroundColor = Color.FromArgb("#042d53"),
                HasShadow = true,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Content = new Image
                {
                    Margin = new Thickness(0),
                    Source = AppModel.Instance.imagesBase.CamMessage,
                    HeightRequest = 24,
                    WidthRequest = 24,
                },
                GestureRecognizers = { tapBem },
            };
            quest.stack_Bem_Badge = new StackLayout
            {
                Padding = new Thickness(0),
                Margin = new Thickness(-16, -8, 0, 0),
                Spacing = 0,
                HorizontalOptions = LayoutOptions.End,
                Orientation = StackOrientation.Horizontal,
                Children = {
                    quest.bemWSO != null && quest.bemWSO.photos != null ? GetBadgeRoundFrame(
                        quest.bemWSO.photos.Count() > 0
                            ? quest.bemWSO.photos.Count()
                            : (String.IsNullOrWhiteSpace(quest.bemWSO.text) ? 0 : 1),
                        false,
                        quest.bemWSO.photos.Count() == 0)
                    : GetBadgeRoundFrame(0, false, true) }
            };
            quest.entry = new Entry
            {
                Text = quest.a2,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Margin = new Thickness(0),
                BackgroundColor = Color.FromArgb("#ffffff"),
                HorizontalTextAlignment = TextAlignment.End,
                FontSize = 12,
                TextColor = Colors.Black,
                MaxLength = 499,
                HeightRequest = 40,
                Keyboard = Keyboard.Numeric,
            };
            quest.entry.TextChanged -= (object sender, TextChangedEventArgs e) => { quest.Text_a2_Changed(); };
            quest.entry.TextChanged += (object sender, TextChangedEventArgs e) => { quest.Text_a2_Changed(); };
            quest.entry.Completed -= (object sender, EventArgs e) => { quest.Text_a2_Completed(); };
            quest.entry.Completed += (object sender, EventArgs e) => { quest.Text_a2_Completed(); };
            quest.entry.Unfocused -= (object sender, FocusEventArgs e) => { quest.Text_a2_Focused(); };
            quest.entry.Unfocused += (object sender, FocusEventArgs e) => { quest.Text_a2_Focused(); };

            quest.frame_None = new Frame
            {
                IsVisible = quest.required != 1,
                Padding = new Thickness(8, 5),
                Margin = new Thickness(0, 5, 10, 0),
                Opacity = quest.none ? 0.5 : 1,
                BackgroundColor = quest.none ? Color.FromArgb("#666666") : Color.FromArgb("#938302"),// #666666
                HasShadow = true,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center,
                BorderColor = quest.none ? Colors.White : Colors.Transparent,
                Content = new Label
                {
                    Text = "KEINE",
                    FontSize = 16,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Color.FromArgb("#ffffff"),
                    Margin = new Thickness(0),
                    Padding = new Thickness(0)
                },
                GestureRecognizers = { tapNone },
            };
            quest.frame_Reset = new Frame
            {
                IsVisible = quest.isReady,
                Padding = new Thickness(5),
                Margin = new Thickness(0, 5, 0, 0),
                BackgroundColor = Color.FromArgb("#042d53"),
                HasShadow = true,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Content = new Image
                {
                    Margin = new Thickness(0),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Source = AppModel.Instance.imagesBase.Undo,
                    HeightRequest = 24,
                    WidthRequest = 24,
                },
                GestureRecognizers = { tapReset },
            };
            quest.lb_quest = new Label
            {
                Text = quest.frage,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                FontSize = 16,
                TextColor = Colors.White,
                Margin = new Thickness(0),
                Padding = new Thickness(10, (quest.required == 1 ? 0 : 5), 10, 0),
                LineBreakMode = quest.isReady ? LineBreakMode.TailTruncation : LineBreakMode.WordWrap,
            };
            quest.lb_notiz = new Label
            {
                Text = quest.notiz,
                IsVisible = !String.IsNullOrWhiteSpace(quest.notiz),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                FontSize = 13,
                FontAttributes = FontAttributes.Italic,
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(0, 5),
                Padding = new Thickness(10, 0),
                LineBreakMode = quest.isReady ? LineBreakMode.TailTruncation : LineBreakMode.WordWrap,
            };
            quest.lb_required = new Label
            {
                IsVisible = quest.required == 1 && !quest.isReady,
                Text = "*PFLICHTANTWORT",
                HorizontalOptions = LayoutOptions.FillAndExpand,
                FontSize = 12,
                TextColor = quest.required == 1 ? Color.FromArgb("#ffcc00") : Colors.Transparent,
                HorizontalTextAlignment = TextAlignment.End,
                Margin = new Thickness(0),
                Padding = new Thickness(0, -3, 0, 0)
            };
            quest.img_ready = new Image
            {
                IsVisible = quest.isReady,
                Margin = new Thickness(0, 0, 0, -20),
                Source = AppModel.Instance.imagesBase.CheckGreen,
                HeightRequest = 20,
                WidthRequest = 20,
                HorizontalOptions = LayoutOptions.EndAndExpand
            };

            var frame = new Frame
            {
                Padding = new Thickness(0),
                Margin = new Thickness(15, 8, 15, 0),
                BackgroundColor = Color.FromArgb("#99042d53"),
                HasShadow = true,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                CornerRadius = 10,
                Content = new StackLayout
                {
                    Padding = new Thickness(5),
                    Margin = new Thickness(0),
                    Spacing = 0,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Orientation = StackOrientation.Vertical,
                    Children = {
                        quest.lb_required,
                        quest.img_ready,
                        quest.lb_quest,
                        quest.lb_notiz,
                        new StackLayout
                        {
                            Padding = new Thickness(0),
                            Margin = new Thickness(10,5),
                            Spacing = 15,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            Orientation = StackOrientation.Horizontal,
                            Children = {
                                quest.frame_Reset,
                                quest.frame_Bem,
                                quest.stack_Bem_Badge,
                                quest.frame_None
                            }
                        },
                        new StackLayout
                        {
                            Padding = new Thickness(0),
                            Margin = new Thickness(10,5),
                            Spacing = 5,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            Orientation = StackOrientation.Horizontal,
                            Children = {
                                new Frame
                                {
                                    Padding = new Thickness(0),
                                    Margin = new Thickness(0),
                                    BackgroundColor = Color.FromArgb("#99ffffff"),
                                    HasShadow = true,
                                    HorizontalOptions = LayoutOptions.FillAndExpand,
                                    CornerRadius = 10,
                                    Content = quest.entry,
                                },
                            }
                        },
                    }
                }
            };
            quest.mainFrame = frame;
            quest.CheckIsReadyAndSet_a2();
            return frame;
        }

        // Bild Antwort
        public static Frame GetQuestMain_3(CheckLeistungAntwort quest)
        {
            var tapNone = new TapGestureRecognizer();
            tapNone.Tapped -= (object o, TappedEventArgs ev) => { quest.Tap_a3_None(); };
            tapNone.Tapped += (object o, TappedEventArgs ev) => { quest.Tap_a3_None(); };
            var tapReset = new TapGestureRecognizer();
            tapReset.Tapped -= (object o, TappedEventArgs ev) => { quest.Tap_a3_Reset(); };
            tapReset.Tapped += (object o, TappedEventArgs ev) => { quest.Tap_a3_Reset(); };
            var tapBem = new TapGestureRecognizer();
            tapBem.Tapped -= (object o, TappedEventArgs ev) => { quest.Tap_a_Bem(); };
            tapBem.Tapped += (object o, TappedEventArgs ev) => { quest.Tap_a_Bem(); };
            var tapPic = new TapGestureRecognizer();
            tapPic.Tapped -= (object o, TappedEventArgs ev) => { quest.Tap_a3_Pic(); };
            tapPic.Tapped += (object o, TappedEventArgs ev) => { quest.Tap_a3_Pic(); };

            //var pics = quest.a3.Split(new String[] { "[##]" }, StringSplitOptions.RemoveEmptyEntries);

            quest.frame_None = new Frame
            {
                IsVisible = quest.required != 1,
                Padding = new Thickness(5, 5),
                Margin = new Thickness(0),
                Opacity = quest.isReady && quest.a0 != 0 ? 0.5 : 1,
                BackgroundColor = !quest.none ? Color.FromArgb("#666666") : Color.FromArgb("#938302"),
                HasShadow = true,
                HorizontalOptions = LayoutOptions.EndAndExpand,
                VerticalOptions = LayoutOptions.Start,
                BorderColor = quest.isReady && quest.a0 != 0 ? Colors.White : Colors.Transparent,
                Content = new Label
                {
                    Text = "KEINE",
                    FontSize = 15,
                    TextColor = Color.FromArgb("#ffffff"),
                    Margin = new Thickness(0),
                    Padding = new Thickness(0)
                },
                GestureRecognizers = { tapNone },
            };
            quest.frame_Pic = new Frame
            {
                Padding = new Thickness(5, 5),
                Margin = new Thickness(0),
                Opacity = quest.none ? 0.75 : 1,
                BackgroundColor = Color.FromArgb("#935302"),
                HasShadow = true,
                HorizontalOptions = quest.required != 1 ? LayoutOptions.End : LayoutOptions.EndAndExpand,
                VerticalOptions = LayoutOptions.Start,
                Content = new Label
                {
                    Text = "BILDER HINZUFÜGEN",
                    FontSize = 15,
                    TextColor = Color.FromArgb("#ffffff"),
                    Margin = new Thickness(0),
                    Padding = new Thickness(0, 0, 10, 0)
                },
                GestureRecognizers = { tapPic },
            };
            quest.frame_Bem = new Frame
            {
                Padding = new Thickness(5),
                Margin = new Thickness(0),
                BackgroundColor = Color.FromArgb("#042d53"),
                HasShadow = true,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Content = new Image
                {
                    Margin = new Thickness(0),
                    Source = AppModel.Instance.imagesBase.CamMessage,
                    HeightRequest = 24,
                    WidthRequest = 24,
                },
                GestureRecognizers = { tapBem },
            };
            quest.frame_Reset = new Frame
            {
                IsVisible = quest.isReady || true,
                Padding = new Thickness(5),
                Margin = new Thickness(0),
                BackgroundColor = Color.FromArgb("#042d53"),
                HasShadow = true,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Content = new Image
                {
                    Margin = new Thickness(0),
                    Source = AppModel.Instance.imagesBase.Undo,
                    HeightRequest = 24,
                    WidthRequest = 24,
                },
                GestureRecognizers = { tapReset },
            };
            quest.lb_quest = new Label
            {
                Text = quest.frage,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                FontSize = 16,
                TextColor = Colors.White,
                Margin = new Thickness(0),
                Padding = new Thickness(10, (quest.required == 1 ? 0 : 5), 10, 0),
                LineBreakMode = quest.isReady ? LineBreakMode.TailTruncation : LineBreakMode.WordWrap,
            };
            quest.lb_notiz = new Label
            {
                Text = quest.notiz,
                IsVisible = !String.IsNullOrWhiteSpace(quest.notiz),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                FontSize = 13,
                FontAttributes = FontAttributes.Italic,
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(0, 5),
                Padding = new Thickness(10, 0),
                LineBreakMode = quest.isReady ? LineBreakMode.TailTruncation : LineBreakMode.WordWrap,
            };
            quest.lb_required = new Label
            {
                IsVisible = quest.required == 1 && !quest.isReady,
                Text = "*PFLICHTANTWORT",
                HorizontalOptions = LayoutOptions.FillAndExpand,
                FontSize = 12,
                TextColor = quest.required == 1 ? Color.FromArgb("#ffcc00") : Colors.Transparent,
                HorizontalTextAlignment = TextAlignment.End,
                Margin = new Thickness(0),
                Padding = new Thickness(0, -3, 0, 0)
            };
            quest.img_ready = new Image
            {
                IsVisible = quest.isReady,
                Margin = new Thickness(0, 0, 0, -20),
                Source = AppModel.Instance.imagesBase.CheckGreen,
                HeightRequest = 20,
                WidthRequest = 20,
                HorizontalOptions = LayoutOptions.EndAndExpand
            };
            quest.stack_Badge = new StackLayout
            {
                Padding = new Thickness(0),
                Margin = new Thickness(-26, -8, 0, 0),
                Spacing = 0,
                HorizontalOptions = LayoutOptions.End,
                Orientation = StackOrientation.Horizontal,
                Children = {
                    quest.bemWSO != null && quest.bemWSO.photos != null ? GetBadgeRoundFrame(
                        quest.bemWSO.photos.Count(),
                        false,
                        quest.bemWSO.photos.Count() == 0)
                    : GetBadgeRoundFrame(0, false, true)
                }
            };

            var frame = new Frame
            {
                Padding = new Thickness(0),
                Margin = new Thickness(15, 8, 15, 0),
                BackgroundColor = Color.FromArgb("#99042d53"),
                HasShadow = true,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                CornerRadius = 10,
                Content = new StackLayout
                {
                    Padding = new Thickness(5),
                    Margin = new Thickness(0),
                    Spacing = 0,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Orientation = StackOrientation.Vertical,
                    Children = {
                        quest.lb_required,
                        quest.img_ready,
                        quest.lb_quest,
                        quest.lb_notiz,
                        new StackLayout
                        {
                            Padding = new Thickness(0),
                            Margin = new Thickness(10,5),
                            Spacing = 10,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            Orientation = StackOrientation.Horizontal,
                            Children = {
                                quest.frame_Reset,
                                quest.frame_None,
                                quest.frame_Pic,
                                quest.stack_Badge,
                            }
                        }
                    }
                }
            };
            quest.mainFrame = frame;
            quest.CheckIsReadyAndSet_a3();
            return frame;
        }
        public static Frame GetQuestMain_3_inlay(CheckLeistungAntwort quest)
        {
            var lb_quest = new Label
            {
                Text = "" + quest.frage,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                FontSize = 16,
                TextColor = Colors.White,
                Margin = new Thickness(0),
                Padding = new Thickness(5, (quest.required == 1 ? 0 : 5), 5, 0),
                LineBreakMode = LineBreakMode.WordWrap,
            };
            var lb_notiz = new Label
            {
                Text = "" + quest.notiz,
                IsVisible = !String.IsNullOrWhiteSpace(quest.notiz),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                FontSize = 13,
                FontAttributes = FontAttributes.Italic,
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(0, 5),
                Padding = new Thickness(5, 0),
                LineBreakMode = LineBreakMode.TailTruncation,
            };
            var lb_required = new Label
            {
                IsVisible = quest.required == 1,
                Text = "*PFLICHTANTWORT",
                HorizontalOptions = LayoutOptions.FillAndExpand,
                FontSize = 12,
                TextColor = quest.required == 1 ? Color.FromArgb("#ffcc00") : Colors.Transparent,
                HorizontalTextAlignment = TextAlignment.End,
                Margin = new Thickness(0),
                Padding = new Thickness(0, -3, 0, 0)
            };

            var frame = new Frame
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                BackgroundColor = Color.FromArgb("#aa042d53"),
                HasShadow = true,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                CornerRadius = 10,
                Content = new StackLayout
                {
                    Padding = new Thickness(5),
                    Margin = new Thickness(0),
                    Spacing = 0,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Orientation = StackOrientation.Vertical,
                    Children = {
                        lb_required,
                        lb_quest,
                        lb_notiz
                    }
                }
            };
            return frame;
        }




        // Mehrfachantwort Multiquest
        public static Frame GetQuestMain_4a(CheckLeistungAntwort quest)
        {
            var qs = quest.f4.Split(new String[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            var ants = quest.a4.Split(new String[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

            int z = 0;
            quest.frame_ants = new List<Frame>();
            foreach (var q in qs)
            {
                var img = new Image
                {
                    Margin = new Thickness(5, 0, 0, 0),
                    Source = ants.Contains(z.ToString()) ? AppModel.Instance.imagesBase.Yes : AppModel.Instance.imagesBase.No,
                    Opacity = ants.Contains(z.ToString()) ? 1 : 0.5,
                    HeightRequest = 38,
                    WidthRequest = 38,
                    ClassId = "" + z,
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                };
                var tapAnt = new TapGestureRecognizer();
                tapAnt.Tapped -= (object o, TappedEventArgs ev) => { quest.Tap_a4a_Ant(img); };
                tapAnt.Tapped += (object o, TappedEventArgs ev) => { quest.Tap_a4a_Ant(img); };
                img.GestureRecognizers.Add(tapAnt);

                quest.frame_ants.Add(new Frame
                {
                    Padding = new Thickness(5, 5),
                    Margin = new Thickness(0, 3),
                    BackgroundColor = Color.FromArgb("#33ffffff"),
                    HasShadow = true,
                    CornerRadius = 10,
                    ClassId = "" + z,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    BorderColor = ants.Contains(z.ToString()) ? Colors.White : Colors.Transparent,
                    Content = new StackLayout
                    {
                        Padding = new Thickness(0),
                        Margin = new Thickness(0),
                        Spacing = 0,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        Orientation = StackOrientation.Horizontal,
                        Children = {
                            new Label
                            {
                                Text = q,
                                HorizontalOptions = LayoutOptions.StartAndExpand,
                                VerticalOptions = LayoutOptions.CenterAndExpand,
                                FontSize = 14,
                                TextColor = Colors.White,
                                Margin = new Thickness(0),
                                Padding = new Thickness(0),
                                LineBreakMode = LineBreakMode.WordWrap,
                            },
                            img
                        },
                    }
                });
                z++;
            }
            var questStack = new StackLayout
            {
                Padding = new Thickness(0),
                Margin = new Thickness(5, 0),
                Spacing = 0,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Orientation = StackOrientation.Vertical,
            };
            foreach (var item in quest.frame_ants)
            {
                questStack.Children.Add(item);
            };


            var tapNone = new TapGestureRecognizer();
            tapNone.Tapped -= (object o, TappedEventArgs ev) => { quest.Tap_a4a_None(); };
            tapNone.Tapped += (object o, TappedEventArgs ev) => { quest.Tap_a4a_None(); };

            var tapReset = new TapGestureRecognizer();
            tapReset.Tapped -= (object o, TappedEventArgs ev) => { quest.Tap_a4a_Reset(); };
            tapReset.Tapped += (object o, TappedEventArgs ev) => { quest.Tap_a4a_Reset(); };
            var tapBem = new TapGestureRecognizer();
            tapBem.Tapped -= (object o, TappedEventArgs ev) => { quest.Tap_a_Bem(); };
            tapBem.Tapped += (object o, TappedEventArgs ev) => { quest.Tap_a_Bem(); };

            quest.frame_Bem = new Frame
            {
                Padding = new Thickness(5),
                Margin = new Thickness(0),
                BackgroundColor = Color.FromArgb("#042d53"),
                HasShadow = true,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Content = new Image
                {
                    Margin = new Thickness(0),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Source = AppModel.Instance.imagesBase.CamMessage,
                    HeightRequest = 24,
                    WidthRequest = 24,
                },
                GestureRecognizers = { tapBem },
            };
            quest.stack_Bem_Badge = new StackLayout
            {
                Padding = new Thickness(0),
                Margin = new Thickness(-16, -8, 0, 0),
                Spacing = 0,
                HorizontalOptions = LayoutOptions.End,
                Orientation = StackOrientation.Horizontal,
                Children = {
                    quest.bemWSO != null && quest.bemWSO.photos != null ? GetBadgeRoundFrame(
                        quest.bemWSO.photos.Count() > 0
                            ? quest.bemWSO.photos.Count()
                            : (String.IsNullOrWhiteSpace(quest.bemWSO.text) ? 0 : 1),
                        false,
                        quest.bemWSO.photos.Count() == 0)
                    : GetBadgeRoundFrame(0, false, true) }
            };
            quest.frame_None = new Frame
            {
                IsVisible = quest.required != 1,
                Padding = new Thickness(10, 5),
                Margin = new Thickness(0),
                Opacity = quest.none ? 0.5 : 1,
                BackgroundColor = quest.none ? Color.FromArgb("#666666") : Color.FromArgb("#938302"),// #666666
                HasShadow = true,
                HorizontalOptions = LayoutOptions.EndAndExpand,
                VerticalOptions = LayoutOptions.Center,
                BorderColor = quest.none ? Colors.White : Colors.Transparent,
                Content = new Label
                {
                    Text = "KEINE",
                    FontSize = 16,
                    TextColor = Color.FromArgb("#ffffff"),
                    Margin = new Thickness(0),
                    Padding = new Thickness(0)
                },
                GestureRecognizers = { tapNone },
            };
            quest.frame_Reset = new Frame
            {
                IsVisible = quest.isReady,
                Padding = new Thickness(5),
                Margin = new Thickness(0),
                BackgroundColor = Color.FromArgb("#042d53"),
                HasShadow = true,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Content = new Image
                {
                    Margin = new Thickness(0),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Source = AppModel.Instance.imagesBase.Undo,
                    HeightRequest = 24,
                    WidthRequest = 24,
                },
                GestureRecognizers = { tapReset },
            };
            quest.lb_quest = new Label
            {
                Text = quest.frage,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                FontSize = 16,
                TextColor = Colors.White,
                Margin = new Thickness(0),
                Padding = new Thickness(10, (quest.required == 1 ? 0 : 5), 10, 0),
                LineBreakMode = quest.isReady ? LineBreakMode.TailTruncation : LineBreakMode.WordWrap,
            };
            quest.lb_notiz = new Label
            {
                Text = quest.notiz,
                IsVisible = !String.IsNullOrWhiteSpace(quest.notiz),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                FontSize = 13,
                FontAttributes = FontAttributes.Italic,
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(0, 5),
                Padding = new Thickness(10, 0),
                LineBreakMode = quest.isReady ? LineBreakMode.TailTruncation : LineBreakMode.WordWrap,
            };
            quest.lb_required = new Label
            {
                IsVisible = quest.required == 1 && !quest.isReady,
                Text = "*PFLICHTANTWORT",
                HorizontalOptions = LayoutOptions.FillAndExpand,
                FontSize = 12,
                TextColor = quest.required == 1 ? Color.FromArgb("#ffcc00") : Colors.Transparent,
                HorizontalTextAlignment = TextAlignment.End,
                Margin = new Thickness(0),
                Padding = new Thickness(0, -3, 0, 0)
            };
            quest.img_ready = new Image
            {
                IsVisible = quest.isReady,
                Margin = new Thickness(0, 0, 0, -20),
                Source = AppModel.Instance.imagesBase.CheckGreen,
                HeightRequest = 20,
                WidthRequest = 20,
                HorizontalOptions = LayoutOptions.EndAndExpand
            };

            var frame = new Frame
            {
                Padding = new Thickness(0),
                Margin = new Thickness(15, 8, 15, 0),
                BackgroundColor = Color.FromArgb("#99042d53"),
                HasShadow = true,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                CornerRadius = 10,
                Content = new StackLayout
                {
                    Padding = new Thickness(5),
                    Margin = new Thickness(0),
                    Spacing = 0,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Orientation = StackOrientation.Vertical,
                    Children = {
                        quest.lb_required,
                        quest.img_ready,
                        quest.lb_quest,
                        quest.lb_notiz,
                        new Label
                        {
                            Text = "Mehrere Antworten sind möglich!",
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            FontSize = 12,
                            TextColor = Colors.Yellow,
                            Margin = new Thickness(10,0,0,0),
                            Padding = new Thickness(0)
                        },
                        questStack,
                        new StackLayout
                        {
                            Padding = new Thickness(0),
                            Margin = new Thickness(10,5),
                            Spacing = 10,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            Orientation = StackOrientation.Horizontal,
                            Children = {
                                quest.frame_Reset,
                                quest.frame_Bem,
                                quest.stack_Bem_Badge,
                                quest.frame_None,
                            }
                        }
                    }
                }
            };
            quest.mainFrame = frame;
            quest.CheckIsReadyAndSet_a4a();
            return frame;
        }

        // Mehrfachantwort Singlequest
        public static Frame GetQuestMain_4b(CheckLeistungAntwort quest)
        {
            var qs = quest.f4.Split(new String[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            var ants = quest.a4.Split(new String[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

            int z = 0;
            quest.frame_ants = new List<Frame>();
            foreach (var q in qs)
            {
                var img = new Image
                {
                    Margin = new Thickness(5, 0, 0, 0),
                    Source = ants.Contains(z.ToString()) ? AppModel.Instance.imagesBase.Yes_Round : AppModel.Instance.imagesBase.No_Round,
                    Opacity = ants.Contains(z.ToString()) ? 1 : 0.5,
                    HeightRequest = 38,
                    WidthRequest = 38,
                    ClassId = "" + z,
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                };
                var tapAnt = new TapGestureRecognizer();
                tapAnt.Tapped -= (object o, TappedEventArgs ev) => { quest.Tap_a4b_Ant(img); };
                tapAnt.Tapped += (object o, TappedEventArgs ev) => { quest.Tap_a4b_Ant(img); };
                img.GestureRecognizers.Add(tapAnt);

                quest.frame_ants.Add(new Frame
                {
                    IsVisible = !quest.none && (ants.Length == 0 || ants.Contains(z.ToString())),
                    Padding = new Thickness(5, 5),
                    Margin = new Thickness(0, 3),
                    BackgroundColor = Color.FromArgb("#33ffffff"),
                    HasShadow = true,
                    CornerRadius = 10,
                    ClassId = "" + z,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    BorderColor = ants.Contains(z.ToString()) ? Colors.White : Colors.Transparent,
                    Content = new StackLayout
                    {
                        Padding = new Thickness(0),
                        Margin = new Thickness(0),
                        Spacing = 0,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        Orientation = StackOrientation.Horizontal,
                        Children = {
                            new Label
                            {
                                Text = q,
                                HorizontalOptions = LayoutOptions.StartAndExpand,
                                VerticalOptions = LayoutOptions.CenterAndExpand,
                                FontSize = 14,
                                TextColor = Colors.White,
                                Margin = new Thickness(0),
                                Padding = new Thickness(0),
                                LineBreakMode = LineBreakMode.WordWrap,
                            },
                            img
                        },
                    }
                }); ;
                z++;
            }
            var questStack = new StackLayout
            {
                Padding = new Thickness(0),
                Margin = new Thickness(5, 0),
                Spacing = 0,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Orientation = StackOrientation.Vertical,
            };
            foreach (var item in quest.frame_ants)
            {
                questStack.Children.Add(item);
            };


            var tapNone = new TapGestureRecognizer();
            tapNone.Tapped -= (object o, TappedEventArgs ev) => { quest.Tap_a4b_None(); };
            tapNone.Tapped += (object o, TappedEventArgs ev) => { quest.Tap_a4b_None(); };

            var tapReset = new TapGestureRecognizer();
            tapReset.Tapped -= (object o, TappedEventArgs ev) => { quest.Tap_a4b_Reset(); };
            tapReset.Tapped += (object o, TappedEventArgs ev) => { quest.Tap_a4b_Reset(); };
            var tapBem = new TapGestureRecognizer();
            tapBem.Tapped -= (object o, TappedEventArgs ev) => { quest.Tap_a_Bem(); };
            tapBem.Tapped += (object o, TappedEventArgs ev) => { quest.Tap_a_Bem(); };

            quest.frame_Bem = new Frame
            {
                Padding = new Thickness(5),
                Margin = new Thickness(0, 5, 0, 0),
                BackgroundColor = Color.FromArgb("#042d53"),
                HasShadow = true,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Content = new Image
                {
                    Margin = new Thickness(0),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Source = AppModel.Instance.imagesBase.CamMessage,
                    HeightRequest = 24,
                    WidthRequest = 24,
                },
                GestureRecognizers = { tapBem },
            };
            quest.stack_Bem_Badge = new StackLayout
            {
                Padding = new Thickness(0),
                Margin = new Thickness(-16, -8, 0, 0),
                Spacing = 0,
                HorizontalOptions = LayoutOptions.End,
                Orientation = StackOrientation.Horizontal,
                Children = {
                    quest.bemWSO != null && quest.bemWSO.photos != null ? GetBadgeRoundFrame(
                        quest.bemWSO.photos.Count() > 0
                            ? quest.bemWSO.photos.Count()
                            : (String.IsNullOrWhiteSpace(quest.bemWSO.text) ? 0 : 1),
                        false,
                        quest.bemWSO.photos.Count() == 0)
                    : GetBadgeRoundFrame(0, false, true) }
            };
            quest.frame_None = new Frame
            {
                IsVisible = quest.required != 1,
                Padding = new Thickness(5, 5),
                Margin = new Thickness(0),
                Opacity = quest.none ? 0.5 : 1,
                BackgroundColor = quest.none ? Color.FromArgb("#666666") : Color.FromArgb("#938302"),// #666666
                HasShadow = true,
                HorizontalOptions = LayoutOptions.EndAndExpand,
                VerticalOptions = LayoutOptions.Center,
                BorderColor = quest.none ? Colors.White : Colors.Transparent,
                Content = new Label
                {
                    Text = "KEINE",
                    FontSize = 16,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Color.FromArgb("#ffffff"),
                    Margin = new Thickness(0),
                    Padding = new Thickness(0)
                },
                GestureRecognizers = { tapNone },
            };
            quest.frame_Reset = new Frame
            {
                IsVisible = quest.isReady,
                Padding = new Thickness(5),
                Margin = new Thickness(0, 5, 0, 0),
                BackgroundColor = Color.FromArgb("#042d53"),
                HasShadow = true,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Content = new Image
                {
                    Margin = new Thickness(0),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Source = AppModel.Instance.imagesBase.Undo,
                    HeightRequest = 24,
                    WidthRequest = 24,
                },
                GestureRecognizers = { tapReset },
            };
            quest.lb_quest = new Label
            {
                Text = quest.frage,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                FontSize = 16,
                TextColor = Colors.White,
                Margin = new Thickness(0),
                Padding = new Thickness(10, (quest.required == 1 ? 0 : 5), 10, 0),
                LineBreakMode = quest.isReady ? LineBreakMode.TailTruncation : LineBreakMode.WordWrap,
            };
            quest.lb_notiz = new Label
            {
                Text = quest.notiz,
                IsVisible = !String.IsNullOrWhiteSpace(quest.notiz),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                FontSize = 13,
                FontAttributes = FontAttributes.Italic,
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(0, 5),
                Padding = new Thickness(10, 0),
                LineBreakMode = quest.isReady ? LineBreakMode.TailTruncation : LineBreakMode.WordWrap,
            };
            quest.lb_required = new Label
            {
                IsVisible = quest.required == 1 && !quest.isReady,
                Text = "*PFLICHTANTWORT",
                HorizontalOptions = LayoutOptions.FillAndExpand,
                FontSize = 12,
                TextColor = quest.required == 1 ? Color.FromArgb("#ffcc00") : Colors.Transparent,
                HorizontalTextAlignment = TextAlignment.End,
                Margin = new Thickness(0),
                Padding = new Thickness(0, -3, 0, 0)
            };
            quest.img_ready = new Image
            {
                IsVisible = quest.isReady,
                Margin = new Thickness(0, 0, 0, -20),
                Source = AppModel.Instance.imagesBase.CheckGreen,
                HeightRequest = 20,
                WidthRequest = 20,
                HorizontalOptions = LayoutOptions.EndAndExpand
            };

            var frame = new Frame
            {
                Padding = new Thickness(0),
                Margin = new Thickness(15, 8, 15, 0),
                BackgroundColor = Color.FromArgb("#99042d53"),
                HasShadow = true,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                CornerRadius = 10,
                Content = new StackLayout
                {
                    Padding = new Thickness(5),
                    Margin = new Thickness(0),
                    Spacing = 0,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Orientation = StackOrientation.Vertical,
                    Children = {
                        quest.lb_required,
                        quest.img_ready,
                        quest.lb_quest,
                        quest.lb_notiz,
                        new Label
                        {
                            Text = "Nur eine Antwort ist möglich!",
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            FontSize = 12,
                            TextColor = Colors.Yellow,
                            Margin = new Thickness(10,0,0,0),
                            Padding = new Thickness(0)
                        },
                        questStack,
                        new StackLayout
                        {
                            Padding = new Thickness(0),
                            Margin = new Thickness(10,5),
                            Spacing = 10,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            Orientation = StackOrientation.Horizontal,
                            Children = {
                                quest.frame_Reset,
                                quest.frame_Bem,
                                quest.stack_Bem_Badge,
                                quest.frame_None,
                            }
                        }
                    }
                }
            };
            quest.mainFrame = frame;
            quest.CheckIsReadyAndSet_a4b();
            return frame;
        }


        // Unterschrift
        public static Frame GetQuestMain_7(CheckLeistungAntwort quest)
        {
            var tapYes = new TapGestureRecognizer();
            tapYes.Tapped -= (object o, TappedEventArgs ev) => { quest.Tap_a7_OpenSig(); };
            tapYes.Tapped += (object o, TappedEventArgs ev) => { quest.Tap_a7_OpenSig(); };
            var tapNone = new TapGestureRecognizer();
            tapNone.Tapped -= (object o, TappedEventArgs ev) => { quest.Tap_a7_None(); };
            tapNone.Tapped += (object o, TappedEventArgs ev) => { quest.Tap_a7_None(); };
            var tapReset = new TapGestureRecognizer();
            tapReset.Tapped -= (object o, TappedEventArgs ev) => { quest.Tap_a7_Reset(); };
            tapReset.Tapped += (object o, TappedEventArgs ev) => { quest.Tap_a7_Reset(); };
            var tapBem = new TapGestureRecognizer();
            tapBem.Tapped -= (object o, TappedEventArgs ev) => { quest.Tap_a_Bem(true); };
            tapBem.Tapped += (object o, TappedEventArgs ev) => { quest.Tap_a_Bem(true); };

            quest.frame_Bem = new Frame
            {
                Padding = new Thickness(5),
                Margin = new Thickness(quest.isReady ? 10 : 0, 5, 0, 0),
                BackgroundColor = Color.FromArgb("#042d53"),
                HasShadow = true,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Content = new Image
                {
                    Margin = new Thickness(0),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Source = AppModel.Instance.imagesBase.CamMessage,
                    HeightRequest = 24,
                    WidthRequest = 24,
                },
                GestureRecognizers = { tapBem },
            };
            quest.frame_Yes = new Frame
            {
                IsVisible = !quest.isReady,
                Padding = new Thickness(5),
                Margin = new Thickness(0, 5, 0, 0),
                BackgroundColor = Color.FromArgb("#04732d"),// #666666
                HasShadow = true,
                HorizontalOptions = quest.required != 1 ? LayoutOptions.End : LayoutOptions.EndAndExpand,
                Content = new Label
                {
                    Text = "BEARBEITEN",
                    FontSize = 16,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Color.FromArgb("#ffffff"),
                    Margin = new Thickness(0),
                    Padding = new Thickness(0)
                },
                GestureRecognizers = { tapYes },
            };
            quest.frame_None = new Frame
            {
                IsVisible = quest.required != 1,
                Padding = new Thickness(5),
                Margin = new Thickness(0, 5, 0, 0),
                Opacity = !quest.none ? 0.5 : 1,
                BackgroundColor = !quest.none ? Color.FromArgb("#666666") : Color.FromArgb("#938302"),// #666666
                HasShadow = true,
                HorizontalOptions = LayoutOptions.EndAndExpand,
                VerticalOptions = LayoutOptions.Start,
                BorderColor = quest.none ? Colors.White : Colors.Transparent,
                Content = new Label
                {
                    Text = "KEINE",
                    FontSize = 16,
                    TextColor = Color.FromArgb("#ffffff"),
                    Margin = new Thickness(0),
                    Padding = new Thickness(0)
                },
                GestureRecognizers = { tapNone },
            };
            quest.frame_Reset = new Frame
            {
                IsVisible = quest.isReady,
                Padding = new Thickness(5),
                Margin = new Thickness(0, 5, 0, 0),
                BackgroundColor = Color.FromArgb("#042d53"),
                HasShadow = true,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Content = new Image
                {
                    Margin = new Thickness(0),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Source = AppModel.Instance.imagesBase.Undo,
                    HeightRequest = 24,
                    WidthRequest = 24,
                },
                GestureRecognizers = { tapReset },
            };
            quest.lb_quest = new Label
            {
                Text = quest.frage,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                FontSize = 16,
                TextColor = Colors.White,
                Margin = new Thickness(0),
                Padding = new Thickness(10, (quest.required == 1 ? 0 : 5), 10, 0),
                LineBreakMode = quest.isReady ? LineBreakMode.TailTruncation : LineBreakMode.WordWrap,
            };
            quest.lb_notiz = new Label
            {
                Text = quest.notiz,
                IsVisible = !String.IsNullOrWhiteSpace(quest.notiz),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                FontSize = 13,
                FontAttributes = FontAttributes.Italic,
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(0, 5),
                Padding = new Thickness(10, 0),
                LineBreakMode = quest.isReady ? LineBreakMode.TailTruncation : LineBreakMode.WordWrap,
            };
            quest.lb_required = new Label
            {
                IsVisible = quest.required == 1 && !quest.isReady,
                Text = "*PFLICHTANTWORT",
                HorizontalOptions = LayoutOptions.FillAndExpand,
                FontSize = 12,
                TextColor = quest.required == 1 ? Color.FromArgb("#ffcc00") : Colors.Transparent,
                HorizontalTextAlignment = TextAlignment.End,
                Margin = new Thickness(0),
                Padding = new Thickness(0, -3, 0, 0)
            };
            quest.img_ready = new Image
            {
                IsVisible = quest.isReady,
                Margin = new Thickness(0, 0, 0, -20),
                Source = AppModel.Instance.imagesBase.CheckGreen,
                HeightRequest = 20,
                WidthRequest = 20,
                HorizontalOptions = LayoutOptions.EndAndExpand
            };
            quest.img_sig = new Image
            {
                Margin = new Thickness(0),
                Source = String.IsNullOrWhiteSpace(quest.a7) ?
                    AppModel.Instance.imagesBase.SignPad :
                    ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(quest.a7))),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.Start,
            };

            quest.stack_Bem_Badge = new StackLayout
            {
                Padding = new Thickness(0),
                Margin = new Thickness(-16, -8, 0, 0),
                Spacing = 0,
                HorizontalOptions = LayoutOptions.End,
                Orientation = StackOrientation.Horizontal,
                Children = {
                    quest.bemWSO != null && quest.bemWSO.photos != null ? GetBadgeRoundFrame(
                        quest.bemWSO.photos.Count() > 0
                            ? quest.bemWSO.photos.Count()
                            : (String.IsNullOrWhiteSpace(quest.bemWSO.text) ? 0 : 1),
                        false,
                        quest.bemWSO.photos.Count() == 0)
                    : GetBadgeRoundFrame(0, false, true) }
            };
            var tapSig = new TapGestureRecognizer();
            tapSig.Tapped -= (object o, TappedEventArgs ev) => { quest.Tap_a7_OpenSig(); };
            tapSig.Tapped += (object o, TappedEventArgs ev) => { quest.Tap_a7_OpenSig(); };

            // Wird nur im PopUp verwendet !

            var frame = new Frame
            {
                Padding = new Thickness(0),
                Margin = new Thickness(15, 8, 15, 0),
                BackgroundColor = Color.FromArgb("#99042d53"),
                HasShadow = true,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                CornerRadius = 10,
                Content = new StackLayout
                {
                    Padding = new Thickness(5),
                    Margin = new Thickness(0),
                    Spacing = 0,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Orientation = StackOrientation.Vertical,
                    Children = {
                        quest.lb_required,
                        quest.img_ready,
                        quest.lb_quest,
                        quest.lb_notiz,
                        new StackLayout
                        {
                            Padding = new Thickness(0),
                            Margin = new Thickness(0),
                            Spacing = 0,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            Orientation = StackOrientation.Vertical,
                            GestureRecognizers = { tapSig },
                            Children = {
                                quest.img_sig,
                            }
                        },
                        new StackLayout
                        {
                            Padding = new Thickness(0),
                            Margin = new Thickness(10,5),
                            Spacing = 10,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            Orientation = StackOrientation.Horizontal,
                            Children = {
                                quest.frame_Reset,
                                quest.frame_Bem,
                                quest.stack_Bem_Badge,
                                quest.frame_None,
                                quest.frame_Yes,
                            }
                        }
                    }
                }
            };
            quest.mainFrame = frame;
            quest.CheckIsReadyAndSet_a7();
            return frame;
        }

        public static Frame GetQuestMain_7_PopUp(CheckLeistungAntwort originalQuest)
        {
            var tapNone = new TapGestureRecognizer();
            tapNone.Tapped -= (object o, TappedEventArgs ev) => { originalQuest.signPad.Clear(); };
            tapNone.Tapped += (object o, TappedEventArgs ev) => { originalQuest.signPad.Clear(); };
            var tapYes = new TapGestureRecognizer();
            tapYes.Tapped -= (object o, TappedEventArgs ev) => { originalQuest.Tap_a7_ReturnSig(); };
            tapYes.Tapped += (object o, TappedEventArgs ev) => { originalQuest.Tap_a7_ReturnSig(); };

            var frame_Yes = new Frame
            {
                Padding = new Thickness(5),
                Margin = new Thickness(0, 5, 0, 0),
                BackgroundColor = Color.FromArgb("#04732d"),// #666666
                HasShadow = true,
                HorizontalOptions = LayoutOptions.End,
                Content = new Label
                {
                    Text = "ÜBERNEHMEN",
                    FontSize = 16,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Color.FromArgb("#ffffff"),
                    Margin = new Thickness(0),
                    Padding = new Thickness(0)
                },
                GestureRecognizers = { tapYes },
            };
            var frame_None = new Frame
            {
                Padding = new Thickness(5),
                Margin = new Thickness(0, 5, 0, 0),
                BackgroundColor = Color.FromArgb("#73042d"),// #666666
                HasShadow = true,
                HorizontalOptions = LayoutOptions.EndAndExpand,
                VerticalOptions = LayoutOptions.Start,
                Content = new Label
                {
                    Text = "LÖSCHEN",
                    FontSize = 16,
                    TextColor = Color.FromArgb("#ffffff"),
                    Margin = new Thickness(0),
                    Padding = new Thickness(0)
                },
                GestureRecognizers = { tapNone },
            };
            var lb_quest = new Label
            {
                Text = originalQuest.frage,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                FontSize = 16,
                TextColor = Colors.White,
                Margin = new Thickness(0),
                Padding = new Thickness(10, (originalQuest.required == 1 ? 0 : 5), 10, 0),
                LineBreakMode = LineBreakMode.WordWrap,
            };
            var lb_notiz = new Label
            {
                Text = originalQuest.notiz,
                IsVisible = !String.IsNullOrWhiteSpace(originalQuest.notiz),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                FontSize = 13,
                FontAttributes = FontAttributes.Italic,
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(0, 5),
                Padding = new Thickness(10, 0),
                LineBreakMode = LineBreakMode.WordWrap,
            };
            var lb_required = new Label
            {
                IsVisible = originalQuest.required == 1,
                Text = "*PFLICHTANTWORT",
                HorizontalOptions = LayoutOptions.FillAndExpand,
                FontSize = 12,
                TextColor = Color.FromArgb("#ffcc00"),
                HorizontalTextAlignment = TextAlignment.End,
                Margin = new Thickness(0),
                Padding = new Thickness(0, -3, 0, 0)
            };

            var frame = new Frame
            {
                Padding = new Thickness(0),
                Margin = new Thickness(15, 8, 15, 0),
                BackgroundColor = Color.FromArgb("#99042d53"),
                HasShadow = true,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                CornerRadius = 10,
                Content = new StackLayout
                {
                    Padding = new Thickness(5),
                    Margin = new Thickness(0),
                    Spacing = 0,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Orientation = StackOrientation.Vertical,
                    Children = {
                        lb_required,
                        lb_quest,
                        lb_notiz,
                        originalQuest.signPad,
                        new StackLayout
                        {
                            Padding = new Thickness(0),
                            Margin = new Thickness(10,5),
                            Spacing = 30,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            Orientation = StackOrientation.Horizontal,
                            Children = {
                                frame_None,
                                frame_Yes,
                            }
                        }
                    }
                }
            };
            return frame;
        }

        // TODO: SignaturePad not MAUI-compatible - comment out until migrated
        /*
        public static SignaturePadView GetSignElement()
        {
            return new SignaturePadView()
            {
                Margin = new Thickness(0, 20, 0, 20),
                Padding = new Thickness(10),
                StrokeWidth = 3f,
                StrokeColor = Colors.Black,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                HeightRequest = 250,
                MinimumHeightRequest = 250,
                PromptText = "Unterschrift",
                PromptFontSize = 11,
                PromptTextColor = Color.FromArgb("#999999"),
                BackgroundColor = Color.FromArgb("#ccffffff"),
                SignatureLineColor = Color.FromArgb("#999999"),
                CaptionText = "",
                ClearTextColor = Color.FromArgb("#2f84bd"),
                ClearText = "",
                ClearFontSize = 12,
            };
        }
        */
        // In Check.cs oder CheckLeistungAntwort.cs

        public SignaturePadView signPad;

        public StackLayout GetSignElement()
        {
            signPad = new SignaturePadView
            {
                HeightRequest = 200,
                StrokeColor = Colors.Black,
                StrokeWidth = 2
            };

            var stack = new StackLayout();
            stack.Children.Add(signPad);
            return stack;
        }

        // Signatur speichern
        public async Task<byte[]> GetSignatureImage()
        {
            if (signPad.IsBlank)
                return null;

            return await signPad.GetImageStreamAsync();
        }

        // NUR TEXT
        public static Frame GetQuestMain_10(CheckLeistungAntwort quest)
        {
            var frame = new Frame
            {
                Padding = new Thickness(0),
                Margin = new Thickness(10),
                BackgroundColor = Color.FromArgb("#5504732d"),
                HasShadow = true,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                CornerRadius = 10,
                Content = new StackLayout
                {
                    Padding = new Thickness(5),
                    Margin = new Thickness(0),
                    Spacing = 0,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Orientation = StackOrientation.Vertical,
                    Children = {
                        new Label {
                            Text = quest.kat,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            FontSize = 16, FontAttributes = FontAttributes.Bold,
                            TextColor = Colors.White,
                            Margin = new Thickness(0),
                            Padding = new Thickness(5,3)
                        },
                        new Label {
                            Text = quest.frage,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            FontSize = 16,
                            TextColor = Colors.White,
                            Margin = new Thickness(0),
                            Padding = new Thickness(5,0),
                            LineBreakMode = LineBreakMode.WordWrap,
                        },
                        new Label {
                            Text = quest.notiz,
                            IsVisible = !String.IsNullOrWhiteSpace(quest.notiz),
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            FontSize = 13, FontAttributes = FontAttributes.Italic,
                            TextColor = Color.FromArgb("#cccccc"),
                            Margin = new Thickness(0,5),
                            Padding = new Thickness(5,0),
                            LineBreakMode = LineBreakMode.WordWrap,
                        }
                    }
                }
            };
            return frame;
        }


    }

}