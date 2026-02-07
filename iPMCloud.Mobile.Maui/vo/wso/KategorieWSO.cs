using iPMCloud.Mobile.vo;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;

namespace iPMCloud.Mobile
{
    [Serializable]
    public class KategorieWSO
    {
        public Int32 id = 0;
        //public Int32 gruppeid 
        //public Int32 objektid 
        //public Int32 auftragid 
        //public int indexa 
        public String type = "0";
        public String titel = "";//titel varchar(100) 
        public String titelLang = "";//titel varchar(100) 
        public int del = 0;//TINYINT(4)
        //public String lastchange  = "";//varchar(50)
        //public int mobil  = 1;//gruppeid bigint(20)
        public String art = "Leistung";//varchar(45) 
        public String notiz = "";//varchar(500) 

        public String saison = "0";//varchar(2) 
        //public String startsaison  = "0";//varchar(60) 
        //public String endesaison  = "0";//varchar(60) 
        public int winterservice = 0;//gruppeid bigint(20)


        public List<LeistungWSO> leistungen = new List<LeistungWSO>();

        public string GetMobileText()
        {
            if (!String.IsNullOrWhiteSpace(titelLang) && AppModel.Instance.AppControll.translation)
            {
                return AppModel.Instance.Lang.lang == "de" ? titel : titelLang;
            }
            else
            {
                return titel;
            }
        }


        public Boolean aussetzen = false;
        public Boolean selected = false;
        public String kategorieTitel = "";
        public Boolean inwork = false;
        public Boolean faelligeLeistungen = false;

        public bool isInTodoVisible = true;
        public double prio = 100000000;

        public KategorieWSO() { }








        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp / 1000).ToLocalTime();
            return dtDateTime;
        }

        public static double CalcOverdue(KategorieWSO cat, AppModel model)
        {
            double _prio = 100000000;
            cat.leistungen.ForEach(l =>
            {
                if (long.Parse(l.lastwork) > 1 && l.timevaldays > 0)
                {
                    l.prio = Prio.GetLeistungPrio(l, model);
                    _prio = Math.Min(_prio, l.prio.days);
                }
            });
            return _prio;
        }

        public static double CalcOverdue(AuftragWSO order, AppModel model)
        {
            double _prio = 100000000;
            order.kategorien.ForEach(c =>
            {
                c.leistungen.ForEach(l =>
                {
                    //if (long.Parse(l.lastwork) > 1 && l.timevaldays > 0)
                    //{
                    //    l.prio = LeistungPrio.GetLeistungPrio(l, model);
                    _prio = Math.Min(_prio, l.prio.days);
                    //}
                });
            });
            return _prio;
        }

        public static StackLayout GetCategoryListView(AppModel model, ICommand func)
        {
            var stack = new StackLayout
            {
                Padding = new Thickness(5, 0, 5, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };
            bool isObjektInPlan = AppModel.Instance.Plan_ObjekteThisWeek.Contains(model.LastSelectedOrder.objektid);

            model._showall_OrderCategory_frame.IsVisible = !AppModel.Instance.AppControll.ignoreKategorieFilterByPerson && (isObjektInPlan || AppModel.Instance.Plan_KatThisWeek.Count > 0);

            model.LastSelectedOrder.kategorien.ForEach(c =>
            {
                if (model._showall_OrderCategory || !AppModel.Instance.AppControll.filterKategories || !isObjektInPlan || AppModel.Instance.AppControll.ignoreKategorieFilterByPerson)
                {
                    // Show all
                    stack.Children.Add(GetCategoryCardView(c, model, func));
                }
                else
                {
                    // Filtered Kategories
                    if ((AppModel.Instance.Plan_KatThisWeek.Count > 0 && AppModel.Instance.Plan_KatThisWeek.Contains(c.id)) || c.art == "Produkt")
                    {
                        stack.Children.Add(GetCategoryCardView(c, model, func));
                    }
                }
            });
            return stack;
        }
        public static Frame GetCategoryCardView(KategorieWSO cat, AppModel model, ICommand func)
        {
            var _prio = CalcOverdue(cat, model);
            var imageL = new Image
            {
                Margin = new Thickness(0, 0, 5, 0),
                HeightRequest = 32,
                WidthRequest = 32,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Source = (cat.art == "Leistung" ? model.imagesBase.KLSymbol :
                            (cat.art == "Produkt" ? model.imagesBase.KPSymbol :
                                (cat.art == "Texte" ? model.imagesBase.KTSymbol :
                                (cat.art == "Check" ? model.imagesBase.KCSymbol :
                                    model.imagesBase.Quest))))
            };
            var imgInfo = new Image
            {
                Margin = new Thickness(2),
                HeightRequest = 24,
                WidthRequest = 24,
                Opacity = 0.4,
                Source = model.imagesBase.InfoCircle,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start,
            };
            var hInfo = new StackLayout()
            {
                Padding = new Thickness(5, 0, 5, 2),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start,
                BackgroundColor = Color.Transparent,
                IsVisible = !String.IsNullOrWhiteSpace(cat.notiz)
            };
            hInfo.Children.Add(imgInfo);
            hInfo.GestureRecognizers.Clear();
            var t_imgInfo = new TapGestureRecognizer();
            t_imgInfo.Tapped += (object o, EventArgs ev) => { AppModel.Instance.MainPage.OpenKategorieInfoDialog(cat); };
            hInfo.GestureRecognizers.Add(t_imgInfo);

            var lb = new Label()
            {
                Text = cat.GetMobileText(),
                TextColor = Color.FromHex("#cccccc"),
                Margin = new Thickness(7, 0, 5, 1),
                FontSize = 16,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                LineBreakMode = LineBreakMode.WordWrap,
            };
            var direct = new Label
            {
                Text = "Direkterfassung",
                TextColor = Color.FromHex("#ffcc00"),
                Margin = new Thickness(7, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.TailTruncation,
                HorizontalOptions = LayoutOptions.Start,
                IsVisible = cat.type == "1",
            };

            string typLb = (cat.saison == "0" ? "" : (cat.saison == "1" ? "Saison: Sommer" : (cat.saison == "2" ? "Saison: Winter" : (cat.saison == "3" ? "Saison: Benutzedefiniert" : ""))));
            var typ = new Label
            {
                Text = typLb,
                TextColor = Color.FromHex("#999999"),
                Margin = new Thickness(7, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.TailTruncation,
                HorizontalOptions = LayoutOptions.Start
            };
            var h = new StackLayout()
            {
                Padding = new Thickness(5, 5, 5, 5),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromHex("#042d53"),
            };
            var v = new StackLayout()
            {
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };
            v.Children.Add(lb);
            v.Children.Add(direct);
            if (!String.IsNullOrWhiteSpace(typLb))
            {
                v.Children.Add(typ);
            }

            var badge = new Frame
            {
                BackgroundColor = Color.FromHex(_prio < 0 ? "#ff0000" : (_prio < 1 ? "#ffcc00" : "#009900")),
                IsClippedToBounds = true,
                HasShadow = true,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(-14, -3, 0, 0),
                Padding = new Thickness(4, 2, 4, 2),
                CornerRadius = 5,
                IsVisible = (_prio < 1360),
                Content = new Label
                {
                    Text = Int32.Parse("" + _prio).ToString(),
                    Margin = new Thickness(0, 0, 0, 0),
                    Padding = new Thickness(0, 0, 0, 0),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 11,
                    TextColor = Color.White,
                    FontAttributes = FontAttributes.Bold,
                    MinimumWidthRequest = 50,
                    LineBreakMode = LineBreakMode.NoWrap,
                    HorizontalTextAlignment = TextAlignment.Center
                }
            };
            h.Children.Add(imageL);
            h.Children.Add(badge);
            h.Children.Add(v);
            h.Children.Add(hInfo);

            var mainFrame = new Frame()
            {
                Padding = new Thickness(1, 1, 1, 1),
                Margin = new Thickness(0, 15, 0, 5),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromHex("#041d43"),
                Content = h,
                CornerRadius = 0,
                HasShadow = true,
                IsClippedToBounds = true,
                ClassId = "" + cat.id,
            };

            if (func != null)
            {
                mainFrame.GestureRecognizers.Clear();
                mainFrame.GestureRecognizers.Add(new TapGestureRecognizer() { Command = func, CommandParameter = cat });
            }


            return mainFrame;
        }

        public static StackLayout GetCategoryWinterCardView(KategorieWSO cat)
        {
            var imageL = new Image
            {
                Margin = new Thickness(5, 0, 0, 0),
                HeightRequest = 24,
                WidthRequest = 24,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Source = (cat.art == "Leistung" ? AppModel.Instance.imagesBase.KLSymbol :
                            (cat.art == "Produkt" ? AppModel.Instance.imagesBase.KPSymbol :
                                (cat.art == "Texte" ? AppModel.Instance.imagesBase.KTSymbol :
                                (cat.art == "Check" ? AppModel.Instance.imagesBase.KCSymbol :
                                    AppModel.Instance.imagesBase.Quest))))
            };
            var lb = new Label()
            {
                Text = cat.GetMobileText(),
                TextColor = Color.FromHex("#cccccc"),
                Margin = new Thickness(5, 0, 5, 1),
                FontSize = 16,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                LineBreakMode = LineBreakMode.WordWrap,
            };
            var h = new StackLayout()
            {
                Padding = new Thickness(5, 5, 5, 5),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromHex("#cc042d53"),
            };

            h.Children.Add(imageL);
            h.Children.Add(lb);

            var mainFrame = new Frame()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(40, 1, 0, 1),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.Transparent,
                Content = h,
                CornerRadius = 0,
                HasShadow = false,
                IsClippedToBounds = true,
                ClassId = "" + cat.id,
            };

            var container = new StackLayout
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                IsVisible = true,
            };

            //mainFrame.GestureRecognizers.Clear();
            //mainFrame.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command<StackLayout>(ShowPositionContainer), CommandParameter = container });

            var wrapper = new StackLayout
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children = { mainFrame, container }
            };

            return wrapper;
        }


        public static StackLayout GetCategoryTodoCardView(KategorieWSO cat, AppModel model, double prio, bool onlyText)
        {
            var imageL = new Image
            {
                Margin = new Thickness(5, 0, 0, 0),
                HeightRequest = 24,
                WidthRequest = 24,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Source = (cat.art == "Leistung" ? model.imagesBase.KLSymbol :
                            (cat.art == "Produkt" ? model.imagesBase.KPSymbol :
                                (cat.art == "Texte" ? model.imagesBase.KTSymbol :
                                (cat.art == "Check" ? model.imagesBase.KCSymbol :
                                    model.imagesBase.Quest))))
            };
            var lb = new Label()
            {
                Text = cat.GetMobileText(),
                TextColor = Color.FromHex("#cccccc"),
                Margin = new Thickness(5, 0, 5, 1),
                FontSize = 16,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                LineBreakMode = LineBreakMode.WordWrap,
            };
            var h = new StackLayout()
            {
                Padding = new Thickness(5, 5, 5, 5),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = onlyText ? Color.FromHex("#cc04532d") : Color.FromHex("#cc042d53"),
            };

            Frame warn = new Frame { IsVisible = false };
            if (prio < 1360)
            {
                warn = AuftragWSO.GetTodoCountWarningSmall(prio);
            }

            if (onlyText)
            {
                h.Children.Add(imageL);
            }
            else
            {
                h.Children.Add(warn);
                h.Children.Add(imageL);
            }
            h.Children.Add(lb);

            var mainFrame = new Frame()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(40, 1, 0, 1),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.Transparent,
                Content = h,
                CornerRadius = 0,
                HasShadow = false,
                IsClippedToBounds = true,
                ClassId = "" + cat.id,
            };

            var container = new StackLayout
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                IsVisible = false,
            };

            mainFrame.GestureRecognizers.Clear();
            mainFrame.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command<StackLayout>(ShowPositionContainer), CommandParameter = container });

            var wrapper = new StackLayout
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children = { mainFrame, container }
            };

            return wrapper;
        }

        public static void ShowPositionContainer(StackLayout value)
        {
            value.IsVisible = !value.IsVisible;
        }

        public static StackLayout GetCategoryWarning(double count, AppModel model)
        {
            var warnText = count < 0 ? "Hier gibt es überfällige Arbeiten!" : (count < 1 ? "Heute sind fällige Arbeiten!" : "Arbeiten erst fällig in (Tagen)");
            var warnColor = Color.FromHex(count < 0 ? "#50ff0000" : (count < 1 ? "#50ffcc00" : "#50009900"));


            var imageL = new Image
            {
                Margin = new Thickness(0, 0, 5, 0),
                HeightRequest = 18,
                WidthRequest = 18,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start,
                Source = model.imagesBase.WarnTriangleYellow
            };
            var lb = new Label()
            {
                Text = warnText,
                TextColor = Color.FromHex("#ffffff"),
                Margin = new Thickness(5, 0, 5, 0),
                FontSize = 12,
                HorizontalOptions = LayoutOptions.StartAndExpand,
                VerticalOptions = LayoutOptions.Center,
                LineBreakMode = LineBreakMode.WordWrap,
            };

            var badge = new Frame
            {
                BackgroundColor = Color.FromHex(count < 0 ? "#ff0000" : (count < 1 ? "#ffcc00" : "#009900")),
                IsClippedToBounds = true,
                HasShadow = true,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0),
                Padding = new Thickness(4, 2, 4, 2),
                CornerRadius = 5,
                Content = new Label
                {
                    Text = Int32.Parse("" + count).ToString(),
                    Margin = new Thickness(0, 0, 0, 0),
                    Padding = new Thickness(0, 0, 0, 0),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 12,
                    TextColor = Color.White,
                    HorizontalTextAlignment = TextAlignment.Center
                }
            };

            var h = new StackLayout()
            {
                Padding = new Thickness(5, 1, 5, 1),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Start,
                BackgroundColor = warnColor,
            };
            h.Children.Add(imageL);
            h.Children.Add(lb);
            h.Children.Add(badge);
            return h;
        }
        public static StackLayout GetCategoryInfoElement(KategorieWSO c, AppModel model)
        {
            return new StackLayout
            {
                Padding = new Thickness(5, 5, 5, 5),
                Margin = new Thickness(34, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromHex("#90144d73"),
                Children = {
                    new Image {
                        Margin = new Thickness(0, 0, 10, 0),
                        HeightRequest = 30,
                        WidthRequest = 30,
                        VerticalOptions = LayoutOptions.StartAndExpand,
                        Source = (c.art == "Leistung" ? model.imagesBase.KLSymbol :
                                 (c.art == "Produkt" ? model.imagesBase.KPSymbol :
                                 (c.art == "Texte" ? model.imagesBase.KTSymbol :
                                 (c.art == "Check" ? model.imagesBase.KCSymbol : model.imagesBase.Quest))))
                    },
                    new Label {
                        Text = c.GetMobileText(),// + " \nNr.: " + c.id,// + "  Art: " + c.art + "  Saison: " + c.saison,
                        VerticalOptions = LayoutOptions.StartAndExpand,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        FontSize = 14,
                        TextColor = Color.White,
                        HorizontalTextAlignment = TextAlignment.Start,
                        Margin = new Thickness(0, 0, 0, 0),
                        Padding = new Thickness(0, 0, 0, 0)
                    }
                }
            };
        }


        public static StackLayout GetCategoryAgainListView(AppModel model, ICommand func)
        {
            var art = model.posAgain == 1 ? "Leistung" : "Produkt";
            var stack = new StackLayout
            {
                Padding = new Thickness(5, 0, 5, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };


            bool isObjektInPlan = AppModel.Instance.Plan_ObjekteThisWeek.Contains(model.LastSelectedOrderAgain.objektid);

            model._showall_again_OrderCategory_frame.IsVisible = !AppModel.Instance.AppControll.ignoreKategorieFilterByPerson &&
                art != "Produkt" && (isObjektInPlan || AppModel.Instance.Plan_KatThisWeek.Count > 0);

            model.LastSelectedOrderAgain.kategorien.ForEach(c =>
            {
                if (model._showall_again_OrderCategory || !AppModel.Instance.AppControll.filterKategories ||
                    !isObjektInPlan || AppModel.Instance.AppControll.ignoreKategorieFilterByPerson || art == "Produkt")
                {
                    if (c.art == art)
                    {
                        stack.Children.Add(GetCategoryCardAgainView(c, model, func));
                    }
                }
                else
                {
                    if ((AppModel.Instance.Plan_KatThisWeek.Count > 0 && AppModel.Instance.Plan_KatThisWeek.Contains(c.id)))
                    {
                        stack.Children.Add(GetCategoryCardView(c, model, func));
                    }
                }
            });
            return stack;
        }
        public static Frame GetCategoryCardAgainView(KategorieWSO cat, AppModel model, ICommand func)
        {
            var _prio = CalcOverdue(cat, model);
            var imageL = new Image
            {
                Margin = new Thickness(0, 0, 5, 0),
                HeightRequest = 32,
                WidthRequest = 32,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Source = (cat.art == "Leistung" ? model.imagesBase.KLSymbol :
                            (cat.art == "Produkt" ? model.imagesBase.KPSymbol :
                                (cat.art == "Texte" ? model.imagesBase.KTSymbol :
                                (cat.art == "Check" ? model.imagesBase.KCSymbol :
                                    model.imagesBase.Quest))))
            };
            var lb = new Label()
            {
                Text = cat.GetMobileText(),
                TextColor = Color.FromHex("#cccccc"),
                Margin = new Thickness(5, 0, 5, 1),
                FontSize = 16,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                LineBreakMode = LineBreakMode.WordWrap,
            };
            var id = new Label
            {
                Text = "Nr.: " + cat.id,
                TextColor = Color.FromHex("#999999"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                MinimumWidthRequest = 85,
                LineBreakMode = LineBreakMode.TailTruncation,
                HorizontalOptions = LayoutOptions.End,
            };
            var direct = new Label
            {
                Text = "Direkterfassung",
                TextColor = Color.FromHex("#ffcc00"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.TailTruncation,
                HorizontalOptions = LayoutOptions.Start,
                IsVisible = cat.type == "1",
            };
            var typ = new Label
            {
                Text = "Art: " + cat.art + "   " + (cat.saison == "0" ? "" : (cat.saison == "1" ? "Saison: Sommer" : (cat.saison == "2" ? "Saison: Winter" : (cat.saison == "3" ? "Saison: Benutzedefiniert" : "")))),
                TextColor = Color.FromHex("#999999"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.TailTruncation,
                HorizontalOptions = LayoutOptions.Start,
            };
            var h = new StackLayout()
            {
                Padding = new Thickness(5, 5, 5, 5),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromHex("#042d53"),
            };
            var v = new StackLayout()
            {
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };
            v.Children.Add(lb);
            v.Children.Add(direct);
            v.Children.Add(typ);

            if (_prio < 1360) // kleiner 4 Jahre
            {
                var warn = GetCategoryWarning(_prio, model);
                warn.Margin = new Thickness(-35, 5, -40, 0);
                v.Children.Add(warn);
            }
            h.Children.Add(imageL);
            h.Children.Add(v);
            h.Children.Add(id);

            var mainFrame = new Frame()
            {
                Padding = new Thickness(1, 1, 1, 1),
                Margin = new Thickness(0, 15, 0, 5),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromHex("#041d43"),
                Content = h,
                CornerRadius = 0,
                HasShadow = true,
                IsClippedToBounds = true,
                ClassId = "" + cat.id,
            };

            if (func != null)
            {
                mainFrame.GestureRecognizers.Clear();
                mainFrame.GestureRecognizers.Add(new TapGestureRecognizer() { Command = func, CommandParameter = cat });
            }


            return mainFrame;
        }


    }


    public class KategorieParameter
    {
        public StackLayout stack;
        public KategorieWSO cat;
        public AppModel model;
    }
}