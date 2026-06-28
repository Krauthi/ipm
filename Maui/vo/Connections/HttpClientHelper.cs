using System;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace iPMCloud.Mobile.vo
{


    public static class HttpClientProvider
    {
        // Eine Client-Instanz pro "Profil" (z.B. Sync vs. Log) – je nachdem, was ihr braucht.
        public static HttpClient CreateClient(TimeSpan pooledConnectionLifetime, TimeSpan timeout)
        {
            var handler = new SocketsHttpHandler
            {
                // Entspricht vom Zweck her am ehesten ConnectionLeaseTimeout:
                // Nach Ablauf wird die Verbindung nicht weiter aus dem Pool wiederverwendet.
                PooledConnectionLifetime = pooledConnectionLifetime,

                // Optional, aber oft sinnvoll:
                // Wie lange darf eine unbenutzte Verbindung im Pool bleiben?
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),

                // Optional: wenn ihr viele parallele Calls pro Host macht
                MaxConnectionsPerServer = 20
            };

            var client = new HttpClient(handler)
            {
                Timeout = timeout
            };

            return client;
        }
    }



    /// <summary>
    /// Zentrale Verwaltung für HttpClient-Instanzen mit SSL-Validierung
    /// </summary>
    public static class HttpClientHelper
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Erstellt einen konfigurierten HttpClient mit SSL-Handler
        /// </summary>
        public static HttpClient CreateHttpClient(TimeSpan? timeout = null)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = ValidateCertificate,
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 5
            };

            var client = new HttpClient(handler)
            {
                Timeout = timeout ?? DefaultTimeout
            };

            return client;
        }

        /// <summary>
        /// Validiert SSL-Zertifikate (WARNUNG: Akzeptiert momentan alle Zertifikate!)
        /// </summary>
        private static bool ValidateCertificate(
            HttpRequestMessage request,
            X509Certificate2 certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            // Produktionscode sollte hier echte Validierung implementieren!
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            // Für Entwicklung: Alle Zertifikate akzeptieren
            // TODO: In Produktion durch echte Validierung ersetzen
            return true;
        }
    }
}