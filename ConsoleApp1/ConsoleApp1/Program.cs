using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using Tobii.Interaction;
using Tobii.Interaction.Framework;
using Tobii.Interaction.Model;




namespace ConsoleApp1
{
    class Program
    {

        [DllImport("User32.dll")]
        public static extern System.IntPtr FindWindowEx(System.IntPtr parent, System.IntPtr childe, string strclass, string strname);
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern int ShowWindow(IntPtr hwnd, int nCmdShow);
        [DllImport("Kernel32.dll")]
        public static extern System.IntPtr GetConsoleWindow();

        static void Main(string[] args)
        {

            IntPtr p = GetConsoleWindow();
            ShowWindow(p, 3); //0,1,2,3 - Close, normal, min, max

            Console.WriteLine("*******************************************************************\n");
            Console.WriteLine("                       Gaze Interactive Art                        \n");
            Console.WriteLine("*******************************************************************\n\n");
            Console.WriteLine("Press any key to begin");
            Console.ReadKey();
            

            try
            {
                var env = System.Environment.CurrentDirectory;
                env = Path.GetFullPath(Path.Combine(env, @"..\"));
           
            // Everything starts with initializing Host, which manages connection to the 
            // Tobii Engine and provides all the Tobii Core SDK functionality.
            // NOTE: Make sure that Tobii.EyeX.exe is running
            var host = new Host();

            //LaunchCalibration
            System.Threading.Thread.Sleep(1000);
            host.Context.LaunchConfigurationTool(ConfigurationTool.RetailCalibration, (data) => { });
            System.Threading.Thread.Sleep(10000);
            Console.WriteLine("Calibration Complete, minimising window\n");
            //Minimise our current window
            ShowWindow(p, 2);
            Console.WriteLine("Minimised, opening Electron:\n");
            Console.WriteLine(env);
            //Open Electron.exe
            Process.Start(Path.Combine(env, @"electron\electron.exe"));

            //int id = process.Id;
            //Process tempProc = Process.GetProcessById(id);
            Console.WriteLine("Electron Opened, Setting up server\n");
            //Setup Server
            UdpClient udpClient = new UdpClient();
            udpClient.Connect("127.0.0.1", 33333);
            Console.WriteLine("Server setup, initiating gaze data flow\n");
            //Create stream. 
            var gazePointDataStream = host.Streams.CreateGazePointDataStream();
 
            // Get the gaze data
            gazePointDataStream.GazePoint((x, y, ts) => SendInput(udpClient,x, y, ts));


            // okay, it is 4 lines, but you won't be able to see much without this one :)
            Console.ReadKey();

            // we will close the coonection to the Tobii Engine before exit.
            host.DisableConnection();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: "+e+"\n");
                Console.ReadKey();
            }
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
