using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace CascadiaMipSdkLib
{
    public static class ChannelSettings
    {
        public static int MaxBufferPoolSize { get; set; } = 2147483647;
        public static int MaxBufferSize { get; set; } = 2147483647;
        public static int MaxReceivedMessageSize { get; set; } = 2147483647;
        public static int MaxStringContentLength { get; set; } = 2147483647;

        public static RemoteCertificateValidationCallback RemoteCertificateValidationCallback { get; set; } = ValidateAllCerts;

        public static bool ValidateAllCerts(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors errors) => true;

        public static class Timeouts
        {
            public static TimeSpan AllTimeouts
            {
                set
                {
                    OpenTimeout = value;
                    CloseTimeout = value;
                    ReceiveTimeout = value;
                    SendTimeout = value;
                }
            }

            public static TimeSpan OpenTimeout { get; set; } = TimeSpan.FromMinutes(10);
            public static TimeSpan CloseTimeout { get; set; } = TimeSpan.FromMinutes(10);
            public static TimeSpan ReceiveTimeout { get; set; } = TimeSpan.FromMinutes(10);
            public static TimeSpan SendTimeout { get; set; } = TimeSpan.FromMinutes(10);
        }
    }
}