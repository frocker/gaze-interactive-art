using System;
using System.Net.Sockets;
using System.Text;
using Tobii.Interaction;
using Tobii.Interaction.Framework;
using Tobii.Interaction.Model;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            

            // Everything starts with initializing Host, which manages connection to the 
            // Tobii Engine and provides all the Tobii Core SDK functionality.
            // NOTE: Make sure that Tobii.EyeX.exe is running
            var host = new Host();

            //LaunchCalibration
            System.Threading.Thread.Sleep(1000);
            host.Context.LaunchConfigurationTool(ConfigurationTool.RetailCalibration, (data) => { });
            System.Threading.Thread.Sleep(10000);




            //Setup Server
            UdpClient udpClient = new UdpClient();
            udpClient.Connect("127.0.0.1", 33333);

            //Create stream. 
            var gazePointDataStream = host.Streams.CreateGazePointDataStream();


            //LaunchCalibration
            
       
            // Get the gaze data
            gazePointDataStream.GazePoint((x, y, ts) => SendInput(udpClient,x, y, ts));


            // okay, it is 4 lines, but you won't be able to see much without this one :)
            Console.ReadKey();

            // we will close the coonection to the Tobii Engine before exit.
            host.DisableConnection();

        }

        static void SendInput(UdpClient client, double x, double y, double ts)
        {
            String sendString = @"{""id"":""gaze_data"", ""x"":" + x + @", ""y"": " + y + @", ""timestamp"":" + ts + @"}";
            Console.WriteLine(sendString);
            Byte[] senddata = Encoding.ASCII.GetBytes(sendString);
            client.Send(senddata, senddata.Length);
            Console.WriteLine();
        }
    }
}
