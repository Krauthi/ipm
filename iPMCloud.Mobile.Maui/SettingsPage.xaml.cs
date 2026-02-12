using iPMCloud.Mobile.vo;
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Devices;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace iPMCloud.Mobile
{
    public partial class SettingsPage : ContentPage
    {
        public AppModel model;
        public Editor entry_LoginName = new Editor();
        public Entry entry_LoginPassword = new Entry();
        public Editor entry_LoginHost = new Editor();
        public Editor entry_LoginAddress = new Editor();
        bool isInitialize = false;

        public SettingsPage()
        {
            InitializeComponent();
        }

        public SettingsPage(AppModel model)
        {
            InitializeComponent();
            isInitialize = true;
            this.model = model;

            //img_back.Source = new ImagesBase().DropLeftImage;
            //img_save.Source = new ImagesBase().SaveImage;


            // Check NetworkState
            //var current = Connectivity.NetworkAccess;
            //var profiles = Connectivity.ConnectionProfiles;
            //lblNetworkStatus.Text = (current == NetworkAccess.Internet) ? "Network is Available":"Network is Not Available";
            //lblNetworkProfile.Text = (profiles.Contains(ConnectionProfile.WiFi)) ? profiles.FirstOrDefault().ToString():lblNetworkProfile.Text = profiles.FirstOrDefault().ToString();


            InitializeEventHandlers();
        }
        public void LoginNameChangedHandeler(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue.IndexOf(" ") > -1) { entry_LoginName.Text = e.NewTextValue.Replace(" ", String.Empty); };
            //ent_loginname.Text = e.NewTextValue.Trim();
            model.SettingModel.SettingDTO.LoginName = entry_LoginName.Text;
        }

        public void LoginPasswordChangedHandeler(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue.IndexOf(" ") > -1) { entry_LoginPassword.Text = e.NewTextValue.Replace(" ", String.Empty); };
            model.SettingModel.SettingDTO.LoginPassword = entry_LoginPassword.Text;
        }

        public void HostChangedHandeler(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue.IndexOf(" ") > -1) { entry_LoginHost.Text = e.NewTextValue.Replace(" ", String.Empty); };
        }

        public void AddressChangedHandeler(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue.IndexOf(" ") > -1) { entry_LoginAddress.Text = e.NewTextValue.Replace(" ", String.Empty); };
        }

        public void CredentialsChangedHandeler(object sender, EventArgs e)
        {
            return;
        }



        public async void InitializeEventHandlers()
        {
            //foreach (var i in radioGroup_fontsize.Children)
            //{
            //    var rb = (i as SfRadioButton);
            //    rb.IsChecked = (rb.Text == model.SettingModel.SettingDTO.FontSize);
            //}

            InitializeLoginElements();

            await Task.Delay(1);
            isInitialize = false;
        }

        public void InitializeLoginElements()
        {
            isInitialize = true;




            entry_LoginName.TextChanged -= LoginNameChangedHandeler;
            entry_LoginName.TextChanged += LoginNameChangedHandeler;
            entry_LoginPassword.TextChanged -= LoginPasswordChangedHandeler;
            entry_LoginPassword.TextChanged += LoginPasswordChangedHandeler;

            entry_LoginHost.TextChanged -= HostChangedHandeler;
            entry_LoginHost.TextChanged += HostChangedHandeler;
            entry_LoginAddress.TextChanged -= AddressChangedHandeler;
            entry_LoginAddress.TextChanged += AddressChangedHandeler;

            isInitialize = false;
        }

        public Page GetPage(string subPage = "")
        {
            return this;
        }

        public async void FlashON_Handle_Clicked(object sender, System.EventArgs e)
        {
            try
            {
                // Turn On Flashlight  
                await Flashlight.TurnOnAsync();
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                await DisplayAlert("(FNS) Faild by take ON Flashlight", fnsEx.Message, "Ok");
            }
            catch (PermissionException pEx)
            {
                await DisplayAlert("(P) Faild by take ON Flashlight", pEx.Message, "Ok");
            }
            catch (Exception ex)
            {
                await DisplayAlert("(R) Faild by take ON Flashlight", ex.Message, "Ok");
            }
        }
        public async void FlashOFF_Handle_Clicked_1(object sender, System.EventArgs e)
        {
            try
            {
                // Turn Off Flashlight  
                await Flashlight.TurnOffAsync();
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                await DisplayAlert("(FNS)´Faild by take OFF Flashlight", fnsEx.Message, "Ok");
            }
            catch (PermissionException pEx)
            {
                await DisplayAlert("(P) Faild by take OFF Flashlight", pEx.Message, "Ok");
            }
            catch (Exception ex)
            {
                await DisplayAlert("(R) Faild by take OFF Flashlight", ex.Message, "Ok");
            }
        }


    }
}