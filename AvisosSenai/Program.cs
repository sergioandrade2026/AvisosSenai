using nanoFramework.Networking;
using System;
using System.Net.NetworkInformation;
using System.Threading;

namespace AvisosSenai
{
    public class Program
    {
        private static ErrorManager _errorManager;

        public static void Main()
        {
            _errorManager = new ErrorManager(2, 5);

            if (WifiNetworkHelper.ConnectDhcp("Alert", "ssssssss", requiresDateTime: true))
            {
                var ni = NetworkInterface.GetAllNetworkInterfaces()[0];
                Console.WriteLine("Conectado com sucesso!");
                Console.WriteLine("IP atribuído: " + ni.IPv4Address);
                _errorManager.Status = ErrorStatus.None;
                new WebServer().Start();
            }
            else
            {
                _errorManager.Status = ErrorStatus.ConectionError;


            }



            Thread.Sleep(Timeout.Infinite);
        }
    }
}