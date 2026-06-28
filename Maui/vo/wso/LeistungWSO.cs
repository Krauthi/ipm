using iPMCloud.Mobile.vo;
using Microsoft.Maui.Controls;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using System.Globalization;

namespace iPMCloud.Mobile
{
    [Serializable]
    public class LeistungWSO
    {
        public Int32 id = 0;
        public Int32 gruppeid;
        public Int32 kategorieid;
        public Int32 auftragid;
        public Int32 objektid;
        public string type = "";
        public string beschreibung = "";
        public string timeval = "1x wöchentlich";
        public int del = 0;
        public int timevaldays = 7;
        public string lastwork = "0";
        public long lastworkNumber = 0;
        public int winterservice = 0;
        public int dstd = 0;
        public int dmin = 0;

        public string anzahl = "1,00";
        public string produktAnzahl = "1,00";
        public string einheit = "std";
        public string notiz = "";
        public string art = "Leistung";
        public string workat = "";

        public int nichtpauschal = 0;
        public int muell = 0;

        public bool selected = false;
        public bool disabled = false;
        public bool hasChanged = false;

        public bool isInTodoVisible = true;
        public double _prio = 100000000;

        public InOutWSO inout;
        public LeistungExtWSO ext;

        [NonSerialized]
        public BuildingWSO objekt = null;
        [NonSerialized]
        public Prio prio = new Prio();
        [NonSerialized]
        public LeistungInWorkWSO leiInWork = null;

        public LeistungWSO() { }

        public string GetMobileText()
        {
            if (ext != null)
            {
                if (!String.IsNullOrWhiteSpace(ext.anweisung))
                {
                    // Anweisung gefüllt
                    if (!String.IsNullOrWhiteSpace(ext.anweisungLang) && AppModel.Instance.AppControll.translation)
                    {
                        // Übersetzter Text gefüllt
                        return AppModel.Instance.Lang.lang == "de" ? ext.anweisung : ext.anweisungLang;
                    }
                    else
                    {
                        return ext.anweisung;
                    }
                }
                else
                {
                    if (!String.IsNullOrWhiteSpace(ext.anweisungLang) && AppModel.Instance.AppControll.translation)
                    {
                        return AppModel.Instance.Lang.lang == "de" ? beschreibung : ext.anweisungLang;
                    }
                    else
                    {
                        return beschreibung;
                    }
                }
            }
            else
            {
                ext = new LeistungExtWSO();
                return beschreibung;
            }
        }

        public string GetMobileLangText()
        {
            if (ext != null)
            {
                if (!String.IsNullOrWhiteSpace(ext.anweisungLang))
                {
                    // Übersetzter Text gefüllt
                    return ext.anweisungLang;
                }
                else
                {
                    return "";
                }
            }
            else
            {
                ext = new LeistungExtWSO();
                return GetMobileOriginalText();
            }
        }

        public string GetMobileOriginalText()
        {
            if (ext != null)
            {
                if (!String.IsNullOrWhiteSpace(ext.anweisung))
                {
                    return ext.anweisung;
                }
                else
                {
                    return beschreibung;
                }
            }
            else
            {
                ext = new LeistungExtWSO();
                return beschreibung;
            }
        }


        public static StackLayout GetPositionListView(AppModel model, ICommand func)
        {
            var stack = new StackLayout
            {
                Padding = new Thickness(5, 0, 5, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.Fill,
            };
            model.allPositionInShowingListView.Clear();
            model.LastSelectedCategory.leistungen = model.LastSelectedCategory.leistungen.OrderBy(s => s.prio.days).ToList();//.ThenBy(s => s.Name);
            model.LastSelectedCategory.leistungen.ForEach(pos =>
            {
                bool inWork = false;
                if (model.allPositionInWork != null)
                {
                    var foundInWork = model.allPositionInWork.leistungen.Find(l => l.id == pos.id);
                    inWork = foundInWork != null;
                }
                var stackPos = inWork ? GetInWorkPositionCardView(pos, model, func) : (pos.disabled ? GetDisabledPositionCardView(pos, model, func) : (pos.selected ? GetSelectedPositionCardView(pos, model, func) : GetPositionCardView(pos, model, func)));
                model.allPositionInShowingListView.Add(pos.id, stackPos);
                stack.Children.Add(stackPos);
            });
            return stack;
        }
        public static Border GetPositionCardView(LeistungWSO pos, AppModel model, ICommand func)
        {
            //var _prio = CalcOverdue(pos);
            var imageL = new Image
            {
                Margin = new Thickness(0, 0, 5, 0),
                HeightRequest = 32,
                WidthRequest = 32,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Source = (pos.art == "Leistung" ? "leistung_symbol.png" :
                            (pos.art == "Produkt" ? "produkt_symbol.png" :
                                (pos.art == "Texte" ? "text_symbol.png" :
                                (pos.art == "Check" ? "check_white.png" :
                                    "quest.png"
                         ))))
            };

            var imgInfo = new Image
            {
                Margin = new Thickness(5, 0, 5, 2),
                HeightRequest = 32,
                WidthRequest = 32,
                Opacity = 0.7,
                Source = "ic_info_b.png",//AppModel._Instance.imagesBase.InfoCircle,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start,
                IsVisible = !String.IsNullOrWhiteSpace(pos.notiz)
            };
            imgInfo.GestureRecognizers.Clear();
            var t_imgInfo = new TapGestureRecognizer();
            t_imgInfo.Tapped += (object o, TappedEventArgs ev) => { AppModel.Instance.MainPage.OpenLeistungInfoDialog(pos); };
            imgInfo.GestureRecognizers.Add(t_imgInfo);

            var hInfo = new HorizontalStackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Colors.Transparent
            };


            var lb = new Label()
            {
                Text = pos.GetMobileText(),
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(5, 0, 5, 1),
                FontSize = 14,
                HorizontalOptions = LayoutOptions.Start,
                LineBreakMode = LineBreakMode.WordWrap,
            };
            var direkt = new Label
            {
                Text = "Direkterfassung: " + pos.dstd.ToString("00") + ":" + pos.dmin.ToString("00"),
                TextColor = Color.FromArgb("#ffcc00"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.Start,
                IsVisible = pos.type == "1",
            };
            var last = "";
            if (pos.muell == 1 && pos.inout != null)
            {
                if (pos.inout.last != "0")
                {
                    DateTime d = JavaScriptDateConverter.Convert(long.Parse(pos.inout.last));
                    last = d.ToString("dd.MM.yyyy");
                }
                else
                {
                    last = "Nicht bekannt!";
                }
            }
            else
            {
                last = (pos.prio != null && pos.prio.lastWorkDate != null ? pos.prio.lastWorkDate.Value.ToString("dd.MM.yyyy") : "Nicht bekannt!");
            }
            hInfo.Children.Add(imgInfo);

            var grid = new Grid()
            {
                Padding = new Thickness(5),
                Margin = new Thickness(0),
                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
                ClassId = "" + pos.id,
                RowSpacing = 0,
                ColumnSpacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Color.FromArgb(pos.nichtpauschal == 1 ? "#044320" : "#042d53"),
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto },
                },
            };
            var hmuell = new HorizontalStackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                HeightRequest = 1,
                HorizontalOptions = LayoutOptions.Fill,
                IsVisible = pos.muell == 1
            };
            var imageMuellSign2 = new Image
            {
                Margin = new Thickness(0, -12, 0, 0),
                HeightRequest = 32,
                WidthRequest = 32,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.End,
                Source = "muell_sign.png",
            };
            var imageMuellSign = new Image
            {
                Margin = new Thickness(0, -12, -8, 0),
                HeightRequest = 32,
                WidthRequest = 32,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.End,
                Source = pos.muell == 1 ? (pos.inout.inout == 1 ? "muell_out_tonne.png" : "muell_in_tonne.png") : null,
            };
            hmuell.Children.Add(imageMuellSign);
            hmuell.Children.Add(imageMuellSign2);
            var v = new VerticalStackLayout()
            {
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
            };
            v.Children.Add(lb);
            if (pos.type == "1")
            {
                v.Children.Add(direkt);
            }
            var typ = new Label
            {
                Text = pos.timeval + " --- Zuletzt: " + last,
                TextColor = Color.FromArgb("#ffcc00"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.Start,
            };
            v.Children.Add(typ);

            if (double.Parse(pos.lastwork) == 0 && pos.timevaldays > 0)
            {
                v.Children.Add(GetWarningLineText(pos.prio.warnText, pos.prio.barColor));
            }

            var badge = new Border
            {
                BackgroundColor = Color.FromArgb(pos.prio.badgeColor),
                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(28, -3, 0, 0),
                Padding = new Thickness(4, 2, 4, 2),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 9 },
                IsVisible = pos.prio.showBadge && (pos.prio.days > 10000000 ? false : true),
                Content = new Label
                {
                    Text = Int32.Parse((pos.prio.days > 10000000 ? "0" : pos.prio.days + "")).ToString(),
                    Margin = new Thickness(0, 0, 0, 0),
                    Padding = new Thickness(0, 0, 0, 0),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 11,
                    TextColor = Colors.White,
                    FontAttributes = FontAttributes.Bold,
                    LineBreakMode = LineBreakMode.NoWrap,
                    HorizontalTextAlignment = TextAlignment.Center
                }
            };
            grid.Add(imageL,0,0);
            grid.Add(badge,0,0);
            grid.Add(v,1,0);
            grid.Add(hmuell, 2, 0);
            grid.Add(hInfo, 3, 0);

            if (func != null)
            {
                grid.GestureRecognizers.Clear();
                grid.GestureRecognizers.Add(new TapGestureRecognizer() { Command = func, CommandParameter = pos });
            }

            return new Border
            {
                ClassId = "" + pos.id,
                Content = grid,
                Padding = new Thickness(0),
                Margin = new Thickness(0, 15, 0, 5),
                HorizontalOptions = LayoutOptions.Fill,
            };
        }
        public static Border GetSelectedPositionCardView(LeistungWSO pos, AppModel model, ICommand func)
        {
            //var _prio = CalcOverdue(pos);
            var imageL = new Image
            {
                Margin = new Thickness(0, 0, 5, 0),
                HeightRequest = 32,
                WidthRequest = 32,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Source = (pos.art == "Leistung" ? "leistung_symbol.png" :
                            (pos.art == "Produkt" ? "produkt_symbol.png" :
                                (pos.art == "Texte" ? "text_symbol.png" :
                                (pos.art == "Check" ? "check_white.png" :
                                    "quest.png"
                         ))))
            };
            var imageR = new Image
            {
                Margin = new Thickness(5, 0, 0, 0),
                HeightRequest = 32,
                WidthRequest = 32,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Source = "check_white.png"
            };
            var lb = new Label()
            {
                Text = pos.GetMobileText(),
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(5, 0, 5, 1),
                FontSize = 14,
                HorizontalOptions = LayoutOptions.Start,
                LineBreakMode = LineBreakMode.TailTruncation,
            };
            var direkt = new Label
            {
                Text = "Direkterfassung: " + pos.dstd.ToString("00") + ":" + pos.dmin.ToString("00"),
                TextColor = Color.FromArgb("#999999"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.Start,
                IsVisible = pos.type == "1",
            };
            var typ = new Label
            {
                Text = pos.timeval + " --- Zuletzt: " + (pos.prio != null && pos.prio.lastWorkDate != null ? pos.prio.lastWorkDate.Value.ToString("dd.MM.yyyy") : "Nicht bekannt!"),
                TextColor = Color.FromArgb("#999999"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.Start,
            };
            var h = new Grid()
            {
                Padding = new Thickness(5, 5, 5, 5),
                Margin = new Thickness(0),
                ColumnSpacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Color.FromArgb("#333333"),
                ClassId = "" + pos.id,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto },
                },
            };
            var hmuell = new HorizontalStackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                HeightRequest = 1,
                HorizontalOptions = LayoutOptions.Fill,
                IsVisible = pos.muell == 1
            };
            var imageMuellSign = new Image
            {
                Margin = new Thickness(0, -32, -32, 0),
                HeightRequest = 32,
                WidthRequest = 32,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.End,
                Source = "muell_sign.png",
            };
            hmuell.Children.Add(imageMuellSign);
            var v = new StackLayout()
            {
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.Fill,
            };
            v.Children.Add(lb);
            v.Children.Add(direkt);
            v.Children.Add(typ);

            if (double.Parse(pos.lastwork) == 0 && pos.timevaldays > 0)
            {
                v.Children.Add(GetWarningLineText(pos.prio.warnText, pos.prio.badgeColor));
            }


            var badge = new Border
            {
                BackgroundColor = Color.FromArgb(pos.prio.badgeColor),
                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(28, -1, 0, 0),
                Padding = new Thickness(4, 2, 4, 2),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 9 },
                IsVisible = pos.prio.showBadge && (pos.prio.days > 10000000 ? false : true),
                Content = new Label
                {
                    Text = Int32.Parse((pos.prio.days > 10000000 ? "0" : pos.prio.days + "")).ToString(),
                    Margin = new Thickness(0, 0, 0, 0),
                    Padding = new Thickness(0, 0, 0, 0),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 11,
                    TextColor = Colors.White,
                    FontAttributes = FontAttributes.Bold,
                    LineBreakMode = LineBreakMode.NoWrap,
                    HorizontalTextAlignment = TextAlignment.Center
                }
            };
            h.Add(imageL,0,0);
            h.Add(badge,0,0);
            h.Add(v,1,0);
            h.Add(hmuell, 2, 0);
            h.Add(imageR,3,0);

            if (func != null)
            {
                h.GestureRecognizers.Clear();
                h.GestureRecognizers.Add(new TapGestureRecognizer() { Command = func, CommandParameter = pos });
            }

            return new Border
            {
                ClassId = "" + pos.id,
                Content = h,
                Padding = new Thickness(0),
                Margin = new Thickness(0, 15, 0, 5),
                HorizontalOptions = LayoutOptions.Fill,
            };
        }
        public static Border GetDisabledPositionCardView(LeistungWSO pos, AppModel model, ICommand func)
        {
            //var _prio = CalcOverdue(pos);
            var imageL = new Image
            {
                Margin = new Thickness(0, 0, 5, 0),
                HeightRequest = 32,
                WidthRequest = 32,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Source = (pos.art == "Leistung" ? "leistung_symbol.png" :
                            (pos.art == "Produkt" ? "produkt_symbol.png" :
                                (pos.art == "Texte" ? "text_symbol.png" :
                                (pos.art == "Check" ? "check_white.png" :
                                    "quest.png"
                         ))))
            };
            var hi = new HorizontalStackLayout()
            {
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                HeightRequest = 32,
                WidthRequest = 1,
                BackgroundColor = Colors.Transparent,
            };
            var imageR = new Image
            {
                Margin = new Thickness(-32, 0, 0, 0),
                HeightRequest = 24,
                WidthRequest = 24,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.End,
                Source = "disable_red.png",
            };
            hi.Children.Add(imageR);

            var lb = new Label()
            {
                Text = pos.GetMobileText(),
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(5, 0, 5, 1),
                FontSize = 14,
                HorizontalOptions = LayoutOptions.Start,
                LineBreakMode = LineBreakMode.WordWrap,
            };
            var status = new Label
            {
                Text = "Direkterfassung: " + pos.dstd.ToString("00") + ":" + pos.dmin.ToString("00"),
                TextColor = Color.FromArgb("#999999"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.Start,
                IsVisible = pos.type == "1",
            };
            var typ = new Label
            {
                Text = pos.timeval + " --- Zuletzt: " + (pos.prio != null && pos.prio.lastWorkDate != null ? pos.prio.lastWorkDate.Value.ToString("dd.MM.yyyy") : "Nicht bekannt!"),
                TextColor = Color.FromArgb("#999999"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.Start,
            };
            var h = new Grid()
            {
                Padding = new Thickness(5),
                Margin = new Thickness(0),
                ColumnSpacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Color.FromArgb("#333333"),
                Opacity = 0.5,
                IsEnabled = false,
                ClassId = "" + pos.id,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto },
                },
            };
            var hmuell = new StackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                HeightRequest = 1,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Fill,
                IsVisible = pos.muell == 1
            };
            var imageMuellSign2 = new Image
            {
                Margin = new Thickness(0, -32, 0, 0),
                HeightRequest = 32,
                WidthRequest = 32,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.End,
                Source = "muell_sign.png",
            };
            var imageMuellSign = new Image
            {
                Margin = new Thickness(0, -32, -8, 0),
                HeightRequest = 32,
                WidthRequest = 32,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.End,
                Source = pos.muell == 1 ? (pos.inout.inout == 1 ? "muell_out_tonne.png" : "muell_in_tonne.png") : null,
            };
            hmuell.Children.Add(imageMuellSign);
            hmuell.Children.Add(imageMuellSign2);
            var v = new VerticalStackLayout()
            {
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
            };
            v.Children.Add(lb);
            v.Children.Add(status);
            v.Children.Add(typ);

            if (double.Parse(pos.lastwork) == 0 && pos.timevaldays > 0)
            {
                v.Children.Add(GetWarningLineText(pos.prio.warnText, pos.prio.badgeColor));
            }

            //v.Children.Add(hmuell);

            var badge = new Border
            {
                BackgroundColor = Color.FromArgb(pos.prio.badgeColor),
                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(28, -3, 0, 0),
                Padding = new Thickness(4, 2, 4, 2),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 9 },
                IsVisible = pos.prio.showBadge && (pos.prio.days > 10000000 ? false : true),
                Content = new Label
                {
                    Text = Int32.Parse((pos.prio.days > 10000000 ? "0" : pos.prio.days + "")).ToString(),
                    Margin = new Thickness(0, 0, 0, 0),
                    Padding = new Thickness(0, 0, 0, 0),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 11,
                    TextColor = Colors.White,
                    FontAttributes = FontAttributes.Bold,
                    LineBreakMode = LineBreakMode.NoWrap,
                    HorizontalTextAlignment = TextAlignment.Center
                }
            };
            h.Add(imageL,0,0);
            h.Add(badge,0,0);
            h.Add(v,1,0);
            h.Add(hmuell, 2, 0);
            h.Add(hi,3,0);

            if (func != null)
            {
                h.GestureRecognizers.Clear();
                h.GestureRecognizers.Add(new TapGestureRecognizer() { Command = func, CommandParameter = pos });
            }

            return new Border
            {
                ClassId = "" + pos.id,
                Content = h,
                Padding = new Thickness(0),
                Margin = new Thickness(0, 15, 0, 5),
                HorizontalOptions = LayoutOptions.Fill,
            };
        }
        public static Border GetInWorkPositionCardView(LeistungWSO pos, AppModel model, ICommand func)
        {
            //var _prio = CalcOverdue(pos);
            var imageL = new Image
            {
                Margin = new Thickness(0, 0, 5, 0),
                HeightRequest = 32,
                WidthRequest = 32,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Source = (pos.art == "Leistung" ? "leistung_symbol.png" :
                            (pos.art == "Produkt" ? "produkt_symbol.png" :
                                (pos.art == "Texte" ? "text_symbol.png" :
                                (pos.art == "Check" ? "check_white.png" :
                                    "quest.png"
                         ))))
            };
            var lb = new Label()
            {
                Text = pos.GetMobileText(),
                TextColor = Color.FromArgb("#eeeeee"),
                Margin = new Thickness(5, 0, 5, 1),
                FontSize = 14,
                HorizontalOptions = LayoutOptions.Start,
                LineBreakMode = LineBreakMode.TailTruncation,
            };
            var status = new Label
            {
                Text = "Direkterfassung: " + pos.dstd.ToString("00") + ":" + pos.dmin.ToString("00"),
                TextColor = Color.FromArgb("#999999"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.Start,
                IsVisible = pos.type == "1",
            };
            var typ = new Label
            {
                Text = pos.timeval + " --- Zuletzt: " + (pos.prio != null && pos.prio.lastWorkDate != null ? pos.prio.lastWorkDate.Value.ToString("dd.MM.yyyy") : "Nicht bekannt!"),
                TextColor = Color.FromArgb("#999999"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.Start,
            };
            var grid = new Grid()
            {
                Padding = new Thickness(5),
                Margin = new Thickness(0),
                ColumnSpacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Color.FromArgb("#333333"),
                Opacity = 0.8,
                IsEnabled = false,
                ClassId = "" + pos.id,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
            };
            var hmuell = new HorizontalStackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                HeightRequest = 1,
                HorizontalOptions = LayoutOptions.Fill,
                IsVisible = pos.muell == 1
            };
            var imageMuellSign2 = new Image
            {
                Margin = new Thickness(0, -32, 0, 0),
                HeightRequest = 32,
                WidthRequest = 32,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.End,
                Source = "muell_sign.png",
            };
            var imageMuellSign = new Image
            {
                Margin = new Thickness(0, -32, -8, 0),
                HeightRequest = 32,
                WidthRequest = 32,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.End,
                Source = pos.muell == 1 ? (pos.inout.inout == 1 ? "muell_out_tonne.png" : "muell_in_tonne.png") : null,
            };
            hmuell.Children.Add(imageMuellSign);
            hmuell.Children.Add(imageMuellSign2);
            var v = new VerticalStackLayout()
            {
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
            };
            v.Children.Add(lb);
            v.Children.Add(status);
            v.Children.Add(typ);

            var warn = GetPositionInWork(pos, model);
            warn.Margin = new Thickness(-35, 5, 0, 0);
            v.Children.Add(warn);

            grid.Add(imageL,0,0);
            grid.Add(v,1,0);
            grid.Add(hmuell, 2, 0);

            if (func != null)
            {
                grid.GestureRecognizers.Clear();
                grid.GestureRecognizers.Add(new TapGestureRecognizer() { Command = func, CommandParameter = pos });
            }

            return new Border
            {
                ClassId = "" + pos.id,
                Content = grid,
                Padding = new Thickness(0),
                Margin = new Thickness(0, 15, 0, 5),
                HorizontalOptions = LayoutOptions.Fill,
            };
        }



        public static StackLayout GetPositionAgainListView(AppModel model, ICommand func)
        {
            var stack = new StackLayout
            {
                Padding = new Thickness(5, 0, 5, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.Fill,
            };
            model.LastSelectedCategoryAgain.leistungen = model.LastSelectedCategoryAgain.leistungen.OrderBy(s => s.prio.days).ToList();//.ThenBy(s => s.Name);
            bool isOptionalPos = false;
            model.allPositionAgainInShowingListView.Clear();
            model.LastSelectedCategoryAgain.leistungen.ForEach(pos =>
            {
                bool inWork = false;
                if (model.allPositionInWork != null)
                {
                    var foundInWork = model.allPositionInWork.leistungen.Find(l => l.id == pos.id);
                    inWork = foundInWork != null;
                }
                Border stackPos = null;
                isOptionalPos = (pos.art == "Leistung" && pos.nichtpauschal == 1 || pos.art == "Produkt");

                if (pos.art == "Produkt")
                {
                    stackPos = inWork ? GetInWorkPositionCardView(pos, model, null) : (pos.selected ? GetSelectedPositionCardView(pos, model, func) : GetPositionCardView(pos, model, func));
                }
                else
                {
                    if (model.IsOptionalPosAgain)
                    {
                        stackPos = inWork ? GetInWorkPositionCardView(pos, model, null) : (!isOptionalPos ? GetDisabledPositionCardView(pos, model, null) : (pos.selected ? GetSelectedPositionCardView(pos, model, func) : GetPositionCardView(pos, model, func)));
                    }
                    else
                    {
                        stackPos = inWork ? GetInWorkPositionCardView(pos, model, null) : (isOptionalPos ? GetDisabledPositionCardView(pos, model, null) : (pos.selected ? GetSelectedPositionCardView(pos, model, func) : GetPositionCardView(pos, model, func)));
                    }
                }
                model.allPositionAgainInShowingListView.Add(pos.id, stackPos);
                stack.Children.Add(stackPos);
            });
            return stack;
        }


        public static VerticalStackLayout GetSelectedPositionListView(AppModel model, ICommand func, ICommand funcB)
        {
            var stack = new VerticalStackLayout
            {
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
            };

            model.allPositionInShowingSmallListView.Clear();

            model.LastBuilding.ArrayOfAuftrag.ForEach(o =>
            {
                LeistungWSO foundInOrderSelected = null;
                o.kategorien.ForEach(c =>
                {
                    if (foundInOrderSelected == null)
                    {
                        foundInOrderSelected = c.leistungen.Find(f => f.selected == true);
                    }
                });
                if (foundInOrderSelected != null)
                {
                    stack.Children.Add(new Border
                    {
                        BackgroundColor = Colors.Gray,
                        HeightRequest = 1,
                        HorizontalOptions = LayoutOptions.Fill,
                        Margin = new Thickness(0, 0, 0, 0)
                    });
                    stack.Children.Add(AuftragWSO.GetOrderInfoElement(o, model));
                    o.kategorien.ForEach(c =>
                    {
                        var foundSelected = c.leistungen.Find(f => f.selected == true);
                        if (foundSelected != null)
                        {
                            stack.Children.Add(new Border
                            {
                                BackgroundColor = Colors.Gray,
                                HeightRequest = 1,
                                HorizontalOptions = LayoutOptions.Fill,
                                Margin = new Thickness(0, 0, 0, 0)
                            });
                            stack.Children.Add(KategorieWSO.GetCategoryInfoElement(c, model));
                            stack.Children.Add(new Border
                            {
                                BackgroundColor = Colors.Gray,
                                HeightRequest = 1,
                                HorizontalOptions = LayoutOptions.Fill,
                                Margin = new Thickness(0, 0, 0, 0)
                            });
                            var positions = c.leistungen.OrderBy(s => s.prio.days).ToList();//.ThenBy(s => s.Name);
                            positions.ForEach(pos =>
                            {
                                if (pos.selected)
                                {
                                    var stackPos = GetSelectedPositionSmallCardView(pos, model, func, funcB);
                                    model.allPositionInShowingSmallListView.Add(pos.id, stackPos);
                                    stack.Children.Add(stackPos);
                                }
                            });
                        }
                    });
                }
            });
            return stack;
        }

        public static SwipeView GetSelectedPositionSmallCardView(LeistungWSO leistung, AppModel model, ICommand func, ICommand funcB)
        {
            var item = new SwipeItem
            {
                Text = "Entfernen",
                IconImageSource = "trash.png",
                BackgroundColor = Colors.DarkRed,
                Command = func,
                CommandParameter = leistung,
            };
            var swipeItems = new SwipeItems
            {
                Mode = SwipeMode.Execute,
            };
            swipeItems.Add(item);
            var swipe = new SwipeView
            {
                Margin = new Thickness(34, 1, 0, 1),
                BackgroundColor = Colors.Transparent,
                RightItems = swipeItems,
            };



            var imageL = new Image
            {
                Margin = new Thickness(0, 0, 5, 0),
                HeightRequest = 24,
                WidthRequest = 24,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Source = (leistung.art == "Leistung" ? "leistung_symbol.png" :
                            (leistung.art == "Produkt" ? "produkt_symbol.png" :
                                (leistung.art == "Texte" ? "text_symbol.png" :
                                (leistung.art == "Check" ? "check_white.png" :
                                    "quest.png"
                         ))))
            };
            //var _removeBtn = new ImageButton
            //{
            //    Margin = new Thickness(2, 0, 0, 0),
            //    HeightRequest = 24,
            //    WidthRequest = 24,
            //    VerticalOptions = LayoutOptions.Start,
            //    HorizontalOptions = LayoutOptions.Start,
            //    Source = "x_image_bold_red.png",
            //    BackgroundColor = Colors.Transparent,
            //    Command = func,
            //    CommandParameter = leistung
            //};

            var lb = new Label()
            {
                Text = leistung.GetMobileText(),
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(5, 0, 5, 1),
                FontSize = 14,
                HeightRequest = 34,
                HorizontalOptions = LayoutOptions.Start,
                LineBreakMode = LineBreakMode.WordWrap,
            };
            var status = new Label
            {
                Text = "Direkterfassung: " + leistung.dstd.ToString("00") + ":" + leistung.dmin.ToString("00"),
                TextColor = Color.FromArgb("#999999"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.Start,
                IsVisible = leistung.type == "1" && leistung.art != "Produkt",
            };
            var vGrid = new Grid()
            {
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                RowSpacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
                RowDefinitions = {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
            };
            vGrid.Add(lb,0,0);
            vGrid.Add(status,0,1);

            
            if (leistung.art == "Produkt")
            {
                var vAnzahlGrid = new Grid()
                {
                    Padding = new Thickness(5, 2, 5, 0),
                    Margin = new Thickness(0, 5, 0, 0),
                    RowSpacing = 0,
                    BackgroundColor = Color.FromArgb("#144d73"),
                    RowDefinitions = {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                },
                };
                var lbanzahl = new Label()
                {
                    Text = "Anzahl (" + Utils.getEinheitStr(leistung.einheit) + "):",
                    TextColor = Color.FromArgb("#aaaaaa"),
                    Margin = new Thickness(0, 0, 0, 0),
                    Padding = new Thickness(5, 0, 0, 0),
                    FontSize = 11,
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Start,
                };
                var SelectedPositionSmallCardViewEntry = new CustomEntry()
                {
                    Margin = new Thickness(0, -17, 0, -5),
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Start,
                    TextColor = Colors.White,
                    FontSize = 16,
                    Keyboard = Keyboard.Numeric,
                    HeightRequest = 40,
                    Text = Utils.formatDEStr(decimal.Parse(leistung.anzahl) > 0 ? decimal.Parse(leistung.anzahl) : 1),
                    MinimumWidthRequest = 100,
                    HorizontalTextAlignment = TextAlignment.End,
                    BackgroundColor = Colors.Transparent
                };

                leistung.produktAnzahl = Utils.formatDEStr(decimal.Parse(leistung.anzahl) > 0 ? decimal.Parse(leistung.anzahl) : 1);
                SelectedPositionSmallCardViewEntry.ReturnCommandParameter = leistung;
                SelectedPositionSmallCardViewEntry.Unfocused -= UnfocusedAnzahlChange;
                SelectedPositionSmallCardViewEntry.Unfocused += UnfocusedAnzahlChange;
                SelectedPositionSmallCardViewEntry.TextChanged -= SelectedPositionSmallCardViewEntryChanged;
                SelectedPositionSmallCardViewEntry.TextChanged += SelectedPositionSmallCardViewEntryChanged;
                vAnzahlGrid.Add(lbanzahl, 0, 0);
                vAnzahlGrid.Add(SelectedPositionSmallCardViewEntry, 0, 1);

                var addBtn = new VerticalStackLayout()
                {
                    Padding = new Thickness(5, 2, 5, 2),
                    Margin = new Thickness(3, 0, 0, 0),
                    Spacing = 0,
                    WidthRequest = 50,
                    BackgroundColor = Color.FromArgb("#04532d"),
                    Children = {
                    new Label()
                    {
                        Text = "+",
                        TextColor = Color.FromArgb("#ffffff"),
                        Margin = new Thickness(0),
                        Padding = new Thickness(0),
                        FontSize = 30,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                    }

                }
                };
                addBtn.GestureRecognizers.Clear();
                var t_addBtn = new TapGestureRecognizer();
                t_addBtn.Tapped += (object o, TappedEventArgs ev) => { AddSubAnzahlChange(leistung, SelectedPositionSmallCardViewEntry, 1); };
                addBtn.GestureRecognizers.Add(t_addBtn);
                var subBtn = new VerticalStackLayout()
                {
                    Padding = new Thickness(5, 2, 5, 2),
                    Margin = new Thickness(3, 0, 0, 0),
                    Spacing = 0,
                    WidthRequest = 50,
                    BackgroundColor = Color.FromArgb("#73042d"),
                    Children = {
                    new Label()
                    {
                        Text = "-",
                        TextColor = Color.FromArgb("#ffffff"),
                        Margin = new Thickness(0),
                        Padding = new Thickness(0),
                        FontSize = 30,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                    }

                }
                };
                subBtn.GestureRecognizers.Clear();
                var t_subBtn = new TapGestureRecognizer();
                t_subBtn.Tapped += (object o, TappedEventArgs ev) => { AddSubAnzahlChange(leistung, SelectedPositionSmallCardViewEntry, -1); };
                subBtn.GestureRecognizers.Add(t_subBtn);
                var ho_anzahlGrid = new Grid()
                {
                    Padding = new Thickness(0),
                    Margin = new Thickness(0),
                    ColumnSpacing = 0,
                    HorizontalOptions = LayoutOptions.Fill,
                    ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                };
                ho_anzahlGrid.Add(vAnzahlGrid, 0, 0);
                ho_anzahlGrid.Add(addBtn, 1, 0);
                ho_anzahlGrid.Add(subBtn, 2, 0);

                vGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                vGrid.Add(ho_anzahlGrid,0,2);
            }
            if (leistung.muell == 1)
            {
                var hmuell = new Grid()
                {
                    Padding = new Thickness(0),
                    Margin = new Thickness(0),
                    ColumnSpacing = 0,
                    HorizontalOptions = LayoutOptions.Fill,
                    IsVisible = leistung.muell == 1,
                    ColumnDefinitions = {
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition { Width = GridLength.Auto },
                    }
                };
                var imageMuellSign2 = new Image
                {
                    Margin = new Thickness(0, -6, 0, 0),
                    HeightRequest = 26,
                    WidthRequest = 26,
                    VerticalOptions = LayoutOptions.Start,
                    HorizontalOptions = LayoutOptions.End,
                    Source = leistung.muell == 1 ? (leistung.inout.inout == 0 ? "muell_out.png" : "muell_in_tonne.png") : null,
                };
                var imageMuellSign = new Image
                {
                    Margin = new Thickness(0, -6, 0, 0),
                    HeightRequest = 26,
                    WidthRequest = 26,
                    VerticalOptions = LayoutOptions.Start,
                    HorizontalOptions = LayoutOptions.End,
                    Source = leistung.muell == 1 ? (leistung.inout.inout == 0 ? "muell_out_tonne.png" : "muell_in.png") : null,
                };
                var lbMuell = new Label()
                {
                    Text = leistung.muell == 1 ? (leistung.inout.inout == 0 ? "Ich werde RAUSSTELLEN" : "Ich werde REINSTELLEN") : "",
                    TextColor = Color.FromArgb(leistung.muell == 1 ? (leistung.inout.inout == 0 ? "#dd0000" : "#00aa00") : "#cccccc"),
                    Margin = new Thickness(0, 0, 0, 0),
                    Padding = new Thickness(5, 0, 0, 0),
                    FontSize = 16,
                    HorizontalOptions = LayoutOptions.Start,
                };
                var hmuellquest = new Grid()
                {
                    Padding = new Thickness(5, 5, 5, 5),
                    Margin = new Thickness(5, 8, 0, 2),
                    ColumnSpacing = 0,
                    HorizontalOptions = LayoutOptions.Fill,
                    IsVisible = leistung.muell == 1,
                    BackgroundColor = Color.FromArgb("#1f74ad"),
                    ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto },
                }
                };
                hmuellquest.Add(new Image
                {
                    Margin = new Thickness(0, 0, 10, 0),
                    HeightRequest = 23,
                    WidthRequest = 23,
                    VerticalOptions = LayoutOptions.Start,
                    HorizontalOptions = LayoutOptions.End,
                    Source = "change.png",
                }, 0, 0);
                hmuellquest.Add(new Label()
                {
                    Text = "ÄNDERN",// + ( leistung.inout != null && leistung.inout.inout == 1 ? "RAUSSTELLEN":"REINSTELLEN"),
                    TextColor = Color.FromArgb("#ffffff"),
                    Margin = new Thickness(0),
                    Padding = new Thickness(0),
                    FontSize = 16,
                    HorizontalOptions = LayoutOptions.Start,
                }, 1, 0);
                if (funcB != null && leistung.muell == 1)
                {
                    hmuellquest.GestureRecognizers.Clear();
                    hmuellquest.GestureRecognizers.Add(new TapGestureRecognizer()
                    {
                        Command = funcB,
                        CommandParameter = new ChangeSelectedMuellPos
                        {
                            img = imageMuellSign,
                            img2 = imageMuellSign2,
                            lb = lbMuell,
                            pos = leistung
                        }
                    });
                }
                hmuell.Add(lbMuell,0,0);
                hmuell.Add(imageMuellSign,1,0);
                hmuell.Add(imageMuellSign2,2,0);

                vGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                vGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                vGrid.Add(hmuell,0,2);
                vGrid.Add(hmuellquest,1,3);
            }

            var h = new Grid()
            {
                Padding = new Thickness(5, 5, 5, 5),
                Margin = new Thickness(0, 0, 0, 0),
                ColumnSpacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Color.FromArgb("#042d53"),
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star },
                },
            };
            h.Add(imageL,0,0);
            h.Add(vGrid,1,0);
            swipe.Content = h;

            return swipe;// mainFrame;
        }

        //public static void SelectedPositionSmallCardViewEntryChanged(object sender, TextChangedEventArgs e)
        //{
        //    var entry = (CustomEntry)sender;
        //    var leiInWork = (LeistungWSO)entry.ReturnCommandParameter;
        //    entry.TextChanged -= SelectedPositionSmallCardViewEntryChanged;
        //    try
        //    {
        //        string text = entry.Text.Replace(".", "").Replace(",", "");
        //        if (String.IsNullOrWhiteSpace(text)) { text = "0,00"; }
        //        var t = decimal.Parse(text) / 100;
        //        entry.Text = Utils.formatDEStr(t);
        //        leiInWork.produktAnzahl = entry.Text;
        //    }
        //    finally
        //    {
        //        entry.TextChanged += SelectedPositionSmallCardViewEntryChanged;
        //    }
        //}

        static bool _updatingProduktAnzahl;
        static readonly CultureInfo De = CultureInfo.GetCultureInfo("de-DE");

        public static void SelectedPositionSmallCardViewEntryChanged(object sender, TextChangedEventArgs e)
        {
            if (_updatingProduktAnzahl) return;

            var entry = (CustomEntry)sender;
            var leiInWork = (LeistungWSO)entry.ReturnCommandParameter;

            entry.Dispatcher.Dispatch(() =>
            {
                if (_updatingProduktAnzahl) return;
                _updatingProduktAnzahl = true;
                try
                {
                    var raw = (entry.Text ?? "").Replace(".", "").Replace(",", "").Trim();
                    if (string.IsNullOrWhiteSpace(raw))
                        raw = "0";

                    if (!decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var v))
                        v = 0m;

                    var t = v / 100m;
                    var formatted = Utils.formatDEStr(t);

                    if (entry.Text != formatted)
                        entry.Text = formatted;

                    leiInWork.produktAnzahl = entry.Text;
                }
                finally
                {
                    _updatingProduktAnzahl = false;
                }
            });
        }




        //public static void UnfocusedAnzahlChange(object sender, FocusEventArgs e)
        //{
        //    var entry = (CustomEntry)sender;
        //    entry.Text = Utils.formatDEStr(decimal.Parse(entry.Text));
        //    var leistung = (LeistungWSO)entry.ReturnCommandParameter;
        //    leistung.produktAnzahl = entry.Text;
        //}
        static bool _updatingUnfocusedAnzahl;

        public static void UnfocusedAnzahlChange(object sender, FocusEventArgs e)
        {
            if (_updatingUnfocusedAnzahl) return;

            var entry = (CustomEntry)sender;
            var leistung = (LeistungWSO)entry.ReturnCommandParameter;

            _updatingUnfocusedAnzahl = true;
            try
            {
                var text = (entry.Text ?? "").Trim();

                // typische „Zwischenzustände“ abfangen
                if (text.StartsWith(".")) text = text.Substring(1);
                if (text.StartsWith(",")) text = "0" + text;
                if (string.IsNullOrWhiteSpace(text)) text = "0";

                // falls du sowohl "." als auch "," zulässt, ggf. normalisieren:
                // text = text.Replace(".", "").Replace(",", ".");  // je nach gewünschter Kultur
                // oder besser: explizit mit de-DE parsen:

                if (!decimal.TryParse(text, out var value))
                    value = 0m;

                var formatted = Utils.formatDEStr(value);

                if (entry.Text != formatted)
                    entry.Text = formatted;

                leistung.produktAnzahl = entry.Text;
            }
            finally
            {
                _updatingUnfocusedAnzahl = false;
            }
        }



        public static void AddSubAnzahlChange(LeistungWSO l, CustomEntry entry, int add)
        {
            var dec = decimal.Parse(entry.Text) + add;
            if (dec < 0) { dec = 0; }
            entry.Text = Utils.formatDEStr(dec);
            l.produktAnzahl = entry.Text;
        }



        public static VerticalStackLayout GetSelectedPositionAgainListView(ICommand func, ICommand funcB)
        {
            var stack = new VerticalStackLayout
            {
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
            };

            AppModel.Instance.allPositionAgainInShowingSmallListView.Clear();

            AppModel.Instance.LastBuilding.ArrayOfAuftrag.ForEach(o =>
            {
                LeistungWSO foundInOrderSelected = null;
                o.kategorien.ForEach(c =>
                {
                    if (foundInOrderSelected == null)
                    {
                        foundInOrderSelected = c.leistungen.Find(f => f.selected == true);
                    }
                });
                if (foundInOrderSelected != null)
                {
                    stack.Children.Add(new Border
                    {
                        BackgroundColor = Colors.Gray,
                        HeightRequest = 1,
                        HorizontalOptions = LayoutOptions.Fill,
                        Margin = new Thickness(0, 0, 0, 0)
                    });
                    stack.Children.Add(AuftragWSO.GetOrderInfoElement(o, AppModel.Instance));
                    o.kategorien.ForEach(c =>
                    {
                        var foundSelected = c.leistungen.Find(f => f.selected == true);
                        if (foundSelected != null)
                        {
                            stack.Children.Add(new Border
                            {
                                BackgroundColor = Colors.Gray,
                                HeightRequest = 1,
                                HorizontalOptions = LayoutOptions.Fill,
                                Margin = new Thickness(0, 0, 0, 0)
                            });
                            stack.Children.Add(KategorieWSO.GetCategoryInfoElement(c, AppModel.Instance));
                            stack.Children.Add(new Border
                            {
                                BackgroundColor = Colors.Gray,
                                HeightRequest = 1,
                                HorizontalOptions = LayoutOptions.Fill,
                                Margin = new Thickness(0, 0, 0, 0)
                            });
                            var positions = c.leistungen.OrderBy(s => s.prio.days).ToList();//.ThenBy(s => s.Name);
                            positions.ForEach(pos =>
                            {
                                if (pos.selected)
                                {
                                    var stackPos = GetSelectedPositionAgainSmallCardView(pos, AppModel.Instance, func, funcB);
                                    AppModel.Instance.allPositionAgainInShowingSmallListView.Add(pos.id, stackPos);
                                    stack.Children.Add(stackPos);
                                }
                            });
                        }
                    });
                }
            });
            return stack;
        }
        public static SwipeView GetSelectedPositionAgainSmallCardView(LeistungWSO leistung, AppModel model, ICommand func,ICommand funcB)
        {
            var item = new SwipeItem
            {
                Text = "Entfernen",
                IconImageSource = "trash.png",
                BackgroundColor = Colors.DarkRed,
                Command = func,
                CommandParameter = leistung,
            };
            var swipeItems = new SwipeItems
            {
                Mode = SwipeMode.Execute,
            };
            swipeItems.Add(item);
            var swipe = new SwipeView
            {
                Margin = new Thickness(34, 1, 0, 1),
                BackgroundColor = Colors.Transparent,
                RightItems = swipeItems,
            };



            var imageL = new Image
            {
                Margin = new Thickness(0, 0, 5, 0),
                HeightRequest = 24,
                WidthRequest = 24,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Source = (leistung.art == "Leistung" ? "leistung_symbol.png" :
                            (leistung.art == "Produkt" ? "produkt_symbol.png" :
                                (leistung.art == "Texte" ? "text_symbol.png" :
                                (leistung.art == "Check" ? "check_white.png" :
                                    "quest.png"
                         ))))
            };
            //var _removeBtn = new ImageButton
            //{
            //    Margin = new Thickness(2, 0, 0, 0),
            //    HeightRequest = 24,
            //    WidthRequest = 24,
            //    VerticalOptions = LayoutOptions.Start,
            //    HorizontalOptions = LayoutOptions.Start,
            //    Source = "x_image_bold_red.png",
            //    BackgroundColor = Colors.Transparent,
            //    Command = func,
            //    CommandParameter = leistung
            //};

            var lb = new Label()
            {
                Text = leistung.GetMobileText(),
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(5, 0, 5, 1),
                FontSize = 14,
                HeightRequest = 34,
                HorizontalOptions = LayoutOptions.Start,
                LineBreakMode = LineBreakMode.WordWrap,
            };
            var status = new Label
            {
                Text = "Direkterfassung: " + leistung.dstd.ToString("00") + ":" + leistung.dmin.ToString("00"),
                TextColor = Color.FromArgb("#999999"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.Start,
                IsVisible = leistung.type == "1" && leistung.art != "Produkt",
            };
            var vGrid = new Grid()
            {
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                RowSpacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
                RowDefinitions = {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
            };
            vGrid.Add(lb, 0, 0);
            vGrid.Add(status, 0, 1);


            if (leistung.art == "Produkt")
            {
                var vAnzahlGrid = new Grid()
                {
                    Padding = new Thickness(5, 2, 5, 0),
                    Margin = new Thickness(0, 5, 0, 0),
                    RowSpacing = 0,
                    BackgroundColor = Color.FromArgb("#144d73"),
                    RowDefinitions = {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                },
                };
                var lbanzahl = new Label()
                {
                    Text = "Anzahl (" + Utils.getEinheitStr(leistung.einheit) + "):",
                    TextColor = Color.FromArgb("#aaaaaa"),
                    Margin = new Thickness(0, 0, 0, 0),
                    Padding = new Thickness(5, 0, 0, 0),
                    FontSize = 11,
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Start,
                };
                var SelectedPositionAgainSmallCardViewEntry = new CustomEntry()
                {
                    Margin = new Thickness(0, -17, 0, -5),
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Start,
                    TextColor = Colors.White,
                    FontSize = 16,
                    Keyboard = Keyboard.Numeric,
                    HeightRequest = 40,
                    Text = Utils.formatDEStr(decimal.Parse(leistung.anzahl) > 0 ? decimal.Parse(leistung.anzahl) : 1),
                    MinimumWidthRequest = 100,
                    HorizontalTextAlignment = TextAlignment.End,
                    BackgroundColor = Colors.Transparent
                };

                leistung.produktAnzahl = Utils.formatDEStr(decimal.Parse(leistung.anzahl) > 0 ? decimal.Parse(leistung.anzahl) : 1);
                SelectedPositionAgainSmallCardViewEntry.ReturnCommandParameter = leistung;
                SelectedPositionAgainSmallCardViewEntry.Unfocused -= UnfocusedAnzahlChange;
                SelectedPositionAgainSmallCardViewEntry.Unfocused += UnfocusedAnzahlChange;
                SelectedPositionAgainSmallCardViewEntry.TextChanged -= SelectedPositionAgainSmallCardViewEntryChanged;
                SelectedPositionAgainSmallCardViewEntry.TextChanged += SelectedPositionAgainSmallCardViewEntryChanged;
                vAnzahlGrid.Add(lbanzahl, 0, 0);
                vAnzahlGrid.Add(SelectedPositionAgainSmallCardViewEntry, 0, 1);

                var addBtn = new VerticalStackLayout()
                {
                    Padding = new Thickness(5, 2, 5, 2),
                    Margin = new Thickness(3, 0, 0, 0),
                    Spacing = 0,
                    WidthRequest = 50,
                    BackgroundColor = Color.FromArgb("#04532d"),
                    Children = {
            new Label()
            {
                Text = "+",
                TextColor = Color.FromArgb("#ffffff"),
                Margin = new Thickness(0),
                Padding = new Thickness(0),
                        FontSize = 30,
                        HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            }

        }
                };
                addBtn.GestureRecognizers.Clear();
                var t_addBtn = new TapGestureRecognizer();
                t_addBtn.Tapped += (object o, TappedEventArgs ev) => { AddSubAnzahlChange(leistung, SelectedPositionAgainSmallCardViewEntry, 1); };
                addBtn.GestureRecognizers.Add(t_addBtn);
                var subBtn = new VerticalStackLayout()
                {
                    Padding = new Thickness(5, 2, 5, 2),
                    Margin = new Thickness(3, 0, 0, 0),
                    Spacing = 0,
                    WidthRequest = 50,
                    BackgroundColor = Color.FromArgb("#73042d"),
                    Children = {
            new Label()
            {
                Text = "-",
                TextColor = Color.FromArgb("#ffffff"),
                Margin = new Thickness(0),
                Padding = new Thickness(0),
                        FontSize = 30,
                        HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            }

        }
                };
                subBtn.GestureRecognizers.Clear();
                var t_subBtn = new TapGestureRecognizer();
                t_subBtn.Tapped += (object o, TappedEventArgs ev) => { AddSubAnzahlChange(leistung, SelectedPositionAgainSmallCardViewEntry, -1); };
                subBtn.GestureRecognizers.Add(t_subBtn);
                var ho_anzahlGrid = new Grid()
                {
                    Padding = new Thickness(0),
                    Margin = new Thickness(0),
                    ColumnSpacing = 0,
                    HorizontalOptions = LayoutOptions.Fill,
                    ColumnDefinitions =
        {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                };
                ho_anzahlGrid.Add(vAnzahlGrid, 0, 0);
                ho_anzahlGrid.Add(addBtn, 1, 0);
                ho_anzahlGrid.Add(subBtn, 2, 0);

                vGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                vGrid.Add(ho_anzahlGrid, 0, 2);
            }
            if (leistung.muell == 1)
            {
                var hmuell = new Grid()
                {
                    Padding = new Thickness(0),
                    Margin = new Thickness(0),
                    ColumnSpacing = 0,
                    HorizontalOptions = LayoutOptions.Fill,
                    IsVisible = leistung.muell == 1,
                    ColumnDefinitions = {
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition { Width = GridLength.Auto },
                    }
                };
                var imageMuellSign2 = new Image
                {
                    Margin = new Thickness(0, -6, 0, 0),
                    HeightRequest = 26,
                    WidthRequest = 26,
                    VerticalOptions = LayoutOptions.Start,
                    HorizontalOptions = LayoutOptions.End,
                    Source = leistung.muell == 1 ? (leistung.inout.inout == 0 ? "muell_out.png" : "muell_in_tonne.png") : null,
                };
                var imageMuellSign = new Image
                {
                    Margin = new Thickness(0, -6, 0, 0),
                    HeightRequest = 26,
                    WidthRequest = 26,
                    VerticalOptions = LayoutOptions.Start,
                    HorizontalOptions = LayoutOptions.End,
                    Source = leistung.muell == 1 ? (leistung.inout.inout == 0 ? "muell_out_tonne.png" : "muell_in.png") : null,
                };
                var lbMuell = new Label()
                {
                    Text = leistung.muell == 1 ? (leistung.inout.inout == 0 ? "Ich werde RAUSSTELLEN" : "Ich werde REINSTELLEN") : "",
                    TextColor = Color.FromArgb(leistung.muell == 1 ? (leistung.inout.inout == 0 ? "#dd0000" : "#00aa00") : "#cccccc"),
                    Margin = new Thickness(0, 0, 0, 0),
                    Padding = new Thickness(5, 0, 0, 0),
                    FontSize = 16,
                    HorizontalOptions = LayoutOptions.Start,
                };
                var hmuellquest = new Grid()
                {
                    Padding = new Thickness(5, 5, 5, 5),
                    Margin = new Thickness(5, 8, 0, 2),
                    ColumnSpacing = 0,
                    HorizontalOptions = LayoutOptions.Fill,
                    IsVisible = leistung.muell == 1,
                    BackgroundColor = Color.FromArgb("#1f74ad"),
                    ColumnDefinitions =
    {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto },
    }
                };
                hmuellquest.Add(new Image
                {
                    Margin = new Thickness(0, 0, 10, 0),
                    HeightRequest = 23,
                    WidthRequest = 23,
                    VerticalOptions = LayoutOptions.Start,
                    HorizontalOptions = LayoutOptions.End,
                    Source = "change.png",
                }, 0, 0);
                hmuellquest.Add(new Label()
                {
                    Text = "ÄNDERN",// + ( leistung.inout != null && leistung.inout.inout == 1 ? "RAUSSTELLEN":"REINSTELLEN"),
                    TextColor = Color.FromArgb("#ffffff"),
                    Margin = new Thickness(0),
                    Padding = new Thickness(0),
                    FontSize = 16,
                    HorizontalOptions = LayoutOptions.Start,
                }, 1, 0);
                if (funcB != null && leistung.muell == 1)
                {
                    hmuellquest.GestureRecognizers.Clear();
                    hmuellquest.GestureRecognizers.Add(new TapGestureRecognizer()
                    {
                        Command = funcB,
                        CommandParameter = new ChangeSelectedMuellPos
                        {
                            img = imageMuellSign,
                            img2 = imageMuellSign2,
                            lb = lbMuell,
                            pos = leistung
                        }
                    });
                }
                hmuell.Add(lbMuell, 0, 0);
                hmuell.Add(imageMuellSign, 1, 0);
                hmuell.Add(imageMuellSign2, 2, 0);

                vGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                vGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                vGrid.Add(hmuell, 0, 2);
                vGrid.Add(hmuellquest, 1, 3);
            }

            var h = new Grid()
            {
                Padding = new Thickness(5, 5, 5, 5),
                Margin = new Thickness(0, 0, 0, 0),
                ColumnSpacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Color.FromArgb("#042d53"),
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star },
                },
            };
            h.Add(imageL, 0, 0);
            h.Add(vGrid, 1, 0);
            swipe.Content = h;

            return swipe;// mainFrame;
        }

        //public static void SelectedPositionAgainSmallCardViewEntryChanged(object sender, TextChangedEventArgs e)
        //{
        //    var entry = (CustomEntry)sender;
        //    var leiInWork = (LeistungWSO)entry.ReturnCommandParameter;
        //    entry.TextChanged -= SelectedPositionAgainSmallCardViewEntryChanged;
        //    try
        //    {
        //        string text = entry.Text.Replace(".", "").Replace(",", "");
        //        if (String.IsNullOrWhiteSpace(text)) { text = "0,00"; }
        //        var t = decimal.Parse(text) / 100;
        //        entry.Text = Utils.formatDEStr(t);
        //        leiInWork.produktAnzahl = entry.Text;
        //    }
        //    finally
        //    {
        //        entry.TextChanged += SelectedPositionAgainSmallCardViewEntryChanged;
        //    }
        //}
        public static void SelectedPositionAgainSmallCardViewEntryChanged(object sender, TextChangedEventArgs e)
        {
            if (_updatingProduktAnzahl)
                return;

            var entry = (CustomEntry)sender;
            var leiInWork = (LeistungWSO)entry.ReturnCommandParameter;

            // Nach dem aktuellen afterTextChanged-Call ausführen -> deutlich weniger Emoji2/Samsung-Probleme
            entry.Dispatcher.Dispatch(() =>
            {
                if (_updatingProduktAnzahl)
                    return;

                _updatingProduktAnzahl = true;
                try
                {
                    // Nur Ziffern behalten (du entfernst . und ,)
                    var raw = (entry.Text ?? string.Empty)
                        .Replace(".", string.Empty)
                        .Replace(",", string.Empty)
                        .Trim();

                    if (string.IsNullOrWhiteSpace(raw))
                        raw = "0"; // wichtig: nicht "0,00", weil wir gleich nur Ziffern parsen

                    // raw enthält jetzt nur Ziffern => InvariantCulture ist passend
                    if (!decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var v))
                        v = 0m;

                    var t = v / 100m;
                    var formatted = Utils.formatDEStr(t);

                    // unnötige Setzungen vermeiden
                    if (!string.Equals(entry.Text, formatted, StringComparison.Ordinal))
                        entry.Text = formatted;

                    leiInWork.produktAnzahl = entry.Text;
                }
                finally
                {
                    _updatingProduktAnzahl = false;
                }
            });
        }





        public static void AnzahlAgainChange(object sender, FocusEventArgs e)
        {
            var entry = (CustomEntry)sender;
            entry.Text = Utils.formatDEStr(decimal.Parse(entry.Text));
            var leistung = (LeistungWSO)entry.ReturnCommandParameter;
            leistung.produktAnzahl = entry.Text;
        }

        public static VerticalStackLayout GetInWorkPositionListView(AppModel model, ICommand func)
        {
            var stack = new VerticalStackLayout
            {
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 3),
                Spacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
            };

            try
            {
                if (model?.allPositionInWork?.leistungen == null)
                {
                    AppModel.Logger?.Warn("WARN: GetInWorkPositionListView called without allPositionInWork.leistungen.");
                    stack.Children.Add(new Label
                    {
                        Text = "GetInWorkPositionListView:(NULL) Keine Positionen in Arbeit.",
                        TextColor = Color.FromArgb("#cccccc"),
                        Margin = new Thickness(5, 5, 5, 5),
                        FontSize = 14,
                        LineBreakMode = LineBreakMode.WordWrap,
                    });
                    return stack;
                }

                foreach (var lei in model.allPositionInWork.leistungen)
                {
                    if (lei == null)
                        continue;

                    var building = model.LastBuilding ?? BuildingWSO.LoadBuilding(model, lei.objektid);
                    if (building?.ArrayOfAuftrag == null)
                    {
                        AppModel.Logger?.Warn($"WARN: Building or ArrayOfAuftrag missing for InWork item {lei.id} (objektid={lei.objektid}).");
                        continue;
                    }

                    var o = building.ArrayOfAuftrag.Find(auf => auf != null && auf.id == lei.auftragid);
                    if (o?.kategorien == null)
                    {
                        AppModel.Logger?.Warn($"WARN: Auftrag missing for InWork item {lei.id} (auftragid={lei.auftragid}).");
                        continue;
                    }

                    var c = o.kategorien.Find(kat => kat != null && kat.id == lei.kategorieid);
                    if (c?.leistungen == null)
                    {
                        AppModel.Logger?.Warn($"WARN: Kategorie missing for InWork item {lei.id} (kategorieid={lei.kategorieid}).");
                        continue;
                    }

                    var l = c.leistungen.Find(f => f != null && f.id == lei.id);
                    if (l == null)
                    {
                        AppModel.Logger?.Warn($"WARN: Leistung missing for InWork item {lei.id}.");
                        continue;
                    }

                    var stackPos = GetInWorkPositionSmallCardView(o, c, l, lei, model, func);
                    stack.Children.Add(stackPos);
                }

                return stack;
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error(ex, "ERROR: LeistungWSO - GetInWorkPositionListView(): ");
                return stack;
            }
        }
        public static VerticalStackLayout GetInWorkPositionListView_OLD(AppModel model, ICommand func)
        {

            var stack = new VerticalStackLayout
            {
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 3),
                Spacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
            };
            try
            {
                model.allPositionInWork.leistungen.ForEach(lei =>
                {
                    BuildingWSO building;
                    if (model.LastBuilding == null)
                    {
                        building = BuildingWSO.LoadBuilding(model, lei.objektid);
                    }
                    else
                    {
                        building = model.LastBuilding;
                    }
                    var o = building.ArrayOfAuftrag.Find(auf => auf.id == lei.auftragid);
                    var c = o.kategorien.Find(kat => kat.id == lei.kategorieid);
                    var l = c.leistungen.Find(f => f.id == lei.id);
                    var stackPos = GetInWorkPositionSmallCardView(o, c, l, lei, model, func);
                    stack.Children.Add(stackPos);
                });
                return stack;

            }
            catch (Exception ex)
            {
                AppModel.Logger.Error(ex, "ERROR: LeistungWSO - GetInWorkPositionListView(): ");
                return stack;
            }
        }

        public static Grid GetInWorkPositionSmallCardView(AuftragWSO o, KategorieWSO c, LeistungWSO leistung, LeistungInWorkWSO leiInWork, AppModel model, ICommand func)
        {
            //var _prio = CalcOverdue(pos);
            var imageL = new Image
            {
                Margin = new Thickness(0, 0, 5, 0),
                HeightRequest = 32,
                WidthRequest = 32,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Source = (leistung.art == "Leistung" ? "leistung_symbol.png" :
                            (leistung.art == "Produkt" ? "produkt_symbol.png" :
                                (leistung.art == "Texte" ? "text_symbol.png" :
                                (leistung.art == "Check" ? "check_white.png" :
                                    "quest.png"
                         ))))
            };
            var lb = new Label()
            {
                Text = leistung.GetMobileText(),
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(5, 0, 5, 1),
                FontSize = 14,
                LineBreakMode = LineBreakMode.WordWrap,
            };
            var order = new Label
            {
                Text = "Auftrag: " + o.GetMobileText(),
                TextColor = Color.FromArgb("#ffffff"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12, 
                FontAttributes = FontAttributes.Bold,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.Fill,
            };
            var category = new Label
            {
                Text = "Kategorie: " + c.GetMobileText(),
                TextColor = Color.FromArgb("#ffffff"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.Fill,
            };

            var btnNoticeFrame = new Border()
            {
                Padding = new Thickness(1, 1, 1, 1),
                Margin = new Thickness(3, 0, 3, 0),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                BackgroundColor = Color.FromArgb("#144d73"),
                Content = new VerticalStackLayout
                {
                    HeightRequest = 40,
                    WidthRequest = 40,
                    BackgroundColor = Color.FromArgb("144d73"),
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    Spacing = 0,
                    Padding = new Thickness(0, 0, 0, 0),
                    Margin = new Thickness(0, 0, 0, 0),
                    Children = {new Image
                        {
                            Margin = new Thickness(0, 3, 0, 0),
                            HeightRequest = 30,
                            WidthRequest = 30,
                            VerticalOptions = LayoutOptions.Center,
                            HorizontalOptions = LayoutOptions.Center,
                            Source = "cam_message_warn.png"
                        }
                    }
                },
            };
            if (func != null)
            {
                btnNoticeFrame.GestureRecognizers.Clear();
                btnNoticeFrame.GestureRecognizers.Add(new TapGestureRecognizer() { Command = func, CommandParameter = leistung });
            }

            var notices = new Label
            {
                Text = (leiInWork.bemerkungen != null ? leiInWork.bemerkungen.Count : 0) + " Bemerkung(en)",
                TextColor = Color.FromArgb("#ffffff"),
                Margin = new Thickness(12, 0, 0, 0),
                FontSize = 16,
                LineBreakMode = LineBreakMode.TailTruncation,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center,
            };

            var hNote = new HorizontalStackLayout()
            {
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 5, 0, 0),
                Spacing = 0,                
                HorizontalOptions = LayoutOptions.Fill,
                Children = { btnNoticeFrame, notices },
            };



            var vGrid = new Grid()
            {
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                RowSpacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                }
            };

            vGrid.Add(order, 0, 0);
            vGrid.Add(category, 0, 1);
            vGrid.Add(lb, 0, 2);
            vGrid.Add(hNote, 0, 3);




            if (leistung.art == "Produkt")
            {

                var ho_anzahl = new HorizontalStackLayout()
                {
                    Padding = new Thickness(0),
                    Margin = new Thickness(0, 5, 0, 0),
                    Spacing = 0
                };
                var lbanzahl = new Label()
                {
                    Text = "Anzahl (" + Utils.getEinheitStr(leistung.einheit) + "):",
                    TextColor = Color.FromArgb("#aaaaaa"),
                    Margin = new Thickness(0, 0, 0, 0),
                    Padding = new Thickness(5, 0, 0, 0),
                    FontSize = 12,
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Start,
                };
                var InWorkPosSmallCardEntryAnzahl = new CustomEntry()
                {
                    Margin = new Thickness(0, -7, 0, -5),
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Start,
                    TextColor = Colors.White,
                    FontSize = 16,
                    Keyboard = Keyboard.Numeric,
                    HeightRequest = 40,
                    Text = Utils.formatDEStr(decimal.Parse(leiInWork.anzahl) > 0 ? decimal.Parse(leiInWork.anzahl) : 1),
                    MinimumWidthRequest = 100,
                    HorizontalTextAlignment = TextAlignment.End,
                    BackgroundColor = Colors.Transparent
                };
                InWorkPosSmallCardEntryAnzahl.ReturnCommandParameter = leiInWork;
                InWorkPosSmallCardEntryAnzahl.Unfocused -= InWorkAnzahlChange;
                InWorkPosSmallCardEntryAnzahl.Unfocused += InWorkAnzahlChange;
                InWorkPosSmallCardEntryAnzahl.TextChanged -= InWorkPosSmallCardEntryAnzahlChanged;
                InWorkPosSmallCardEntryAnzahl.TextChanged += InWorkPosSmallCardEntryAnzahlChanged;

                var vanzahl = new VerticalStackLayout()
                {
                    Padding = new Thickness(5, 2, 5, 0),
                    Margin = new Thickness(0),
                    Spacing = 0,
                    HorizontalOptions = LayoutOptions.Fill,
                    BackgroundColor = Color.FromArgb("#144d73"),
                };
                var addBtn = new VerticalStackLayout()
                {
                    Padding = new Thickness(5, 5, 5, 2),
                    Margin = new Thickness(3, 0, 0, 0),
                    Spacing = 0,
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
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                    }

                }
                };
                addBtn.GestureRecognizers.Clear();
                var t_addBtn = new TapGestureRecognizer();
                t_addBtn.Tapped += (object ob, TappedEventArgs ev) => { InWorkAddSubAnzahlChange(leiInWork, InWorkPosSmallCardEntryAnzahl, 1); };
                addBtn.GestureRecognizers.Add(t_addBtn);
                var subBtn = new VerticalStackLayout()
                {
                    Padding = new Thickness(5, 5, 5, 2),
                    Margin = new Thickness(3, 0, 0, 0),
                    Spacing = 0,
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
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                    }

                }
                };
                subBtn.GestureRecognizers.Clear();
                var t_subBtn = new TapGestureRecognizer();
                t_subBtn.Tapped += (object ob, TappedEventArgs ev) => { InWorkAddSubAnzahlChange(leiInWork, InWorkPosSmallCardEntryAnzahl, -1); };
                subBtn.GestureRecognizers.Add(t_subBtn);
                vanzahl.Children.Add(lbanzahl);
                vanzahl.Children.Add(InWorkPosSmallCardEntryAnzahl);
                ho_anzahl.Children.Add(vanzahl);
                ho_anzahl.Children.Add(addBtn);
                ho_anzahl.Children.Add(subBtn);


                vGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                vGrid.Add(ho_anzahl, 0, 4);
            }


            if (leistung.muell == 1)
            {
                var hmuell = new Grid()
                {
                    Padding = new Thickness(0),
                    Margin = new Thickness(0),
                    ColumnSpacing = 0,
                    HorizontalOptions = LayoutOptions.Fill,
                    IsVisible = leistung.muell == 1,
                    ColumnDefinitions = {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto },
                }
                };
                var imageMuellSign2 = new Image
                {
                    Margin = new Thickness(0, -6, 0, 0),
                    HeightRequest = 26,
                    WidthRequest = 26,
                    VerticalOptions = LayoutOptions.Start,
                    HorizontalOptions = LayoutOptions.End,
                    Source = leistung.muell == 1 ? (leistung.inout.inout == 0 ? "muell_out.png" : "muell_in_tonne.png") : null,
                };
                var imageMuellSign = new Image
                {
                    Margin = new Thickness(0, -6, 0, 0),
                    HeightRequest = 26,
                    WidthRequest = 26,
                    VerticalOptions = LayoutOptions.Start,
                    HorizontalOptions = LayoutOptions.End,
                    Source = leistung.muell == 1 ? (leistung.inout.inout == 0 ? "muell_out_tonne.png" : "muell_in.png") : null,
                };
                var lbMuell = new Label()
                {
                    Text = leistung.muell == 1 ? (leistung.inout.inout == 0 ? "Ich werde RAUSSTELLEN" : "Ich werde REINSTELLEN") : "",
                    TextColor = Color.FromArgb(leistung.muell == 1 ? (leistung.inout.inout == 0 ? "#dd0000" : "#00aa00") : "#cccccc"),
                    Margin = new Thickness(0, 0, 0, 0),
                    Padding = new Thickness(5, 0, 0, 0),
                    FontSize = 16,
                    HorizontalOptions = LayoutOptions.Start,
                };
//                var hmuellquest = new Grid()
//                {
//                    Padding = new Thickness(5, 5, 5, 5),
//                    Margin = new Thickness(5, 8, 0, 2),
//                    ColumnSpacing = 0,
//                    HorizontalOptions = LayoutOptions.Fill,
//                    IsVisible = leistung.muell == 1,
//                    BackgroundColor = Color.FromArgb("#1f74ad"),
//                    ColumnDefinitions =
//{
//                new ColumnDefinition { Width = GridLength.Auto },
//                new ColumnDefinition { Width = GridLength.Auto },
//}
//                };
                //hmuellquest.Add(new Image
                //{
                //    Margin = new Thickness(0, 0, 10, 0),
                //    HeightRequest = 23,
                //    WidthRequest = 23,
                //    VerticalOptions = LayoutOptions.Start,
                //    HorizontalOptions = LayoutOptions.End,
                //    Source = "change.png",
                //}, 0, 0);
                //hmuellquest.Add(new Label()
                //{
                //    Text = "ÄNDERN",// + ( leistung.inout != null && leistung.inout.inout == 1 ? "RAUSSTELLEN":"REINSTELLEN"),
                //    TextColor = Color.FromArgb("#ffffff"),
                //    Margin = new Thickness(0),
                //    Padding = new Thickness(0),
                //    FontSize = 16,
                //    HorizontalOptions = LayoutOptions.Start,
                //}, 1, 0);
                //if (funcB != null && leistung.muell == 1)
                //{
                //    hmuellquest.GestureRecognizers.Clear();
                //    hmuellquest.GestureRecognizers.Add(new TapGestureRecognizer()
                //    {
                //        Command = funcB,
                //        CommandParameter = new ChangeSelectedMuellPos
                //        {
                //            img = imageMuellSign,
                //            img2 = imageMuellSign2,
                //            lb = lbMuell,
                //            pos = leistung
                //        }
                //    });
                //}
                hmuell.Add(lbMuell, 0, 0);
                hmuell.Add(imageMuellSign, 1, 0);
                hmuell.Add(imageMuellSign2, 2, 0);

                //vGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                vGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                vGrid.Add(hmuell, 0, 4);
                //vGrid.Add(hmuellquest, 1, 3);
            }

            var hGrid = new Grid()
            {
                Padding = new Thickness(5, 5, 5, 5),
                Margin = new Thickness(0, 0, 0, 5),
                ColumnSpacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Color.FromArgb("#99042d53"),
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star }
                }
            };

            hGrid.Add(imageL,0,0);
            hGrid.Add(vGrid,1,0);
            return hGrid;
        }


        //public static void InWorkPosSmallCardEntryAnzahlChanged(object sender, TextChangedEventArgs e)
        //{
        //    var entry = (CustomEntry)sender;
        //    var leiInWork = (LeistungInWorkWSO)entry.ReturnCommandParameter;
        //    entry.TextChanged -= InWorkPosSmallCardEntryAnzahlChanged;
        //    try
        //    {
        //        string text = entry.Text.Replace(".", "").Replace(",", "");
        //        if (String.IsNullOrWhiteSpace(text)) { text = "0,00"; }
        //        var t = decimal.Parse(text) / 100;
        //        entry.Text = Utils.formatDEStr(t);
        //        leiInWork.anzahl = entry.Text;
        //    }
        //    finally
        //    {
        //        entry.TextChanged += InWorkPosSmallCardEntryAnzahlChanged;
        //    }
        //}
        static bool _updatingInWorkPosAnzahl;

        public static void InWorkPosSmallCardEntryAnzahlChanged(object sender, TextChangedEventArgs e)
        {
            if (_updatingInWorkPosAnzahl)
                return;

            var entry = (CustomEntry)sender;
            var leiInWork = (LeistungInWorkWSO)entry.ReturnCommandParameter;

            entry.Dispatcher.Dispatch(() =>
            {
                if (_updatingInWorkPosAnzahl)
                    return;

                _updatingInWorkPosAnzahl = true;
                try
                {
                    var raw = (entry.Text ?? string.Empty)
                        .Replace(".", string.Empty)
                        .Replace(",", string.Empty)
                        .Trim();

                    if (string.IsNullOrWhiteSpace(raw))
                        raw = "0"; // nicht "0,00" (wir parsen nur Ziffern)

                    if (!decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var v))
                        v = 0m;

                    var t = v / 100m;
                    var formatted = Utils.formatDEStr(t);

                    if (!string.Equals(entry.Text, formatted, StringComparison.Ordinal))
                        entry.Text = formatted;

                    leiInWork.anzahl = entry.Text;
                }
                finally
                {
                    _updatingInWorkPosAnzahl = false;
                }
            });
        }



        //public static void InWorkAnzahlChange(object sender, FocusEventArgs e)
        //{
        //    var entry = (CustomEntry)sender;
        //    var leiInWork = (LeistungInWorkWSO)entry.ReturnCommandParameter;
        //    try
        //    {
        //        entry.Text = Utils.formatDEStr(decimal.Parse(entry.Text));
        //    }
        //    catch (Exception)
        //    {
        //        leiInWork.anzahl = "1,00";
        //        return;
        //    }
        //    leiInWork.anzahl = entry.Text;
        //}

        static bool _updatingInWorkAnzahl;

        public static void InWorkAnzahlChange(object sender, FocusEventArgs e)
        {
            if (_updatingInWorkAnzahl) return;

            var entry = (CustomEntry)sender;
            var leiInWork = (LeistungInWorkWSO)entry.ReturnCommandParameter;

            _updatingInWorkAnzahl = true;
            try
            {
                var text = (entry.Text ?? "").Trim();

                // typische Zwischenzustände abfangen
                if (text.StartsWith(".")) text = text.Substring(1);
                if (text.StartsWith(",")) text = "0" + text;
                if (string.IsNullOrWhiteSpace(text)) text = "0";

                // Optional: wenn Nutzer "." als Dezimaltrenner eingibt, in "," überführen:
                // (nur wenn du das wirklich unterstützen willst)
                text = text.Replace(".", ",");

                if (!decimal.TryParse(text, NumberStyles.Number, De, out var value))
                    value = 1.00m; // oder 0m, je nach Fachlogik

                var formatted = Utils.formatDEStr(value);

                if (entry.Text != formatted)
                    entry.Text = formatted;

                leiInWork.anzahl = entry.Text;
            }
            finally
            {
                _updatingInWorkAnzahl = false;
            }
        }

        public static void InWorkAddSubAnzahlChange(LeistungInWorkWSO l, CustomEntry entry, int add)
            {
                var dec = (decimal.Parse(entry.Text) + add);
                if (dec < 0) { dec = 0; }
                entry.Text = Utils.formatDEStr(dec);
                l.anzahl = entry.Text;
            }


        public static Border GetInWorkPositionSmallCardView_DirektPos(AuftragWSO o, KategorieWSO c, LeistungWSO pos, LeistungWSO lei)
        {
            //var _prio = CalcOverdue(pos);
            var imageL = new Image
            {
                Margin = new Thickness(0, 0, 5, 0),
                HeightRequest = 32,
                WidthRequest = 32,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Source = (pos.art == "Leistung" ? "leistung_symbol.png" :
                            (pos.art == "Produkt" ? "produkt_symbol.png" :
                                (pos.art == "Texte" ? "text_symbol.png" :
                                (pos.art == "Check" ? "check_white.png" :
                                    "quest.png"
                         ))))
            };
            var lb = new Label()
            {
                Text = pos.GetMobileText(),
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(5, 0, 5, 1),
                FontSize = 14,
                HorizontalOptions = LayoutOptions.Start,
                LineBreakMode = LineBreakMode.TailTruncation,
            };
            var anzahl = new Label
            {
                Text = "Anzahl: " + lei.anzahl,
                TextColor = Color.FromArgb("#ffcc00"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.Start,
                IsVisible = pos.einheit != "std" && pos.art == "Leistung",
            };
            var order = new Label
            {
                Text = "Auftrag: " + o.GetMobileText(),
                TextColor = Color.FromArgb("#ffffff"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.Fill,
            };
            var category = new Label
            {
                Text = "Kategorie: " + c.GetMobileText(),
                TextColor = Color.FromArgb("#ffffff"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.Fill,
            };
            var h = new StackLayout()
            {
                Padding = new Thickness(5, 5, 5, 5),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Color.FromArgb("#99042d53"),
            };
            var v = new StackLayout()
            {
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.Fill,
            };

            v.Children.Add(order);
            v.Children.Add(category);
            v.Children.Add(lb);
            v.Children.Add(anzahl);

            h.Children.Add(imageL);
            h.Children.Add(v);

            var mainFrame = new Border()
            {
                Padding = new Thickness(1, 1, 1, 1),
                Margin = new Thickness(0, 5, 0, 5),
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Colors.Transparent,
                Content = h,
                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
            };

            return mainFrame;
        }

        public static Border GetInWorkPositionSmallCardView(AuftragWSO o, KategorieWSO c, LeistungWSO pos, LeistungInWorkWSO lei, AppModel model)
        {
            //var _prio = CalcOverdue(pos);
            var imageL = new Image
            {
                Margin = new Thickness(0, 0, 5, 0),
                HeightRequest = 32,
                WidthRequest = 32,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Source = (pos.art == "Leistung" ? "leistung_symbol.png" :
                            (pos.art == "Produkt" ? "produkt_symbol.png" :
                                (pos.art == "Texte" ? "text_symbol.png" :
                                (pos.art == "Check" ? "check_white.png" :
                                    "quest.png"
                         ))))
            };
            var lb = new Label()
            {
                Text = pos.GetMobileText(),
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(5, 0, 5, 1),
                FontSize = 14,
                HorizontalOptions = LayoutOptions.Start,
                LineBreakMode = LineBreakMode.TailTruncation,
            };
            var anzahl = new Label
            {
                Text = "Anzahl: " + lei.anzahl,
                TextColor = Color.FromArgb("#ffcc00"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.Start,
                IsVisible = pos.einheit != "std",
            };
            var order = new Label
            {
                Text = "Auftrag: " + o.GetMobileText(),
                TextColor = Color.FromArgb("#ffffff"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.Fill,
            };
            var category = new Label
            {
                Text = "Kategorie: " + c.GetMobileText(),
                TextColor = Color.FromArgb("#ffffff"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.Fill,
            };
            var h = new Grid()
            {
                Padding = new Thickness(5, 5, 5, 5),
                Margin = new Thickness(0, 0, 0, 0),
                ColumnSpacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Color.FromArgb("#99042d53"),
                ColumnDefinitions = {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star }
                }
            };
            var v = new Grid()
            {
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                RowSpacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
                RowDefinitions =                 {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                }
            };

            v.Add(order,0,0);
            v.Add(category,0,1);
            v.Add(lb,0,2);
            v.Add(anzahl,0,3);

            h.Add(imageL,0,0);
            h.Add(v,0,1);

            var mainFrame = new Border()
            {
                Padding = new Thickness(1, 1, 1, 1),
                Margin = new Thickness(0, 5, 0, 5),
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Colors.Transparent,
                Content = h,
            };

            return mainFrame;
        }




        public static StackLayout GetPositionInWork(LeistungWSO l, AppModel model)
        {
            var imageL = new Image
            {
                Margin = new Thickness(0, 0, 5, 0),
                HeightRequest = 26,
                WidthRequest = 26,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start,
                Source = "worker_in_progress_warn_red.png"
            };
            var lb = new Label()
            {
                Text = "Aktuell in der Ausführung!",
                TextColor = Color.FromArgb("#ff0000"),
                Margin = new Thickness(5, 0, 5, 0),
                FontSize = 14,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                LineBreakMode = LineBreakMode.TailTruncation,
            };


            var h = new StackLayout()
            {
                Padding = new Thickness(5, 2, 5, 2),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Start,
            };
            h.Children.Add(imageL);
            h.Children.Add(lb);
            return h;
        }

        public static StackLayout GetPositionWarning(LeistungWSO l)
        {
            var imageL = new Image
            {
                Margin = new Thickness(0, 0, 5, 0),
                HeightRequest = 18,
                WidthRequest = 18,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start,
                Source = l.prio.image,
                IsVisible = l.prio.showWarn,
            };
            var lb = new Label()
            {
                Text = l.prio.warnText,
                TextColor = Color.FromArgb("#ffffff"),
                Margin = new Thickness(5, 0, 5, 0),
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                LineBreakMode = LineBreakMode.WordWrap,
            };

            var badge = new Border
            {
                BackgroundColor = Color.FromArgb(l.prio.badgeColor),
                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0),
                Padding = new Thickness(4, 2, 4, 2),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 5 },
                IsVisible = l.prio.showBadge && (l.prio.days > 10000000 ? false : true),
                Content = new Label
                {
                    Text = Int32.Parse((l.prio.days > 10000000 ? "0" : l.prio.days + "")).ToString(),
                    Margin = new Thickness(0, 0, 0, 0),
                    Padding = new Thickness(0, 0, 0, 0),
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
                BackgroundColor = Color.FromArgb(l.prio.barColor),
                IsVisible = l.prio.showWarn,
            };
            h.Children.Add(imageL);
            h.Children.Add(lb);
            h.Children.Add(badge);
            return h;
        }


        public static StackLayout GetWarningLineText(string text, string barColor)
        {
            var imageL = new Image
            {
                Margin = new Thickness(0, 0, 5, 0),
                HeightRequest = 18,
                WidthRequest = 18,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start,
                Source = "warn_triangle_yellow.png",
            };
            var lb = new Label()
            {
                Text = text,
                TextColor = Color.FromArgb("#ffffff"),
                Margin = new Thickness(5, 0, 5, 0),
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                LineBreakMode = LineBreakMode.WordWrap,
            };

            var h = new StackLayout()
            {
                Padding = new Thickness(5, 1, 5, 1),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Start,
                BackgroundColor = Color.FromArgb(barColor),
            };
            h.Children.Add(imageL);
            h.Children.Add(lb);
            return h;
        }


        public static Border GetPositionWinterCardView(LeistungWSO pos)
        {
            var imageL = new Image
            {
                Margin = new Thickness(0, 0, 5, 0),
                HeightRequest = 24,
                WidthRequest = 24,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Source = (pos.art == "Leistung" ? "leistung_symbol.png" :
                            (pos.art == "Produkt" ? "produkt_symbol.png" :
                                (pos.art == "Texte" ? "text_symbol.png" :
                                (pos.art == "Check" ? "check_white.png" :
                                    "quest.png"
                         ))))
            };
            var lb = new Label()
            {
                Text = pos.GetMobileText(),
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(5, 0, 5, 1),
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Start,
                LineBreakMode = LineBreakMode.WordWrap,
            };
            var direkt = new Label
            {
                Text = "Direkterfassung: " + pos.dstd.ToString("00") + ":" + pos.dmin.ToString("00"),
                TextColor = Color.FromArgb("#ffcc00"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.Start,
                IsVisible = pos.type == "1",
            };
            var typ = new Label
            {
                Text = pos.timeval + " --- Zuletzt: " + (pos.prio != null && pos.prio.lastWorkDate != null ? pos.prio.lastWorkDate.Value.ToString("dd.MM.yyyy") : "Nicht bekannt!"),
                TextColor = Color.FromArgb("#ffcc00"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.Start,
            };
            var h = new StackLayout()
            {
                Padding = new Thickness(5, 5, 5, 5),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Color.FromArgb(pos.nichtpauschal == 1 ? "#044320" : "#042d53"),
            };

            var v = new StackLayout()
            {
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.Fill,
            };
            v.Children.Add(lb);
            v.Children.Add(direkt);
            v.Children.Add(typ);

            if (double.Parse(pos.lastwork) == 0 && pos.timevaldays > 0)
            {
                v.Children.Add(GetWarningLineText(pos.prio.warnText, pos.prio.barColor));
            }

            h.Children.Add(imageL);
            h.Children.Add(v);

            var mainFrame = new Border()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(60, 1, 0, 1),
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Colors.Transparent,
                Content = h,
                ClassId = "" + pos.id,
            };


            return mainFrame;
        }



        public static Border GetPositionTodoCardView(LeistungWSO pos, AppModel model, bool onlyText)
        {
            var imageL = new Image
            {
                Margin = new Thickness(0, 0, 5, 0),
                HeightRequest = 24,
                WidthRequest = 24,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Source = (pos.art == "Leistung" ? "leistung_symbol.png" :
                            (pos.art == "Produkt" ? "produkt_symbol.png" :
                                (pos.art == "Texte" ? "text_symbol.png" :
                                (pos.art == "Check" ? "check_white.png" :
                                    "quest.png"
                         ))))
            };

            var lb = new Label
            {
                Text = pos.GetMobileText(),
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(5, 0, 5, 1),
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Start,
                LineBreakMode = LineBreakMode.WordWrap,
            };

            var direkt = new Label
            {
                Text = "Direkterfassung: " + pos.dstd.ToString("00") + ":" + pos.dmin.ToString("00"),
                TextColor = Color.FromArgb("#ffcc00"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.Start,
                IsVisible = pos.type == "1" && !onlyText,
            };

            var typ = new Label
            {
                Text = pos.timeval + " --- Zuletzt: " + (pos.prio != null && pos.prio.lastWorkDate != null ? pos.prio.lastWorkDate.Value.ToString("dd.MM.yyyy") : "Nicht bekannt!"),
                TextColor = Color.FromArgb("#ffcc00"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.Start,
                IsVisible = !onlyText,
            };

            var imageMuellSign2 = new Image
            {
                Margin = new Thickness(0, -32, 0, 0),
                HeightRequest = 32,
                WidthRequest = 32,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.End,
                Source = "muell_sign.png",
            };

            var imageMuellSign = new Image
            {
                Margin = new Thickness(0, -32, -8, 0),
                HeightRequest = 32,
                WidthRequest = 32,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.End,
                Source = pos.muell == 1 ? (pos.inout.inout == 1 ? "muell_out_tonne.png" : "muell_in_tonne.png") : null,
            };

            var badge = new Border
            {
                BackgroundColor = Color.FromArgb(pos.prio.badgeColor),
                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(-13, -3, 0, 0),
                Padding = new Thickness(4, 2, 4, 2),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 5 },
                IsVisible = pos.prio.showBadge && (pos.prio.days > 10000000 ? false : true),
                Content = new Label
                {
                    Text = Int32.Parse((pos.prio.days > 10000000 ? "0" : pos.prio.days + "")).ToString(),
                    Margin = new Thickness(0),
                    Padding = new Thickness(0),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 10,
                    TextColor = Colors.White,
                    FontAttributes = FontAttributes.Bold,
                    MinimumWidthRequest = 20,
                    LineBreakMode = LineBreakMode.NoWrap,
                    HorizontalTextAlignment = TextAlignment.Center
                }
            };

            var hmuell = new Grid
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                HeightRequest = 1,
                HorizontalOptions = LayoutOptions.Fill,
                IsVisible = pos.muell == 1,
                ColumnDefinitions =
        {
            new ColumnDefinition { Width = GridLength.Star },
            new ColumnDefinition { Width = GridLength.Auto }
        }
            };

            hmuell.Add(imageMuellSign, 1, 0);
            hmuell.Add(imageMuellSign2, 1, 0);

            var contentGrid = new Grid
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                HorizontalOptions = LayoutOptions.Fill,
                RowSpacing = 0
            };

            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            int currentRow = 0;

            contentGrid.Add(lb, 0, currentRow++);
            contentGrid.Add(direkt, 0, currentRow++);
            contentGrid.Add(typ, 0, currentRow++);

            if (double.Parse(pos.lastwork) == 0 && pos.timevaldays > 0)
            {
                contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                contentGrid.Add(GetWarningLineText(pos.prio.warnText, pos.prio.barColor), 0, currentRow++);
            }

            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.Add(hmuell, 0, currentRow++);

            var h = new Grid
            {
                Padding = new Thickness(5),
                Margin = new Thickness(0),
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Color.FromArgb(pos.nichtpauschal == 1 ? "#044320" : "#042d53"),
                ColumnSpacing = 0,
                ColumnDefinitions =
        {
            new ColumnDefinition { Width = GridLength.Auto },
            new ColumnDefinition { Width = GridLength.Auto },
            new ColumnDefinition { Width = GridLength.Star }
        }
            };

            h.Add(imageL, 0, 0);
            h.Add(badge, 1, 0);
            h.Add(contentGrid, 2, 0);

            var mainFrame = new Border
            {
                Padding = new Thickness(0),
                Margin = new Thickness(60, 1, 0, 1),
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Colors.Transparent,
                Content = h,
                ClassId = "" + pos.id,
            };

            return mainFrame;
        }
        public static Border GetPositionTodoCardView_aaa(LeistungWSO pos, AppModel model, bool onlyText)
        {
            var imageL = new Image
            {
                Margin = new Thickness(0, 0, 5, 0),
                HeightRequest = 24,
                WidthRequest = 24,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Source = (pos.art == "Leistung" ? "leistung_symbol.png" :
                            (pos.art == "Produkt" ? "produkt_symbol.png" :
                                (pos.art == "Texte" ? "text_symbol.png" :
                                (pos.art == "Check" ? "check_white.png" :
                                    "quest.png"
                         ))))
            };
            var lb = new Label()
            {
                Text = pos.GetMobileText(),
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(5, 0, 5, 1),
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Start,
                LineBreakMode = LineBreakMode.WordWrap,
            };
            var direkt = new Label
            {
                Text = "Direkterfassung: " + pos.dstd.ToString("00") + ":" + pos.dmin.ToString("00"),
                TextColor = Color.FromArgb("#ffcc00"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.Start,
                IsVisible = pos.type == "1" && !onlyText,
            };
            var typ = new Label
            {
                Text = pos.timeval + " --- Zuletzt: " + (pos.prio != null && pos.prio.lastWorkDate != null ? pos.prio.lastWorkDate.Value.ToString("dd.MM.yyyy") : "Nicht bekannt!"),
                TextColor = Color.FromArgb("#ffcc00"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.Start,
                IsVisible = !onlyText,
            };
            var h = new StackLayout()
            {
                Padding = new Thickness(5, 5, 5, 5),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Color.FromArgb(pos.nichtpauschal == 1 ? "#044320" : "#042d53"),
            };
            var hmuell = new StackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                HeightRequest = 1,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Fill,
                IsVisible = pos.muell == 1
            };
            var imageMuellSign2 = new Image
            {
                Margin = new Thickness(0, -32, 0, 0),
                HeightRequest = 32,
                WidthRequest = 32,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.End,
                Source = "muell_sign.png",
            };
            var imageMuellSign = new Image
            {
                Margin = new Thickness(0, -32, -8, 0),
                HeightRequest = 32,
                WidthRequest = 32,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.End,
                Source = pos.muell == 1 ? (pos.inout.inout == 1 ? "muell_out_tonne.png" : "muell_in_tonne.png") : null,
            };
            hmuell.Children.Add(imageMuellSign);
            hmuell.Children.Add(imageMuellSign2);
            var v = new StackLayout()
            {
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.Fill,
            };
            v.Children.Add(lb);
            v.Children.Add(direkt);
            v.Children.Add(typ);

            if (double.Parse(pos.lastwork) == 0 && pos.timevaldays > 0)
            {
                v.Children.Add(GetWarningLineText(pos.prio.warnText, pos.prio.barColor));
            }

            v.Children.Add(hmuell);

            var badge = new Border
            {
                BackgroundColor = Color.FromArgb(pos.prio.badgeColor),
                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(-13, -3, 0, 0),
                Padding = new Thickness(4, 2, 4, 2),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 5 },
                IsVisible = pos.prio.showBadge && (pos.prio.days > 10000000 ? false : true),
                Content = new Label
                {
                    Text = Int32.Parse((pos.prio.days > 10000000 ? "0" : pos.prio.days + "")).ToString(),
                    Margin = new Thickness(0, 0, 0, 0),
                    Padding = new Thickness(0, 0, 0, 0),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 10,
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

            var mainFrame = new Border()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(60, 1, 0, 1),
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Colors.Transparent,
                Content = h,
                ClassId = "" + pos.id,
            };

            return mainFrame;
        }


    }
    public class Prio
    {
        public string warnText = "";
        public string barColor = "";
        public ImageSource image = null;
        public string badgeColor = "";
        public double days = 100000000;
        public double underDays = 1360; // unter 4 Jahre (Anzeigen)
        public DateTime? lastWorkDate = null;
        public DateTime? nextWorkDate = null;
        public bool showBadge = true;
        public bool showWarn = true;

        public Prio()
        {
        }

        public static Prio GetLeistungPrio(LeistungWSO l, AppModel model)
        {
            var prio = new Prio();
            double limitDays = 1;

            // Leistungen/Produkte nach Bedarf oder Nach etc.
            if (l.timevaldays == 0)
            {
                //prio.days = prio.days;
                if (double.Parse(l.lastwork) > 0)
                {
                    prio.lastWorkDate = UnixTimeStampToDateTime(double.Parse(l.lastwork)).Date;
                }
                else
                {
                    prio.lastWorkDate = null;
                }
                prio.nextWorkDate = null;

                prio.badgeColor = "#009900";
                prio.barColor = l.nichtpauschal == 1 ? "#70044320" : "#70ff8000";
                prio.showBadge = false;
                prio.warnText = "Ausführung " + (l.nichtpauschal == 1 ? (l.timeval + " (Optional)") : l.timeval);
                prio.image = l.nichtpauschal == 1 ? "quest.png" : "time.png";
            }
            else if (double.Parse(l.lastwork) == 0 && l.timevaldays > 0)
            {
                // Wurde noch nie ausgeführt !!!
                //prio.days = prio.days;
                prio.lastWorkDate = null;
                prio.nextWorkDate = null;
                prio.badgeColor = "#009900";
                prio.barColor = "#70cccccc";
                prio.showBadge = false;
                prio.warnText = "Wurde bisher noch nicht ausgeführt!";
                //prio.image = model.imagesBase.InfoCircle;
                prio.image = "ic_info_b.png";//AppModel._Instance.imagesBase.InfoCircle,
            }
            else if (long.Parse(l.lastwork) > 1 && l.timevaldays > 0)
            {
                if (String.IsNullOrWhiteSpace(l.workat)) { l.workat = "" + (double.Parse(l.lastwork) + (double.Parse("" + l.timevaldays) * 24 * 60 * 60 * 1000)); }
                prio.days = (UnixTimeStampToDateTime(double.Parse(l.workat)).Date - DateTime.Now.Date).TotalDays;
                prio.lastWorkDate = UnixTimeStampToDateTime(double.Parse(l.lastwork)).Date;
                prio.nextWorkDate = DateTime.Now.Date.AddDays(prio.days);

                prio.badgeColor = prio.days < 0 ? "#ff0000" : (prio.days < limitDays ? "#ffcc00" : "#009900");
                prio.barColor = prio.days < 0 ? "#50ff0000" : (prio.days < limitDays ? "#50ffcc00" : "#50009900");
                prio.warnText = prio.days < 0 ? "Überfällig seit (Tagen): " : (prio.days < limitDays ? "Heute fällig! " : "Erst fällig in (Tagen) ");
                prio.image = prio.days < 0 ? "warn_triangle_yellow.png" : (prio.days < limitDays ? "warn_triangle_white.png" : "time.png");
            }
            prio.showWarn = prio.days < 1 || true;

            return prio;
        }
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp / 1000).ToLocalTime();
            return dtDateTime;
        }

    }

    public class ChangeSelectedMuellPos
    {
        public LeistungWSO pos;
        public Label lb;
        public Image img;
        public Image img2;
    }


}