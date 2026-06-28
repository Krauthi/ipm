using iPMCloud.Mobile.vo;
using Microsoft.Maui.Controls;
using System;
using System.Threading;
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

        /// <summary>
        /// Maximum time (ms) to wait for startup navigation before forcing a
        /// fallback to StartPage.  Prevents a permanent hang on the splash screen
        /// if navigation fails silently (e.g. after a notification-tap restart).
        /// </summary>
        private const int StartupTimeoutMs = 15_000;

        public SplashOverlayPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // The CancellationTokenSource below guards only against the minimum-splash
            // delay and CheckLoginAsync potentially blocking (CheckLoginAsync is currently
            // synchronous and completes instantly, so this is a safety net for future
            // refactors).  NavigateTo() is fire-and-forget (BeginInvokeOnMainThread), so
            // navigation itself is not cancellable here; it relies on the navigator-state
            // reset in App.InitApp() to always succeed.
            using var cts = new CancellationTokenSource(StartupTimeoutMs);

            try
            {
                // Run the login check and the minimum splash delay concurrently so the
                // total wait is max(loginCheckTime, MinimumSplashDisplayTimeMs).
                var splashDelayTask = Task.Delay(MinimumSplashDisplayTimeMs, cts.Token);
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
            catch (OperationCanceledException)
            {
                // Startup timed out – this should never happen in production after the
                // navigator-state reset fix, but serves as a last-resort safety net.
                AppModel.Logger?.Error(
                    $"SplashOverlayPage: startup timed out after {StartupTimeoutMs / 1000}s " +
                    "(navigator state was not reset before this page appeared). " +
                    "Forcing navigation to StartPage.");

                try
                {
                    AppModel.Instance?.PageNavigator.CurrentMainPage = "";
                    AppModel.Instance?.PageNavigator.NavigateTo(TFPageNavigator.PAGE_STARTPAGE);
                }
                catch (Exception fallbackEx)
                {
                    AppModel.Logger?.Error($"SplashOverlayPage: timeout fallback navigation failed: {fallbackEx.Message}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SplashOverlayPage.OnAppearing error: {ex.Message}");
                AppModel.Logger?.Error($"SplashOverlayPage: startup decision failed: {ex.Message}");

                // Fallback: navigate to StartPage so the app doesn't get stuck
                try
                {
                    AppModel.Instance?.PageNavigator.NavigateTo(TFPageNavigator.PAGE_STARTPAGE);
                }
                catch (Exception innerEx)
                {
                    System.Diagnostics.Debug.WriteLine($"SplashOverlayPage emergency fallback error: {innerEx.Message}");
                    AppModel.Logger?.Error($"SplashOverlayPage: emergency fallback failed: {innerEx.Message}");
                }
            }
        }
    }
}
