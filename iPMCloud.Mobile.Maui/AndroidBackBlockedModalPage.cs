namespace iPMCloud.Mobile
{
    internal class AndroidBackBlockedModalPage : ContentPage
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
