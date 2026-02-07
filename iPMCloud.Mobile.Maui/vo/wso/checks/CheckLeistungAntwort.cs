using iPMCloud.Mobile.vo;
// TODO: SignaturePad not MAUI-compatible - needs replacement
// using SignaturePad.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Maui.Controls;

namespace iPMCloud.Mobile
{
    public class CheckLeistungAntwort
    {
        public Int32 id = 0;
        public Int32 leiid = 0;
        public Int32 check_a_id = 0;
        public string datum = "";
        public string frage = "";
        public int multi = 0;
        public int required = 1;
        public int a0 = -1;             // Ja/Nein Antwort
        public string a1 = "";          // Text Antwort
        public string a2 = "";      // Wert Antwort
        public string a3 = "";  // Bild Antwort
        public string a4 = "";     // (FOLGT)Einfach -/ Mehrfachauswahl Antworten
        public string a5 = "";     // (FOLGT)Ja / Nein Einfach-/Mehrfachauswahl Antwort
        public string a6 = "0";         // (FOLGT)Bewertung / Voting Antwort
        public string a7 = "";  // Unterschriftsfeld
        public string f4 = "-;-;-";     // 
        public string f5 = "-;-;-";     //
        public string kat = "";
        public string type = "";
        public string notiz = "";
        public int del = 0;


        public string check_guid = "";
        public string guid = "";
        public CheckLeistungAntwortBem bem { get; set; } = new CheckLeistungAntwortBem();
        public BemerkungWSO bemWSO { get; set; } = new BemerkungWSO();

        public bool isReady = false;
        public bool none = false;

        [NonSerialized]
        public bool inChangeMode = false;

        [NonSerialized]
        public Frame mainFrame = null;
        [NonSerialized]
        public Frame frame_Reset = null;
        [NonSerialized]
        public Frame frame_Yes = null;
        [NonSerialized]
        public Frame frame_No = null;
        [NonSerialized]
        public Frame frame_None = null;
        [NonSerialized]
        public Frame frame_Pic = null;
        [NonSerialized]
        public Frame frame_Bem = null;
        [NonSerialized]
        public List<Frame> frame_ants = null;


        [NonSerialized]
        public Editor textEditor = null;

        [NonSerialized]
        public Entry entry = null;

        [NonSerialized]
        public Label lb_quest = null;
        [NonSerialized]
        public Label lb_notiz = null;
        [NonSerialized]
        public Label lb_required = null;

        [NonSerialized]
        public Image img_ready = null;

        [NonSerialized]
        public Image img_sig = null;

        // TODO: SignaturePad not MAUI-compatible - comment out until migrated
        // [NonSerialized]
        // public SignaturePadView signPad = null;

        [NonSerialized]
        public StackLayout stack_Badge = null;
        [NonSerialized]
        public StackLayout stack_Bem_Badge = null;



        public void Tap_a0_Yes()
        {
            none = false;
            a0 = 2;
            CheckIsReadyAndSet_a0();
        }
        public void Tap_a0_No()
        {
            none = false;
            a0 = 1;
            CheckIsReadyAndSet_a0();
        }
        public void Tap_a0_None()
        {
            none = true;
            a0 = 0;
            CheckIsReadyAndSet_a0();
        }
        public void Tap_SetAllGuis_a0()
        {
            frame_Yes.BorderColor = isReady && a0 == 2 ? Color.White : Color.Transparent;
            frame_Yes.BackgroundColor = isReady && a0 != 2 ? Color.FromHex("#666666") : Color.FromHex("#04732d");
            frame_Yes.Opacity = isReady && a0 != 2 ? 0.5 : 1;
            frame_No.BorderColor = isReady && a0 == 1 ? Color.White : Color.Transparent;
            frame_No.BackgroundColor = isReady && a0 != 1 ? Color.FromHex("#666666") : Color.FromHex("#73042d");
            frame_No.Opacity = isReady && a0 != 1 ? 0.5 : 1;
            frame_None.BorderColor = none ? Color.White : Color.Transparent;
            frame_None.BackgroundColor = !none ? Color.FromHex("#666666") : Color.FromHex("#938302");
            frame_None.Opacity = !none ? 0.5 : 1;
            frame_Reset.IsVisible = isReady;

            lb_required.IsVisible = required == 1 && !isReady;
            img_ready.IsVisible = isReady;

            lb_quest.LineBreakMode = isReady ? LineBreakMode.TailTruncation : LineBreakMode.WordWrap;
            lb_notiz.LineBreakMode = isReady ? LineBreakMode.TailTruncation : LineBreakMode.WordWrap;

            mainFrame.BackgroundColor = isReady ? Color.FromHex("#66cccccc") : Color.FromHex("#99042d53");
            AppModel.Instance.MainPage.UpdateCheckAState();
        }
        public void Tap_a0_Reset()
        {
            none = false;
            a0 = -1;
            CheckIsReadyAndSet_a0();
            Tap_SetAllGuis_a0();
            frame_Reset.IsVisible = false;
        }
        public void CheckIsReadyAndSet_a0()
        {
            if (required == 1)
            {
                isReady = a0 > 0;
                Tap_SetAllGuis_a0();
                return;
            }
            isReady = a0 > -1 || none;
            Tap_SetAllGuis_a0();
        }



        public void Text_a1_Changed()
        {
            if (inChangeMode) return;
            none = false;
            a1 = textEditor.Text;
        }
        public void Text_a1_Focused()
        {
            textEditor.HeightRequest = -1;
        }
        public void Tap_a1_Reset()
        {
            none = false;
            a1 = "";
            textEditor.Text = "";
            CheckIsReadyAndSet_a1();
            frame_Reset.IsVisible = false;
            textEditor.HeightRequest = -1;
        }
        public void Text_a1_Completed()
        {
            if (textEditor != null)
            {
                a1 = textEditor.Text;
                if (!String.IsNullOrEmpty(a1)) { none = false; }
                textEditor.HeightRequest = 40;
                CheckIsReadyAndSet_a1();
            }
        }
        public void Tap_a1_None()
        {
            none = true;
            a1 = "";
            inChangeMode = true;
            textEditor.Text = a1;
            CheckIsReadyAndSet_a1();
            inChangeMode = false;
        }
        public void Tap_SetAllGuis_a1()
        {
            frame_Reset.IsVisible = isReady;
            lb_required.IsVisible = required == 1 && !isReady;
            img_ready.IsVisible = isReady;

            frame_None.BorderColor = none ? Color.White : Color.Transparent;
            frame_None.BackgroundColor = !none ? Color.FromHex("#666666") : Color.FromHex("#938302");
            frame_None.Opacity = !none ? 0.5 : 1;

            lb_quest.LineBreakMode = isReady ? LineBreakMode.TailTruncation : LineBreakMode.WordWrap;
            lb_notiz.LineBreakMode = isReady ? LineBreakMode.TailTruncation : LineBreakMode.WordWrap;

            mainFrame.BackgroundColor = isReady ? Color.FromHex("#66cccccc") : Color.FromHex("#99042d53");

            textEditor.HeightRequest = isReady ? 40 : -1;

            AppModel.Instance.MainPage.UpdateCheckAState();
        }
        public void CheckIsReadyAndSet_a1()
        {
            if (required == 1)
            {
                isReady = !String.IsNullOrWhiteSpace(a1);
                Tap_SetAllGuis_a1();
                return;
            }
            isReady = a1.Length > 0 || none;
            Tap_SetAllGuis_a1();
        }



        public void Text_a2_Changed()
        {
            if (inChangeMode) return;
            none = false;
            a2 = entry.Text;
        }
        public void Text_a2_Focused()
        {
            a2 = entry.Text;
            CheckIsReadyAndSet_a2();
        }
        public void Tap_a2_Reset()
        {
            none = false;
            a2 = "";
            entry.Text = "";
            CheckIsReadyAndSet_a2();
            frame_Reset.IsVisible = false;
        }
        public void Text_a2_Completed()
        {
            a2 = entry.Text;
            if (!String.IsNullOrEmpty(a2)) { none = false; }
            CheckIsReadyAndSet_a2();
        }
        public void Tap_a2_None()
        {
            none = true;
            a2 = "";
            inChangeMode = true;
            entry.Text = a2;
            CheckIsReadyAndSet_a2();
            inChangeMode = false;
        }
        public void Tap_SetAllGuis_a2()
        {
            frame_Reset.IsVisible = isReady;
            lb_required.IsVisible = required == 1 && !isReady;
            img_ready.IsVisible = isReady;

            frame_None.BorderColor = none ? Color.White : Color.Transparent;
            frame_None.BackgroundColor = !none ? Color.FromHex("#666666") : Color.FromHex("#938302");
            frame_None.Opacity = !none ? 0.5 : 1;

            lb_quest.LineBreakMode = isReady ? LineBreakMode.TailTruncation : LineBreakMode.WordWrap;
            lb_notiz.LineBreakMode = isReady ? LineBreakMode.TailTruncation : LineBreakMode.WordWrap;

            mainFrame.BackgroundColor = isReady ? Color.FromHex("#66cccccc") : Color.FromHex("#99042d53");

            AppModel.Instance.MainPage.UpdateCheckAState();
        }
        public void CheckIsReadyAndSet_a2()
        {
            if (required == 1)
            {
                isReady = !String.IsNullOrWhiteSpace(a2);
                Tap_SetAllGuis_a2();
                return;
            }
            isReady = a2.Length > 0 || none;
            Tap_SetAllGuis_a2();
        }





        public void Tap_a3_None()
        {
            none = true;
            a3 = "";
            bemWSO = new BemerkungWSO();
            CheckIsReadyAndSet_a3();
        }
        public async void Tap_SetAllGuis_a3()
        {
            frame_None.BorderColor = none ? Color.White : Color.Transparent;
            frame_None.BackgroundColor = !none ? Color.FromHex("#666666") : Color.FromHex("#938302");
            frame_None.Opacity = !none ? 0.5 : 1;

            frame_Reset.IsVisible = isReady;
            lb_required.IsVisible = required == 1 && !isReady;
            img_ready.IsVisible = isReady;

            frame_Pic.Opacity = none ? 0.75 : 1;

            lb_quest.LineBreakMode = isReady ? LineBreakMode.TailTruncation : LineBreakMode.WordWrap;
            lb_notiz.LineBreakMode = isReady ? LineBreakMode.TailTruncation : LineBreakMode.WordWrap;

            stack_Badge.Children.Clear();
            stack_Badge.Children.Add(
                    bemWSO != null && bemWSO.photos != null ? Check.GetBadgeRoundFrame(
                        bemWSO.photos.Count(),
                        false,
                        bemWSO.photos.Count() == 0)
                    : Check.GetBadgeRoundFrame(0, false, true)
                );


            mainFrame.BackgroundColor = isReady ? Color.FromHex("#66cccccc") : Color.FromHex("#99042d53");
            AppModel.Instance.MainPage.UpdateCheckAState();
        }
        public void Tap_a3_Pic()
        {
            none = false;
            AppModel.Instance.MainPage.ShowNoticeView_check_bem(this, true, false);
        }
        public void Tap_a3_Pic_Refresh()
        {
            if (bemWSO != null && bemWSO.photos != null && bemWSO.photos.Count > 0)
            {
                foreach (var item in bemWSO.photos)
                {
                    //a3 = "[##]" + System.Convert.ToBase64String(item.bytes);
                    item.stack = null;
                }
            }
            else
            {
                a3 = "";
            }
            CheckIsReadyAndSet_a3();
        }
        public void Tap_a3_Reset()
        {
            none = false;
            a3 = "";
            bemWSO = new BemerkungWSO();
            CheckIsReadyAndSet_a3();
        }
        public void CheckIsReadyAndSet_a3()
        {
            if (required == 1)
            {
                isReady = (bemWSO != null && bemWSO.photos != null && bemWSO.photos.Count > 0);
                Tap_SetAllGuis_a3();
                return;
            }
            isReady = none || (bemWSO != null && bemWSO.photos != null && bemWSO.photos.Count > 0);
            Tap_SetAllGuis_a3();
        }






        public void Tap_a4a_Ant(Image img)
        {
            none = false;
            var ants = a4.Split(new String[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (ants.Contains(img.ClassId))
            {
                ants.Remove(img.ClassId);
                img.Source = AppModel.Instance.imagesBase.No;
                img.Opacity = 0.5;
                foreach (var item in frame_ants) { if (item.ClassId == img.ClassId) item.BorderColor = Color.Transparent; };
            }
            else
            {
                ants.Add(img.ClassId);
                img.Source = AppModel.Instance.imagesBase.Yes;
                img.Opacity = 1;
                foreach (var item in frame_ants) { if (item.ClassId == img.ClassId) item.BorderColor = Color.White; };
            }
            a4 = Utils.ConvertStringListToString(ants, ";");
            CheckIsReadyAndSet_a4a();
        }
        public void Tap_a4a_Reset()
        {
            none = false;
            a4 = "";
            foreach (var item in frame_ants)
            {
                item.BorderColor = Color.Transparent;
                var img = (item.Content as StackLayout).Children[1] as Image;
                img.Source = AppModel.Instance.imagesBase.No;
                img.Opacity = 0.5;
            };
            CheckIsReadyAndSet_a4a();
            frame_Reset.IsVisible = false;
        }
        public void Tap_a4a_None()
        {
            Tap_a4a_Reset();
            none = true;
            CheckIsReadyAndSet_a4a();
            frame_Reset.IsVisible = true;
        }
        public void Tap_SetAllGuis_a4a()
        {
            frame_Reset.IsVisible = isReady;
            lb_required.IsVisible = required == 1 && !isReady;
            img_ready.IsVisible = isReady;

            frame_None.BorderColor = none ? Color.White : Color.Transparent;
            frame_None.BackgroundColor = !none ? Color.FromHex("#666666") : Color.FromHex("#938302");
            frame_None.Opacity = !none ? 0.5 : 1;

            lb_quest.LineBreakMode = isReady ? LineBreakMode.TailTruncation : LineBreakMode.WordWrap;
            lb_notiz.LineBreakMode = isReady ? LineBreakMode.TailTruncation : LineBreakMode.WordWrap;

            mainFrame.BackgroundColor = isReady ? Color.FromHex("#66cccccc") : Color.FromHex("#99042d53");

            AppModel.Instance.MainPage.UpdateCheckAState();
        }
        public void CheckIsReadyAndSet_a4a()
        {
            var ants = a4.Split(new String[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (required == 1)
            {
                isReady = ants.Count > 0;
                Tap_SetAllGuis_a4a();
                return;
            }
            isReady = ants.Count > 0 || none;
            Tap_SetAllGuis_a4a();
        }


        public void Tap_a4b_Ant(Image img)
        {
            none = false;
            bool wasSet = false;
            //List<string> listA = new List<string>(); 
            var ants = a4.Split(new String[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            //foreach (var an in ants) { listA.Add((int.Parse(an)+1)+""); };
            if (ants.Contains(img.ClassId))
            {
                ants.Remove(img.ClassId);
                img.Source = AppModel.Instance.imagesBase.No_Round;
                img.Opacity = 0.5;
                wasSet = true;
                foreach (var item in frame_ants)
                {
                    if (item.ClassId == img.ClassId) { item.BorderColor = Color.Transparent; }
                };
            }
            else
            {
                ants.Add(img.ClassId);
                img.Source = AppModel.Instance.imagesBase.Yes_Round;
                img.Opacity = 1;
                foreach (var item in frame_ants)
                {
                    if (item.ClassId == img.ClassId) { item.BorderColor = Color.White; }
                };
            }
            foreach (var item in frame_ants)
            {
                item.IsVisible = false;
                if (item.ClassId == img.ClassId || wasSet)
                {
                    item.IsVisible = true;
                }
            };
            a4 = Utils.ConvertStringListToString(ants, ";");
            CheckIsReadyAndSet_a4b();
        }
        public void Tap_a4b_Reset()
        {
            none = false;
            a4 = "";
            foreach (var item in frame_ants)
            {
                item.IsVisible = true;
                item.BorderColor = Color.Transparent;
                var img = (item.Content as StackLayout).Children[1] as Image;
                img.Source = AppModel.Instance.imagesBase.No_Round;
                img.Opacity = 0.5;
            };
            CheckIsReadyAndSet_a4b();
            frame_Reset.IsVisible = false;
        }
        public void Tap_a4b_None()
        {
            Tap_a4b_Reset();
            foreach (var item in frame_ants)
            {
                item.IsVisible = false;
            };
            none = true;
            CheckIsReadyAndSet_a4b();
            frame_Reset.IsVisible = true;
        }
        public void Tap_SetAllGuis_a4b()
        {
            frame_Reset.IsVisible = isReady;
            lb_required.IsVisible = required == 1 && !isReady;
            img_ready.IsVisible = isReady;

            frame_None.BorderColor = none ? Color.White : Color.Transparent;
            frame_None.BackgroundColor = !none ? Color.FromHex("#666666") : Color.FromHex("#938302");
            frame_None.Opacity = !none ? 0.5 : 1;

            lb_quest.LineBreakMode = isReady ? LineBreakMode.TailTruncation : LineBreakMode.WordWrap;
            lb_notiz.LineBreakMode = isReady ? LineBreakMode.TailTruncation : LineBreakMode.WordWrap;

            mainFrame.BackgroundColor = isReady ? Color.FromHex("#66cccccc") : Color.FromHex("#99042d53");

            AppModel.Instance.MainPage.UpdateCheckAState();
        }
        public void CheckIsReadyAndSet_a4b()
        {
            var ants = a4.Split(new String[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (required == 1)
            {
                isReady = ants.Count == 1;
                Tap_SetAllGuis_a4b();
                return;
            }
            isReady = ants.Count == 1 || none;
            Tap_SetAllGuis_a4b();
        }




        public void Tap_a7_None()
        {
            none = true;
            a7 = "";
            CheckIsReadyAndSet_a7();
        }
        public void Tap_SetAllGuis_a7()
        {
            frame_None.BorderColor = none ? Color.White : Color.Transparent;
            frame_None.BackgroundColor = !none ? Color.FromHex("#666666") : Color.FromHex("#938302");
            frame_None.Opacity = !none ? 0.5 : 1;

            frame_Reset.IsVisible = isReady;
            lb_required.IsVisible = required == 1 && !isReady;
            img_ready.IsVisible = isReady;
            frame_Yes.IsVisible = !isReady;
            frame_Bem.Margin = new Thickness(isReady ? 10 : 0, 5, 0, 0);


            lb_quest.LineBreakMode = isReady ? LineBreakMode.TailTruncation : LineBreakMode.WordWrap;
            lb_notiz.LineBreakMode = isReady ? LineBreakMode.TailTruncation : LineBreakMode.WordWrap;

            img_sig.IsVisible = !none;
            img_sig.Source = String.IsNullOrWhiteSpace(a7) ?
                AppModel.Instance.imagesBase.SignPad :
                ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(a7)));

            mainFrame.BackgroundColor = isReady ? Color.FromHex("#66cccccc") : Color.FromHex("#99042d53");
            AppModel.Instance.MainPage.UpdateCheckAState();
        }
        public void Tap_a7_Reset()
        {
            none = false;
            a7 = "";
            CheckIsReadyAndSet_a7();
        }
        public void Tap_a7_OpenSig()
        {
            AppModel.Instance.MainPage.OpenCheckA_Singature(this);
        }
        public async void Tap_a7_ReturnSig()
        {
            none = false;
            AppModel.Instance.MainPage.CloseCheckA_Singature(null, null);
            var stream = await signPad.GetImageStreamAsync(SignatureImageFormat.Png);
            if (stream != null)
            {
                byte[] imageArray = null;

                using (MemoryStream memory = new MemoryStream())
                {
                    stream.CopyTo(memory);
                    imageArray = memory.ToArray();
                    //image.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
                    a7 = System.Convert.ToBase64String(imageArray);
                }
            }
            CheckIsReadyAndSet_a7();
        }
        public void CheckIsReadyAndSet_a7()
        {
            if (required == 1)
            {
                isReady = a7.Length > 0;
                Tap_SetAllGuis_a7();
                return;
            }
            isReady = a7.Length > 0 || none;
            Tap_SetAllGuis_a7();
        }






        public void Tap_a_Bem(bool isSign = false)
        {
            AppModel.Instance.MainPage.ShowNoticeView_check_bem(this, false, isSign);
        }
        public void Tap_a_Bem_Refresh()
        {
            if (bemWSO != null && bemWSO.photos != null && bemWSO.photos.Count > 0)
            {
                foreach (var item in bemWSO.photos)
                {
                    //a3 = "[##]" + System.Convert.ToBase64String(item.bytes);
                    item.stack = null;
                }
            }
            else
            {
                a3 = "";
            }
            stack_Bem_Badge.Children.Clear();
            stack_Bem_Badge.Children.Add(
                    bemWSO != null && bemWSO.photos != null ? Check.GetBadgeRoundFrame(
                        bemWSO.photos.Count() > 0
                            ? bemWSO.photos.Count()
                            : (String.IsNullOrWhiteSpace(bemWSO.text) ? 0 : 1),
                        false,
                        bemWSO.photos.Count() == 0)
                    : Check.GetBadgeRoundFrame(0, false, true)
                );
        }




        public void ClearGui()
        {
            mainFrame = null;
            entry = null;
            textEditor = null;
            frame_Reset = null;
            frame_Yes = null;
            frame_No = null;
            frame_None = null;
            lb_quest = null;
            lb_notiz = null;
            lb_required = null;
            frame_ants = null;
            img_ready = null;
            frame_Pic = null;
            frame_Bem = null;
            img_sig = null;
            signPad = null;
            stack_Badge = null;
            stack_Bem_Badge = null;
        }
    }
}