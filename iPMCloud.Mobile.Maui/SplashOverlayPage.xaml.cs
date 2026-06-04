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
        private const int MinimumSplashDisplayTimeMs = 250;

        public SplashOverlayPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                // Run the login check and the minimum splash delay concurrently so the
                // total wait is max(loginCheckTime, MinimumSplashDisplayTimeMs).
                var splashDelayTask = Task.Delay(MinimumSplashDisplayTimeMs);
                var loginTask = AppModel.Instance?.CheckLoginAsync() ?? Task.FromResult(false);

                await Task.WhenAll(splashDelayTask, loginTask);

                bool loginValid = loginTask.Result;

                if (loginValid)
                {
                    AppModel.Logger.Info("SplashOverlayPage: valid login -> navigating to MainPage");
                    AppModel.Instance?.PageNavigator.NavigateTo(TFPageNavigator.PAGE_MAINPAGE);
                }
                else
                {
                    AppModel.Logger.Info("SplashOverlayPage: no valid login -> navigating to StartPage");
                    AppModel.Instance?.PageNavigator.NavigateTo(TFPageNavigator.PAGE_STARTPAGE);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SplashOverlayPage.OnAppearing error: {ex.Message}");
                AppModel.Logger.Error($"SplashOverlayPage: startup decision failed: {ex.Message}");

                // Fallback: navigate to StartPage so the app doesn't get stuck
                try
                {
                    AppModel.Instance?.PageNavigator.NavigateTo(TFPageNavigator.PAGE_STARTPAGE);
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
