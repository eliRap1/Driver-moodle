using System.ServiceModel;
using DriverService;

namespace driver_maui.Services
{
    public static class ServiceHelper
    {
        // 10.0.2.2 maps to host's localhost from Android emulator
        private const string ServiceUrl = "http://10.0.2.2:8733/Design_Time_Addresses/WcfServiceLibrary1/Service1/";

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
