using iPMCloud.Mobile.vo;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace iPMCloud.Mobile
{
    /// <summary>
    /// Startup overlay shown immediately after app launch.
    /// Displays the splash image with a spinning ActivityIndicator while the
    /// app initialises.  Transitions to the real StartPage once ready.
    /// </summary>
    public partial class SplashOverlayPage : ContentPage
    {
        /// <summary>Minimum time (ms) the splash overlay is visible.</summary>
        private const int MinimumSplashDisplayTimeMs = 1000;
        private readonly bool _navigateToStartPageOnAppear;

        public SplashOverlayPage()
            : this(true)
        {
        }

        public SplashOverlayPage(bool navigateToStartPageOnAppear)
        {
            InitializeComponent();
            _navigateToStartPageOnAppear = navigateToStartPageOnAppear;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                // Minimum display time so the splash + loader is visible.
                // App.InitApp() already ran synchronously in the App constructor,
                // so StartPage is available immediately; this delay is purely visual.
                await Task.Delay(MinimumSplashDisplayTimeMs);
                if (!_navigateToStartPageOnAppear)
                {
                    return;
                }

                AppModel.Instance?.PageNavigator.NavigateTo(TFPageNavigator.PAGE_STARTPAGE);
                //ContentPage startPage = AppModel.Instance?.StartPage ?? new ContentPage { BackgroundColor = Colors.Black };

                // Switch the window's root page – no navigation stack needed
                //if (Application.Current?.Windows?.Count > 0)
                //{
                //    Application.Current.Windows[0].Page = startPage;
                //}
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SplashOverlayPage.OnAppearing error: {ex.Message}");

                // Emergency fallback
                try
                {
                    var fallback = _navigateToStartPageOnAppear
                        ? (AppModel.Instance?.StartPage ?? new ContentPage { BackgroundColor = Colors.Black })
                        : new ContentPage { BackgroundColor = Colors.Black };
                    if (Application.Current?.Windows?.Count > 0)
                        Application.Current.Windows[0].Page = fallback;
                }
                catch (Exception innerEx)
                {
                    System.Diagnostics.Debug.WriteLine($"SplashOverlayPage emergency fallback error: {innerEx.Message}");
                    // Nothing more we can do at this stage
                }
            }
        }
    }
}
