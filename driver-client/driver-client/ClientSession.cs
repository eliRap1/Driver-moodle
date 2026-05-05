using driver_client.driver;
using System;
using System.ServiceModel;

namespace driver_client
{
    public static class ServiceGateway
    {
        public static T Use<T>(Func<Service1Client, T> action)
        {
            var client = new Service1Client();
            try
            {
                T result = action(client);
                Close(client);
                return result;
            }
            catch
            {
                Abort(client);
                throw;
            }
        }

        public static void Use(Action<Service1Client> action)
        {
            Use(client =>
            {
                action(client);
                return true;
            });
        }

        private static void Close(Service1Client client)
        {
            try
            {
                if (client.State != CommunicationState.Faulted)
                    client.Close();
                else
                    client.Abort();
            }
            catch
            {
                client.Abort();
            }
        }

        private static void Abort(Service1Client client)
        {
            try { client.Abort(); } catch { }
        }
    }

    public static class ClientSession
    {
        public static int CurrentUserId => LogIn.sign?.Id ?? -1;

        public static int StudentId
        {
            get
            {
                if (LogIn.sign == null)
                    return -1;

                if (!LogIn.sign.IsTeacher)
                    return LogIn.sign.Id;

                return -1;
            }
        }

        public static int TeacherId
        {
            get
            {
                if (LogIn.sign == null)
                    return -1;

                if (LogIn.sign.IsTeacher)
                    return LogIn.sign.Id;

                return ServiceGateway.Use(client => client.GetTeacherId(LogIn.sign.Id));
            }
        }
    }
}
