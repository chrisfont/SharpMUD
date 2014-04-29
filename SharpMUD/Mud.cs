using System;
using System.IO;

using System.Linq;

using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SharpMUD
{
	public class Message
	{
		public String cmd;
		public String msg;

		public Message(String input)
		{
			String[] in_split = input.Split(' ');

			this.cmd = (input.Length > 0) ? in_split[0].ToLower()              : String.Empty;
			this.msg = (input.Length > 1) ? String.Join(" ", in_split.Skip(1)) : String.Empty;
		}
	}

	class MainClass
	{
		public static void Main(string[] args)
		{
			Commands   Command   = new Commands();
			WorldState world     = new WorldState();

			while(world.Running)
			{
				// Try to get new members
				world.Accept();

				foreach(Client client in world.clientList)
				{
					if(client.IsOpen() && client.InAvail())
					{
						Message input = new Message(client.Recv());

						switch(client.State)
						{
						case Client.CStates.login_name:
							Command.CmdLogin(world, client, input);
							break;
						case Client.CStates.login_newpass:
						case Client.CStates.login_conpass:
							Command.CmdNewPass(world, client, input);
							break;
						case Client.CStates.login_pass:
							Command.CmdPass(world, client, input);
							break;
						case Client.CStates.playing:
							Console.WriteLine("CMD: {0}, MSG: {1}", input.cmd, input.msg);
							Command.CmdHandle(world, client, input);
							break;
						}

					}
					else if(client.State == Client.CStates.closing)
					{
						world.removeList.Add(client);
					}
					else if(!client.IsOpen())
					{
						client.State = Client.CStates.closing;
					}
				}

				world.Remove();
			}
		}
	}
}
