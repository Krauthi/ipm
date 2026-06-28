using iPMCloud.Mobile.vo;

public class GPSService : IDisposable
{
    private IDispatcherTimer _gpsTimer;
    private bool _isRunning = false;
    private readonly IDispatcher _dispatcher;

    public GPSService(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public bool IsRunning => _isRunning;

    public void Start()
    {
        if (_isRunning) return;

        var permissionMessage = CheckPermissionGPS();
        if (!string.IsNullOrWhiteSpace(permissionMessage))
        {
            // Keine GPS-Berechtigung
            return;
        }

        _isRunning = true;

        _gpsTimer = _dispatcher.CreateTimer();
        _gpsTimer.Interval = TimeSpan.FromSeconds(8);
        _gpsTimer.Tick += async (s, e) =>
        {
            try
            {
                await UpdateGPSLocationAsync();
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error($"GPS Update failed: {ex.Message}");
            }
        };
        _gpsTimer.Start();
    }

    public void Stop()
    {
        if (_gpsTimer != null)
        {
            _gpsTimer.Stop();
            _gpsTimer = null;
        }
        _isRunning = false;
    }

    private async Task UpdateGPSLocationAsync()
    {
        await Task.Run(() => AppModel.Instance.SetLocationGPS(true));
    }

    private string CheckPermissionGPS()
    {
        // Deine Berechtigungs-Prüfung
        return null; // oder Fehlermeldung
    }

    public void Dispose()
    {
        Stop();
    }
}

// Verwendung in einer Page oder ViewModel:
//public partial class MyPage : ContentPage
//{
//    private GPSService _gpsService;

//    public MyPage()
//    {
//        InitializeComponent();
//        _gpsService = new GPSService(Dispatcher);
//    }

//    protected override void OnAppearing()
//    {
//        base.OnAppearing();
//        _gpsService.Start();
//    }

//    protected override void OnDisappearing()
//    {
//        base.OnDisappearing();
//        _gpsService.Stop();
//    }
//}