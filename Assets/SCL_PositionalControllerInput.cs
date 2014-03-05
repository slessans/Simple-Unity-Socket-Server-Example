using UnityEngine;
using System;
using System.Threading;
using System.Globalization;
using System.Text;

public class SCL_PositionalControllerInput : MonoBehaviour, SCL_IClientSocketHandlerDelegate {

	public GameObject controlledObject;

	private readonly object positionLock = new object();
	private Vector3 position = new Vector3();

	private SCL_SocketServer socketServer;

	// Use this for initialization
	void Start () {
		Debug.Log("Hello From Me");

		SCL_IClientSocketHandlerDelegate clientSocketHandlerDelegate = this;
		int maxClients = 1;
		string separatorString = "&";
		int portNumber = 13000;
		Encoding encoding = Encoding.UTF8;

		this.socketServer = new SCL_SocketServer(
			clientSocketHandlerDelegate, maxClients, separatorString, portNumber, encoding);

		this.socketServer.StartListeningForConnections();

		Debug.Log (String.Format (
			"Started socket server at {0} on port {1}", 
			this.socketServer.LocalEndPoint.Address, this.socketServer.PortNumber));
	}

	void OnApplicationQuit() {
		Debug.Log ("Cleaning up socket server");
		this.socketServer.Cleanup();
		this.socketServer = null;
	}

	void FixedUpdate() {
		this.controlledObject.transform.position = this.GetPositionValue();
	}

	// this delegate method will be called on another thread, so use locks for synchronization
	public void ClientSocketHandlerDidReadMessage(SCL_ClientSocketHandler handler, string message)
	{
		string [] pieces = message.Split(',');
		if (pieces.Length == 3) {
			try {
				float x = float.Parse(pieces[0], CultureInfo.InvariantCulture);
				float y = float.Parse(pieces[1], CultureInfo.InvariantCulture);
				float z = float.Parse(pieces[2], CultureInfo.InvariantCulture);
				
				Debug.Log (String.Format("x: {0}, y: {1}, z: {2}", x, y, z));

				this.SetPositionValue(x,y,z);
			} catch(FormatException) {
				Debug.Log ("Invalid number format: " + message);
			}
		} else {
			Debug.Log ("Length is not three: " + message);
		}
	}
	
	protected Vector3 GetPositionValue() {
		Vector3 vec;
		lock(this.positionLock) {
			vec = new Vector3(this.position.x, this.position.y, this.position.z);
		}
		return vec;
	}

	protected void SetPositionValue(float x, float y, float z)
	{
		lock(this.positionLock) {
			this.position = new Vector3(x,y,z);
		}
	}

}
