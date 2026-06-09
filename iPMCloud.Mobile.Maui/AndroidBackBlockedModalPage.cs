namespace iPMCloud.Mobile
{
    public class AndroidBackBlockedModalPage : ContentPage
    {
        protected override bool OnBackButtonPressed()
        {
            if (OperatingSystem.IsAndroid())
            {
                return true;
            }

            return base.OnBackButtonPressed();
        }
    }
}
