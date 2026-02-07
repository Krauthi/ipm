using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using iPMCloud.Mobile.vo;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Essentials;

namespace iPMCloud.Mobile
{

    [Serializable]
    public class PersonSmallWSORequest
    {
        public string token = "";

        public PersonSmallWSORequest()
        {
        }
    }

    [Serializable]
    public class PersonSmallWSOResponse
    {
        public bool success = false;
        public string message;
        public bool active = true;
        public List<PersonSmallWSO> data = new List<PersonSmallWSO>();

        public PersonSmallWSOResponse()
        {
        }
    }


    [Serializable]
    public class PersonSmallWSO
    {
        public Int32 id { get; set; } = 0;
        public int rolle { get; set; } = 0;
        public String anrede { get; set; } = "";
        public String vorname { get; set; } = "";
        public String name { get; set; } = "";
        public String mobile { get; set; } = "";
        public String typ { get; set; } = "";

        //public ICommand _navigationCommand = new Command<string>((url) =>
        //{
        //    AppModel.Instance.UseOutSideHardware = true;
        //    Launcher.OpenAsync(new Uri(url));
        //});

        public PersonSmallWSO()
        {
        }

        //public Label mobillabel
        //{
        //    get
        //    {
        //        return
        //            new Label
        //            {
        //                Text = "ANRUFEN",
        //                TextColor = Color.FromHex("#ffcc00"),
        //                Margin = new Thickness(0, 0, 0, 0),
        //                FontSize = 12,
        //                LineBreakMode = LineBreakMode.NoWrap,
        //                HorizontalOptions = LayoutOptions.End,
        //                TextDecorations = TextDecorations.Underline,
        //                MinimumWidthRequest = 65,
        //                WidthRequest = 65,
        //                HorizontalTextAlignment = TextAlignment.End,
        //                GestureRecognizers = { new TapGestureRecognizer() { Command = _navigationCommand, CommandParameter = "tel:" + this.mobile } }
        //            };
        //    }
        //}
    }

    public class ObservablePersonSmallWSOCollection<K, T> : ObservableCollection<T>
    {
        private readonly K _key;

        public ObservablePersonSmallWSOCollection(IGrouping<K, T> group)
            : base(group)
        {
            _key = group.Key;
        }

        public K Key
        {
            get { return _key; }
        }
    }



}
