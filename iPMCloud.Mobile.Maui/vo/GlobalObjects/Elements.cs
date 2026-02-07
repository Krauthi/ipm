using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;


namespace iPMCloud.Mobile.vo
{
    public class Elements
    {
        public Elements() { }



        public static Frame GetIconButtonGray(TapGestureRecognizer tapGestureRecognizer, ImageSource imageSource)
        {
            return new Frame()
            {
                Padding = new Thickness(1, 1, 1, 1),
                Margin = new Thickness(2, 2, 2, 2),
                HasShadow = true,
                BackgroundColor = Color.FromHex("#666666"),
                Content = new StackLayout()
                {
                    GestureRecognizers = { tapGestureRecognizer },
                    Padding = new Thickness(0, 0, 0, 0),
                    Margin = new Thickness(0, 0, 0, 0),
                    Spacing = 0,
                    BackgroundColor = Color.FromHex("#777777"),
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    WidthRequest = 38,
                    HeightRequest = 38,
                    Children = { new Image { 
                            Source = imageSource,
                            HeightRequest = 30,
                            WidthRequest = 30,
                            Margin = new Thickness(0, 0, 0, 0),
                            VerticalOptions = LayoutOptions.CenterAndExpand,
                            HorizontalOptions = LayoutOptions.CenterAndExpand
                        }
                    }
                }
            };
        }

        public static Frame GetIconButtonBlue(TapGestureRecognizer tapGestureRecognizer, ImageSource imageSource)
        {
            return new Frame()
            {
                Padding = new Thickness(1, 1, 1, 1),
                Margin = new Thickness(2, 2, 2, 2),
                HasShadow = true,
                BackgroundColor = Color.FromHex("#041d43"),
                Content = new StackLayout()
                {
                    GestureRecognizers = { tapGestureRecognizer },
                    Padding = new Thickness(0, 0, 0, 0),
                    Margin = new Thickness(0, 0, 0, 0),
                    Spacing = 0,
                    BackgroundColor = Color.FromHex("#042d53"),
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    WidthRequest = 38,
                    HeightRequest = 38,
                    Children = { new Image {
                            Source = imageSource,
                            HeightRequest = 30,
                            WidthRequest = 30,
                            Margin = new Thickness(0, 0, 0, 0),
                            VerticalOptions = LayoutOptions.CenterAndExpand,
                            HorizontalOptions = LayoutOptions.CenterAndExpand
                        }
                    }
                }
            };
        }

        public static Frame GetWorkerCategoryTreeItem(string category, string count, ImageSource imageLeftSource, ICommand command)
        {
            var imageL = new Image
            {
                Margin = new Thickness(0, 2, 5, 2),
                HeightRequest = 24,
                WidthRequest = 24,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Source = imageLeftSource
            };
            var lb = new Label()
            {
                Text = category,
                TextColor = Color.FromHex("#ffffff"),
                Margin = new Thickness(0, 3, 0, 3),
                FontSize = 18,
                HorizontalOptions = LayoutOptions.StartAndExpand,
                LineBreakMode = LineBreakMode.TailTruncation,
            };
            var lbCount = new Label()
            {
                Text = "(" + count + ")",
                TextColor = Color.FromHex("#ffcc00"),
                Margin = new Thickness(8, 3, 0, 3),
                FontSize = 16,
                HorizontalOptions = LayoutOptions.End,
                LineBreakMode = LineBreakMode.NoWrap,
            };
            var headerStackHorizontal = new StackLayout()
            {
                Padding = new Thickness(8, 0, 10, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            headerStackHorizontal.Children.Add(imageL);
            headerStackHorizontal.Children.Add(lb);
            headerStackHorizontal.Children.Add(lbCount);

            var mainStack = new StackLayout()
            {
                Padding = new Thickness(0, 5, 0, 5),
                Margin = new Thickness(1,1,1,1),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromHex("#042d53")
            };
            if(command != null) {
                mainStack.GestureRecognizers.Clear();
                mainStack.GestureRecognizers.Add( new TapGestureRecognizer() { Command = command, CommandParameter = mainStack } );
            }
            mainStack.Children.Add(headerStackHorizontal);

            var mainFrame = new Frame()
            {
                Padding = new Thickness(0,0,0,0),
                Margin = new Thickness(5, 5, 5, 5),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromHex("#041d43"),
                Content = mainStack, 
                CornerRadius = 0,
                HasShadow = true,
                IsClippedToBounds = true
            };
            return mainFrame;
        }
        public static StackLayout GetWorkerTreeItem(PersonWSO p, ImageSource imageLeftSource, ImageSource imageRightSource,ICommand command)
        {
            var imageL = new Image
            {
                Margin = new Thickness(0, 2, 5, 0),
                HeightRequest = 20,
                WidthRequest = 20,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Source = imageLeftSource
            };
            var lb = new Label()
            {
                Text = p.firma,
                TextColor = Color.FromHex("#ffffff"),
                Margin = new Thickness(0, 0, 0, 0),
                FontSize = 16,
                LineBreakMode = LineBreakMode.TailTruncation,
            };
            var lbcategory= new Label()
            {
                Text = p.kategorie,
                TextColor = Color.FromHex("#ffcc00"),
                Margin = new Thickness(0, 0, 0, 0),
                FontSize = 10,
                LineBreakMode = LineBreakMode.NoWrap,
                VerticalTextAlignment = TextAlignment.Start,
                HorizontalTextAlignment = TextAlignment.End,
                HorizontalOptions = LayoutOptions.EndAndExpand
            };
            var headerStackHorizontal = new StackLayout()
            {
                Padding = new Thickness(8, 0, 10, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            headerStackHorizontal.Children.Add(imageL);
            headerStackHorizontal.Children.Add(lb);
            headerStackHorizontal.Children.Add(lbcategory);//Kategorie


            var lbaddress = new Label()
            {
                Text = p.strasse + " " + p.hsnr + " - " + (String.IsNullOrWhiteSpace(p.land) ? "" : p.land.Substring(0, 2).ToUpper() + " ") + p.plz + " " + p.ort,
                TextColor = Color.FromHex("#cccccc"),
                Padding = new Thickness(33, 0, 10, 0),
                Margin = new Thickness(0, 0, -3, 8),
                FontSize = 14,
                LineBreakMode = LineBreakMode.NoWrap
            };
            var headerStackVertical = new StackLayout()
            {
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            headerStackVertical.Children.Add(headerStackHorizontal);
            headerStackVertical.Children.Add(lbaddress);


            var moreDetailStackVertical = new StackLayout()
            {
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Fill,
                Children = { GetWorkerDetailsTreeItem(p, null,null, command) }
            };


            var mainStack = new StackLayout()
            {
                Padding = new Thickness(0, 5, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromHex("#144d73"),
            };
            mainStack.Children.Add(headerStackVertical);
            mainStack.Children.Add(moreDetailStackVertical);
            mainStack.Children.Add(new BoxView() { ClassId = "", Margin = new Thickness(0, 8, 0, 0), HeightRequest = 1, BackgroundColor = Color.FromHex("#041d43") });
            return mainStack;
        }
        public static StackLayout GetWorkerDetailsTreeItem(PersonWSO p, ImageSource imageLeftSource, ImageSource imageRightSource, ICommand command)
        {

            var detailTelefon = new StackLayout()
            {
                Padding = new Thickness(0, 3, 0, 3),
                Margin = new Thickness(0, 0, 0, 3),
                Spacing = 0,
                IsVisible = !String.IsNullOrWhiteSpace(p.telefon),
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Fill,
                Children = { new Label{
                        Text = "Telefon: " + p.telefon,
                        TextColor = Color.FromHex("#cccccc"),
                        Margin = new Thickness(0, 0, 0, 0),
                        FontSize = 14,
                        LineBreakMode = LineBreakMode.TailTruncation,
                        HorizontalOptions = LayoutOptions.StartAndExpand
                    },
                    new Label{
                        Text = "ANRUFEN",
                        TextColor = Color.FromHex("#ffffff"),
                        Margin = new Thickness(0, 0, 0, 0),
                        FontSize = 12,
                        LineBreakMode = LineBreakMode.NoWrap,
                        HorizontalOptions = LayoutOptions.End, TextDecorations = TextDecorations.Underline,
                        MinimumWidthRequest = 65, WidthRequest = 65, HorizontalTextAlignment = TextAlignment.End,
                        GestureRecognizers = { new TapGestureRecognizer(){ Command = command, CommandParameter = "tel:" + p.telefon} }
                    },
                }
            };

            var detailMobile = new StackLayout()
            {
                Padding = new Thickness(0, 3, 0, 3),
                Margin = new Thickness(0, 0, 0, 3),
                Spacing = 0,
                IsVisible = !String.IsNullOrWhiteSpace(p.mobile),
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Fill,
                Children = { new Label{
                        Text = "Mobile: " + p.mobile,
                        TextColor = Color.FromHex("#cccccc"),
                        Margin = new Thickness(0, 0, 0, 0),
                        FontSize = 14,
                        LineBreakMode = LineBreakMode.TailTruncation,
                        HorizontalOptions = LayoutOptions.StartAndExpand
                    },
                    new Label{
                        Text = "ANRUFEN",
                        TextColor = Color.FromHex("#ffffff"),
                        Margin = new Thickness(0, 0, 0, 0),
                        FontSize = 12,
                        LineBreakMode = LineBreakMode.NoWrap,
                        HorizontalOptions = LayoutOptions.End, TextDecorations = TextDecorations.Underline,
                        MinimumWidthRequest = 65, WidthRequest = 65, HorizontalTextAlignment = TextAlignment.End,
                        GestureRecognizers = { new TapGestureRecognizer(){ Command = command, CommandParameter = "tel:" + p.mobile } }
                    },
                }
            };
            var detailMail = new StackLayout()
            {
                Padding = new Thickness(0, 3, 0, 3),
                Margin = new Thickness(0, 0, 0, 3),
                Spacing = 0,
                IsVisible = !String.IsNullOrWhiteSpace(p.mail),
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Fill,
                Children = { new Label{
                        Text = "E-Mail: " + p.mail,
                        TextColor = Color.FromHex("#cccccc"),
                        Margin = new Thickness(0, 0, 0, 0),
                        FontSize = 14,
                        LineBreakMode = LineBreakMode.TailTruncation,
                        HorizontalOptions = LayoutOptions.Start
                    },
                    new Label{
                        Text = "SENDEN",
                        TextColor = Color.FromHex("#ffffff"),
                        Margin = new Thickness(0, 0, 0, 0),
                        FontSize = 12,
                        LineBreakMode = LineBreakMode.NoWrap,
                        HorizontalOptions = LayoutOptions.EndAndExpand, TextDecorations = TextDecorations.Underline,
                        MinimumWidthRequest = 65, WidthRequest = 65, HorizontalTextAlignment = TextAlignment.End,
                        GestureRecognizers = { new TapGestureRecognizer(){ Command = command, CommandParameter = "mailto:" + p.mail } }
                    },
                }
            };
            var contentHori = new StackLayout()
            {
                Padding = new Thickness(32, 0, 10, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand, 
            };
            contentHori.Children.Add(detailTelefon);
            contentHori.Children.Add(detailMobile);
            contentHori.Children.Add(detailMail);
            return contentHori;
        }
        public static Frame GetWorkerNamesTreeItem(PersonWSO p, ImageSource imageLeftSource, ICommand command)
        {
            var imageL = new Image
            {
                Margin = new Thickness(0, -4, 5, 2),
                HeightRequest = 24,
                WidthRequest = 24,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Source = imageLeftSource
            };
            var lb = new Label()
            {
                Text = String.IsNullOrEmpty(p.firma) ? p.name : p.firma,
                TextColor = Color.FromHex("#ffffff"),
                Margin = new Thickness(2, 0, 0, 1),
                FontSize = 18,
                HorizontalOptions = LayoutOptions.StartAndExpand,
                LineBreakMode = LineBreakMode.TailTruncation,
            };
            var lbCount = new Label()
            {
                Text = p.kategorie,
                TextColor = Color.FromHex("#ffcc00"),
                Margin = new Thickness(0, 0, 3, 0),
                FontSize = 12,
                HorizontalOptions = LayoutOptions.EndAndExpand,
                LineBreakMode = LineBreakMode.NoWrap,
            };
            var more = new Label
            {
                Text = "MEHR",
                TextColor = Color.FromHex("#ffffff"),
                Margin = new Thickness(0, 30, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.NoWrap,
                HorizontalOptions = LayoutOptions.End,
                TextDecorations = TextDecorations.Underline,
                MinimumWidthRequest = 42,
                WidthRequest = 42,
                HorizontalTextAlignment = TextAlignment.End
            };
            var headerStackHorizontal = new StackLayout()
            {
                Padding = new Thickness(8, 0, 10, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            headerStackHorizontal.Children.Add(imageL);
            headerStackHorizontal.Children.Add(lb);
            headerStackHorizontal.Children.Add(more);

            var mainStack = new StackLayout()
            {
                Padding = new Thickness(0, 0, 0, 5),
                Margin = new Thickness(1, 1, 1, 1),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromHex("#042d53")
            };
            if (command != null)
            {
                mainStack.GestureRecognizers.Clear();
                mainStack.GestureRecognizers.Add(new TapGestureRecognizer() { Command = command, CommandParameter = mainStack });
            }

            var lbaddress = new Label()
            {
                Text = p.strasse + " " + p.hsnr + " - " + (String.IsNullOrWhiteSpace(p.land) ? "" : p.land.Substring(0, 2).ToUpper() + " ") + p.plz + " " + p.ort,
                TextColor = Color.FromHex("#cccccc"),
                Padding = new Thickness(38, 0, 0, 0),
                Margin = new Thickness(0, -18, 33, 5),
                FontSize = 14,
                LineBreakMode = LineBreakMode.TailTruncation
            };

            mainStack.Children.Add(lbCount);
            mainStack.Children.Add(headerStackHorizontal);
            mainStack.Children.Add(lbaddress);

            var mainFrame = new Frame()
            {
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(5, 5, 5, 5),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromHex("#041d43"),
                Content = mainStack,
                CornerRadius = 0,
                HasShadow = true,
                IsClippedToBounds = true
            };
            return mainFrame;
        }
 
        public static Frame GetWorkerBuildingTreeItem(BuildingWSO b, ImageSource imageLeftSource, ICommand command)
        {
            var imageL = new Image
            {
                Margin = new Thickness(0, 0, 5, 2),
                HeightRequest = 24,
                WidthRequest = 24,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Source = imageLeftSource
            };
            var lb = new Label()
            {
                Text = b.plz + " " + b.ort,
                TextColor = Color.FromHex("#cccccc"),
                Margin = new Thickness(2, 0, 0, 1),
                FontSize = 16,
                HorizontalOptions = LayoutOptions.StartAndExpand,
                LineBreakMode = LineBreakMode.TailTruncation,
            };
            var more = new Label
            {
                Text = "(" + b.ArrayOfHandwerker.Count+ ")", 
                TextColor = Color.FromHex("#ffcc00"),
                Margin = new Thickness(0, 0, 0, 0),
                FontSize = 16,
                LineBreakMode = LineBreakMode.NoWrap,
                HorizontalOptions = LayoutOptions.End,
                MinimumWidthRequest = 45,
                WidthRequest = 45,
                HorizontalTextAlignment = TextAlignment.End
            };
            var headerStackHorizontal = new StackLayout()
            {
                Padding = new Thickness(8, 5, 10, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            headerStackHorizontal.Children.Add(imageL);
            headerStackHorizontal.Children.Add(lb);
            headerStackHorizontal.Children.Add(more);

            var lbaddress = new Label()
            {
                Text = b.strasse + " " + b.hsnr,
                TextColor = Color.FromHex("#ffffff"),
                Margin = new Thickness(40, 0, 0, 1),
                FontSize = 18,
                HorizontalOptions = LayoutOptions.StartAndExpand,
                LineBreakMode = LineBreakMode.TailTruncation,
            };

            var mainStack = new StackLayout()
            {
                Padding = new Thickness(0, 0, 0, 5),
                Margin = new Thickness(1, 1, 1, 1),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromHex("#042d53")
            };
            if (command != null)
            {
                mainStack.GestureRecognizers.Clear();
                mainStack.GestureRecognizers.Add(new TapGestureRecognizer() { Command = command, CommandParameter = mainStack });
            }

            mainStack.Children.Add(headerStackHorizontal);
            mainStack.Children.Add(lbaddress);

            var mainFrame = new Frame()
            {
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(5, 5, 0, 5),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.Transparent,//Color.FromHex("#041d43"),
                Content = mainStack,
                CornerRadius = 0,
                HasShadow = true,
                IsClippedToBounds = true,
            };
            return mainFrame;
        }

        public static Frame GetWorkerBuildingTreeInfoItem(BuildingWSO b, Frame frame, TapGestureRecognizer tgr)
        {
            var horiStack = new StackLayout()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0,0,10,0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.Transparent,
            };
            var mainStack = new StackLayout()
            {
                Padding = new Thickness(6,3,6,0),
                Margin = new Thickness(0,6,0,6),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.End,
                BackgroundColor = Color.FromHex("#042d53"),
                IsVisible = !String.IsNullOrWhiteSpace(b.notiz),
                Children = { new Image {
                    Margin = new Thickness(2),
                    HeightRequest = 25,
                    WidthRequest = 25,
                    Source = AppModel.Instance.imagesBase.InfoCircle,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Start,
                    }
                }
            };

            mainStack.GestureRecognizers.Clear();
            mainStack.GestureRecognizers.Add(tgr);

            horiStack.Children.Add(frame);
            horiStack.Children.Add(mainStack);
            var mainFrame = new Frame()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.Transparent,//Color.FromHex("#041d43"),
                Content = horiStack,
                CornerRadius = 0,
                HasShadow = false,
                IsClippedToBounds = true,
            };
            return mainFrame;
        }



        public static Frame GetCompanySelectionItem(Company c, ImageSource imageLeftSource, bool isSelected)
        {
            var imageL = new Image
            {
                Margin = new Thickness(0, 0, 5, 0),
                HeightRequest = 32,
                WidthRequest = 32,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Source = imageLeftSource
            };
            var lb = new Label()
            {
                Text = c.CustomerName,
                TextColor = Color.FromHex("#cccccc"),
                Margin = new Thickness(5, 0, 5, 1),
                FontSize = 16,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                LineBreakMode = LineBreakMode.WordWrap,
            };
            var more = new Label
            {
                Text = "Unternehmens-Nr.: " + c.CustomerNumber,
                TextColor = Color.FromHex("#999999"),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                LineBreakMode = LineBreakMode.TailTruncation,
                HorizontalOptions = LayoutOptions.Start,
            };
            var headerStackHorizontal = new StackLayout()
            {
                Padding = new Thickness(5, 5, 5, 5),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                ClassId = c.CustomerNumber,
                BackgroundColor = isSelected ? Color.FromRgba(4,45,83,1):Color.FromHex("#042d53"),
            };
            var headerStackVertical = new StackLayout()
            {
                Padding = new Thickness(0,0,0,0),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                ClassId = c.CustomerNumber
            };
            headerStackVertical.Children.Add(lb);
            headerStackVertical.Children.Add(more);
            headerStackHorizontal.Children.Add(imageL);
            headerStackHorizontal.Children.Add(headerStackVertical);

            var mainFrame = new Frame()
            {
                Padding = new Thickness(1,1,1,1),
                Margin = new Thickness(0, 15, isSelected ? 0:10, 5),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = isSelected ? Color.FromHex("#70144d73") : Color.FromHex("#041d43"),
                Content = headerStackHorizontal,
                CornerRadius = 0,
                HasShadow = true,
                IsClippedToBounds = true, //WidthRequest = 280,
            };
            return mainFrame;
        }
        public static Frame GetXButton(Company c, ImageSource xImage, bool isSelected)
        {
            var imageL = new Image
            {
                Margin = new Thickness(0, 0, 0, 0),
                HeightRequest = 28,
                WidthRequest = 28,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center, 
                Source = xImage
            };
            var headerStackHorizontal = new StackLayout()
            {
                Padding = new Thickness(5, 5, 5, 5),
                Margin = new Thickness(0, 0, 0, 0),
                Spacing = 0,
                BackgroundColor = Color.FromHex("#042d53"),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                ClassId = c.CustomerNumber,
                WidthRequest = 39,
                HeightRequest = 39,
                MinimumWidthRequest = 39,
                MinimumHeightRequest = 39,
            };
            headerStackHorizontal.Children.Add(imageL);

            var mainFrame = new Frame()
            {
                Padding = new Thickness(1,1,1,1),
                Margin = new Thickness(0, 15, 0, 5),
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start,
                BackgroundColor = Color.FromHex("#041d43"),
                Content = headerStackHorizontal,
                CornerRadius = 0,
                HasShadow = true, IsVisible = !isSelected,
                IsClippedToBounds = true, WidthRequest = 40, HeightRequest = 40,
                MinimumWidthRequest = 40,
                MinimumHeightRequest = 40,
            };
            return mainFrame;
        }


        public static BoxView GetBoxViewLine()
        {
            return new BoxView() { Margin = new Thickness(0, 0, 0, 0), HeightRequest = 1, BackgroundColor = Color.Gray };
        }
        public static BoxView GetHeightSpacer(double height = 5)
        {
            return new BoxView() { Margin = new Thickness(0, 0, 0, 0), HeightRequest = height, BackgroundColor = Color.Transparent };
        }

    }
}
