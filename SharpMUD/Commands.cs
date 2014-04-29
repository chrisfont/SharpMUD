using System;
using System.IO;

using System.Collections;
using System.Collections.Generic;

using System.Linq;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SharpMUD
{
	public class Commands
	{
		struct CmdAccess {
			public Action<WorldState,Client,Message> func;
			public Profile.ALevels reqAccess;
		}

		private String pfile_base = "../../players/";
		private Dictionary<String,  CmdAccess> CmdTable;

		public Commands()
		{
			CmdTable = new Dictionary<String, CmdAccess>()
			{
				{"quit",     new CmdAccess{func = new Action<WorldState,Client,Message>(CmdQuit),     reqAccess = Profile.ALevels.Mortal}},
				{"who",      new CmdAccess{func = new Action<WorldState,Client,Message>(CmdWho),      reqAccess = Profile.ALevels.Mortal}},
				{"say",      new CmdAccess{func = new Action<WorldState,Client,Message>(CmdSend),     reqAccess = Profile.ALevels.Mortal}},
				{"ooc",      new CmdAccess{func = new Action<WorldState,Client,Message>(CmdSendOOC),  reqAccess = Profile.ALevels.Mortal}},
				{"tell",     new CmdAccess{func = new Action<WorldState,Client,Message>(CmdSendPriv), reqAccess = Profile.ALevels.Mortal}},
				{"shutdown", new CmdAccess{func = new Action<WorldState,Client,Message>(CmdShutdown), reqAccess = Profile.ALevels.Admin}}
			};
		}

		private Profile FindProfile(String username)
		{
			Profile outProfile = new Profile();

			String cwd = System.Environment.CurrentDirectory;

			String player_dir = String.Format("{0}/{1}", cwd, pfile_base);

			String filename = String.Format("{0}.json", username);
			String pfile = String.Format("{0}{1}/{2}", player_dir, username.ToCharArray()[0], filename);

			if(File.Exists(pfile))
				JsonConvert.PopulateObject(File.ReadAllText(pfile), outProfile);
			else
				outProfile.Name = username;

			System.Environment.CurrentDirectory = cwd;

			return outProfile;
		}

		public void CmdHandle(WorldState world, Client client, Message input)
		{
			try
			{
				if(CmdTable[input.cmd].reqAccess <= client.UserProfile.AccessLevel)
				{
					Console.WriteLine("Found command: {0}", input.cmd);
					CmdTable[input.cmd].func(world, client, input);
				}
			}
			catch(KeyNotFoundException)
			{
				return;
			}
		}

		public void CmdWho(WorldState world, Client client, Message input)
		{
			SortedList wholist = new SortedList();

			foreach(Client lclient in world.clientList)
				if(lclient.State == Client.CStates.playing)
					wholist.Add(lclient.UserProfile.Name, lclient.UserProfile.AccessLevel);

			client.SendLine(String.Format("---------// {0} Users Online //---------", wholist.Count));

			for(int i = 0; i < wholist.Count; i++)
				client.SendLine(String.Format("  {0} \t|\t{1}", wholist.GetByIndex(i), wholist.GetKey(i)));
		}

		public void CmdQuit(WorldState world, Client client, Message input)
		{
			client.SendLine("Closing socket. Goodbye.");

			foreach(Client out_client in world.clientList)
				if(!out_client.Equals(client)) out_client.SendLine(String.Format("{0} has quit.", client.profile.Name));

			client.State = Client.CStates.closing;
		}

		public void CmdSend(WorldState world, Client client, Message input)
		{
			client.Send(String.Format("You say, \"{0}\"{1}", input.msg, Environment.NewLine));

			foreach(Client out_client in world.clientList)
				if(!out_client.Equals(client)) out_client.SendLine(String.Format("{0} says, \"{1}\"", client.profile.Name, input.msg));
		}

		public void CmdSendOOC(WorldState world, Client client, Message input)
		{
			client.SendLine(String.Format("OOC You: {0}", input.msg));

			foreach(Client out_client in world.clientList)
				if(!out_client.Equals(client)) out_client.SendLine(String.Format("OOC {0}: {1}", client.UserProfile.Name, input.msg));
		}

		public void CmdSendPriv(WorldState world, Client client, Message input)
		{
			String[] msg_split = input.msg.Split(' ');

			if(msg_split.Length < 1)
			{
				client.SendLine("Tell who what?");
			}
			else if(msg_split.Length == 1)
			{
				client.SendLine(String.Format("Tell {0} what?", msg_split[0]));
			}
			else if(msg_split[0].ToLower() == client.UserProfile.Name.ToLower())
			{
				client.SendLine("Stop talking to yourself.");
			}
			else
			{
				Client target = null;

				String username = msg_split[0];
				String msg      = String.Join(" ", msg_split.Skip(1));

				foreach(Client cl_target in world.clientList)
				{
					if(cl_target.UserProfile.Name.ToLower() == username.ToLower())
					{
						target = cl_target;
						break;
					}
				}

				if(target != null)
				{
					client.SendLine(String.Format("You tell {0}, \"{1}\"", username, msg));
					target.SendLine(String.Format("{0} tells you, \"{1}\"", client.UserProfile.Name, msg));
				}
				else
				{
					client.SendLine(String.Format("{0} is not connected.", username));
				}
			}
		}

		public void CmdShutdown(WorldState world, Client client, Message input)
		{
			foreach(Client out_client in world.clientList)
			{
				out_client.SendLine("Server is closing.");
				out_client.State = Client.CStates.closing;
			}
		}

		public void CmdLogin(WorldState world, Client client, Message input)
		{
			client.UserProfile = FindProfile(input.cmd);

			if(client.UserProfile.AccessLevel == Profile.ALevels.New)
			{
				client.SendLine("New account detected.");
				client.Send("Enter new password: ");
				client.State = Client.CStates.login_newpass;
			}
			else
			{
				client.SendLine(String.Format("Welcome back, {0}!", client.UserProfile.Name));
				client.Send("Enter password: ");
				client.State = Client.CStates.login_pass;
			}

			client.UserProfile.Try = 0;
		}

		public void CmdNewPass(WorldState world, Client client, Message input)
		{
			if(client.State == Client.CStates.login_newpass)
			{
				client.UserProfile.Pass = input.cmd;
				client.SendLine("Please verify your new password: ");
				client.State = Client.CStates.login_conpass;
			}
			else if(client.UserProfile.ChkPass(input.cmd))
			{
				client.SendLine("Welcome to the server!");
				client.UserProfile.AccessLevel = Profile.ALevels.Mortal;
				client.UserProfile.Save();

				client.State = Client.CStates.playing;
			}
			else
			{
				client.SendLine("Passwords do not match.");
				client.SendLine("Please enter your new password: ");
				client.State = Client.CStates.login_newpass;
			}
		}

		public void CmdPass(WorldState world, Client client, Message input)
		{
			if(client.UserProfile.ChkPass(input.cmd))
			{
				client.SendLine(String.Format("{0} has finished signing in.", client.UserProfile.Name));
				client.State = Client.CStates.playing;
			}
			else if(client.UserProfile.Try.Equals(3))
			{
				client.SendLine("Failed to login. Please see an admin.");
				client.State = Client.CStates.closing;
			}
			else
			{
				client.SendLine("Bad password. Please try again: ");
				client.UserProfile.Try += 1;
			}
		}
	}
}
