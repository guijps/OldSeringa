using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class ConnectToServerSocket : MonoBehaviour
{

	#region private members 	
	private TcpClient socketConnection;
	private Thread clientReceiveThread;
	public ARControl arRef;
	public int port = 9000;

	#endregion
	[Serializable]
	public class TrackingInfoOBJ
	{
		public float timestamp;
		public bool success;
		public float translation_x;
		public float translation_y;
		public float translation_z;
		//public float rx;
		//public float ry;
		//public float rz;
		public float rotation_up_x;
		public float rotation_up_y;
		public float rotation_up_z;
		public float rotation_right_x;
		public float rotation_right_y;
		public float rotation_right_z;
		public float rotation_forward_x;
		public float rotation_forward_y;
		public float rotation_forward_z;
	}

	public TrackingInfoOBJ JsonInfo = new TrackingInfoOBJ();
	public float step = 1f;
	public float speed = 1;
	float oneDegreeRadian = (float)(Math.PI / 180);

	void Start()
	{
		
		JsonInfo.success = false;
		if (GameManager.ligaARtracking)
		{
			ConnectToTcpServer();
			GameManager.manager_instance.conexaoServerSocketAR = this;
			arRef.funcaoRemendo();
		}
	}
	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space) && GameManager.ligaARtracking)
		{
			SendMessage();
		}
	}
	/// <summary> 	
	/// Setup socket connection. 	
	/// </summary> 	
	private void ConnectToTcpServer()
	{
		try
		{
			clientReceiveThread = new Thread(new ThreadStart(ListenForData));
			clientReceiveThread.IsBackground = true;
			clientReceiveThread.Start();

		}
		catch (Exception e)
		{
			Debug.Log("On client connect exception " + e);
		}
	}
	private void FixedUpdate()
	{
		if (JsonInfo.success)
		{
			step = speed * Time.deltaTime;
			//print("Msg recebida:" + " Tx: " + (JsonInfo.translation_x/100) + " ;Ty: " +  (-JsonInfo.translation_y/100) + " ;Tz: " + (-JsonInfo.translation_z/100));
			//transform.localPosition = new Vector3(-JsonInfo.translation_x / 100, -JsonInfo.translation_y / 100, -JsonInfo.translation_z / 100);
			//Vector3 up = new Vector3(JsonInfo.rotation_up_x, JsonInfo.rotation_up_y, JsonInfo.rotation_up_z);
			//Vector3 forward = new Vector3(JsonInfo.rotation_forward_x, JsonInfo.rotation_forward_y, JsonInfo.rotation_forward_z);
			//transform.localRotation = Quaternion.LookRotation(forward, up);

		}
	}
	private void ListenForData()
	{
		//UDP
		UdpClient listener = new UdpClient(port);
		IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, port);

		try
		{
			while (true)
			{
				Console.WriteLine("Esperando mensagem do AR Tracking");
				byte[] bytes = listener.Receive(ref groupEP);

				//print($"Received broadcast from {groupEP} :");
				string msg = Encoding.ASCII.GetString(bytes, 0, bytes.Length);

				JsonInfo = JsonUtility.FromJson<TrackingInfoOBJ>(msg);


				//formatacao do JSON
				//{"timestamp": 1602183586.0879989, "success": 1, "tx": 80.96737370851973, "ty": -57.617939220089404, "tz": 145.4147766817202, "rx": 2.9612484489575275, "ry": 0.05042517689604442, "rz": 0.3664380639308892}
			}
		}
		catch (SocketException e)
		{
			Console.WriteLine(e);
		}
		finally
		{
			listener.Close();
		}


		//TCP
		/*try { 			
			socketConnection = new TcpClient("127.0.0.1", port);  			
			Byte[] bytes = new Byte[1024];             
			while (true) { 				
				// Get a stream object for reading 				
				using (NetworkStream stream = socketConnection.GetStream()) { 					
					int length; 					
					// Read incomming stream into byte arrary. 					
					while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) { 						
						var incommingData = new byte[length]; 						
						Array.Copy(bytes, 0, incommingData, 0, length); 						
						// Convert byte array to string message. 						
						string serverMessage = Encoding.ASCII.GetString(incommingData); 						
						Debug.Log("server message received as: " + serverMessage); 					
					} 				
				} 			
			}         
		}         
		catch (SocketException socketException) {             
			Debug.Log("Socket exception: " + socketException);         
		}   */
	}
	/// <summary> 	
	/// Send message to server using socket connection. 	
	/// </summary> 	
	private void SendMessage()
	{
		if (socketConnection == null)
		{
			return;
		}
		try
		{
			// Get a stream object for writing. 			
			NetworkStream stream = socketConnection.GetStream();
			if (stream.CanWrite)
			{
				string clientMessage = "This is a message from one of your clients.";
				// Convert string message to byte array.                 
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
				// Write byte array to socketConnection stream.                 
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
				Debug.Log("Client sent his message - should be received by server");
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
	}

	private void OnApplicationQuit()
	{
		clientReceiveThread.Abort();

	}
}
