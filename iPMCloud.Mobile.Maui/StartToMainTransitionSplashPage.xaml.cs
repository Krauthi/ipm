using iPMCloud.Mobile.vo;
using Microsoft.Maui.Controls;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace iPMCloud.Mobile
{
    /// <summary>
    /// Dedicated transition splash shown only when navigating from StartPage to MainPage.
    /// </summary>
    public partial class StartToMainTransitionSplashPage : ContentPage
    {
        private const int MinimumDisplayTimeMs = 350;
        private readonly string _targetSubPage;
        private readonly CancellationTokenSource _navigationCts = new();
        private bool _hasNavigated;

        public StartToMainTransitionSplashPage(string targetSubPage = "")
        {
            _targetSubPage = targetSubPage ?? "";
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_hasNavigated)
            {
                return;
            }

            _hasNavigated = true;

            try
            {
                await Task.Delay(MinimumDisplayTimeMs, _navigationCts.Token);
                AppModel.Logger.Info("StartToMainTransitionSplashPage: transitioning to MainPage");
                AppModel.Instance?.PageNavigator?.NavigateToMainPageAfterStartTransition(_targetSubPage);
            }
            catch (OperationCanceledException)
            {
                AppModel.Logger.Info("StartToMainTransitionSplashPage: transition cancelled.");
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error($"StartToMainTransitionSplashPage failed: {ex.Message}");
                AppModel.Instance?.PageNavigator?.NavigateToMainPageAfterStartTransition(_targetSubPage);
            }
        }

        protected override void OnDisappearing()
        {
            if (!_navigationCts.IsCancellationRequested)
            {
                _navigationCts.Cancel();
            }

            base.OnDisappearing();
        }
    }
}
