using iPMCloud.Mobile.TFControls;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(TFEntry), typeof(TFEntry_iOS_Renderer))]
namespace iPMCloud.Mobile.TFControls
{
    public class TFEntry_iOS_Renderer : EntryRenderer
    {

        public TFEntry_iOS_Renderer() { }
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                // do whatever you want to the UITextField here!
                Control.BackgroundColor = UIColor.FromRGB(204, 153, 255);
                Control.BorderStyle = UITextBorderStyle.Line;
            }
        }

    }
}
