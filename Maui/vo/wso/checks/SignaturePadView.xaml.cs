namespace iPMCloud.Mobile;

public partial class SignaturePage : ContentPage
{
    public SignaturePage()
    {
        InitializeComponent();
    }

    private void OnClearClicked(object sender, EventArgs e)
    {
        signaturePad.Clear();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (signaturePad.IsBlank)
        {
            await DisplayAlertAsync("Fehler", "Bitte unterschreiben Sie zuerst", "OK");
            return;
        }

        // Signatur als Byte-Array holen
        var imageBytes = await signaturePad.GetImageStreamAsync();

        // Base64 String holen
        var base64 = signaturePad.SignatureImage;

        // Speichern oder weiterverarbeiten
        await DisplayAlertAsync("Erfolg", "Unterschrift gespeichert", "OK");

        // Optional: Zur vorherigen Seite zurück
        await Navigation.PopAsync();
    }
}