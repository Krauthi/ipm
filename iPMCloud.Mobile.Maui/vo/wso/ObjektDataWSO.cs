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
    public class ObjektDataWSO
    {
        public Int32 id { get; set; }
        public Int32 gruppeid { get; set; }
        public Int32 objektid { get; set; }
        //public Int32 objektdataid { get; set; }
        //public int mobil { get; set; }
        public string text { get; set; }
        public string typ { get; set; }
        //public string datum { get; set; }
        public string nr { get; set; }
        public string mieter { get; set; }
        public string stand { get; set; }
        public string firstStand { get; set; }
        //public int del { get; set; }
        public string lastchange { get; set; }
        public string status { get; set; }
        //public string beendet { get; set; }

        public string ablesegrund { get; set; } = "";

        public string lastStand { get; set; } = "";
        public string standGeaendertAm { get; set; } = "";

        public string einheit { get; set; } = "";
        public string mieterb { get; set; }
        public string standdatum { get; set; } = "";  //"standdatum" in DB --- last stand datum
        public string eich { get; set; } = "";  //"eich" in DB --- eichdatum des Zählers  (Jahr)

        public string guid { get; set; } = "";
        public long ticks { get; set; } = 0;

        //public List<ObjektDataStandWSO> objektdatenstandlist = new List<ObjektDataStandWSO>();


        public ObjektDataWSO()
        {
            this.guid = Guid.NewGuid().ToString();
            this.ticks = DateTime.Now.Ticks;
        }

        public static StackLayout GetObjektDataListView(AppModel model, ICommand func, bool isChangedToday = false)
        {
            var stack = new StackLayout
            {
                Padding = new Thickness(5, 0, 5, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };
            string lastyp = "";
            model.LastBuilding.ArrayOfObjektdata = model.LastBuilding.ArrayOfObjektdata.OrderBy(s => s.typ).ToList();//.ThenBy(s => s.Name);
            model.LastBuilding.ArrayOfObjektdata.ForEach(od =>
            {
                if (od.status == "Aktiv")
                {
                    Frame stackPos = null;
                    var changed = JavaScriptDateConverter.Convert(long.Parse(od.standdatum)).ToString("ddMMyyyy");
                    var today = DateTime.Now.ToString("ddMMyyyy");

                    if (isChangedToday)
                    {
                        if (changed == today)
                        {
                            if (od.typ != lastyp)
                            {
                                stack.Children.Add(Elements.GetBoxViewLine());
                                stack.Children.Add(GetTypInfoElement(od.typ, model));
                                //stack.Children.Add(Elements.GetBoxViewLine());
                            }
                            stackPos = GetCardView(od, model, func);
                            stack.Children.Add(stackPos);
                            lastyp = od.typ;
                        }
                    }
                    else
                    {
                        if (changed != today)
                        {
                            if (od.typ != lastyp)
                            {
                                stack.Children.Add(Elements.GetBoxViewLine());
                                stack.Children.Add(GetTypInfoElement(od.typ, model));
                                //stack.Children.Add(Elements.GetBoxViewLine());
                            }
                            stackPos = GetCardView(od, model, func);
                            stack.Children.Add(stackPos);
                            lastyp = od.typ;
                        }
                    }
                }
            });
            return stack;
        }

        public static Border GetCardView(ObjektDataWSO od, AppModel model, ICommand func)
        {
            //var _prio = CalcOverdue(pos);
            var imageL = new Image
            {
                Margin = new Thickness(0, 0, 5, 0),
                HeightRequest = 22,
                WidthRequest = 22,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Source = GetImageFromTyp(od.typ, model),
            };
            var lb_nr = new Label()
            {
                Text = od.nr,
                TextColor = Color.FromArgb("#dddddd"),
                Margin = new Thickness(5, -5, -5, 3),
                Padding = new Thickness(5, 5, 5, 5),
                FontSize = 26,
                FontAttributes = FontAttributes.None,
                HorizontalTextAlignment = TextAlignment.End,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                LineBreakMode = LineBreakMode.WordWrap,
                CharacterSpacing = 1.05,
                BackgroundColor = Color.FromArgb("#143d63"),
            };
            var lb_text = new Label()
            {
                Text = od.text,
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(5, 0, 5, 5),
                FontSize = 16,
                HorizontalOptions = LayoutOptions.StartAndExpand,
                LineBreakMode = LineBreakMode.WordWrap,
            };
            var zaehlerGanzzahl = new Label
            {
                Text = od.stand.Split(',')[0],
                TextColor = Color.FromArgb("#ffffff"),
                Margin = new Thickness(0, 0, 0, 0),
                Padding = new Thickness(5, 5, 5, 5),
                FontSize = 26,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                HorizontalTextAlignment = TextAlignment.End,
                BackgroundColor = Color.FromArgb("#000000"),
            };
            var zaehlerKomma = new Label
            {
                Text = ",",
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(0, 0, 0, 0),
                Padding = new Thickness(0, 5, 0, 5),
                FontSize = 22,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.End,
                BackgroundColor = Color.FromArgb("#e14b3a"),
            };
            var zaehlerKommazahl = new Label
            {
                Text = od.stand.Split(',').Length > 1 ? od.stand.Split(',')[1] : "000",
                TextColor = Color.FromArgb("#ffffff"),
                Margin = new Thickness(0),
                Padding = new Thickness(0, 5, 0, 5),
                FontSize = 26,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.End,
                BackgroundColor = Color.FromArgb("#e14b3a"),
            };
            var zaehlerEinheit = new Label
            {
                Text = od.einheit,
                TextColor = Color.FromArgb("#000000"),
                Margin = new Thickness(0),
                Padding = new Thickness(3, 5, 5, 5),
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.End,
                BackgroundColor = Color.FromArgb("#e14b3a"),
            };
            var hz = new StackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(-37, 5, -5, -5),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromArgb("#55ffcc00"),
            };

            var noticeStack = new StackLayout
            {
                HeightRequest = 44,
                WidthRequest = 44,
                BackgroundColor = Color.FromArgb("#143d63"),
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Spacing = 0,
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Children = { new Image
                {
                    Margin = new Thickness(3, 0, 0, 0),
                    HeightRequest = 28,
                    WidthRequest = 28,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    Source = model.imagesBase.Pen
                } }
            };

            var hzedit = new StackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };
            var editFrame = new Border()
            {
                HeightRequest = 45,
                WidthRequest = 45,
                BackgroundColor = Color.FromArgb("#143d63"),
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.End,
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, -3, 0),
                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
                Content = noticeStack
            };

            hz.Children.Add(zaehlerGanzzahl);
            hz.Children.Add(zaehlerKomma);
            hz.Children.Add(zaehlerKommazahl);
            hz.Children.Add(zaehlerEinheit);
            var typ2 = new Label
            {
                Text = "Standort: " + od.mieter + "\nStandort: " + od.mieterb
                + "\nEichjahr: " + od.eich,
                TextColor = Color.FromArgb("#ffcc00"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 16,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.StartAndExpand,
            };
            var last = new Label
            {
                Text = "Zuletzt:  " + JavaScriptDateConverter.Convert(long.Parse(od.standdatum)).ToString("dd.MM.yyyy (HH:mm)"),
                TextColor = Color.FromArgb("#00cc00"),
                VerticalTextAlignment = TextAlignment.End,
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 14,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.StartAndExpand,
            };
            var h = new StackLayout()
            {
                Padding = new Thickness(5, 5, 5, 5),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromArgb("#042d53"),
            };
            var v = new StackLayout()
            {
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };
            hzedit.Children.Add(last);
            hzedit.Children.Add(editFrame);
            v.Children.Add(lb_nr);
            v.Children.Add(lb_text);
            v.Children.Add(typ2);
            v.Children.Add(hzedit);
            //v.Children.Add(editFrame);
            v.Children.Add(hz);

            h.Children.Add(imageL);
            h.Children.Add(v);

            var mainFrame = new Border()
            {
                Padding = new Thickness(1, 1, 1, 1),
                Margin = new Thickness(38, 10, 0, 5),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Colors.Transparent,
                Content = h,
                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
                IsClippedToBounds = false,
                ClassId = "" + od.id,
            };

            if (func != null)
            {
                editFrame.GestureRecognizers.Clear();
                editFrame.GestureRecognizers.Add(new TapGestureRecognizer() { Command = func, CommandParameter = od });
            }

            return mainFrame;
        }

        public static Border GetObjektValueInfoElement(ObjektDataWSO od, AppModel model, ICommand func)
        {
            //var _prio = CalcOverdue(pos);
            var imageL = new Image
            {
                Margin = new Thickness(0, 0, 5, 0),
                HeightRequest = 22,
                WidthRequest = 22,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Source = GetImageFromTyp(od.typ, model),
            };
            var lb_nr = new Label()
            {
                Text = od.nr,
                TextColor = Color.FromArgb("#dddddd"),
                Margin = new Thickness(5, -5, -5, 3),
                Padding = new Thickness(5, 5, 5, 5),
                FontSize = 26,
                FontAttributes = FontAttributes.None,
                HorizontalTextAlignment = TextAlignment.End,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                LineBreakMode = LineBreakMode.WordWrap,
                CharacterSpacing = 1.05,
                BackgroundColor = Color.FromArgb("#143d63"),
            };
            var lb_text = new Label()
            {
                Text = od.text,
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(5, 0, 5, 5),
                FontSize = 16,
                HorizontalOptions = LayoutOptions.StartAndExpand,
                LineBreakMode = LineBreakMode.WordWrap,
            };
            var zaehlerGanzzahl = new Label
            {
                Text = od.stand.Split(',')[0],
                TextColor = Color.FromArgb("#ffffff"),
                Margin = new Thickness(0, 0, 0, 0),
                Padding = new Thickness(5, 5, 5, 5),
                FontSize = 26,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                HorizontalTextAlignment = TextAlignment.End,
                BackgroundColor = Color.FromArgb("#000000"),
            };
            var zaehlerKomma = new Label
            {
                Text = ",",
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(0, 0, 0, 0),
                Padding = new Thickness(0, 5, 0, 5),
                FontSize = 22,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.End,
                BackgroundColor = Color.FromArgb("#e14b3a"),
            };
            var zaehlerKommazahl = new Label
            {
                Text = od.stand.Split(',')[1],
                TextColor = Color.FromArgb("#ffffff"),
                Margin = new Thickness(0),
                Padding = new Thickness(0, 5, 0, 5),
                FontSize = 26,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.End,
                BackgroundColor = Color.FromArgb("#e14b3a"),
            };
            var zaehlerEinheit = new Label
            {
                Text = od.einheit,
                TextColor = Color.FromArgb("#000000"),
                Margin = new Thickness(0),
                Padding = new Thickness(3, 5, 5, 5),
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.End,
                BackgroundColor = Color.FromArgb("#e14b3a"),
            };
            var hz = new StackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(-37, 5, -5, -5),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromArgb("#55ffcc00"),
            };

            hz.Children.Add(zaehlerGanzzahl);
            hz.Children.Add(zaehlerKomma);
            hz.Children.Add(zaehlerKommazahl);
            hz.Children.Add(zaehlerEinheit);
            var typ2 = new Label
            {
                Text = "Standort: " + od.mieter + "\nStandort: " + od.mieterb
                + "\nEichjahr: " + od.eich,
                TextColor = Color.FromArgb("#ffcc00"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 16,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.StartAndExpand,
            };
            var h = new StackLayout()
            {
                Padding = new Thickness(5, 5, 5, 5),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromArgb("#042d53"),
            };
            var v = new StackLayout()
            {
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };
            v.Children.Add(lb_nr);
            v.Children.Add(lb_text);
            v.Children.Add(typ2);
            //v.Children.Add(editFrame);
            v.Children.Add(hz);

            h.Children.Add(imageL);
            h.Children.Add(v);

            var mainFrame = new Border()
            {
                Padding = new Thickness(1, 1, 1, 1),
                Margin = new Thickness(38, 10, 0, 5),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Colors.Transparent,
                Content = h,
                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
                IsClippedToBounds = false,
                ClassId = "" + od.id,
            };

            //if (func != null)
            //{
            //    editFrame.GestureRecognizers.Clear();
            //    editFrame.GestureRecognizers.Add(new TapGestureRecognizer() { Command = func, CommandParameter = od });
            //}

            return mainFrame;
        }

        private static CustomEntry entryanzahl;
        public static StackLayout EditObjektValueField(ObjektDataWSO od, AppModel model, ICommand func, ICommand funcLight, ICommand funcCam)
        {
            model.allAblesegrundStack = new Dictionary<int, Switch>();
            var noticeStack = new StackLayout
            {
                HeightRequest = 44,
                WidthRequest = 44,
                BackgroundColor = Color.FromArgb("#143d63"),
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Spacing = 0,
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Children = { new Image
                {
                    Margin = new Thickness(3, 0, 0, 0),
                    HeightRequest = 28,
                    WidthRequest = 28,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    Source = model.imagesBase.Refresh
                } }
            };

            var refreshFrame = new Border()
            {
                HeightRequest = 45,
                WidthRequest = 45,
                BackgroundColor = Color.FromArgb("#143d63"),
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, -3, 0),
                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
                Content = noticeStack
            };


            entryanzahl = new CustomEntry()
            {
                Margin = new Thickness(10, 0, 0, 0),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.StartAndExpand,
                TextColor = Colors.White,
                FontSize = 30,
                Keyboard = Keyboard.Numeric,
                HeightRequest = 60,
                MinimumWidthRequest = 100,
                Text = Utils.formatDEStr3(decimal.Parse(od.stand) > 0 ? decimal.Parse(od.stand) : 0),
                HorizontalTextAlignment = TextAlignment.End,
                BackgroundColor = Color.FromArgb("#333333")
            };

            od.firstStand = Utils.formatDEStr3(decimal.Parse(od.stand) > 0 ? decimal.Parse(od.stand) : 0);
            entryanzahl.ReturnCommandParameter = od;
            entryanzahl.Unfocused -= ValueChange;
            entryanzahl.Unfocused += ValueChange;
            entryanzahl.TextChanged -= ValueChanged;
            entryanzahl.TextChanged += ValueChanged;







            //var labelnewEdit = new Label
            //{
            //    Text = "Neuer Stand:",
            //    TextColor = Color.FromArgb("#ffffff"),
            //    Margin = new Thickness(5, 0, 5, 0),
            //    Padding = new Thickness(0),
            //    FontSize = 14,
            //    LineBreakMode = LineBreakMode.WordWrap,
            //    HorizontalOptions = LayoutOptions.StartAndExpand,
            //};
            var zaehlerEinheitEdit = new Label
            {
                Text = od.einheit,
                TextColor = Color.FromArgb("#ffffff"),
                Margin = new Thickness(0, 0, 5, 0),
                Padding = new Thickness(3, 5, 5, 5),
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.End,
                BackgroundColor = Color.FromArgb("#333333"),
            };
            var hanzahl = new StackLayout()
            {
                Padding = new Thickness(5, 5, 0, 5),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            var vanzahl = new StackLayout()
            {
                Padding = new Thickness(5, 5, 0, 5),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromArgb("#55042d53")
            };
            hanzahl.Children.Add(refreshFrame);

            hanzahl.Children.Add(entryanzahl);
            hanzahl.Children.Add(zaehlerEinheitEdit);

            //vanzahl.Children.Add(labelnewEdit);
            vanzahl.Children.Add(hanzahl);

            //var labelGrund = new Label
            //{
            //    Text = "Ablesegrund:",
            //    TextColor = Color.FromArgb("#ffffff"),
            //    Margin = new Thickness(5, 10, 5, 10),
            //    Padding = new Thickness(0),
            //    FontSize = 14,
            //    LineBreakMode = LineBreakMode.WordWrap,
            //    HorizontalOptions = LayoutOptions.Start,
            //};

            refreshFrame.GestureRecognizers.Clear();
            refreshFrame.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(() =>
                {
                    entryanzahl.Text = Utils.formatDEStr3(decimal.Parse(od.stand) > 0 ? decimal.Parse(od.stand) : 0);
                    od.firstStand = Utils.formatDEStr3(decimal.Parse(od.stand) > 0 ? decimal.Parse(od.stand) : 0);
                })
            });

            var l1 = new Label { Text = "Jahresendablesung", TextColor = Color.FromArgb("#cccccc"), WidthRequest = 125, MinimumWidthRequest = 125, VerticalOptions = LayoutOptions.Center };
            var r1 = new Switch { IsToggled = true, InputTransparent = true };
            model.allAblesegrundStack.Add(1, r1);
            var h1 = new StackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 5,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children = { l1, r1 }
            };
            h1.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(() =>
                {
                    radioChange2(model, od, l1, 1);
                })
            });
            var l2 = new Label { Text = "Ablesung", TextColor = Color.FromArgb("#cccccc"), WidthRequest = 95, MinimumWidthRequest = 95, VerticalOptions = LayoutOptions.Center };
            var r2 = new Switch { IsToggled = false, InputTransparent = true };
            model.allAblesegrundStack.Add(2, r2);
            var h2 = new StackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 5,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children = { l2, r2 }
            };
            h2.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(() =>
                {
                    radioChange2(model, od, l2, 2);
                })
            });
            var l3 = new Label { Text = "Erstablesung", TextColor = Color.FromArgb("#cccccc"), WidthRequest = 125, MinimumWidthRequest = 125, VerticalOptions = LayoutOptions.Center };
            var r3 = new Switch { IsToggled = false, InputTransparent = true };
            model.allAblesegrundStack.Add(3, r3);
            var h3 = new StackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 5,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children = { l3, r3 }
            };
            h3.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(() =>
                {
                    radioChange2(model, od, l3, 3);
                })
            });
            var l4 = new Label { Text = "Zwischenstand", TextColor = Color.FromArgb("#cccccc"), WidthRequest = 95, MinimumWidthRequest = 95, VerticalOptions = LayoutOptions.Center };
            var r4 = new Switch { IsToggled = false, InputTransparent = true };
            model.allAblesegrundStack.Add(4, r4);
            var h4 = new StackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 5,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children = { l4, r4 }
            };
            h4.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(() =>
                {
                    radioChange2(model, od, l4, 4);
                })
            });

            var l5 = new Label { Text = "Zählerwechsel", TextColor = Color.FromArgb("#cccccc"), WidthRequest = 125, MinimumWidthRequest = 125, VerticalOptions = LayoutOptions.Center };
            var r5 = new Switch { IsToggled = false, InputTransparent = true };
            model.allAblesegrundStack.Add(5, r5);
            var h5 = new StackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 5,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children = { l5, r5 }
            };
            h5.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(() =>
                {
                    radioChange2(model, od, l5, 5);
                })
            });
            var l6 = new Label { Text = "Auffüllung", TextColor = Color.FromArgb("#cccccc"), WidthRequest = 95, MinimumWidthRequest = 95, VerticalOptions = LayoutOptions.Center };
            var r6 = new Switch { IsToggled = false, InputTransparent = true };
            model.allAblesegrundStack.Add(6, r6);
            var h6 = new StackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 5,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children = { l6, r6 }
            };
            h6.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(() =>
                {
                    radioChange2(model, od, l6, 6);
                })
            });

            var l7 = new Label { Text = "Mieter-/Nutzerwechsel", TextColor = Color.FromArgb("#cccccc"), WidthRequest = 125, MinimumWidthRequest = 125, VerticalOptions = LayoutOptions.Center };
            var r7 = new Switch { IsToggled = false, InputTransparent = true };
            model.allAblesegrundStack.Add(7, r7);
            var h7 = new StackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 5,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children = { l7, r7 }
            };
            h7.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(() =>
                {
                    radioChange2(model, od, l7, 7);
                })
            });

            var l8 = new Label { Text = "Kontrolle", TextColor = Color.FromArgb("#cccccc"), WidthRequest = 95, MinimumWidthRequest = 95, VerticalOptions = LayoutOptions.Center };
            var r8 = new Switch { IsToggled = false, InputTransparent = true };
            model.allAblesegrundStack.Add(8, r8);
            var h8 = new StackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 5,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children = { l8, r8 }
            };
            h8.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(() =>
                {
                    radioChange2(model, od, l8, 8);
                })
            });

            var m1 = new StackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(5, 2, 5, 2),
                Spacing = 10,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children = { h1, h2 }
            };
            var m2 = new StackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(5, 2, 5, 2),
                Spacing = 10,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children = { h3, h4 }
            };
            var m3 = new StackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(5, 2, 5, 2),
                Spacing = 10,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children = { h5, h6 }
            };
            var m4 = new StackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(5, 2, 5, 2),
                Spacing = 10,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children = { h7, h8 }
            };

            var stackRadio = new StackLayout
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0, 5, 0, 5),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Colors.Transparent,
                Children = { m1, m2, m3, m4 }
            };

            vanzahl.Children.Add(stackRadio);



            var stackBtn = new StackLayout
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Colors.Transparent
            };


            var saveFrame = new Border()
            {
                BackgroundColor = Color.FromArgb("#143d63"),
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Padding = new Thickness(1),
                Margin = new Thickness(8, 15, 8, 10),
                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
                Content = new StackLayout
                {
                    Padding = new Thickness(2, 2, 10, 2),
                    Margin = new Thickness(0),
                    HeightRequest = 40,
                    BackgroundColor = Color.FromArgb("#04532d"),
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Orientation = StackOrientation.Horizontal,
                    Children =
                    {
                        new Image
                        {
                            Margin = new Thickness(5,0,5,0),
                            HeightRequest = 30,
                            WidthRequest = 30,
                            Source = model.imagesBase.CheckWhite,
                            VerticalOptions = LayoutOptions.CenterAndExpand,
                        },
                        new Label
                        {
                            Text = "JETZT ÜBERNEHMEN", FontSize = 16, TextColor = Colors.White,
                            Padding = new Thickness(0),
                            Margin = new Thickness(0,2,0,0),
                            VerticalOptions = LayoutOptions.CenterAndExpand,
                        }
                    }
                }
            };

            if (func != null)
            {
                saveFrame.GestureRecognizers.Clear();
                saveFrame.GestureRecognizers.Add(new TapGestureRecognizer() { Command = func, CommandParameter = od });
            }

            var flashlightFrame = new Border()
            {
                BackgroundColor = Color.FromArgb("#041d43"),
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.EndAndExpand,
                Padding = new Thickness(1),
                Margin = new Thickness(0, 17, 5, 10),
                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
                Content = new StackLayout
                {
                    BackgroundColor = Color.FromArgb("#042d53"),
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Padding = new Thickness(0),
                    Margin = new Thickness(0),
                    HeightRequest = 40,
                    WidthRequest = 40,
                    Spacing = 0,
                    Children = { new Image
                        {
                            VerticalOptions = LayoutOptions.CenterAndExpand,
                            HorizontalOptions = LayoutOptions.CenterAndExpand,
                            Margin = new Thickness(0),
                            HeightRequest = 30,
                            WidthRequest = 30,
                            Source = AppModel.Instance.imagesBase.Flashlight
                        }
                    }
                }
            };

            if (funcLight != null)
            {
                flashlightFrame.GestureRecognizers.Clear();
                flashlightFrame.GestureRecognizers.Add(new TapGestureRecognizer() { Command = funcLight, CommandParameter = od });
            }
            var camFrame = new Border()
            {
                BackgroundColor = Color.FromArgb("#041d43"),
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.End,
                Padding = new Thickness(1),
                Margin = new Thickness(5, 17, 5, 10),
                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
                Content = new StackLayout
                {
                    BackgroundColor = Color.FromArgb("#042d53"),
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Padding = new Thickness(0),
                    Margin = new Thickness(0),
                    HeightRequest = 40,
                    WidthRequest = 40,
                    Spacing = 0,
                    Children = { new Image
                        {
                            VerticalOptions = LayoutOptions.CenterAndExpand,
                            HorizontalOptions = LayoutOptions.CenterAndExpand,
                            Margin = new Thickness(0),
                            HeightRequest = 30,
                            WidthRequest = 30,
                            Source = AppModel.Instance.imagesBase.Cam
                        }
                    }
                }
            };

            if (funcCam != null)
            {
                camFrame.GestureRecognizers.Clear();
                camFrame.GestureRecognizers.Add(new TapGestureRecognizer() { Command = funcCam, CommandParameter = od });
            }


            stackBtn.Children.Add(saveFrame);
            stackBtn.Children.Add(flashlightFrame);
            stackBtn.Children.Add(camFrame);

            vanzahl.Children.Add(stackBtn);

            var stack = new StackLayout
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Colors.Transparent,
                Children = { vanzahl
                }
            };
            return stack;
        }

        public static void radioChange2(AppModel model, ObjektDataWSO od, Label l, int i)
        {
            od.ablesegrund = l.Text;
            model.allAblesegrundStack[1].IsToggled = false;
            model.allAblesegrundStack[2].IsToggled = false;
            model.allAblesegrundStack[3].IsToggled = false;
            model.allAblesegrundStack[4].IsToggled = false;
            model.allAblesegrundStack[5].IsToggled = false;
            model.allAblesegrundStack[6].IsToggled = false;
            model.allAblesegrundStack[7].IsToggled = false;
            model.allAblesegrundStack[8].IsToggled = false;
            model.allAblesegrundStack[i].IsToggled = true;
        }





        public static void ValueChanged(object sender, TextChangedEventArgs e)
        {
            var entry = (CustomEntry)sender;
            var od = (ObjektDataWSO)entry.ReturnCommandParameter;
            entryanzahl.TextChanged -= ValueChanged;
            try
            {
                char[] allowedChars = { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };
                string text = entryanzahl.Text.Replace(".", "").Replace(",", "");
                if (String.IsNullOrWhiteSpace(text)) { text = "0,000"; }
                var t = decimal.Parse(text) / 1000;
                entryanzahl.Text = Utils.formatDEStr3(t);
                od.firstStand = entryanzahl.Text;
            }
            finally
            {
                entryanzahl.TextChanged += ValueChanged;
            }
        }
        public static void ValueChange(object sender, FocusEventArgs e)
        {
            var entry = (CustomEntry)sender;
            var od = (ObjektDataWSO)entry.ReturnCommandParameter;
            try
            {
                entry.Text = (entry.Text.Substring(0, 1) == "." ? entry.Text.Substring(1) : entry.Text);
                entry.Text = (entry.Text.Substring(0, 1) == "," ? "0" + entry.Text : entry.Text);
                entry.Text = Utils.formatDEStr3(decimal.Parse(entry.Text));
            }
            catch (Exception)
            {
                od.firstStand = "1,000";
                return;
            }
            od.firstStand = entry.Text;
        }

        public static StackLayout GetTypInfoElement(string typ, AppModel model)
        {
            return new StackLayout
            {
                Padding = new Thickness(5, 5, 5, 5),
                Margin = new Thickness(0, 10, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromArgb("#90144d73"),
                Children = {
                    new Image
                    {
                        Margin = new Thickness(0, 0, 10, 0),
                        HeightRequest = 28,
                        WidthRequest = 28,
                        VerticalOptions = LayoutOptions.Start,
                        HorizontalOptions = LayoutOptions.Start,
                        Source = GetImageFromTyp(typ, model)
                    },
                    new Label {
                        Text = typ,
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        FontSize = 18,
                        TextColor = Colors.White,
                        HorizontalTextAlignment = TextAlignment.Start,
                        Margin = new Thickness(0, 0, 0, 0),
                        Padding = new Thickness(0, 0, 0, 0)
                    }
                }
            };
        }

        public static ImageSource GetImageFromTyp(string typ, AppModel model)
        {
            switch (typ)
            {
                case "Strom":
                    return model.imagesBase.ZStrom;

                case "Gas":
                    return model.imagesBase.ZGas;

                case "Wasser":
                    return model.imagesBase.ZWasser;

                case "Öl":
                    return model.imagesBase.ZOil;

                case "Heizung":
                    return model.imagesBase.ZHeizung;

                case "Fernwärme":
                    return model.imagesBase.ZFernwaerme;

                default:
                    return model.imagesBase.ObjectValues;
            }
        }

        public static bool ToUploadStack(AppModel model, ObjektDataWSO od)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                var json = JsonConvert.SerializeObject(od);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, json);
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/objectvalueupload/");
                if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/objectvalueupload/" + od.ticks + ".ipm");
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
                AppModel.Logger.Error(ex);
                return false;
            }
        }

        public static int CountFromStack()
        {
            try
            {
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/objectvalueupload/");
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
        public static List<ObjektDataWSO> LoadAllFromUploadStack(AppModel model)
        {
            List<ObjektDataWSO> list = new List<ObjektDataWSO>();
            try
            {
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/objectvalueupload/");
                if (Directory.Exists(directoryPath))
                {
                    //Directory.Delete(directoryPath,true);
                    var files = Directory.GetFiles(directoryPath);
                    if (files != null && files.Length > 0)
                    {
                        files.ToList().ForEach(file =>
                        {
                            list.Add(ObjektDataWSO.LoadFromUploadStack(model, file));
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error(ex);
            }
            return list;
        }

        private static ObjektDataWSO LoadFromUploadStack(AppModel model, string filename)
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
                    return JsonConvert.DeserializeObject<ObjektDataWSO>(json);
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

        public static bool DeleteFromUploadStack(AppModel model, ObjektDataWSO od)
        {
            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/objectvalueupload/" + od.ticks + ".ipm");
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