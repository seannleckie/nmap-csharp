using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;


namespace nmapcsharp
{
	internal static class MainClass
	{
		private static CountdownEvent _countdown;
		private static int _upCount;
		private static readonly object LockObj = new object();
        private const bool ResolveNames = true;

        static void Main(string[] args)
        {
			Functions.InitColors();
			Functions.Log ("Starting nmap-csharp ( forked by seannleckie from github.com/Simran/nmap-csharp )", 4);
			
			CheckArgs(args);
        }

        private static void CheckArgs(IList<string> args)
        {
	        StartScan(args[0] == "-local"
		        ? Regex.Match(DefGateway(), "(\\d+.\\d+.\\d+.)").Groups[0].Value
		        : Regex.Match(args[0], "(\\d+.\\d+.\\d+.)").Groups[0].Value);
        }

        private static void StartScan(string ipBase)
		{
				_countdown = new CountdownEvent(1);
	            var sw = new Stopwatch();
	            sw.Start();
	            for (var i = 1; i < 255; i++)
	            {
	                var ip = ipBase + i.ToString();
					new Thread(delegate()
					{       
					try
					{
						var p = new Ping();
		                p.PingCompleted += PingDone;
		                _countdown.AddCount();
		                p.SendAsync(ip, 100, ip);
					}
					catch (SocketException)
					{
						Functions.Log ($"Could not contact {ip}", 3);
					}
					}).Start();
	            }
	            _countdown.Signal();
	            _countdown.Wait();
	            sw.Stop();
	            //TimeSpan span = new TimeSpan(sw.ElapsedTicks);
	            Functions.Log($"Took {sw.ElapsedMilliseconds} milliseconds. {_upCount} hosts active.", 1);
			}

        private static void PingDone(object sender, PingCompletedEventArgs e)
        {
            var ip = (string)e.UserState;
            if (e.Reply != null && e.Reply.Status == IPStatus.Success)
            {
                if (ResolveNames)
                {
                    string name;
                    try
                    {
                        var hostEntry = Dns.GetHostEntry(ip);
                        name = hostEntry.HostName;
                    }
                    catch (SocketException)
                    {
                        name = "?";
                    }
                    Functions.Log($"{ip} ({name}) is up: ({e.Reply.RoundtripTime} ms)", 2);
                }
                else
                { //but it's reachable doe.
                    Functions.Log($"{ip} is up: ({e.Reply.RoundtripTime} ms)", 2);
                }
                lock(LockObj)
                {
                    _upCount++;
                }
            }
            else if (e.Reply == null)
            {
                Functions.Log($"Pinging {ip} failed. (Null Reply object?)", 3);
            }
            _countdown.Signal();
        }

        private static string DefGateway()
		{
			string ip = null;
			foreach (var f in NetworkInterface.GetAllNetworkInterfaces())
				if (f.OperationalStatus == OperationalStatus.Up)
				{
					foreach (GatewayIPAddressInformation d in f.GetIPProperties().GatewayAddresses)
					{
						ip = d.Address.ToString();
					}
				}
			Functions.Log ($"Network Gateway: {ip}", 5);
			return ip;
		}
    }
}
