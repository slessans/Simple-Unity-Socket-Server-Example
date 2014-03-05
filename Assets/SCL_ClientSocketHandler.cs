using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Threading;

public interface SCL_IClientSocketHandlerDelegate
{
	// callback, will be NOT be invoked on main thread, so make sure to synchronize in Unity
	void ClientSocketHandlerDidReadMessage(SCL_ClientSocketHandler handler, string message);
}

public class SCL_ClientSocketHandler {

	protected readonly TcpClient client;
	protected readonly string separatorSequence;
	protected readonly char [] separatorSequenceChars;
	protected readonly Encoding encoding;
	protected SCL_IClientSocketHandlerDelegate clientSocketHandlerDelegate;


	public SCL_ClientSocketHandler (TcpClient client, 
	                                SCL_IClientSocketHandlerDelegate clientSocketHandlerDelegate, 
	                                string separatorString,
	                                Encoding encoding)
	{
		this.client = client;
		this.encoding = encoding;
		this.separatorSequence = separatorString;
		this.separatorSequenceChars = separatorString.ToCharArray();
		this.clientSocketHandlerDelegate = clientSocketHandlerDelegate;
	}	

	public void Cleanup()
	{
		if (this.client != null) {
			Debug.Log ("Socket client closed (cleanup) ");
			this.client.Close();
		}
	}

	public void Run()
	{
		try {
			StringBuilder dataBuffer = new StringBuilder();

			while (true)
			{
				byte [] read_buffer = new byte[256];
				int read_length = this.client.Client.Receive(read_buffer);
				
				// append to token
				if (read_length > 0) 
				{
					dataBuffer.Append(this.encoding.GetString(read_buffer, 0, read_length));
					string data = dataBuffer.ToString();
					dataBuffer = new StringBuilder();

					string [] tokens = data.Split(this.separatorSequenceChars);

					bool ends_with_separator = data.EndsWith(this.separatorSequence);

					// all except the last piece
					for(int i = 0; i < (tokens.Length - 1); i++)
					{
						this.clientSocketHandlerDelegate.ClientSocketHandlerDidReadMessage(this, tokens[i]);
					}
					
					// for the last piece, if the data ended with separator than this is a full token
					// otherwise, its not so append to data buffer
					if (ends_with_separator)
					{
						if (tokens[tokens.Length - 1].Length > 0)
						{
							this.clientSocketHandlerDelegate.ClientSocketHandlerDidReadMessage(this, tokens[tokens.Length - 1]);
						}
					}
					else
					{
						dataBuffer.Append(tokens[tokens.Length - 1]);
					}
				}
			}


		} 
		catch (ThreadAbortException exception) 
		{
			Debug.Log ("Thread aborted");
		} 
		catch (SocketException exception) 
		{
			Debug.Log ("Socket exception");
		} 
		finally 
		{
			this.client.Close();
			Debug.Log ("Socket client closed " + this.client.Client.RemoteEndPoint);
		}
	}

}
