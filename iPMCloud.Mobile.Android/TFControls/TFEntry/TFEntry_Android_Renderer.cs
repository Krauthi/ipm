using Android.Content;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Text;
using iPMCloud.Mobile.TFControls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(TFEntry), typeof(TFEntry_Android_Renderer))]
namespace iPMCloud.Mobile.TFControls
{
    public class TFEntry_Android_Renderer : EntryRenderer
    {
        public TFEntry_Android_Renderer(Context context) : base(context)
        {
        }
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                GradientDrawable gd = new GradientDrawable();
                gd.SetColor(global::Android.Graphics.Color.Blue);
                Control.SetBackgroundDrawable(gd);
                Control.SetHintTextColor(global::Android.Graphics.Color.Blue);
                Control.SetLinkTextColor(global::Android.Graphics.Color.Red);
                Control.SetHighlightColor(global::Android.Graphics.Color.Red);

                Control.SetRawInputType(InputTypes.TextFlagNoSuggestions);
                Control.SetHintTextColor(ColorStateList.ValueOf(global::Android.Graphics.Color.Yellow));
            }
        }

    }
}
