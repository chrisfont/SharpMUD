using System;

using System.Net;
using System.Net.Sockets;

using System.Linq;

namespace SharpMUD
{
	class Server
	{
		private bool        debugging;
		private bool        verbose;
		private TcpListener serv;

		public Server(int port_num = 5000, int back_log = 5, bool debug_en = false, bool verbose = false)
		{
			this.debugging = debug_en;
			this.verbose = verbose;

			//IPAddress ipaddr = IPAddress.Parse("172.29.1.68");
			IPAddress   ipaddr   = null;
			IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

			foreach(IPAddress ip in localIPs)
			{
				if(ip.AddressFamily == AddressFamily.InterNetwork)
				{
					ipaddr = ip;
				}
			}

			if(ipaddr == null) ipaddr = localIPs[0];

			serv = new TcpListener(ipaddr, port_num);

			serv.Start(back_log);

			if(debugging || this.verbose)
				Console.WriteLine("Starting server on host: {0}\tport: {1}...", ipaddr.ToString(), port_num);
		}

		public bool clientReady()
		{
			return serv.Pending();
		}

		public Client clientAccept()
		{
			Client new_client = new Client(serv.AcceptTcpClient());
			Console.WriteLine("New client connected.");
			return new_client;
		}
	}
}

