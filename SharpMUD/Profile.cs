using System;
using System.IO;
using System.Collections.Generic;

using System.Text;

using System.Security.Cryptography;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace SharpMUD
{
	public class Profile
	{
		private String pfile_base = "../../players/";
		public enum PStates {Login_Name, Login_Pass, Login_Pass_Verify};
		public enum ALevels {New, Mortal, Builder, Admin, Coder, Owner};

		public String  Name        { get; set; }
		public String  Pass        { get; set; }
		public PStates State       { get; set; }
		public ALevels AccessLevel { get; set; }

		public long    Hp          { get; set; }
		public long    Mp          { get; set; }
		public long    Xp          { get; set; }
		public long    Gp          { get; set; }

		private sbyte   tries;
				
		public Profile(String name = "", String pass = "", PStates state = PStates.Login_Name, ALevels access = ALevels.New)
		{
			this.Name        = name;
			this.Pass        = pass;
			this.State       = state;
			this.AccessLevel = access;

			this.Hp          = 0;
			this.Mp          = 0;
			this.Xp          = 0;
			this.Gp          = 0;

			this.tries       = 0;
		}

		private String Hash(MD5 md5Hash, String toHash)
		{
			byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(toHash));

			return BitConverter.ToString(data);
		}

		public sbyte Try
		{
			get { return this.tries;  }
			set { this.tries = value; }
		}

		public bool ChkPass(String pass)
		{
			using(MD5 md5Hash = MD5.Create())
			{
				String hash = Hash(md5Hash, pass);

				return this.Pass.Equals(hash);
			}
		}

		public void SetHPass(String pass)
		{
			using(MD5 md5Hash = MD5.Create())
			{
				this.Pass = Hash(md5Hash, pass);
			}
		}

		public void Save()
		{
			// Sentry in case a profile is not set to Mortal before saving.
			if(this.AccessLevel < ALevels.Mortal) this.AccessLevel = ALevels.Mortal;

			String cwd = System.Environment.CurrentDirectory;
			String pfile = String.Format("{0}/{1}{2}/{3}.json", cwd, pfile_base, this.Name.ToCharArray()[0], this.Name);

			String json = JsonConvert.SerializeObject(this, Formatting.Indented);
			File.WriteAllText(pfile, json);
		}

		public String GetHUD()
		{
			return String.Format("Name: {0} HP: {1} MP: {2} XP: {3} GP: {4}", this.Name, this.Hp, this.Mp, this.Xp, this.Gp);
		}
	}
}



