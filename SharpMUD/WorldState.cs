using System;

using System.Collections.Generic;

using System.Net;
using System.Net.Sockets;

namespace SharpMUD
{
	public class WorldState
	{
		private bool          is_evil;

		private bool          debugging;
		private bool          verbose;
		private bool          is_running;
		private Server        serv;

		public List<Client>   clientList;
		public List<Client>   removeList;

		public Help           HelpFiles { get; set; }

		public WorldState(bool debug_en = true, bool verbose_en = true, int port = 5000)
		{
			this.is_evil = true;

			this.debugging = debug_en;
			this.verbose = verbose_en;

			this.is_running = true;

			this.serv = new Server(port, 5, debug_en, verbose_en);

			this.clientList = new List<Client>();
			this.removeList = new List<Client>();
			this.HelpFiles = new Help();
		}

		public bool Running
		{
			get { return this.is_running;  }
			set { this.is_running = value; }
		}

		public void Accept()
		{
			if(this.serv.clientReady())
			{
				Client new_client = this.serv.clientAccept();

				new_client.Send(String.Format("Welcome to the server.{0}What is your name?{0}", Environment.NewLine));

				this.clientList.Add(new_client);
			}
		}

		public void Remove()
		{
			foreach(Client client in removeList)
			{
				if(this.verbose || this.debugging) Console.WriteLine("Client disconnected.");
				client.Close();
				clientList.Remove(client);
			}

			removeList.Clear();
		}

		public bool ISEVIL()
		{
			return this.is_evil;
		}
	}
}

