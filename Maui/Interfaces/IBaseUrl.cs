namespace iPMCloud.Mobile.Interfaces
{
    /// <summary>
    /// Interface für plattformspezifische BaseUrl-Implementierung
    /// (benötigt für iOS WebView-Rendering)
    /// </summary>
    public interface IBaseUrl
    {
        string Get();
    }
}
