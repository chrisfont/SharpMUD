using System;
using System.IO;

using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SharpMUD
{
	public class Client
	{
		public enum CStates {login_name, login_newpass, login_conpass, login_pass, playing, closing}

		private TcpClient     handle;
		private NetworkStream stream;
		private CStates       state;
		public  Profile       UserProfile { get; set; }

		public Client(TcpClient handle)
		{
			this.state = CStates.login_name;
			this.handle = handle;

			// Save some time because we are going to be mostly using streams for interaction
			this.stream = handle.GetStream();

			this.UserProfile = new Profile();
		}

		public bool InAvail()
		{
			// Can we read?
			return stream.CanRead && stream.DataAvailable;
		}

		public bool OutAvail()
		{
			// Can we output?
			return stream.CanWrite;
		}

		public bool IsOpen()
		{
			// This means either a voluntary quit or after a brief timeout (two message unable to be sent)
			return handle.Connected && (this.state != CStates.closing);
		}

		public String Recv()
		{
			// We only want to try receiving if data is actually there
			// due to the fact that receiving is a blocking action.
			if(this.InAvail())
			{
				try
				{
					StreamReader sr = new StreamReader(stream);

					return (sr.Peek() > 0) ? sr.ReadLine() : String.Empty;
				}
				catch (IOException)
				{
					this.state = CStates.closing;
					return String.Empty;
				}
				catch(Exception e)
				{
					Console.WriteLine("ERROR: {0}...Flushing Client Stream.", e.ToString());
					stream.Flush();

					// Failed, return nothing
					return String.Empty;
				}
			}
			else
			{
				return String.Empty;
			}
		}

		public bool Send(String outbound)
		{
			// Check to make sure our outbuffer isn't full
			if(!this.OutAvail()) return false;

			try
			{
				stream.Write(Encoding.ASCII.GetBytes(outbound), 0, outbound.Length);
				return true;
			}
			catch(IOException)
			{
				this.state = CStates.closing;
				return false;
			}
			catch(Exception e)
			{
				Console.WriteLine("ERROR: {0}...Flushing Client Stream.", e.ToString());
				stream.Flush();

				return false;
			}
		}

		public void SendLine(String outbound)
		{
			this.Send(String.Format("{0}{1}", outbound, Environment.NewLine));
		}

		public CStates State
		{
			get { return this.state; }
			set { this.state = value; }
		}

		public bool Close()
		{
			this.stream.Close();
			this.handle.Close();

			return true;
		}
	}
}

