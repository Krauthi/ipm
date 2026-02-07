using Android.Content;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Text;
using iPMCloud.Mobile.TFControls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomScrollView), typeof(CustomScrollView_Android_Renderer))]
namespace iPMCloud.Mobile.TFControls
{
    public class CustomScrollView_Android_Renderer : ScrollViewRenderer
    {
        public CustomScrollView_Android_Renderer(Context context) : base(context)
        {
        }
        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
//            base.OnElementChanged(e);
            //Bounces = false; 
            
        }


    }
}
