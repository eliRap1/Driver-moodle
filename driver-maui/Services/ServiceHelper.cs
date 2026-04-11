using System.ServiceModel;
using DriverService;

namespace driver_maui.Services
{
    public static class ServiceHelper
    {
        // Use the host machine's actual local IP address (run ipconfig to find yours).
        // For Android Emulator (AVD): use 10.0.2.2 instead of the IP below.
        // For a real Android device on the same WiFi: use the machine's WiFi IP (e.g. 192.168.1.136).
        private const string ServiceUrl = "http://192.168.1.136:8733/Design_Time_Addresses/WcfServiceLibrary1/Service1/";

        public static Service1Client GetClient()
        {
            var binding = new BasicHttpBinding
            {
                MaxReceivedMessageSize = 5000000,
                OpenTimeout = TimeSpan.FromSeconds(30),
                SendTimeout = TimeSpan.FromSeconds(30),
                ReceiveTimeout = TimeSpan.FromSeconds(30)
            };
            var endpoint = new EndpointAddress(ServiceUrl);
            return new Service1Client(binding, endpoint);
        }

        public static async Task<T> CallAsync<T>(Func<Service1Client, Task<T>> action)
        {
            var client = GetClient();
            try
            {
                return await action(client);
            }
            finally
            {
                try { await client.CloseAsync(); } catch { client.Abort(); }
            }
        }

        public static async Task CallAsync(Func<Service1Client, Task> action)
        {
            var client = GetClient();
            try
            {
                await action(client);
            }
            finally
            {
                try { await client.CloseAsync(); } catch { client.Abort(); }
            }
        }
    }
}
