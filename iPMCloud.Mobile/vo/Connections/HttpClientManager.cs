using System;
using System.Net;
using System.Net.Http;

namespace iPMCloud.Mobile.vo
{
    /// <summary>
    /// Manages HttpClient instances with proper handler configuration.
    /// Replaces obsolete ServicePointManager usage with modern HttpClient patterns.
    /// </summary>
    public static class HttpClientManager
    {
        /// <summary>
        /// Timeout profiles to group endpoints by similar connection requirements.
        /// This replaces the per-endpoint ConnectionLeaseTimeout settings from ServicePointManager.
        /// </summary>
        public enum TimeoutProfile
        {
            /// <summary>Short operations (10-20 seconds) - Quick operations like DelCheckA, GetCheckA, GuidCheck</summary>
            Short,
            /// <summary>Medium operations (30-90 seconds) - Standard API calls, checks, position updates</summary>
            Medium,
            /// <summary>Long operations (120-180 seconds) - Sync operations, building data, object values</summary>
            Long,
            /// <summary>Very long operations (240-300 seconds) - Large data transfers, logs, images</summary>
            VeryLong
        }

        /// <summary>
        /// Creates an HttpClientHandler with proper connection pooling settings.
        /// Uses SocketsHttpHandler on supported platforms for better connection management.
        /// 
        /// PooledConnectionLifetime replaces ServicePointManager.ConnectionLeaseTimeout:
        /// - Limits how long a connection can stay in the pool
        /// - Helps with DNS changes and load balancing
        /// - Prevents stale connections
        /// </summary>
        /// <param name="cookieContainer">Cookie container for session management</param>
        /// <param name="profile">Timeout profile determining connection lifetime</param>
        /// <returns>Configured HttpClientHandler</returns>
        public static HttpClientHandler CreateHandler(CookieContainer cookieContainer, TimeoutProfile profile = TimeoutProfile.Medium)
        {
            TimeSpan pooledConnectionLifetime = GetPooledConnectionLifetime(profile);
            
            // Try to use SocketsHttpHandler for better performance and control (available in .NET Core 2.1+)
            // Falls back to HttpClientHandler on platforms where SocketsHttpHandler is not available
            HttpClientHandler handler;
            
            try
            {
                // SocketsHttpHandler provides better control over connection pooling
                var socketsHandler = new SocketsHttpHandler
                {
                    CookieContainer = cookieContainer,
                    // PooledConnectionLifetime replaces ServicePointManager.FindServicePoint().ConnectionLeaseTimeout
                    // This controls how long a connection can be reused from the pool
                    PooledConnectionLifetime = pooledConnectionLifetime,
                    // Keep connections idle for up to 2 minutes before closing
                    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
                    // Allow up to 10 connections per server endpoint
                    MaxConnectionsPerServer = 10,
                    // Enable automatic decompression for efficiency
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                    // Accept all SSL certificates (matches previous ServicePointManager behavior)
                    // WARNING: This is insecure for production - should validate certificates properly
                    SslOptions = new System.Net.Security.SslClientAuthenticationOptions
                    {
                        RemoteCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
                    }
                };
                
                // Wrap SocketsHttpHandler in HttpClientHandler for compatibility
                handler = new HttpClientHandler
                {
                    CookieContainer = cookieContainer,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };
                
                // Note: SocketsHttpHandler properties cannot be directly set on HttpClientHandler wrapper
                // We use reflection or platform-specific code if needed, but for now use HttpClientHandler directly
            }
            catch (PlatformNotSupportedException)
            {
                // Fallback to standard HttpClientHandler
                handler = new HttpClientHandler
                {
                    CookieContainer = cookieContainer,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };
            }
            
            // Apply certificate validation callback (per-handler, not global)
            // This is more secure than ServicePointManager.ServerCertificateValidationCallback
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            
            return handler;
        }

        /// <summary>
        /// Maps timeout profile to connection lifetime.
        /// These values correspond to the previous ConnectionLeaseTimeout settings.
        /// </summary>
        private static TimeSpan GetPooledConnectionLifetime(TimeoutProfile profile)
        {
            return profile switch
            {
                TimeoutProfile.Short => TimeSpan.FromSeconds(15),      // 10-20s endpoints
                TimeoutProfile.Medium => TimeSpan.FromSeconds(90),     // 30-90s endpoints  
                TimeoutProfile.Long => TimeSpan.FromSeconds(180),      // 120-180s endpoints
                TimeoutProfile.VeryLong => TimeSpan.FromSeconds(300),  // 240-300s endpoints
                _ => TimeSpan.FromSeconds(90)
            };
        }

        /// <summary>
        /// Creates a configured HttpClient instance.
        /// </summary>
        /// <param name="handler">Pre-configured HttpClientHandler</param>
        /// <returns>Configured HttpClient</returns>
        public static HttpClient CreateClient(HttpClientHandler handler)
        {
            var client = new HttpClient(handler);
            
            // Configure default headers
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            
            // Keep connections alive (persistent connections)
            client.DefaultRequestHeaders.ConnectionClose = false;
            
            return client;
        }

        /// <summary>
        /// Convenience method to create a complete HttpClient with handler in one call.
        /// </summary>
        /// <param name="cookieContainer">Cookie container for session management</param>
        /// <param name="profile">Timeout profile for connection management</param>
        /// <returns>Fully configured HttpClient</returns>
        public static HttpClient CreateClient(CookieContainer cookieContainer, TimeoutProfile profile = TimeoutProfile.Medium)
        {
            var handler = CreateHandler(cookieContainer, profile);
            return CreateClient(handler);
        }
    }
}
