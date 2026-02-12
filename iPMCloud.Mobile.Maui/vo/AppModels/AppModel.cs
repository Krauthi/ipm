//using Plugin.Permissions;
// TODO: Plugin.Connectivity not MAUI-compatible - use Microsoft.Maui.Networking.Connectivity
// using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Devices;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
//using Android.OS;

namespace iPMCloud.Mobile.vo
{
    public class AppModel
    {
        //! Instance ThisAppObject

        public static string Google_Translation_ApiKey = "AIzaSyCVwK7fQxV5PjzEEQUZBfuGh93pMwAtIe4";

        public DateTime AppOnStart;
        public DateTime AppOnSleep;
        public DateTime AppOnResume;
        public Application App { get; set; }
        public Int32 FastSyncCount { get; set; } = 0;
        public object Activity { get; set; }
        public string ActivityLastState { get; set; } = "";

        public AbsoluteLayout MainPageOverlay;

        public MainPage MainPage;
        public StartPage StartPage;

        public bool InclFilesAsJson = false;
        public static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public Connections Connections { get; set; }

        // Page Navigator
        public TFPageNavigator PageNavigator { get; set; }

        public Scanner Scan = null;
        public State State = null;

        //! Management for Settings
        public SettingModel SettingModel { get; set; } = new SettingModel();
        //! Management for Settings
        public AppSetModel AppSetModel { get; set; } = new AppSetModel();

        // Registrierte Companies
        public List<Company> Companies;


        // LoginUser as User
        public Lang Lang { get; set; } = new Lang();
        public List<Lang> Langs { get; set; } = new List<Lang>();
        public PersonWSO Person { get; set; } = null;
        public AppControll AppControll { get; set; } = new AppControll();

        public ChecksResponse ChecksInfoResponse { get; set; } = new ChecksResponse();
        public CheckInfo selectedCheckInfo { get; set; }
        public Check selectedCheck { get; set; }
        public CheckLeistungAntwort selectedCheckQuest { get; set; }
        public Check selectedCheckA { get; set; }
        public ChecksResponse ChecksResponse { get; set; } = new ChecksResponse();

        public PlanResponse PlanResponse { get; set; } = new PlanResponse();
        public PlanResponse PlanOthePersonResponse { get; set; } = new PlanResponse();


        public List<BuildingWSO> AllBuildings = new List<BuildingWSO>();
        public List<KategorieNames> AllKategorieNames = new List<KategorieNames>();
        public List<PersonWSO> AllWorkers = new List<PersonWSO>();

        public BuildingWSO SelectedBuilding { get; set; }
        public BuildingWSO LastBuilding { get; set; }
        public BuildingWSO OutScanBuilding { get; set; }
        public AuftragWSO LastSelectedOrder { get; set; }
        public AuftragWSO LastSelectedOrderTodo { get; set; }
        public KategorieWSO LastSelectedCategory { get; set; }
        public KategorieWSO LastSelectedCategoryTodo { get; set; }
        public LeistungWSO LastSelectedPosition { get; set; }
        public LeistungWSO LastSelectedPositionTodo { get; set; }

        public int posAgain { get; set; } = 1;
        public bool IsOptionalPosAgain { get; set; } = false;
        public AuftragWSO LastSelectedOrderAgain { get; set; }
        public KategorieWSO LastSelectedCategoryAgain { get; set; }
        public LeistungWSO LastSelectedPositionAgain { get; set; }


        // Refresh this data by logout
        public Dictionary<Int32, Border> allPositionInShowingListView = new Dictionary<Int32, Border>();
        public Dictionary<Int32, SwipeView> allPositionInShowingSmallListView = new Dictionary<Int32, SwipeView>();
        public List<LeistungWSO> allSelectedPositionToWork = new List<LeistungWSO>();
        public Dictionary<Int32, Border> allPositionAgainInShowingListView = new Dictionary<Int32, Border>();
        public Dictionary<Int32, SwipeView> allPositionAgainInShowingSmallListView = new Dictionary<Int32, SwipeView>();
        public List<LeistungWSO> allSelectedPositionAgainToWork = new List<LeistungWSO>();
        //public LeistungPackWSO allPositionInWork = null;
        public LeistungPackWSO allPositionInWork = null;
        public LeistungPackWSO allPositionDirectWork = null;
        public bool IsOptionalToWork = false;
        public List<DayOverWSO> dayOverList = new List<DayOverWSO>();
        public ObjektDataWSO selectedObjectValue = null;
        public ObjektDatenBildWSO selectedObjectValueBild = null;


        public bool UseExternHardware = false;
        public bool isInBackground = false;
        public bool isInBackgroundUploaded = false;

        public Dictionary<int, Switch> allAblesegrundStack = new Dictionary<int, Switch>();

        // Images Base
        public ImagesBase imagesBase = new ImagesBase();

        // State for NetworkConnection avilable


        public string entrygruppeid { get; set; } = "";
        public string entryname { get; set; } = "";
        public string entrypw { get; set; } = "";
        public string Build { get; set; } = "";
        public string Version { get; set; } = "";

        public bool isFlashLigthAloneON = false;

        public bool IsPopupOpen { get; set; } = false;
        public bool IsLandscape { get; set; } = false;

        public List<KeyValuePair<string, string>> PNList = new List<KeyValuePair<string, string>>();


        private bool _isInternet = false;

        public bool IsInternet
        {
            get
            {
                try
                {
                    _isInternet = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
                }
                catch (Exception)
                {
                    // Fallback: Bei Fehler annehmen, dass Internet verfügbar ist
                    _isInternet = true;
                }

                return true; // _isInternet;
            }
        }

        public List<string> connectionProfiles = new List<string>();

        private IEnumerable<ConnectionProfile> _isConnProfiles;
        public IEnumerable<ConnectionProfile> IsConnProfiles { get => _isConnProfiles; set => _isConnProfiles = value; }


        public List<Int32> Plan_ObjekteThisWeek = new List<Int32>();
        public List<Int32> Plan_KatThisWeek = new List<Int32>();
        public Border _showall_again_OrderCategory_frame = new Border();
        public Border _showall_OrderCategory_frame = new Border();
        public bool _showall_again_OrderCategory = false;
        public bool _showall_OrderCategory = false;



        // ********************************


        //? TEST ON/OFF 
        //! use Test Login and Server Data

#if DEBUG
        public bool IsTest { get; set; } = true;
        public bool RefreshPNToken = false;
#else
        public bool IsTest { get; set; } = false;
        public bool RefreshPNToken = false;
#endif
        // ********************************

        public AppModel()
        {
        }

        private async void ChekAppVersion()
        {
            try
            {
                //    string versionNumber = CrossStoreInfo.Current.InstalledVersionNumber;
                //    var appStoreInfo = await CrossStoreInfo.Current.GetAppInfo("com.ipmcloud.ipm.mobile");
                //    string latestVersionNumber = CrossStoreInfo.Current.GetLatestVersionNumber().Result;
                //await CrossStoreInfo.Current.OpenAppInStore();
                VersionTracking.Track();
                // First time ever launched application
                var firstLaunch = VersionTracking.IsFirstLaunchEver;

                // First time launching current version
                var firstLaunchCurrent = VersionTracking.IsFirstLaunchForCurrentVersion;

                // First time launching current build
                var firstLaunchBuild = VersionTracking.IsFirstLaunchForCurrentBuild;

                // Current app version (2.0.0)
                var currentVersion = VersionTracking.CurrentVersion;

                // Current build (2)
                var currentBuild = VersionTracking.CurrentBuild;

                // Previous app version (1.0.0)
                var previousVersion = VersionTracking.PreviousVersion;

                // Previous app build (1)
                var previousBuild = VersionTracking.PreviousBuild;

                // First version of app installed (1.0.0)
                var firstVersion = VersionTracking.FirstInstalledVersion;

                // First build of app installed (1)
                var firstBuild = VersionTracking.FirstInstalledBuild;

                // List of versions installed (1.0.0, 2.0.0)
                var versionHistory = VersionTracking.VersionHistory;

                // List of builds installed (1, 2)
                var buildHistory = VersionTracking.BuildHistory;
            }
            catch (Exception ex)
            {
            }
        }


        public Image anImage;

        public static AppModel _Instance;
        public static AppModel Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new AppModel();
                }
                return _Instance;
            }
            private set { }
        }

        public bool InitAppModel()
        {
            AppOnStart = DateTime.Now;

            AppSet.Load();
            SettingModel.model = this;
            SettingModel.InitializeSettings();
            Lang = Lang.Load();
            Companies = Company.LoadCompanies();
            PageNavigator = new TFPageNavigator(this);
            State = new State(this);
            Connections = new Connections(this);
            Scan = new Scanner(this);
            Person = PersonWSO.LoadPerson(this);// Wenn keine Person dann "null" !!
            InitBuildings();
            SetAllKategorieNames();
            AppControll = AppControll.Load(this);
            InitLangs();

            _ = IsInternet; // Initial abfragen 
            connectionProfiles = new List<string>();
            var profiles = Connectivity.Current.ConnectionProfiles;
            if (profiles != null && profiles.Any())
            {
                connectionProfiles = profiles.Select(p => p.ToString()).ToList();
            }

            Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;

            //ChekAppVersion();

            return true;
        }

        [Obsolete]
        void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            try
            {
                // Connection Profiles aktualisieren (ersetzt ConnectivityTypeChanged)
                connectionProfiles = new List<string>();
                if (e.ConnectionProfiles != null)
                {
                    foreach (var profile in e.ConnectionProfiles)
                    {
                        connectionProfiles.Add(profile.ToString());
                    }
                }
            }
            catch (Exception) { }

            // UI-Updates auf Main Thread
            if (this.App.MainPage != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        switch (App.MainPage.ClassId)
                        {
                            case "MainPage":
                                (App.MainPage as MainPage)?.ShowDisconnected();
                                break;
                            case "StartPage":
                                (App.MainPage as StartPage)?.ShowDisconnected();
                                break;
                        }
                    }
                    catch (Exception) { }
                });
            }
        }


        public void InitBuildings()
        {
            AllBuildings = BuildingWSO.GetAllBuildings(this, true);
            AllWorkers = BuildingWSO.GetAllWorkers(AllBuildings);
            // letztes Objekt selektieren
            InitLastBuilding();
        }
        public void SetAllKategorieNames()
        {
            AllKategorieNames = new List<KategorieNames>();
            AllBuildings.ForEach(b =>
            {
                b.ArrayOfAuftrag.ForEach(o =>
                {
                    o.kategorien.ForEach(c =>
                    {
                        AllKategorieNames.Add(new KategorieNames
                        {
                            id = c.id,
                            titel = c.titel,
                            titelLang = c.titelLang
                        });
                    });
                });
            });
        }
        public void AddKategorieNames(KategorieWSO c)
        {
            var kat = AllKategorieNames.Find(k => k.id == c.id);
            if (kat != null) { AllKategorieNames.Remove(kat); }
            AllKategorieNames.Add(new KategorieNames
            {
                id = c.id,
                titel = c.titel,
                titelLang = c.titelLang
            });
        }
        public void InitBuildingsAgain()
        {
            AllWorkers = BuildingWSO.GetAllWorkers(AllBuildings);
            // letztes Objekt selektieren
            InitLastBuilding();
        }
        public void InitLastBuilding()
        {
            if (SettingModel.SettingDTO.LastBuildingIdScanned > 0 && AllBuildings != null && AllBuildings.Count > 0)
            {
                LastBuilding = AllBuildings.Find(bu => bu.id == SettingModel.SettingDTO.LastBuildingIdScanned);
            }
            else
            {
                LastBuilding = null;
            }
        }


        public void SetAllObjectAndValuesToNoSelectedBuilding()
        {
            // Zurücksetzten aller States für die Auswahl der Ausführungen
            LastSelectedOrder = null;
            LastSelectedCategory = null;
            LastSelectedPosition = null;
            allPositionInShowingListView = new Dictionary<int, Border>();
            allPositionInShowingSmallListView = new Dictionary<int, SwipeView>();
            allSelectedPositionToWork = new List<LeistungWSO>();
            // alle selektionen und disabled zurücksetzen 
            if (LastBuilding != null)
            {
                LastBuilding.ArrayOfAuftrag.ForEach(o =>
                {
                    o.kategorien.ForEach(c =>
                    {
                        c.leistungen.ForEach(l =>
                        {
                            l.selected = false;
                            l.disabled = false;
                        });
                    });
                });
            }
            SettingModel.SettingDTO.LastBuildingIdScanned = -1;
            LastBuilding = null;
            SettingModel.SaveSettings();
        }

        //========================================================
        //========================================================
        //========================================================

        public int AnimDurationAndroid { get; set; } = 0;
        public int AnimDurationiOS { get; set; } = 0;
        public int AnimDuration
        {
            get
            {
                return DeviceSystem == "android" ? AnimDurationAndroid : AnimDurationiOS;
            }
        }

        public bool GpsIsRunning { get; set; } = false;
        public bool GpsHasChecked { get; set; } = false;
        public bool IsFirstLocationCheck { get; set; } = false;
        public string LocationStr { get; set; } = "";

        public string DeviceSystem { get; set; } = "undefine";
        public void InitDeviceInformation()
        {
            DeviceSystem = "undefine";
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                DeviceSystem = "android";
            }
            else if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                DeviceSystem = "ios";
            }
        }

        //public string CheckInternet()
        //{
        //    var r = "";
        //    if (IsInternet)
        //    {
        //        r = "Für diese Aktion müssen Sie ins INTERNET. Das INTERNET ist nicht erreichbar! \n\nPrüfen Sie ob MobileDaten oder WLAN aktiviert sind. \n\nPrüfen Sie ob die (iPM-Cloud)App-Datennutzungen aktiviert sind! \n\nWenn Sie WLAN aktiv haben, prüfen Sie ob in diesem WLAN auch INTERNET möglich ist! \n\nPrüfen ebenso ob der Flugmodus eingeschaltet ist. Der Flugmodus muss deaktiviert sein!";
        //    }
        //    return r;
        //}

        public string checkPermissionsMessage = "";
        public async void CheckPermissions()
        {
            checkPermissionsMessage = "";
            var r = "";
            try
            {
                PermissionStatus statusCam = await Permissions.CheckStatusAsync<Permissions.Camera>();
                if (statusCam != PermissionStatus.Granted && statusCam != PermissionStatus.Restricted)
                {
                    //  Berechtigung Fehlt
                    r = r + ";Kamera - Berechtigung muss aktiviert sein!";
                    _ = await Permissions.RequestAsync<Permissions.Camera>();
                    _ = await Permissions.CheckStatusAsync<Permissions.Camera>();
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error("ERROR: CheckPermissions -> " + ex.Message);
            }

            try
            {
                var statusPhoto = await Permissions.CheckStatusAsync<Permissions.Photos>();
                if (statusPhoto != PermissionStatus.Granted && statusPhoto != PermissionStatus.Restricted)
                {
                    //  Berechtigung Fehlt
                    r = r + ";Fotos & Videos - Berechtigung (Lesen) muss aktiviert sein!";
                    _ = await Permissions.RequestAsync<Permissions.Photos>();
                    _ = await Permissions.CheckStatusAsync<Permissions.Photos>();
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error("ERROR: CheckPermissions -> " + ex.Message);
            }

            try
            {
                var statusMedia = await Permissions.CheckStatusAsync<Permissions.Media>();
                if (statusMedia != PermissionStatus.Granted && statusMedia != PermissionStatus.Restricted)
                {
                    //  Berechtigung Fehlt
                    r = r + ";Speicher Fotos & Videos - Berechtigung (Lesen) muss aktiviert sein!";
                    _ = await Permissions.RequestAsync<Permissions.Media>();
                    _ = await Permissions.CheckStatusAsync<Permissions.Media>();
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error("ERROR: CheckPermissions -> " + ex.Message);
            }

            try
            {
                var statusFlash = await Permissions.CheckStatusAsync<Permissions.Flashlight>();
                if (statusFlash != PermissionStatus.Granted && statusFlash != PermissionStatus.Restricted)
                {
                    //  Berechtigung Fehlt
                    r = r + ";Flashlight - Berechtigung muss aktiviert sein!";
                    _ = await Permissions.RequestAsync<Permissions.Flashlight>();
                    _ = await Permissions.CheckStatusAsync<Permissions.Flashlight>();
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error("ERROR: CheckPermissions -> " + ex.Message);
            }


            try
            {
                var statusMaps = await Permissions.CheckStatusAsync<Permissions.Maps>();
                if (statusMaps != PermissionStatus.Granted && statusMaps != PermissionStatus.Restricted)
                {
                    //  Berechtigung Fehlt
                    r = r + ";Maps - Berechtigung muss aktiviert sein!";
                    _ = await Permissions.RequestAsync<Permissions.Maps>();
                    _ = await Permissions.CheckStatusAsync<Permissions.Maps>();
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error("ERROR: CheckPermissions -> " + ex.Message);
            }

            //try
            //{
            //    var statusRead = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            //    if (statusRead != PermissionStatus.Granted && statusRead != PermissionStatus.Restricted)
            //    {
            //        //  Berechtigung Fehlt
            //        r = r + ";Speicher - Berechtigung (Lesen) muss aktiviert sein!";
            //        _ = await Permissions.RequestAsync<Permissions.StorageRead>();
            //        _ = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    AppModel.Logger.Error("ERROR: CheckPermissions -> " + ex.Message);
            //}


            try
            {
                var statusLWIU = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (statusLWIU != PermissionStatus.Granted && statusLWIU != PermissionStatus.Restricted)
                {
                    //  Berechtigung Fehlt
                    r = r + ";LocationWhenInUse muss aktiviert sein!";
                    _ = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    _ = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error("ERROR: CheckPermissions -> " + ex.Message);
            }

            //try
            //{
            //    var statusWrite = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
            //    if (statusWrite != PermissionStatus.Granted && statusWrite != PermissionStatus.Restricted)
            //    {
            //        //  Berechtigung Fehlt
            //        r = r + ";Speicher - Berechtigung (Schreiben) muss aktiviert sein!";
            //        _ = await Permissions.RequestAsync<Permissions.StorageWrite>();
            //        _ = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    AppModel.Logger.Error("ERROR: CheckPermissions -> " + ex.Message);
            //}
            checkPermissionsMessage = r;
        }


        public bool gpsPermissionReady = false;
        public string checkPermissionGPSMessage = "";
        public async void CheckPermissionGPS()
        {

            checkPermissionGPSMessage = "";
            PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status == PermissionStatus.Denied)
            {
                gpsPermissionReady = false;
                AppModel.Logger.Error("ERROR: Permission Denied - Die (GPS)-Standortabfrage ist deaktiviert! ");
                var status1 = Permissions.RequestAsync<Permissions.LocationWhenInUse>().Result;
                //var status2 = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                //if (status2 == PermissionStatus.Denied)
                //{
                //    //checkPermissionGPSMessage = "Permission Denied - Die (GPS)-Standortabfrage ist deaktiviert! ";
                //}
            }
            else
            {
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    if (status == PermissionStatus.Granted)
                    {
                        //AppModel.Logger.Info("INFO: Permission Granted - Die Berechtigung für (GPS)-Standortabfrage ist gesetzt!");
                        checkPermissionGPSMessage = "";
                        gpsPermissionReady = true;
                    }
                    else
                    {
                        gpsPermissionReady = false;
                        AppModel.Logger.Error("ERROR: Permission Error - Du hast nicht die Berechtigung für (GPS-Standortabfrage) gesetzt!");
                        checkPermissionGPSMessage = "Permission Error - Du hast nicht die Berechtigung für (GPS-Standortabfrage) gesetzt!";
                    }
                }
                else if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    if (status == PermissionStatus.Granted || status == PermissionStatus.Restricted)
                    {
                        //AppModel.Logger.Info("INFO: Permission Granted - Die Berechtigung für (GPS)-Standortabfrage ist gesetzt! ");
                        checkPermissionGPSMessage = "";
                        gpsPermissionReady = true;
                    }
                    //else if (status == PermissionStatus.Restricted)
                    //{
                    //    AppModel.Logger.Error("ERROR: Permission Restricted - Du hast keine ausreichende Berechtigung für (GPS)-Standortabfrage auf Android gesetzt!");
                    //    checkPermissionGPSMessage =  "Permission Restricted - Du hast keine ausreichende Berechtigung für (GPS)-Standortabfrage auf Android gesetzt!";
                    //}
                    else
                    {
                        gpsPermissionReady = false;
                        AppModel.Logger.Error("ERROR: Permission Error - Du hast nicht die Berechtigung für (GPS-Standortabfrage) gesetzt!");
                        checkPermissionGPSMessage = "Permission Error - Du hast nicht die Berechtigung für (GPS-Standortabfrage) gesetzt!";
                    }
                }
                else
                {
                    gpsPermissionReady = false;
                    //throw new NotImplementedException("GPS wird nicht unterstützt!");
                    checkPermissionGPSMessage = "GPS wird nicht unterstützt!";
                }
            }

        }

        public bool isReachableServer = true;
        public long lastServerPing = 0;
        public bool gpsTimerIsRunning = false;
        public bool gpsAlertHasSend = false;
        public GPSService _gpsService;

        public void InitGPSTimer()
        {
            if (gpsTimerIsRunning) { return; }

            CheckPermissionGPS();

            if (String.IsNullOrWhiteSpace(checkPermissionGPSMessage))
            {
                gpsTimerIsRunning = true;
                
                _gpsService = new GPSService(App.Dispatcher);
                _gpsService.Start();
            }
        }

        public CancellationTokenSource _cts;
        public async void SetLocationGPS(bool inLog)
        {
            //inLog = false;
            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (this.MainPage != null)
                    {
                        this.MainPage.ShowDisGPS();
                    }
                    if (this.StartPage != null)
                    {
                        this.StartPage.ShowDisGPS();
                    }
                });
                var request = new GeolocationRequest(GeolocationAccuracy.Medium);
                _cts = new CancellationTokenSource(4000);

                var location = await Geolocation.GetLocationAsync(request, _cts.Token);
                if (location != null)
                {
                    this.LocationStr = ("" + location.Latitude + ";" + location.Longitude).Replace(",", ".");
                }
                else
                {
                    location = await Geolocation.GetLastKnownLocationAsync();
                    if (location != null)
                    {
                        this.LocationStr = ("" + location.Latitude + ";" + location.Longitude).Replace(",", ".");
                    }
                    else
                    {
                        this.LocationStr = null;
                    }
                }
                gpsAlertHasSend = false;
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
                if (!gpsAlertHasSend)
                {
                    gpsAlertHasSend = true;
                    if (!inLog)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            if (this.MainPage != null)
                            {
                                this.MainPage?.ShowAlertMessage("GPS", $"FeatureNotSupportedException: {fnsEx.Message}", true);
                            }
                            if (this.StartPage != null)
                            {
                                this.MainPage?.ShowAlertMessage("GPS", $"FeatureNotSupportedException: {fnsEx.Message}", true);
                            }
                        });
                    }
                    else
                    {
                        Logger.Warn("WARN: GPS - " + $"FeatureNotSupportedException: {fnsEx.Message}");
                    }
                }
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
                if (!gpsAlertHasSend)
                {
                    gpsAlertHasSend = true;
                    if (!inLog)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            if (this.MainPage != null)
                            {
                                this.MainPage?.ShowAlertMessage("GPS", "GPS ist nicht eingeschaltet bzw. verfügbar! (" +
                                    fneEx.Message + ")\r\n\r\nSchalten Sie das GPS in Ihrem Gerät ein und starten die App noch einmal neu!", true);
                            }
                            if (this.StartPage != null)
                            {
                                this.StartPage?.ShowAlertMessage("GPS", "GPS ist nicht eingeschaltet bzw. verfügbar! " +
                                    fneEx.Message + ")\r\n\r\nSchalten Sie das GPS in Ihrem Gerät ein und starten die App noch einmal neu!", true);
                            }
                        });
                    }
                    else
                    {
                        Logger.Warn("WARN: GPS ist nicht eingeschaltet bzw. verfügbar! " +
                                fneEx.Message + ")\r\n\r\nSchalten Sie das GPS in Ihrem Gerät ein und starten die App noch einmal neu!");
                    }
                }
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
                if (!gpsAlertHasSend)
                {
                    gpsAlertHasSend = true;
                    if (!inLog)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            if (this.MainPage != null)
                            {
                                this.MainPage?.ShowAlertMessage("GPS", "Die Berechtigung für die Standortabfrage wurde nicht gesetzt! " + pEx.Message, true);
                            }
                            if (this.StartPage != null)
                            {
                                this.MainPage?.ShowAlertMessage("GPS", "Die Berechtigung für die Standortabfrage wurde nicht gesetzt! " + pEx.Message, true);
                            }
                        });
                    }
                    else
                    {
                        Logger.Warn("WARN: GPS - Die Berechtigung für die Standortabfrage wurde nicht gesetzt! " + pEx.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                // Unable to get location
                if (!gpsAlertHasSend)
                {
                    gpsAlertHasSend = true;
                    if (!inLog)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            if (this.MainPage != null)
                            {
                                this.MainPage?.ShowAlertMessage("GPS", $"Exception: {ex.Message}", true);
                            }
                            if (this.StartPage != null)
                            {
                                this.MainPage?.ShowAlertMessage("GPS", $"Exception: {ex.Message}", true);
                            }
                        });
                    }
                    else
                    {
                        Logger.Warn("WARN: GPS " + $"Exception: {ex.Message}");
                    }
                }
            }
        }


        public void InitLangs()
        {
            Langs = new List<Lang>();
            langsStr.ForEach(s =>
            {
                Lang l = new Lang
                {
                    lang = s.Split(',')[1],
                    text = s.Split(',')[0],
                };
                Langs.Add(l);
            });
        }

        private List<string> langsStr = new List<string>
            {
                "Afrikaans,af",
                "Amharisch,am",
                "Arabisch,ar",
                "Aserbaidschanisch,az",
                "Belarussisch, sei",
                "Bulgarisch,bg",
                "Bengali, Milliarden",
                "Bosnisch,bs",
                "Katalanisch,ca",
                "Cebuano,ceb",
                "Korsisch,co",
                "Tschechisch,cs",
                "Walisisch,cy",
                "Dänisch, da",
                "Deutsch  (Standard),de",
                "Griechisch,el",
                "Englisch,en",
                "Esperanto,eo",
                "Spanisch,es",
                "Estnisch,et",
                "Baskisch,eu",
                "Persisch,fa",
                "Finnisch,fi",
                "Französisch,fr",
                "Friesisch, fy",
                "Irisch,ga",
                "Schottisch-Gälisch,gd",
                "Galizisch,gl",
                "Gujarati,gu",
                "Hausa,ha",
                "Hawaiianisch, haw",
                "Hindi, hallo",
                "Hmong,hmn",
                "Kroatisch,hr",
                "HaitianCreole,ht",
                "Ungarisch, hu",
                "Armenisch, hy",
                "Indonesisch,id",
                "Igbo,ig",
                "Isländisch,ist",
                "Italienisch,es",
                "Hebräisch,iw",
                "Japanisch,ja",
                "Javanisch, Zeuge Jehovas",
                "Georgisch,ka",
                "Kasachisch,kk",
                "Khmer,km",
                "Kannada,kn",
                "Koreanisch, ko",
                "Kurdisch Kurmanji,ku",
                "Kirgisisch,ky",
                "Latein,la",
                "Luxemburgisch,lb",
                "Lao,lo",
                "Litauisch,lt",
                "Lettisch,lv",
                "Madagassisch,mg",
                "Maori, mi",
                "Mazedonisch,mk",
                "Malayalam,ml",
                "Mongolisch,mn",
                "Marathi, Herr",
                "Malaiisch, Frau",
                "Maltesisch,mt",
                "Myanmar Burmesisch, mein",
                "Nepali,ne",
                "Niederländisch,nl",
                "Norwegisch, nein",
                "Chichewa,ny",
                "Punjabi, pa",
                "Polnisch,pl",
                "Paschtu,ps",
                "Portugiesisch,pt",
                "Rumänisch,ro",
                "Russisch,ru",
                "Sindhi,sd",
                "Singhalesisch,si",
                "Slowakisch,sk",
                "Slowenisch,sl",
                "Samoanisch,sm",
                "Shona,sn",
                "Somali, also",
                "Albanisch,sq",
                "Serbisch, sr",
                "Sesotho,st",
                "Sundanesisch,su",
                "Schwedisch,sv",
                "Swahili,sw",
                "Tamil,ta",
                "Telugu,te",
                "Tadschikisch,tg",
                "Thai,th",
                "Philippinisch, tl",
                "Türkisch,tr",
                "Ukrainisch, Großbritannien",
                "Urdu,ur",
                "Usbekisch,uz",
                "Vietnamesisch,vi",
                "Xhosa,xh",
                "Jiddisch,yi",
                "Yoruba, yo",
                "Chinesisch vereinfacht,zh",
                "Chinesisch traditionell,zh-TW",
                "Zulu,zu",
            };











        private async Task<bool> QuickZip(string directoryToZip, string destinationZipFullPath)
        {
            try
            {
                // Delete existing zip file if exists
                if (System.IO.File.Exists(destinationZipFullPath))
                    System.IO.File.Delete(destinationZipFullPath);

                if (!System.IO.Directory.Exists(directoryToZip))
                    return false;
                else
                {
                    System.IO.Compression.ZipFile.CreateFromDirectory(directoryToZip, destinationZipFullPath, System.IO.Compression.CompressionLevel.Optimal, true);
                    if (System.IO.File.Exists(destinationZipFullPath))
                    {
                        try
                        {
                            MailManager.SendMailDirect("mk@ipm-cloud.de", "dh@ipm-cloud.de",
                                "iPMCloud.Mobile (" + DeviceSystem + ") LogFile from :" + AppModel.Instance.SettingModel.SettingDTO.CustomerName + ": " + Person.vorname + " " + Person.name,
                                "iPMCloud.Mobile (" + DeviceSystem + ") LogFile from :" + AppModel.Instance.SettingModel.SettingDTO.CustomerName + ": " + Person.vorname + " " + Person.name,
                                "service@ipm-cloud.de",
                                new Attachment(destinationZipFullPath));
                            return true;
                        }
                        catch (Exception ex)
                        {
                            return false;
                        }
                    }
                    return false;
                }
            }
            catch (Exception e)
            {
                AppModel.Logger.Error("ERROR: QuickZip -> " + e.Message);
                return false;
            }
        }

        public void ClearLog()
        {
            Logger = NLog.LogManager.CreateNullLogger();
        }

        public bool SendLogZipFile()
        {
            string headstr = "";
            // Device Model (SMG-950U, iPhone10,6)
            var device = DeviceInfo.Model;
            headstr += "<p style=\"margin:2px 5px;\">DEVICE.INFO - Gerät: " + device + "</p>";
            // Manufacturer (Samsung)
            var manufacturer = DeviceInfo.Manufacturer;
            headstr += "<p style=\"margin:2px 5px;\">DEVICE.INFO - Manufacturer: " + manufacturer + "</p>";
            // Device Name (Motz's iPhone)
            var deviceName = DeviceInfo.Name;
            headstr += "<p style=\"margin:2px 5px;\">DEVICE.INFO - Gerätename: " + deviceName + "</p>";
            // Operating System Version Number (7.0)
            var version = DeviceInfo.VersionString;
            headstr += "<p style=\"margin:2px 5px;\">DEVICE.INFO - System Version: " + version + "</p>";
            // Platform (Android)
            var platform = DeviceInfo.Platform;
            headstr += "<p style=\"margin:2px 5px;\">DEVICE.INFO - Platform: " + platform + "</p>";
            // Idiom (Phone)
            var idiom = DeviceInfo.Idiom;
            headstr += "<p style=\"margin:2px 5px;\">DEVICE.INFO - IDIOM: " + idiom + "</p>";
            // Device Type (Physical)
            var deviceType = DeviceInfo.DeviceType;
            headstr += "<p style=\"margin:2px 5px;\">DEVICE.INFO - GeräteTyp: " + deviceType + "</p>";

            if (AppModel.Instance.Person != null)
            {
                AppModel.Logger.Info("INFO: App neu gestartet V" + AppModel.Instance.Version + " (" + AppModel.Instance.Person.name + " " + AppModel.Instance.Person.vorname + ")");
            }

            headstr += "<p style=\"margin:2px 5px;\">Von:" +
                AppModel.Instance.SettingModel.SettingDTO.CustomerName + ": " +
                AppModel.Instance.Person.name + " " + AppModel.Instance.Person.vorname + "</p>";
            headstr += "<p style=\"margin:2px 5px;\">Last GPS: " + AppModel.Instance.LocationStr + "</p>";


            string zipFilename;
            if (NLog.LogManager.IsLoggingEnabled())
            {
                string folder;

                if (DeviceInfo.Platform == DevicePlatform.iOS)
                    folder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library");
                else if (DeviceInfo.Platform == DevicePlatform.Android)
                    folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                else
                    throw new Exception("Could not show log: Platform undefined.");

                //Delete old zipfiles (housekeeping)
                try
                {
                    foreach (string fileName in System.IO.Directory.GetFiles(folder, "*.zip"))
                    {
                        System.IO.File.Delete(fileName);
                    }
                }
                catch (Exception ex)
                {
                    AppModel.Logger.Error("ERROR: SendLog -> Error deleting old zip files: {e.Message}");
                }

                string logFolder = System.IO.Path.Combine(folder, "logs");
                if (System.IO.Directory.Exists(logFolder))
                {
                    zipFilename = $"{folder}/{DateTime.Now.ToString("yyyyMMdd-HHmmss")}.zip";
                    int filesCount = System.IO.Directory.GetFiles(logFolder, "*.txt").Length;
                    string readText = "<div style=\"background-color: #ccc;font-size: 18px;padding:10px;\">" + headstr + "</div>";
                    System.IO.Directory.GetFiles(logFolder, "*.txt").ToList().ForEach(f =>
                    {
                        readText += "<br/><br/><br/><font style=\"color:#000099;\">" + f + "</font><br/>";
                        foreach (string line in System.IO.File.ReadLines(f))
                        {
                            readText += line.Replace("ERROR", "<font style=\"color:#aa0000;\">ERROR</font>").Replace("Error", "<font style=\"color:#aa0000;\">ERROR</font>")
                            .Replace("WARN", "<font style=\"color:#cc8800;\">WARN</font>").Replace("Warn", "<font style=\"color:#cc8800;\">WARN</font>")
                            .Replace("INFO", "<font style=\"color:#00aa00;\">INFO</font>").Replace("Info", "<font style=\"color:#00aa00;\">INFO</font>") + "<br/>";
                        }
                        readText += "<br/><hr/><br/><br/><br/><br/><br/>";
                    });
                    if (AppModel.Instance.InclFilesAsJson)
                    {
                        readText += GetFilesAsJson();
                    }
                    var res = Task.Run(() => { return AppModel.Instance.Connections.LogSync(readText); }).Result;
                    if (res)
                    {
                        return true;
                    }


                    //if (filesCount > 0)
                    //{
                    //    AppModel.Logger.Info("Info: SendLogZipFile (" +
                    //        AppModel.Instance.SettingModel.SettingDTO.CustomerName + ": " +
                    //        AppModel.Instance.Person.name + " " + AppModel.Instance.Person.vorname + ")");
                    //    AppModel.Logger.Info("Info: Last GPS (" + AppModel.Instance.LocationStr + ")");
                    //    if (!QuickZip(logFolder, zipFilename).Result)
                    //    {
                    //        zipFilename = string.Empty;
                    //    }
                    //}

                    //else
                    //    zipFilename = string.Empty;
                }
                else
                {
                    zipFilename = string.Empty;
                    return false;
                }
            }
            else
            {
                zipFilename = string.Empty;
                return false;
            }

            return false;
        }


        public string GetFilesAsJson()
        {
            string json = "";
            try
            {
                json += "Settings: " + SettingModel.LoadSettings_AsJson();
                json += "<br/><br/><br/>";
            }
            catch
            {
                json += "Settings: ERROR - Settings kann nicht geladen werden!";
                json += "<br/><br/><br/>";
            }
            try
            {
                json += "AppSet: " + AppSet.Load_AsJson();
                json += "<br/><br/><br/>";
            }
            catch
            {
                json += "AppSet: ERROR - AppSet kann nicht geladen werden!";
                json += "<br/><br/><br/>";
            }
            try
            {
                json += "Companies: " + Company.LoadCompanies_AsJson();
                json += "<br/><br/><br/>";
            }
            catch
            {
                json += "Companies: ERROR - Companies kann nicht geladen werden!";
                json += "<br/><br/><br/>";
            }
            try
            {
                json += "AppControll: " + AppControll.Load_AsJson();
                json += "<br/><br/><br/>";
            }
            catch
            {
                json += "AppControll: ERROR - AppControll kann nicht geladen werden!";
                json += "<br/><br/><br/>";
            }
            try
            {
                json += "Person: " + PersonWSO.LoadPerson_AsJson();
                json += "<br/><br/><br/>";
            }
            catch
            {
                json += "Person: ERROR - Person kann nicht geladen werden!";
                json += "<br/><br/><br/>";
            }
            //json += "Buildings: " + BuildingWSO.GetAllBuildings_ASJSON();
            //json += Environment.NewLine + Environment.NewLine;

            //json += "LeistungPackWSO: " + LeistungPackWSO.Load_AsJson();
            //json += "<br/><br/><br/>";
            try
            {
                json += "LeistungPackWSOFromStack: " + LeistungPackWSO.LoadAllFromUploadStack_AsJson();
                json += "<br/><br/><br/>";
            }
            catch
            {
                json += "LeistungPackWSOFromStack: ERROR - LeistungPackWSOFromStack kann nicht geladen werden!";
                json += "<br/><br/><br/>";
            }

            //json += "Bemerkungen: " + BemerkungWSO.LoadAll_AsJson();
            //json += "<br/><br/><br/>";
            try
            {
                json += "BemerkungenFromStack: " + BemerkungWSO.LoadAllFromUploadStack_AsJson();
                json += "<br/><br/><br/>";
            }
            catch
            {
                json += "BemerkungenFromStack: ERROR - BemerkungenFromStack kann nicht geladen werden!";
                json += "<br/><br/><br/>";
            }

            return json;
        }
    }
}
