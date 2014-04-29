using System;
using System.IO;

using System.Linq;

using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace SharpMUD
{
	public class Help
	{
		private String help_base = "/../../help/";

		private Dictionary<String, String> HelpFiles;

		public Help()
		{
			String hFile_Dir = Environment.CurrentDirectory + help_base;

			String[] help_files = Directory.GetFiles(hFile_Dir, "*.json");

			this.HelpFiles = new Dictionary<String, String>();

			Console.WriteLine("Loading helpfiles...");

			foreach(string path in help_files)
			{
				HelpFile curr_file = new HelpFile();

				JsonConvert.PopulateObject(path, curr_file);

				this.HelpFiles.Add(curr_file.Title, curr_file.Title);

				foreach(string synonym in curr_file.Synonyms)
					this.HelpFiles.Add(synonym, curr_file.Title);
			}
		}

		public void GetHelp(Client client, String help)
		{
			HelpFile got_help = new HelpFile();

			// No arguments, we should print all help available
			if(help.Equals(String.Empty))
			{
				client.Send(Environment.NewLine);
				client.SendLine("Help Files: ");
				client.SendLine("--------------------------------------------------------");

				foreach(KeyValuePair<String, String> entry in this.HelpFiles)
					client.SendLine(entry.Key);

				client.Send(Environment.NewLine);

				return;
			}
			
			String hFile = Environment.CurrentDirectory + help_base + this.HelpFiles[help] + ".json";

			if(File.Exists(hFile))
			{
				JsonConvert.PopulateObject(hFile, got_help);
				client.SendLine("");
				client.SendLine(got_help.Title);
				client.SendLine("----------------------------------------------------------------------------------");
				if(got_help.Synonyms.Length > 0)
				{
					client.Send("Synonyms: ");

					foreach(String synonym in got_help.Synonyms)
						client.Send(synonym + ", ");

					client.Send(Environment.NewLine);
				}
				client.SendLine("----------------------------------------------------------------------------------");
				client.SendLine(got_help.Body);
				if(got_help.SeeAlso.Length > 0)
				{
					client.Send("See also: ");
					foreach(String also in got_help.SeeAlso)
						client.Send(also + ", ");
					client.Send(Environment.NewLine);
				}
			}
			else
			{
				client.SendLine(String.Format("Help {0} not found.", help));
			}
		}

		public void NewHelp(Client client, String new_help)
		{

		}
	}

	public class HelpFile
	{

		public String   Title    { get; set; }
		public String   Body     { get; set; }
		public String[] Synonyms { get; set; }
		public String[] SeeAlso  { get; set; }

		public HelpFile()
		{
		}
	}
}

