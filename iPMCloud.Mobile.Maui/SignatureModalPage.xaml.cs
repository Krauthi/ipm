using CommunityToolkit.Maui.Core;
    using CommunityToolkit.Maui.Views;
    using iPMCloud.Mobile.Controls;
using iPMCloud.Mobile.vo;
using SkiaSharp;
using System.Threading.Tasks;

namespace iPMCloud.Mobile
{ 
    public partial class SignatureModalPage : ContentPage
    {
        private static readonly SemaphoreSlim _modalSemaphore = new(1, 1);
        private readonly TaskCompletionSource<SignatureResult?> _tcs = new();
        public Task<SignatureResult?> Result => _tcs.Task;

        public SignatureModalPage()
        {
            InitializeComponent();
//            NavigationPage.SetHasNavigationBar(this, false);

//#if IOS
//    this.On<iOS>().SetUseSafeArea(false);
//#endif


            btn_back_sign.GestureRecognizers.Clear();
            var tgr3 = new TapGestureRecognizer();
            tgr3.Tapped -= OnCancelClicked;
            tgr3.Tapped += OnCancelClicked;
            btn_back_sign.GestureRecognizers.Add(tgr3);
        }

        public static async Task<SignatureResult?> ShowAsync(Page callerPage)
        {
            if (!await _modalSemaphore.WaitAsync(0))
            {
                return null;
            }

            try
            {
                var modal = new SignatureModalPage();
                await MainThread.InvokeOnMainThreadAsync(() =>
                    callerPage.Navigation.PushModalAsync(modal, animated: false));
                return await modal.Result;
            }
            finally
            {
                _modalSemaphore.Release();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _tcs.TrySetResult(null);
        }


//        protected override void OnAppearing()
//        {
//            base.OnAppearing();
//#if ANDROID
//            AndroidFullscreen.SetFullscreen(true);
//#endif
//#if IOS
//        iOSFullscreen.SetFullscreen(true);
//#endif
//        }

//        protected override void OnDisappearing()
//        {
//            base.OnDisappearing();
//#if ANDROID
//            AndroidFullscreen.SetFullscreen(false);
//#endif
//#if IOS
//        iOSFullscreen.SetFullscreen(false);
//#endif
//        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            _tcs.TrySetResult(null);
            await Navigation.PopModalAsync(animated: false);
        }

        private void OnClearClicked(object sender, EventArgs e)
        {
            Pad.Clear();
        }

        private async void OnOkClicked(object sender, EventArgs e)
        {
            try
            {
                // Falls nichts gezeichnet wurde -> abbrechen oder Validierung
                if (Pad.Lines is null || Pad.Lines.Count == 0)
                {
                    _tcs.TrySetResult(null);
                    await Navigation.PopModalAsync(animated: false);
                    return;
                }
                const int width = 900;
                const int height = 450;
                // Zielbild-Größe (Pixel). Du kannst das anpassen.
                var pngBytes = SignatureRender.RenderToPng(
                    Pad.Lines,
                    targetWidth: width,
                    targetHeight: height,
                    background: SKColors.White,   // oder Transparent
                    strokeColor: SKColors.Black,
                    strokeWidth: (float)Pad.LineWidth,
                    padding: 16f);

                var totalPoints = Pad.Lines.Sum(l => l.Points?.Count ?? 0);
                var first = Pad.Lines.FirstOrDefault()?.Points?.FirstOrDefault();
                System.Diagnostics.Debug.WriteLine($"Lines={Pad.Lines.Count}, Points={totalPoints}, First={first}");

                var img = ImageSource.FromStream(() => new MemoryStream(pngBytes));
                _tcs.TrySetResult(new SignatureResult(pngBytes, img));
                await Navigation.PopModalAsync(animated: false);
            }
            catch
            {
                _tcs.TrySetResult(null);
                await Navigation.PopModalAsync(animated: false);
            }
        }

        private static byte[] RenderLinesToPngBytes(
            IReadOnlyList<IDrawingLine> lines,
            int width,
            int height,
            SKColor background,
            SKColor stroke,
            float strokeWidth)
        {
            using var surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
            var canvas = surface.Canvas;

            canvas.Clear(background);

            using var paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = stroke,
                StrokeWidth = strokeWidth,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round
            };

            // WICHTIG:
            // DrawingView speichert Points i.d.R. normalisiert (0..1) relativ zur View.
            // Daher skalieren wir auf unsere Pixelgröße.
            foreach (var line in lines)
            {
                if (line?.Points is null || line.Points.Count < 2)
                    continue;

                using var path = new SKPath();

                var p0 = line.Points[0];
                path.MoveTo((float)p0.X * width, (float)p0.Y * height);

                for (int i = 1; i < line.Points.Count; i++)
                {
                    var p = line.Points[i];
                    path.LineTo((float)p.X * width, (float)p.Y * height);
                }

                canvas.DrawPath(path, paint);
            }

            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }
    }

    public record SignatureResult(byte[] PngBytes, ImageSource Image);
}