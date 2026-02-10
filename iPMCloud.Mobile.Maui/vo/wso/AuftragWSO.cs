using iPMCloud.Mobile.vo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace iPMCloud.Mobile
{
    [Serializable]
    public class AuftragWSO : ICloneable
    {
        public Int32 id;
        //public Int32 gruppeid 
        public Int32 objektid;
        //public Int32 personid 
        public string bezeichnung;
        public string bezeichnungLang;
        //public String verguetung  = "0,00";
        public string typ;
        public string status;
        //public String datum 
        //public String enddatum 
        public int del;
        //public String lastchange 
        //public String p1 
        //public String p2 
        //public String p3 
        //public String p4 
        //public String berechnung  = ""; // Angebotsberechnung / Pauschal  oder Standard
        //public int berechnunginterval  = 1; // Angebotsberechnungintervall / 1w(52), 14t(26), monatlich(12), viertj.(4), halbj.(2), jährl.(1)
        //public Int32 refid  = 0;
        //public String refname  = "";
        //public String usetime  = ""; // für checklisten ein Arbeitszeit definieren der Abarbeitung dieser Liste die dann als Leistung ins Protokoll eingetragen wird und somit auch in die Zeiterfassng
        //public int gesperrt  = 0;//TINYINT(4)
        //public String gesperrtgrund  = "";
        //public Int32 bindingid  = 0;
        //public Int32 adressid  = 0;
        //public String gueltigbis  = "";
        //public String ausfuehrungam  = "";
        //public String berechnungzum  = "Zum ende eines Monats"; // Berechung Zum ...
        //public int berechnungvorher  = 5; // Berechung Zum ...
        //public int zahlung  = 1; // Berechung Zum ...
        //public string saison  = null;


        public string GetMobileText()
        {
            if (!String.IsNullOrWhiteSpace(bezeichnungLang) && AppModel.Instance.AppControll.translation)
            {
                return AppModel.Instance.Lang.lang == "de" ? bezeichnung : bezeichnungLang;
            }
            else
            {
                return bezeichnung;
            }
        }

        public List<KategorieWSO> kategorien = new List<KategorieWSO>();

        public bool isInTodoVisible = true;
        public double prio = 100000000;

        public AuftragWSO() { }

        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp / 1000).ToLocalTime();
            return dtDateTime;
        }

        public static BuildingWSO CalcBuildingdue(BuildingWSO building, AppModel model, bool showAll = false)
        {
            double _prio = 100000000;
            double _prioMax = 100000000;
            int isBuildingInTodoVisible = 0;
            building.ArrayOfAuftrag.ForEach(order =>
            {
                int isKategorieInTodoVisible = 0;
                order.kategorien.ForEach(c =>
                {
                    int isLeistungInTodoVisible = 0;
                    c.leistungen.ForEach(l =>
                    {
                        if (l.nichtpauschal == 0 && l.timevaldays > 0)//long.Parse(l.lastwork) > 1 && l.timevaldays > 0)
                        {
                            l.prio = Prio.GetLeistungPrio(l, model);
                            _prio = Math.Min(_prio, l.prio.days);
                            _prioMax = Math.Max(_prio, l.prio.days);
                            l.isInTodoVisible = showAll || l.prio.days >= -1000 && l.prio.days <= 1000;
                            isLeistungInTodoVisible += l.isInTodoVisible ? 1 : 0;
                        }
                        else
                        {
                            l.isInTodoVisible = false;
                        }
                    });
                    c.isInTodoVisible = showAll || isLeistungInTodoVisible > 0;
                    isKategorieInTodoVisible += c.isInTodoVisible ? 1 : 0;
                });
                order.isInTodoVisible = showAll || isKategorieInTodoVisible > 0;
                isBuildingInTodoVisible += order.isInTodoVisible ? 1 : 0;
            });
            building.isInTodoVisible = showAll || isBuildingInTodoVisible > 0;
            building.ArrayOfAuftrag.ForEach(order =>
            {
                order.kategorien.ForEach(c =>
                {
                    c.leistungen.ForEach(l =>
                    {
                        if (l.nichtpauschal == 0 && l.timevaldays > 0 && l.isInTodoVisible)//long.Parse(l.lastwork) > 1 && l.timevaldays > 0)
                        {
                            l.prio = Prio.GetLeistungPrio(l, model);
                            _prio = Math.Min(_prio, l.prio.days);
                            _prioMax = Math.Max(_prio, l.prio.days);
                        }
                    });
                });
            });

            building.prio = _prio;
            building.prioMax = _prioMax;
            return building;
        }

        public static double CalcOverdue(AuftragWSO order, AppModel model)
        {
            double _prio = 100000000;
            order.kategorien.ForEach(c =>
            {
                c.leistungen.ForEach(l =>
                {
                    if (l.nichtpauschal == 0 && l.timevaldays > 0)//long.Parse(l.lastwork) > 1 && l.timevaldays > 0)
                    {
                        l.prio = Prio.GetLeistungPrio(l, model);
                        _prio = Math.Min(_prio, l.prio.days);
                    }
                });
            });
            return _prio;
        }

        public static double CalcKategorieOverdue(KategorieWSO cat, AppModel model)
        {
            double _prio = 100000000;
            cat.leistungen.ForEach(l =>
            {
                if (long.Parse(l.lastwork) > 1 && l.timevaldays > 0 && l.isInTodoVisible)
                {
                    l.prio = Prio.GetLeistungPrio(l, model);
                    _prio = Math.Min(_prio, l.prio.days);
                }
            });
            return _prio;
        }



        public static double CalcLeistungOverdue(LeistungWSO l, AppModel model)
        {
            double _prio = 100000000;
            if (long.Parse(l.lastwork) > 1 && l.timevaldays > 0)
            {
                l.prio = Prio.GetLeistungPrio(l, model);
                _prio = Math.Min(_prio, l.prio.days);
            }
            return _prio;
        }
        public static double CalcLeistungOverdueTodo(LeistungWSO l, AppModel model)
        {
            double _prio = 100000000;
            if (long.Parse(l.lastwork) > 1 && l.timevaldays > 0 && l.isInTodoVisible)
            {
                l.prio = Prio.GetLeistungPrio(l, model);
                _prio = Math.Min(_prio, l.prio.days);
            }
            return _prio;
        }

        public static StackLayout GetOrderListView(AppModel model, ICommand func)
        {
            var stack = new StackLayout
            {
                Padding = new Thickness(5, 0, 5, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };
            var selOrderId = -1;
            if (model.allSelectedPositionToWork != null && model.allSelectedPositionToWork.Count > 0)
            {
                var first = model.allSelectedPositionToWork.First();
                if (first != null) { selOrderId = first.auftragid; } else { selOrderId = -1; }
            }
            model.LastBuilding.ArrayOfAuftrag.ForEach(o =>
            {
                if (o.id == selOrderId || selOrderId < 0)
                {
                    stack.Children.Add(GetOrderCardView(o, model, func));
                }
                else
                {
                    stack.Children.Add(GetDisableOrderCardView(o, model));
                }
            });
            return stack;
        }
        public static Border GetOrderCardView(AuftragWSO order, AppModel model, ICommand func)
        {
            var _prio = CalcOverdue(order, model);
            var imageL = new Image
            {
                Margin = new Thickness(0, 0, 5, 0),
                HeightRequest = 32,
                WidthRequest = 32,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Source = model.imagesBase.OrderFolderTools
            };
            var lb = new Label()
            {
                Text = order.GetMobileText(),
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(5, 0, 5, 1),
                FontSize = 16,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                LineBreakMode = LineBreakMode.WordWrap,
            };
            var id = new Label
            {
                Text = "Nr.: " + order.id,
                TextColor = Color.FromArgb("#999999"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                MinimumWidthRequest = 100,
                LineBreakMode = LineBreakMode.TailTruncation,
                HorizontalOptions = LayoutOptions.End,
            };
            //var status = new Label
            //{
            //    Text = "Status: " + order.status,
            //    TextColor = Color.FromArgb("#999999"),
            //    Margin = new Thickness(5, 0, 0, 0),
            //    FontSize = 12,
            //    LineBreakMode = LineBreakMode.TailTruncation,
            //    HorizontalOptions = LayoutOptions.Start,
            //};
            var typ = new Label
            {
                Text = "Typ: " + order.typ,
                TextColor = Color.FromArgb("#999999"),
                Margin = new Thickness(6, 0, 0, 0),
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
            v.Children.Add(lb);
            //v.Children.Add(status);
            v.Children.Add(typ);

            //if (_prio < 1360) // kleiner 4 Jahre
            //{
            //    var warn = GetOrderWarning(_prio, model);
            //    warn.Margin = new Thickness(-35, 5, -40, 0);
            //    v.Children.Add(warn);
            //}
            var badge = new Border
            {
                BackgroundColor = Color.FromArgb(_prio < 0 ? "#ff0000" : (_prio < 1 ? "#ffcc00" : "#009900")),
                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(-14, -3, 0, 0),
                Padding = new Thickness(4, 2, 4, 2),
                StrokeShape = new RoundRectangle { CornerRadius = 5 },
                IsVisible = (_prio < 1360),
                Content = new Label
                {
                    Text = Int32.Parse("" + _prio).ToString(),
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
            h.Children.Add(imageL);
            h.Children.Add(badge);
            h.Children.Add(v);
            h.Children.Add(id);

            var mainFrame = new Border()
            {
                Padding = new Thickness(1, 1, 1, 1),
                Margin = new Thickness(0, 15, 0, 5),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromArgb("#041d43"),
                Content = h,
                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
                ClassId = "" + order.id,
            };

            if (func != null)
            {
                mainFrame.GestureRecognizers.Clear();
                mainFrame.GestureRecognizers.Add(new TapGestureRecognizer() { Command = func, CommandParameter = order });
            }


            return mainFrame;
        }
        public static Border GetDisableOrderCardView(AuftragWSO order, AppModel model)
        {
            var _prio = CalcOverdue(order, model);
            var imageL = new Image
            {
                Margin = new Thickness(0, 0, 5, 0),
                HeightRequest = 32,
                WidthRequest = 32,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Source = model.imagesBase.OrderFolderTools
            };
            var hi = new StackLayout()
            {
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                HeightRequest = 32,
                WidthRequest = 1,
                Orientation = StackOrientation.Horizontal,
                BackgroundColor = Colors.Transparent,
            };
            var imageR = new Image
            {
                Margin = new Thickness(-32, 20, 0, 0),
                HeightRequest = 32,
                WidthRequest = 32,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.End,
                Source = model.imagesBase.DisableRed,
            };
            hi.Children.Add(imageR);
            var lb = new Label()
            {
                Text = order.GetMobileText(),
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(5, 0, 5, 1),
                FontSize = 16,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                LineBreakMode = LineBreakMode.WordWrap,
            };
            var id = new Label
            {
                Text = "Nr.: " + order.id,
                TextColor = Color.FromArgb("#999999"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                MinimumWidthRequest = 100,
                LineBreakMode = LineBreakMode.TailTruncation,
                HorizontalOptions = LayoutOptions.End,
            };
            //var status = new Label
            //{
            //    Text = "Status: " + order.status,
            //    TextColor = Color.FromArgb("#999999"),
            //    Margin = new Thickness(5, 0, 0, 0),
            //    FontSize = 12,
            //    LineBreakMode = LineBreakMode.TailTruncation,
            //    HorizontalOptions = LayoutOptions.Start,
            //};
            var typ = new Label
            {
                Text = "Typ: " + order.typ,
                TextColor = Color.FromArgb("#999999"),
                Margin = new Thickness(6, 0, 0, 0),
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
                BackgroundColor = Color.FromArgb("#333333"),
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
            //v.Children.Add(status);
            v.Children.Add(typ);

            //if (_prio < 1360) // kleiner 4 Jahre
            //{
            //    var warn = GetOrderWarning(_prio, model);
            //    warn.Margin = new Thickness(-35, 5, -40, 0);
            //    v.Children.Add(warn);
            //}
            var badge = new Border
            {
                BackgroundColor = Color.FromArgb(_prio < 0 ? "#ff0000" : (_prio < 1 ? "#ffcc00" : "#009900")),
                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(-14, -3, 0, 0),
                Padding = new Thickness(4, 2, 4, 2),
                StrokeShape = new RoundRectangle { CornerRadius = 5 },
                IsVisible = (_prio < 1360),
                Content = new Label
                {
                    Text = Int32.Parse("" + _prio).ToString(),
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
            h.Children.Add(imageL);
            h.Children.Add(badge);
            h.Children.Add(v);
            h.Children.Add(id);
            h.Children.Add(hi);

            var mainFrame = new Border()
            {
                Padding = new Thickness(1, 1, 1, 1),
                Margin = new Thickness(0, 15, 0, 5),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromArgb("#041d43"),
                Content = h,
                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
                ClassId = "" + order.id,
            };

            return mainFrame;
        }
        public static StackLayout GetOrderWarning(double count, AppModel model)
        {
            var warnText = count < 0 ? "Hier gibt es überfällige Arbeiten!" : (count < 1 ? "Heute sind fällige Arbeiten!" : "Arbeiten erst fällig in (Tagen)");
            var warnColor = Color.FromArgb(count < 0 ? "#50ff0000" : (count < 1 ? "#50ffcc00" : "#50009900"));

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
                TextColor = Color.FromArgb("#ffffff"),
                Margin = new Thickness(5, 0, 5, 0),
                FontSize = 12,
                HorizontalOptions = LayoutOptions.StartAndExpand,
                VerticalOptions = LayoutOptions.Center,
                LineBreakMode = LineBreakMode.WordWrap,
            };

            var badge = new Border
            {
                BackgroundColor = Color.FromArgb(count < 0 ? "#ff0000" : (count < 1 ? "#ffcc00" : "#009900")),
                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 0, 0, 0),
                Padding = new Thickness(4, 2, 4, 2),
                StrokeShape = new RoundRectangle { CornerRadius = 5 },
                Content = new Label
                {
                    Text = Int32.Parse("" + count).ToString(),
                    Margin = new Thickness(0),
                    Padding = new Thickness(0),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 12,
                    TextColor = Colors.White,
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
        public static StackLayout GetOrderInfoElement(AuftragWSO order, AppModel model)
        {
            return new StackLayout
            {
                Padding = new Thickness(5, 5, 5, 5),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromArgb("#90144d73"),
                Children = {
                    new Image {
                        Margin = new Thickness(0, 0, 10, 0),
                        HeightRequest = 30,
                        WidthRequest = 30,
                        VerticalOptions = LayoutOptions.StartAndExpand,
                        Source = model.imagesBase.OrderFolderTools
                    },
                    new Label {
                        Text = order.GetMobileText() + " \nNr.: " + order.id + "  Typ: " + order.typ,
                        VerticalOptions = LayoutOptions.StartAndExpand,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        FontSize = 14,
                        TextColor = Colors.White,
                        HorizontalTextAlignment = TextAlignment.Start,
                        Margin = new Thickness(0, 0, 0, 0),
                        Padding = new Thickness(0, 0, 0, 0)
                    }
                }
            };
        }


        public async static Task<StackLayout> GetOrderAgainListView(AppModel model, int pos)
        {
            var stack = new StackLayout
            {
                Padding = new Thickness(5, 0, 5, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };
            var selOrderId = -1;
            if (model.allPositionInWork != null && model.allPositionInWork.leistungen != null && model.allPositionInWork.leistungen.Count > 0)
            {
                var first = model.allPositionInWork.leistungen.First();
                if (first != null) { selOrderId = first.auftragid; } else { selOrderId = -1; }
            }
            model.LastBuilding.ArrayOfAuftrag.ForEach(o =>
            {
                if (o.id == selOrderId || selOrderId < 0)
                {
                    model.LastSelectedOrderAgain = o;
                    stack.Children.Add(GetOrderAgainCardView(o, model));
                }
            });
            return stack;
        }
        public static Border GetOrderAgainCardView(AuftragWSO order, AppModel model)
        {
            var _prio = CalcOverdue(order, model);
            var imageL = new Image
            {
                Margin = new Thickness(0, 0, 5, 0),
                HeightRequest = 32,
                WidthRequest = 32,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Source = model.imagesBase.OrderFolderTools
            };
            var lb = new Label()
            {
                Text = order.GetMobileText(),
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(5, 0, 5, 1),
                FontSize = 16,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                LineBreakMode = LineBreakMode.WordWrap,
            };
            var id = new Label
            {
                Text = "Nr.: " + order.id,
                TextColor = Color.FromArgb("#999999"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.TailTruncation,
                HorizontalOptions = LayoutOptions.End,
            };
            var h = new StackLayout()
            {
                Padding = new Thickness(5, 5, 5, 5),
                Margin = new Thickness(0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromArgb("#33042d53"),
            };
            var v = new StackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };
            v.Children.Add(lb);

            if (_prio < 1360) // kleiner 4 Jahre
            {
                var warn = GetOrderWarning(_prio, model);
                warn.Margin = new Thickness(-35, 5, -40, 0);
                v.Children.Add(warn);
            }
            h.Children.Add(imageL);
            h.Children.Add(v);
            h.Children.Add(id);

            var mainFrame = new Border()
            {
                Padding = new Thickness(1, 1, 1, 1),
                Margin = new Thickness(0, 0, 0, 0),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromArgb("#33041d43"),
                Content = h,
                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
                ClassId = "" + order.id,
            };

            return mainFrame;
        }




        public static StackLayout GetOrderTodoListView(AppModel model, int all, AbsoluteLayout overlay, string s)
        {
            int maxResult = 20;
            var stack = new StackLayout
            {
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            // Faellige Oder Alle
            var oList = new List<BuildingWSO>();
            model.AllBuildings.ForEach(b =>
            {
                string term = b.plz + "##" + b.ort + "##" + b.strasse + "##" + b.hsnr + "##" + b.objektname + "##" + b.objektnr;
                if (term.ToLower().Contains(s.ToLower()))
                {
                    var building = CalcBuildingdue(b, model, all == 1);
                    if (building != null)
                    {
                        oList.Add(building);
                    }
                }
            });

            //oList.ForEach(b => { b = CalcBuildingdue(b, model); });
            oList = oList.OrderBy(b => b.prio).ToList();
            int count = oList.Where(_ => _.isInTodoVisible).Count();
            int pages = (count / maxResult) + (count == maxResult ? 0 : 1);
            AppModel.Instance.MainPage._holdLastTodoPageMax = pages;

            int i = 1;

            if (all == 2)
            {
                // NUR MUELL POS
                var muellist = new List<LeistungWSO>();
                oList.ForEach(b =>
                {
                    b.ArrayOfAuftrag.ForEach(a =>
                    {
                        a.kategorien.ForEach(k =>
                        {
                            k.leistungen.ForEach(l =>
                            {
                                if (l.muell == 1 && l.inout != null && l.inout.inout == 1)// nur Rausgestellte
                                {
                                    var za = (i * (AppModel.Instance.MainPage._holdLastTodoPage - 1));
                                    var ba = (maxResult * (AppModel.Instance.MainPage._holdLastTodoPage - 1)) + 1; // 1
                                    var bb = (maxResult * (AppModel.Instance.MainPage._holdLastTodoPage - 1)) + maxResult; // maxResult 50
                                    if (i >= ba && i <= bb && b != null && b.ArrayOfAuftrag.Count > 0 && b.isInTodoVisible)
                                    {
                                        l.objekt = b;
                                        l.lastworkNumber = long.Parse(l.inout.last != null ? l.inout.last : "0");
                                        muellist.Add(l);
                                    }
                                }
                            });
                        });
                    });
                });
                muellist = muellist.OrderBy(o => o.objekt.plz).ThenBy(o => o.lastworkNumber).ToList();
                pages = (muellist.Count / maxResult) + (muellist.Count == maxResult ? 0 : 1);
                AppModel.Instance.MainPage._holdLastTodoPageMax = pages;
                muellist.ForEach(ml =>
                {
                    stack.Children.Add(LeistungWSO.GetMuellPositionCardView(ml, model, null));
                });
                AppModel.Instance.MainPage.Update_Todopaging(AppModel.Instance.MainPage._holdLastTodoPage, AppModel.Instance.MainPage._holdLastTodoPageMax);
                return stack;
            }
            // all = 1 oder 0
            oList.ForEach(b =>
            {
                var za = (i * (AppModel.Instance.MainPage._holdLastTodoPage - 1));
                var ba = (maxResult * (AppModel.Instance.MainPage._holdLastTodoPage - 1)) + 1; // 1
                var bb = (maxResult * (AppModel.Instance.MainPage._holdLastTodoPage - 1)) + maxResult; // maxResult 50
                if (i >= ba && i <= bb && b != null && b.ArrayOfAuftrag.Count > 0 && b.isInTodoVisible)
                {
                    stack.Children.Add(BuildingWSO.GetBuildingInfoTodoElement(b, model, overlay));
                }
                i++;
            });
            AppModel.Instance.MainPage.Update_Todopaging(AppModel.Instance.MainPage._holdLastTodoPage, AppModel.Instance.MainPage._holdLastTodoPageMax);
            //Task task = Task.Run(() => RestGui(all,oList,stack, overlay));
            return stack;
        }


        public static StackLayout GetNewStack()
        {
            return new StackLayout
            {
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                HeightRequest = 20,
                BackgroundColor = Colors.Red,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };
        }


        public static async void KategorieTapped(KategorieParameter obj)
        {
            //obj.stack = GetOrderTodoListView_Pos(obj.model, obj.cat);
            //await Task.Delay(1);
        }
        public static StackLayout GetOrderTodoListView_Pos(AppModel model, KategorieWSO cat)
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
            cat.leistungen.ForEach(l => { l._prio = CalcLeistungOverdue(l, model); list.Add(l); });
            list = list.OrderBy(l => l._prio).ToList();
            list.ForEach(pos =>
            {
                if (pos._prio < 8)  // vorschau 1 Woche
                {
                    stack.Children.Add(LeistungWSO.GetPositionTodoCardView(pos, model, false));
                }
            });
            return stack;
        }


        //public static StackLayout GetOrderInclWinterCardView(AuftragWSO order)
        //{
        //    var imageL = new Image
        //    {
        //        Margin = new Thickness(0, 0, 5, 0),
        //        HeightRequest = 24,
        //        WidthRequest = 24,
        //        VerticalOptions = LayoutOptions.Start,
        //        HorizontalOptions = LayoutOptions.Start,
        //        Source = AppModel.Instance.imagesBase.OrderFolderTools
        //    };
        //    var lb = new Label()
        //    {
        //        Text = order.bezeichnung,
        //        TextColor = Color.FromArgb("#cccccc"),
        //        Margin = new Thickness(5, 0, 5, 1),
        //        FontSize = 16,
        //        HorizontalOptions = LayoutOptions.FillAndExpand,
        //        LineBreakMode = LineBreakMode.WordWrap,
        //    };
        //    var h = new StackLayout()
        //    {
        //        Padding = new Thickness(5, 5, 5, 5),
        //        Margin = new Thickness(0, 0, 0, 0),
        //        Spacing = 0,
        //        Orientation = StackOrientation.Horizontal,
        //        HorizontalOptions = LayoutOptions.FillAndExpand,
        //        BackgroundColor = Color.FromArgb("#aa042d53"),
        //    };

        //    h.Children.Add(imageL);
        //    h.Children.Add(lb);

        //    var mainFrame = new Frame()
        //    {
        //        Padding = new Thickness(0),
        //        Margin = new Thickness(20, 1, 0, 1),
        //        HorizontalOptions = LayoutOptions.FillAndExpand,
        //        BackgroundColor = Colors.Transparent,
        //        Content = h,
        //        StrokeShape = new RoundRectangle { CornerRadius = 0 },
        //        
        //        
        //        ClassId = "" + order.id,
        //    };

        //    var container = new StackLayout
        //    {
        //        Padding = new Thickness(0),
        //        Margin = new Thickness(0),
        //        Spacing = 0,
        //        Orientation = StackOrientation.Vertical,
        //        HorizontalOptions = LayoutOptions.FillAndExpand,
        //    };

        //    //mainFrame.GestureRecognizers.Clear();
        //    //mainFrame.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command<StackLayout>(ShowCategoryContainer), CommandParameter = container });

        //    var wrapper = new StackLayout
        //    {
        //        Padding = new Thickness(0),
        //        Margin = new Thickness(0),
        //        Spacing = 0,
        //        Orientation = StackOrientation.Vertical,
        //        HorizontalOptions = LayoutOptions.FillAndExpand,
        //        Children = { mainFrame, container }
        //    };

        //    return wrapper;
        //}

        public static StackLayout GetOrderTodoCardViewOnlyKat(AuftragWSO order, AppModel model, double _prio, bool onlyText)
        {
            var imageL = new Image
            {
                Margin = new Thickness(0, 0, 5, 0),
                HeightRequest = 24,
                WidthRequest = 24,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Source = onlyText ? model.imagesBase.TextSymbol : model.imagesBase.OrderFolderTools
            };
            var lb = new Label()
            {
                Text = order.GetMobileText(),
                TextColor = Color.FromArgb("#cccccc"),
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
                BackgroundColor = onlyText ? Color.FromArgb("#aa04532d") : Color.FromArgb("#aa042d53"),
            };


            Border warn = new Border { IsVisible = false };
            if (_prio < 1360)
            {
                warn = GetTodoCountWarningSmall(_prio);
            }

            if (onlyText)
            {
                h.Children.Add(imageL);
            }
            else
            {
                h.Children.Add(warn);
            }
            h.Children.Add(lb);

            var mainFrame = new Border()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(20, 1, 0, 1),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Colors.Transparent,
                Content = h,
                ClassId = "" + order.id,
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
            mainFrame.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command<StackLayout>(ShowCategoryContainerOnlyKat), CommandParameter = container });

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
        public static void ShowCategoryContainerOnlyKat(StackLayout value)
        {
            value.IsVisible = !value.IsVisible;
        }

        public static StackLayout GetOrderTodoCardView(AuftragWSO order, AppModel model, double _prio, bool onlyText)
        {
            var imageL = new Image
            {
                Margin = new Thickness(0, 0, 5, 0),
                HeightRequest = 24,
                WidthRequest = 24,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Source = onlyText ? model.imagesBase.TextSymbol : model.imagesBase.OrderFolderTools
            };
            var lb = new Label()
            {
                Text = order.GetMobileText(),
                TextColor = Color.FromArgb("#cccccc"),
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
                BackgroundColor = onlyText ? Color.FromArgb("#aa04532d") : Color.FromArgb("#aa042d53"),
            };


            Border warn = new Border { IsVisible = false };
            if (_prio < 1360)
            {
                warn = GetTodoCountWarningSmall(_prio);
            }

            if (onlyText)
            {
                h.Children.Add(imageL);
            }
            else
            {
                h.Children.Add(warn);
            }
            h.Children.Add(lb);

            var mainFrame = new Border()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(20, 1, 0, 1),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Colors.Transparent,
                Content = h,
                ClassId = "" + order.id,
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
            mainFrame.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command<StackLayout>(ShowCategoryContainer), CommandParameter = container });

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
        public static void ShowCategoryContainer(StackLayout value)
        {
            value.IsVisible = !value.IsVisible;
        }

        public static StackLayout GetOrderTodoWarning(double count, AppModel model, double countMax = double.NaN)
        {
            var warnText = count < 0 ? "Hier gibt es überfällige Arbeiten!" : (count < 1 ? "Heute sind fällige Arbeiten!" : "Arbeiten erst fällig in (Tagen)");
            var warnColor = Color.FromArgb(count < 0 ? "#50ff0000" : (count < 1 ? "#50ffcc00" : "#50009900"));

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
                TextColor = Color.FromArgb("#ffffff"),
                Margin = new Thickness(5, 0, 5, 0),
                FontSize = 12,
                HorizontalOptions = LayoutOptions.StartAndExpand,
                VerticalOptions = LayoutOptions.Center,
                LineBreakMode = LineBreakMode.WordWrap,
            };

            var badge = new Border
            {
                BackgroundColor = Color.FromArgb(count < 0 ? "#ff0000" : (count < 1 ? "#ffcc00" : "#009900")),
                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0),
                Padding = new Thickness(4, 2, 4, 2),
                StrokeShape = new RoundRectangle { CornerRadius = 5 },
                Content = new Label
                {
                    Text = Int32.Parse("" + count).ToString(),
                    Margin = new Thickness(0, 0, 0, 0),
                    Padding = new Thickness(0, 0, 0, 0),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 12,
                    TextColor = Colors.White,
                    HorizontalTextAlignment = TextAlignment.Center
                }
            };
            Label lbMax = null;
            Border badgeMax = null;
            if (!double.IsNaN(countMax))
            {

                lbMax = new Label()
                {
                    Text = "bis",
                    TextColor = Color.FromArgb("#ffffff"),
                    Margin = new Thickness(5, 0, 5, 0),
                    FontSize = 12,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    LineBreakMode = LineBreakMode.WordWrap,
                };
                badgeMax = new Border
                {
                    BackgroundColor = Color.FromArgb(countMax < 0 ? "#ff0000" : (countMax < 1 ? "#ffcc00" : "#009900")),
                    Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0),
                    Padding = new Thickness(4, 2, 4, 2),
                    StrokeShape = new RoundRectangle { CornerRadius = 5 },
                    Content = new Label
                    {
                        Text = Int32.Parse("" + countMax).ToString(),
                        Margin = new Thickness(0, 0, 0, 0),
                        Padding = new Thickness(0, 0, 0, 0),
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        FontSize = 12,
                        TextColor = Colors.White,
                        HorizontalTextAlignment = TextAlignment.Center
                    }
                };
            }

            var h = new StackLayout()
            {
                Padding = new Thickness(5, 1, 5, 1),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = warnColor,
            };
            h.Children.Add(imageL);
            h.Children.Add(lb);
            h.Children.Add(badge);
            if (!double.IsNaN(countMax))
            {
                h.Children.Add(lbMax);
                h.Children.Add(badgeMax);
            }
            return h;
        }

        public static Border GetTodoCountWarningSmall(double count)
        {
            var badge = new Border
            {
                BackgroundColor = Color.FromArgb(count < 0 ? "#ff0000" : (count < 1 ? "#ffcc00" : "#009900")),
                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0),
                Padding = new Thickness(4, 2, 4, 2),
                StrokeShape = new RoundRectangle { CornerRadius = 5 },
                MinimumWidthRequest = 100,
                Content = new Label
                {
                    Text = Int32.Parse("" + count).ToString(),
                    Margin = new Thickness(0, 0, 0, 0),
                    Padding = new Thickness(0, 0, 0, 0),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 12,
                    TextColor = Colors.White,
                    HorizontalTextAlignment = TextAlignment.Center
                }
            };

            //var h = new StackLayout()
            //{
            //    Padding = new Thickness(5, 1, 5, 1),
            //    Margin = new Thickness(0, 0, 0, 0),
            //    Spacing = 0,
            //    Orientation = StackOrientation.Horizontal,
            //    HorizontalOptions = LayoutOptions.Start,
            //    BackgroundColor = warnColor,
            //};
            //h.Children.Add(badge);
            return badge;
        }



    }
}