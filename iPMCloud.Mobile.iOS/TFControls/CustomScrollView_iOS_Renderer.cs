using iPMCloud.Mobile.TFControls;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomScrollView), typeof(CustomScrollView_iOS_Renderer))]
namespace iPMCloud.Mobile.TFControls
{
    public class CustomScrollView_iOS_Renderer : ScrollViewRenderer
    {

        public CustomScrollView_iOS_Renderer() { }
        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);
            Bounces = false;
        }

    }
}
