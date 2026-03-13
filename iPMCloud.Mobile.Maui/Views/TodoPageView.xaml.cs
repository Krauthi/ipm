using iPMCloud.Mobile.vo;
using System.Windows.Input;

namespace iPMCloud.Mobile.Views
{
    public partial class TodoPageView : ContentView
    {
        public CustomEntry EntryTodosearch => entry_todosearch;

        public TodoPageView()
        {
            InitializeComponent();

            //Todo

            if (btn_todo_back_img.Source == null || btn_todo_back_img.Source.IsEmpty)
            {
                btn_todo_back_img.Source = AppModel.Instance.imagesBase.DropLeftBlueDoubleImage;
            }
            if (btn_todosearch_img.Source == null || btn_todosearch_img.Source.IsEmpty)
            {
                btn_todosearch_img.Source = AppModel.Instance.imagesBase.SearchImage;
            }
            if (btn_todo_inout2_img.Source == null || btn_todo_inout2_img.Source.IsEmpty)
            {
                btn_todo_inout2_img.Source = AppModel.Instance.imagesBase.Muell_Sign;
            }
            if (btn_todo_inout_img.Source == null || btn_todo_inout_img.Source.IsEmpty)
            {
                btn_todo_inout_img.Source = AppModel.Instance.imagesBase.Muell_Out;
            }


            if (btn_todo_all.GestureRecognizers == null || btn_todo_all.GestureRecognizers.Count == 0)
            {
                btn_todo_all.GestureRecognizers.Clear();
                var tgr_todo_all = new TapGestureRecognizer();
                tgr_todo_all.Tapped += btn_todo_allTapped;
                btn_todo_all.GestureRecognizers.Add(tgr_todo_all);
            }

            if (btn_todo_inout.GestureRecognizers == null || btn_todo_inout.GestureRecognizers.Count == 0)
            {
                btn_todo_inout.GestureRecognizers.Clear();
                var tgr_todo_inout = new TapGestureRecognizer();
                tgr_todo_inout.Tapped += btn_todo_inoutTapped;
                btn_todo_inout.GestureRecognizers.Add(tgr_todo_inout);
            }

            if (btn_todo_faellig.GestureRecognizers == null || btn_todo_faellig.GestureRecognizers.Count == 0)
            {
                btn_todo_faellig.GestureRecognizers.Clear();
                var tgr_todo_faellig = new TapGestureRecognizer();
                tgr_todo_faellig.Tapped += btn_todo_faelligTapped;
                btn_todo_faellig.GestureRecognizers.Add(tgr_todo_faellig);
            }

            if (btn_todo_faellig_prev.GestureRecognizers == null || btn_todo_faellig_prev.GestureRecognizers.Count == 0)
            {
                btn_todo_faellig_prev.GestureRecognizers.Clear();
                var tgr_btn_todo_faellig_prev = new TapGestureRecognizer();
                tgr_btn_todo_faellig_prev.Tapped += btn_todo_faelligprevTapped;
                btn_todo_faellig_prev.GestureRecognizers.Add(tgr_btn_todo_faellig_prev);
            }

            if (btn_todo_faellig_next.GestureRecognizers == null || btn_todo_faellig_next.GestureRecognizers.Count == 0)
            {
                btn_todo_faellig_next.GestureRecognizers.Clear();
                var tgr_btn_todo_faellig_next = new TapGestureRecognizer();
                tgr_btn_todo_faellig_next.Tapped += btn_todo_faellignextTapped;
                btn_todo_faellig_next.GestureRecognizers.Add(tgr_btn_todo_faellig_next);
            }

            EntryTodosearch.TextChanged += Entry_todosearch_TextChanged;
        }

        //public Grid Container => TodoPage_Container;
        public void SetVisible(bool visible) => TodoPage_Container.IsVisible = visible;

        // Expose child elements so MainPage can access them after extraction
        public Border BtnTodoBack => btn_todo_back;
        public VerticalStackLayout ListTodo => list_todo;


        private int _holdLastTodoList = 0;
        public int _holdLastTodoPage = 1;
        public int _holdLastTodoPageMax = 1;



        private void Entry_todosearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _holdLastTodoPage = 1;
            list_todo.Children.Clear();
            list_todo_scroll.ScrollToAsync(0, 0, false);
            BuildTodoList(1);
        }


        public async void btn_todo_allTapped(object sender, EventArgs e)
        {
            overlay.IsVisible = true;
            list_todo.IsVisible = false;
            entry_todosearch.Text = "";
            entry_todosearch_lbb.Text = "";
            entry_todosearch_container.IsVisible = true;
            entry_todosearch_stepcontainer.IsVisible = true;
            btn_todo_faellig.BackgroundColor = Color.FromArgb("#53042d");
            btn_todo_all.BackgroundColor = Color.FromArgb("#999999");
            btn_todo_inout.BackgroundColor = Color.FromArgb("#042d53");
            await Task.Delay(1);
            list_todo.Children.Clear();
            await list_todo_scroll.ScrollToAsync(0, 0, false);
            _holdLastTodoList = 1;
            _holdLastTodoPage = 1;
            BuildTodoList(1);
        }
        public async void btn_todo_faelligTapped(object sender, EventArgs e)
        {
            overlay.IsVisible = true;
            list_todo.IsVisible = false;
            entry_todosearch.Text = "";
            entry_todosearch_lbb.Text = "";
            entry_todosearch_stepcontainer.IsVisible = true;
            entry_todosearch_container.IsVisible = true;
            Update_Todopaging(_holdLastTodoPage, _holdLastTodoPageMax);
            btn_todo_faellig.BackgroundColor = Color.FromArgb("#999999");
            btn_todo_all.BackgroundColor = Color.FromArgb("#042d53");
            btn_todo_inout.BackgroundColor = Color.FromArgb("#042d53");
            await Task.Delay(1);
            list_todo.Children.Clear();
            await list_todo_scroll.ScrollToAsync(0, 0, false);
            _holdLastTodoList = 0;
            _holdLastTodoPage = 1;
            BuildTodoList(0);
        }
        public async void btn_todo_faelligprevTapped(object sender, EventArgs e)
        {
            _holdLastTodoPage--;
            if (_holdLastTodoPage < 1) { _holdLastTodoPage = 1; return; }
            overlay.IsVisible = true;
            list_todo.IsVisible = false;
            await Task.Delay(1);
            list_todo.Children.Clear();
            await list_todo_scroll.ScrollToAsync(0, 0, false);
            BuildTodoList(_holdLastTodoList);
        }
        public async void btn_todo_faellignextTapped(object sender, EventArgs e)
        {
            _holdLastTodoPage++;
            if (_holdLastTodoPage > _holdLastTodoPageMax) { _holdLastTodoPage = _holdLastTodoPageMax; return; }
            overlay.IsVisible = true;
            list_todo.IsVisible = false;
            await Task.Delay(1);
            list_todo.Children.Clear();
            await list_todo_scroll.ScrollToAsync(0, 0, false);
            BuildTodoList(_holdLastTodoList);
        }
        public async void btn_todo_inoutTapped(object sender, EventArgs e)
        {
            overlay.IsVisible = true;
            list_todo.IsVisible = false;
            entry_todosearch.Text = "";
            entry_todosearch_lbb.Text = "";
            entry_todosearch_container.IsVisible = true;
            entry_todosearch_stepcontainer.IsVisible = true;
            btn_todo_faellig.BackgroundColor = Color.FromArgb("#53042d");
            btn_todo_all.BackgroundColor = Color.FromArgb("#042d53");
            btn_todo_inout.BackgroundColor = Color.FromArgb("#999999");
            await Task.Delay(1);
            list_todo.Children.Clear();
            await list_todo_scroll.ScrollToAsync(0, 0, false);
            _holdLastTodoList = 2;
            _holdLastTodoPage = 1;
            BuildTodoList(2);
        }
        public async void BuildTodoList(int all = 0)
        {
            if (String.IsNullOrWhiteSpace(entry_todosearch.Text))
            {
                entry_todosearch.Text = "";
            }

            GetOrderTodoListView(all, overlay, entry_todosearch.Text);
            await Task.Delay(1);
            list_todo.IsVisible = true;
            overlay.IsVisible = false;
        }
        public void Update_Todopaging(int page, int maxpage)
        {
            btn_todo_faellig_count.Text = "Seite " + page + " von " + maxpage;
        }


        public List<BuildingWSO> buildingWSOs = new List<BuildingWSO>();
        public VerticalStackLayout GetOrderTodoListView(int all, AbsoluteLayout overlay, string s)
        {
            int maxResult = 20;

            var oList = new List<BuildingWSO>();
            AppModel.Instance.AllBuildings.ForEach(b =>
            {
                string term = b.plz + "##" + b.ort + "##" + b.strasse + "##" + b.hsnr + "##" + b.objektname + "##" + b.objektnr;
                if (term.ToLower().Contains(s.ToLower()))
                {
                    var building = AuftragWSO.CalcBuildingdue(b, AppModel.Instance, all == 1);
                    if (building != null)
                    {
                        oList.Add(building);
                    }
                }
            });

            //oList.ForEach(b => { b = CalcBuildingdue(b, AppModel.Instance); });
            oList = oList.OrderBy(b => b.prio).ToList();
            int count = oList.Where(_ => _.isInTodoVisible).Count();
            int pages = (count / maxResult) + (count == maxResult ? 0 : 1);
            AppModel.Instance.MainPage.TodoPageViewObject._holdLastTodoPageMax = pages;

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
                                    var za = (i * (AppModel.Instance.MainPage.TodoPageViewObject._holdLastTodoPage - 1));
                                    var ba = (maxResult * (AppModel.Instance.MainPage.TodoPageViewObject._holdLastTodoPage - 1)) + 1; // 1
                                    var bb = (maxResult * (AppModel.Instance.MainPage.TodoPageViewObject._holdLastTodoPage - 1)) + maxResult; // maxResult 50
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
                AppModel.Instance.MainPage.TodoPageViewObject._holdLastTodoPageMax = pages;
                muellist.ForEach(ml =>
                {
                    list_todo.Children.Add(GetMuellPositionCardView(ml, AppModel.Instance, null));
                });
                AppModel.Instance.MainPage.TodoPageViewObject.Update_Todopaging(AppModel.Instance.MainPage.TodoPageViewObject._holdLastTodoPage, AppModel.Instance.MainPage.TodoPageViewObject._holdLastTodoPageMax);
                return null;
            }
            // all = 1 oder 0
            buildingWSOs = oList;
            oList.ForEach(b =>
            {
                var za = (i * (AppModel.Instance.MainPage.TodoPageViewObject._holdLastTodoPage - 1));
                var ba = (maxResult * (AppModel.Instance.MainPage.TodoPageViewObject._holdLastTodoPage - 1)) + 1; // 1
                var bb = (maxResult * (AppModel.Instance.MainPage.TodoPageViewObject._holdLastTodoPage - 1)) + maxResult; // maxResult 50
                if (i >= ba && i <= bb && b != null && b.ArrayOfAuftrag.Count > 0 && b.isInTodoVisible)
                {
                    list_todo.Children.Add(GetBuildingInfoTodoElement(b, overlay));
                }
                i++;
            });
            AppModel.Instance.MainPage.TodoPageViewObject.Update_Todopaging(
                AppModel.Instance.MainPage.TodoPageViewObject._holdLastTodoPage, 
                AppModel.Instance.MainPage.TodoPageViewObject._holdLastTodoPageMax);
            //Task task = Task.Run(() => RestGui(all,oList,stack, overlay));
            return null;
        }


        public Int32 holdlastObjektId = 0;

        // In ToDo Liste
        public Border GetMuellPositionCardView(LeistungWSO pos, AppModel model, ICommand func)
        {
            var vStack = new VerticalStackLayout()
            {
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
            };


            var hobj = new HorizontalStackLayout()
            {
                Padding = new Thickness(5, 2, 5, 2),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Color.FromArgb("#aaaaaa"),
                Children = {
                        new Image{
                            Margin = new Thickness(0, 0, 2, 0),
                            HeightRequest = 32,
                            WidthRequest = 32,
                            VerticalOptions = LayoutOptions.Center,
                            HorizontalOptions = LayoutOptions.Start,
                            Source = model.imagesBase.Building
                        },
                        new Label {
                        Text = pos.objekt.plz + " " + pos.objekt.ort + "\n" + pos.objekt.strasse + " " + pos.objekt.hsnr,
                        TextColor = Color.FromArgb("#111111"),
                        Margin = new Thickness(0),
                        FontSize = 16, FontAttributes = FontAttributes.None,
                        HorizontalOptions = LayoutOptions.Fill,
                        LineBreakMode = LineBreakMode.WordWrap,
                    }
                }
            };

            if (holdlastObjektId != pos.objektid)
            {
                vStack.Children.Add(hobj);
            }

            var lb = new Label()
            {
                Text = pos.GetMobileText(),
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(3, 0, 5, 1),
                FontSize = 14,
                HorizontalOptions = LayoutOptions.StartAndExpand,
                LineBreakMode = LineBreakMode.WordWrap,
            };

            long lastworkNumber = long.Parse(pos.inout.last);

            var last = JavaScriptDateConverter.Convert(lastworkNumber, 2);
            var diff = DateTime.Now - last;
            var lastWorkDateAtHou = lastworkNumber > 0 ? (DateTime.Now - last).ToString("hh") : "0";
            var lastWorkDateAtMin = lastworkNumber > 0 ? (DateTime.Now - last).ToString("mm") : "0";
            var lastWorkDateAtDay = lastworkNumber > 0 ? (DateTime.Now - last).ToString("%d") : "0";
            var lastWorkDate = lastworkNumber > 0 ? last.ToString("dd.MM.yyyy - HH:mm") : "Nicht bekannt!";

            var htime = new StackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };
            var typ = new Label
            {
                Text = "Status: Rausgestellt am\n" + lastWorkDate,
                TextColor = Color.FromArgb("#ffcc00"),
                Margin = new Thickness(3, 0, 0, 0),
                FontSize = 14,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.StartAndExpand,
            };
            var vtime = new StackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };
            var typAtlb = new Label
            {
                Text = "Seit: ",
                TextColor = Color.FromArgb("#aaaaaa"),
                HorizontalTextAlignment = TextAlignment.Start,
                Margin = new Thickness(3, 0, 5, 0),
                FontSize = 12,
                FontAttributes = FontAttributes.None,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.StartAndExpand,
            };
            var typAt = new Label
            {
                Text = (int.Parse(lastWorkDateAtDay) == 0 ? "" : lastWorkDateAtDay + "T ") + lastWorkDateAtHou + ":" + lastWorkDateAtMin,
                TextColor = Color.FromArgb("#ffcc00"),
                HorizontalTextAlignment = TextAlignment.Start,
                Margin = new Thickness(3, 0, 5, 0),
                FontSize = 20,
                FontAttributes = FontAttributes.None,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.StartAndExpand,
            };
            vtime.Children.Add(typAtlb);
            vtime.Children.Add(typAt);
            htime.Children.Add(typ);
            htime.Children.Add(vtime);

            var h = new StackLayout()
            {
                Padding = new Thickness(5, 5, 5, 5),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromArgb(pos.nichtpauschal == 1 ? "#044320" : "#042d53"),
            };
            var hmuell = new StackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                HeightRequest = 1,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                IsVisible = pos.muell == 1
            };
            var imageMuellSign = new Image
            {
                Margin = new Thickness(0, -42, -4, 0),
                HeightRequest = 32,
                WidthRequest = 32,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.EndAndExpand,
                Source = pos.muell == 1 ? (pos.inout.inout == 1 ? model.imagesBase.Muell_Out : model.imagesBase.Muell_In) : null,
            };
            hmuell.Children.Add(imageMuellSign);
            var v = new StackLayout()
            {
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };
            v.Children.Add(lb);
            v.Children.Add(htime);

            v.Children.Add(hmuell);

            h.Children.Add(new Image
            {
                Margin = new Thickness(0, 0, 3, 0),
                HeightRequest = 24,
                WidthRequest = 24,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Source = model.imagesBase.Muell_Sign
            });
            h.Children.Add(v);


            vStack.Children.Add(h);

            var mainFrame = new Border()
            {
                Padding = new Thickness(1, 1, 1, 1),
                Margin = new Thickness(0, (holdlastObjektId != pos.objektid ? 15 : 1), 0, (holdlastObjektId != pos.objektid ? 5 : 0)),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Colors.Transparent,
                Content = vStack,
                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
                ClassId = "" + pos.objekt.plz + "##" + pos.objekt.ort + "##" + pos.objekt.strasse + "##" + pos.objekt.hsnr + "##" + pos.objekt.objektname + "##" + pos.objekt.objektnr,
            };

            if (func != null)
            {
                h.GestureRecognizers.Clear();
                h.GestureRecognizers.Add(new TapGestureRecognizer() { Command = func, CommandParameter = pos });
            }

            holdlastObjektId = pos.objektid;

            return mainFrame;
        }



        public VerticalStackLayout GetBuildingInfoTodoElement(BuildingWSO obj, AbsoluteLayout overlay = null)
        {
            var land = String.IsNullOrWhiteSpace(obj.land) ? "" : (obj.land.Length > 3 ? ("(" + obj.land.Substring(0, 3) + ") ") : obj.land);

            Image imgPin = new Image
            {
                Source = AppModel.Instance.imagesBase.Pin,
                Margin = new Thickness(10, 0, 5, 0),
                HeightRequest = 24,
                WidthRequest = 24,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End,
            };
            imgPin.GestureRecognizers.Clear();
            imgPin.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command<Object>(BuildingWSO.btn_MapTapped),
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
                warn = GetTodoCountWarningSmall(obj.prio);
                warn.Margin = new Thickness(0, 0, 0, 0);
            }

            var row = new Grid
            {
                ColumnSpacing = 0,
                RowSpacing = 0,
                Padding = new Thickness(6,0,6,0),
                Margin = 0,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center,
                ColumnDefinitions =
    {
        new ColumnDefinition { Width = GridLength.Auto }, // warn
        new ColumnDefinition { Width = GridLength.Star }, // label nimmt rest
        new ColumnDefinition { Width = GridLength.Auto }, // info
        new ColumnDefinition { Width = GridLength.Auto }, // pin
    }
            };

            row.Add(warn, 0, 0);

            var addressLabel = new Label
            {
                Text = obj.strasse + " " + obj.hsnr + "\n" + land + obj.plz + " " + obj.ort,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Fill,
                FontSize = 16,
                TextColor = Colors.White,
                Margin = new Thickness(10, 0, 0, 0),
                LineBreakMode = LineBreakMode.WordWrap
            };
            row.Add(addressLabel, 1, 0);

            row.Add(imgInfo, 2, 0);
            row.Add(imgPin, 3, 0);


            var container = new VerticalStackLayout
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                BackgroundColor = Color.FromArgb("#000000"),
                HorizontalOptions = LayoutOptions.Fill,
                IsVisible = false
            };

            var o = new List<Object>(){
                container,
                AppModel.Instance,
                obj,
                overlay,
                null
            };

            row.GestureRecognizers.Clear();
            row.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command<Object>(ShowOrderContainer), CommandParameter = o });


            var mainStack = new VerticalStackLayout
            {
                Padding = new Thickness(0,3,0,3),
                Margin = new Thickness(0, 0, 0, 15),
                Spacing = 0,
                BackgroundColor = Color.FromArgb("#90144d73"),
                HorizontalOptions = LayoutOptions.Fill,
                Children = { row, container },
                ClassId = "" + obj.plz + "##" + obj.ort + "##" + obj.strasse + "##" + obj.hsnr + "##" + obj.objektname + "##" + obj.objektnr,
            };
            return mainStack;
        }


        public async void ShowOrderContainer(Object value)
        {
            PlanPersonMobile pp = (value as List<Object>).Count > 4 ? ((value as List<Object>)[4] as PlanPersonMobile) : null;
            if (AppModel.Instance.AppControll.filterKategories && !AppModel.Instance.AppControll.ignoreKategorieFilterByPerson && pp != null)
            {
                ShowOrderContainerOnlyKat(value);
                return;
            }
            var vStack = ((value as List<Object>)[0] as VerticalStackLayout);
            var model = ((value as List<Object>)[1] as AppModel);
            //var list = ((value as List<Object>)[2] as BuildingWSO).ArrayOfAuftrag;
            var obj = ((value as List<Object>)[2] as BuildingWSO);
            List<AuftragWSO> list = new List<AuftragWSO>();
            if (obj != null)
            {
                list = obj.ArrayOfAuftrag != null ? obj.ArrayOfAuftrag : new List<AuftragWSO>();
            }
            var overlay = ((value as List<Object>)[3] as AbsoluteLayout);
            //int w = 0;
            if (vStack.Children.Count == 0)
            {
                //w = 200;
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

                    var prioOrder = BuildingWSO.CalcOverdue(o, model);
                    //if (prioOrder < 8)  // vorschau 1 Woche
                    //{
                    if (o.isInTodoVisible || onlyText)
                    {
                        var order = GetOrderTodoCardView(o, model, prioOrder, onlyText);// prioOrder);
                        vStack.Children.Add(order);
                        o.kategorien.ForEach(c =>
                        {
                            var prioCat = AuftragWSO.CalcKategorieOverdue(c, model);
                            if (c.isInTodoVisible || onlyText)
                            {
                                //if (prioCat < 8) // vorschau 1 Woche
                                //{
                                var categories = GetCategoryTodoCardView(c, model, prioCat, onlyText);
                                (order.Children[1] as VerticalStackLayout).Children.Add(categories);
                                c.leistungen.ForEach(l =>
                                {
                                    var prioPos = AuftragWSO.CalcLeistungOverdueTodo(l, model);
                                    if (l.isInTodoVisible || onlyText)
                                    {
                                        //if (prioPos < 8)  // vorschau 1 Woche
                                        //{
                                        (categories.Children[1] as VerticalStackLayout).Children.Add(GetPositionTodoCardView(l, model, onlyText));
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
            vStack.IsVisible = !vStack.IsVisible;
            overlay.IsVisible = false;
        }

        public Border GetPositionTodoCardView(LeistungWSO pos, AppModel model, bool onlyText)
        {
            //var imageL = new Image
            //{
            //    Margin = new Thickness(0, 0, 5, 0),
            //    HeightRequest = 24,
            //    WidthRequest = 24,
            //    VerticalOptions = LayoutOptions.Start,
            //    HorizontalOptions = LayoutOptions.Start,
            //    Source = (pos.art == "Leistung" ? model.imagesBase.LeistungSymbol :
            //                (pos.art == "Produkt" ? model.imagesBase.ProduktSymbol :
            //                    (pos.art == "Texte" ? model.imagesBase.TextSymbol :
            //                    (pos.art == "Check" ? model.imagesBase.CheckWhite :
            //                        model.imagesBase.Quest
            //             ))))
            //};
            var lb = new Label()
            {
                Text = pos.GetMobileText(),
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(5, 0, 5, 1),
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Fill,
                LineBreakMode = LineBreakMode.WordWrap,
            };
            var direkt = new Label
            {
                Text = "Direkterfassung: " + pos.dstd.ToString("00") + ":" + pos.dmin.ToString("00"),
                TextColor = Color.FromArgb("#ffcc00"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.Fill,
                IsVisible = pos.type == "1" && !onlyText,
            };
            var typ = new Label
            {
                Text = pos.timeval + " --- Zuletzt: " + (pos.prio != null && pos.prio.lastWorkDate != null ? pos.prio.lastWorkDate.Value.ToString("dd.MM.yyyy") : "Nicht bekannt!"),
                TextColor = Color.FromArgb("#ffcc00"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.Fill,
                IsVisible = !onlyText,
            };
            var h = new HorizontalStackLayout()
            {
                Padding = new Thickness(5, 5, 5, 5),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Color.FromArgb(pos.nichtpauschal == 1 ? "#044320" : "#042d53"),
            };
            
            
            var v = new VerticalStackLayout()
            {
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
            };
            v.Children.Add(lb); // beschreibung
            v.Children.Add(direkt); // Direkterfassung


            var htype = new HorizontalStackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
            };
            htype.Children.Add(GetTodoCountWarningSmall(pos.prio.days));
            htype.Children.Add(typ);
            v.Children.Add(htype); // Zeit seit letzter Arbeit



            if (double.Parse(pos.lastwork) == 0 && pos.timevaldays > 0)
            {
                v.Children.Add(GetWarningLineText(pos.prio.warnText, pos.prio.barColor));
            }

            if (pos.muell == 1)
            {
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
                    Source = model.imagesBase.Muell_Sign,
                };
                var imageMuellSign = new Image
                {
                    Margin = new Thickness(0, -32, -8, 0),
                    HeightRequest = 32,
                    WidthRequest = 32,
                    VerticalOptions = LayoutOptions.Start,
                    HorizontalOptions = LayoutOptions.End,
                    Source = pos.muell == 1 ? (pos.inout.inout == 1 ? model.imagesBase.Muell_OutTonne : model.imagesBase.Muell_InTonne) : null,
                };
                hmuell.Children.Add(imageMuellSign);
                hmuell.Children.Add(imageMuellSign2);
                h.Children.Add(hmuell);
            }

            h.Children.Add(v);

            var mainFrame = new Border()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(6, 1, 0, 1),
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Colors.Transparent,
                Content = h,
                ClassId = "" + pos.id,
            };

            return mainFrame;
        }
        public HorizontalStackLayout GetWarningLineText(string text, string barColor)
        {
            var imageL = new Image
            {
                Margin = new Thickness(0, 0, 5, 0),
                HeightRequest = 18,
                WidthRequest = 18,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start,
                Source = AppModel.Instance.imagesBase.WarnTriangleYellow,
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

            var h = new HorizontalStackLayout()
            {
                Padding = new Thickness(5, 1, 5, 1),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                HorizontalOptions = LayoutOptions.Start,
                BackgroundColor = Color.FromArgb(barColor),
            };
            h.Children.Add(imageL);
            h.Children.Add(lb);
            return h;
        }


        public VerticalStackLayout GetOrderTodoCardView(AuftragWSO order, AppModel model, double _prio, bool onlyText)
        {
            var imageL = new Image
            {
                Margin = new Thickness(0, 0, 5, 0),
                HeightRequest = 24,
                WidthRequest = 24,
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
                HorizontalOptions = LayoutOptions.Fill,
                LineBreakMode = LineBreakMode.WordWrap,
            };
            var h = new HorizontalStackLayout()
            {
                Padding = new Thickness(5, 5, 5, 5),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = onlyText ? Color.FromArgb("#aa04532d") : Color.FromArgb("#aa042d53"),
            };


            h.Children.Add(imageL);
            
            h.Children.Add(lb);

            var mainFrame = new Border()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(6, 1, 0, 1),
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Colors.Transparent,
                Content = h,
                ClassId = "" + order.id,
            };

            var container = new VerticalStackLayout
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
                IsVisible = false,
            };

            mainFrame.GestureRecognizers.Clear();
            mainFrame.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command<VerticalStackLayout>(ShowCategoryContainer), CommandParameter = container });

            var wrapper = new VerticalStackLayout
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
                Children = { mainFrame, container }
            };

            return wrapper;
        }
        public void ShowCategoryContainer(VerticalStackLayout value)
        {
            value.IsVisible = !value.IsVisible;
        }

        public async void ShowOrderContainerOnlyKat(Object value)
        {
            var stack = ((value as List<Object>)[0] as VerticalStackLayout);
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
            //int w = 0;
            if (stack.Children.Count == 0)
            {
                //w = 200;
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

                    var prioOrder = BuildingWSO.CalcOverdue(o, model);
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
                                    var categories = GetCategoryTodoCardView(c, model, prioCat, onlyText);
                                    //(order.Children[1] as StackLayout).Children.Add(categories);
                                    stack.Children.Add(categories);
                                    c.leistungen.ForEach(l =>
                                    {
                                        var prioPos = AuftragWSO.CalcLeistungOverdueTodo(l, model);
                                        if (l.isInTodoVisible || onlyText)
                                        {
                                            //if (prioPos < 8)  // vorschau 1 Woche
                                            //{
                                            (categories.Children[1] as VerticalStackLayout).Children.Add(GetPositionTodoCardView(l, model, onlyText));
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



        public VerticalStackLayout GetCategoryTodoCardView(KategorieWSO cat, AppModel model, double prio, bool onlyText)
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
                TextColor = Color.FromArgb("#cccccc"),
                Margin = new Thickness(5, 0, 5, 1),
                FontSize = 16,
                HorizontalOptions = LayoutOptions.Fill,
                LineBreakMode = LineBreakMode.WordWrap,
            };
            var h = new HorizontalStackLayout()
            {
                Padding = new Thickness(5, 5, 5, 5),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = onlyText ? Color.FromArgb("#cc04532d") : Color.FromArgb("#cc042d53"),
            };

            Border warn = new Border { IsVisible = false };
            if (prio < 1360)
            {
                warn = AuftragWSO.GetTodoCountWarningSmall(prio);
                warn.MinimumWidthRequest = 40;
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

            var mainFrame = new Border()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(6, 1, 0, 1),
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Colors.Transparent,
                Content = h,
                ClassId = "" + cat.id,
            };

            var container = new VerticalStackLayout
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
                IsVisible = false,
            };

            mainFrame.GestureRecognizers.Clear();
            mainFrame.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command<VerticalStackLayout>(ShowPositionContainer), CommandParameter = container });

            var wrapper = new VerticalStackLayout
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Spacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
                Children = { mainFrame, container }
            };

            return wrapper;
        }

        public void ShowPositionContainer(VerticalStackLayout value)
        {
            value.IsVisible = !value.IsVisible;
        }

        public Border GetTodoCountWarningSmall(double count)
        {
            var badge = new Border
            {
                BackgroundColor = Color.FromArgb(count < 0 ? "#ff0000" : (count < 1 ? "#ffcc00" : "#009900")),
                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0),
                Padding = new Thickness(4, 2, 4, 2),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 11 },
                MinimumWidthRequest = 20,
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
            return badge;
        }




    }
}
