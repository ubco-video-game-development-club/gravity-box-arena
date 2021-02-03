using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Text;

public class LowLevelClient
{
	public enum RequestHeader
	{
		REQUEST_AUTH = 1,
		REQUEST_JOIN_OR_CREATE_LOBBY = 2,
		SYNC_DATA = 3
	}

	public struct Message
	{
		public RequestHeader header;
		public byte[] data;
	}

	public bool Connected { get { return connected; } }

#if UNITY_STANDALONE
	private ClientWebSocket client;
#else
	private WebSocketClientJS jsClient;
#endif

	private Queue<Message> messageQueue;
	private string hostname;
	private bool connected;
	private string key;

	public LowLevelClient(string hostname) 
	{
		this.hostname = hostname;

	#if UNITY_STANDALONE
		client = new ClientWebSocket();
	#else
		jsClient = new WebSocketClientJS();
		UnityEngine.Debug.Log("Hello world!");
	#endif

		messageQueue = new Queue<Message>();

	}

#if !UNITY_STANDALONE
	public void SendAlert(string msg)
	{
		jsClient.SendAlert(msg);
	}
#endif

	public async void Connect()
	{

	#if UNITY_STANDALONE
		Uri host = new Uri(hostname);
		await client.ConnectAsync(host, CancellationToken.None);
	#else
		jsClient.Connect(hostname);
		UnityEngine.Debug.Log("Connected!");
		await Task.Delay(3);
	#endif

		connected = true;
		UnityEngine.Debug.Log(connected);
		UnityEngine.Debug.Log($"connected = {connected}");
		HandleIncomingMessages();
	}

	public async Task<string> RequestAuth()
	{
		if(!connected) return null;

		//Send data
		byte[] buffer = new byte[1] { (byte)RequestHeader.REQUEST_AUTH };
		await SendData(buffer);

		//Receive data
		Message msg = await GetMessageOfType(RequestHeader.REQUEST_AUTH);
		buffer = msg.data;

		//Parse data
		key = Encoding.UTF8.GetString(buffer, 0, 5);
		return key;
	}

	public async Task<int> RequestJoinOrCreateLobby()
	{
		if(!connected) return -1;

		//Send data
		byte[] buffer = new byte[6];
		buffer[0] = (byte)RequestHeader.REQUEST_JOIN_OR_CREATE_LOBBY;

		byte[] keyBuffer = Encoding.UTF8.GetBytes(key);
		keyBuffer.CopyTo(buffer, 1);

		await SendData(buffer);

		//Receive data
		Message msg = await GetMessageOfType(RequestHeader.REQUEST_JOIN_OR_CREATE_LOBBY);
		buffer = msg.data;

		//Parse data
		if(BitConverter.IsLittleEndian)
		{
			Array.Reverse(buffer, 0, 4);
		}

		int id = BitConverter.ToInt32(buffer, 0);
		return id;
	}

	public async void SyncData(byte[] data)
	{
		if(!connected) return;

		byte[] buffer = new byte[data.Length + 6];
		buffer[0] = (byte)RequestHeader.SYNC_DATA;

		byte[] keyBuffer = Encoding.UTF8.GetBytes(key);
		keyBuffer.CopyTo(buffer, 1);

		data.CopyTo(buffer, 6);

		await SendData(buffer);
	}

	public void Disconnect()
	{
		if(!connected) return;

	#if UNITY_STANDALONE
		client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnected.", CancellationToken.None);
		client.Dispose();
	#else 
		jsClient.Close();
	#endif

	}

	public bool TryGetData(out string key, out byte[] data)
	{
		key = null;
		data = null;
		if(messageQueue.Count < 1) return false;
		if(messageQueue.Peek().header != RequestHeader.SYNC_DATA) return false;

		Message message = messageQueue.Dequeue();
		key = Encoding.UTF8.GetString(message.data, 0, 5);
		
		data = new byte[message.data.Length - 5];
		for(int i = 0; i < data.Length; i++)
		{
			data[i] = message.data[i + 5];
		}

		return true;
	}

	private async Task<Message> GetMessageOfType(RequestHeader header)
	{
		while(messageQueue.Count < 1) //Wait until there are messages in the queue
		{
			await Task.Delay(20);
		}

		Message msg = messageQueue.Peek();
		while(msg.header != header) //Wait until the message we want is available
		{
			if(messageQueue.Count < 1) continue;
			await Task.Delay(20);
			msg = messageQueue.Peek();
		}

		return messageQueue.Dequeue(); //Return it
	}

	private async void HandleIncomingMessages()
	{
		UnityEngine.Debug.Log("Handling incoming!");
		while(connected)
		{
			(int read, byte[] data) = await ReceiveData();
			if(read < 1) continue;

			RequestHeader header = (RequestHeader)data[0];
			byte[] buffer = new byte[read - 1];
			for(int i = 0; i < buffer.Length; i++) { buffer[i] = data[i + 1]; }

			Message message = new Message();
			message.header = header;
			message.data = buffer;
			messageQueue.Enqueue(message);
		}
	}

	private async Task SendData(byte[] buffer)
	{
		if(!connected) return;

	#if UNITY_STANDALONE
		ArraySegment<byte> segment = new ArraySegment<byte>(buffer);
		await client.SendAsync(segment, WebSocketMessageType.Binary, true, CancellationToken.None);
	#else 
		await jsClient.SendData(buffer);
	#endif

	}

	private async Task<(int, byte[])> ReceiveData()
	{
		if(!connected) return (-1, null);

	#if UNITY_STANDALONE
		byte[] buffer = new byte[32];
		ArraySegment<byte> segment = new ArraySegment<byte>(buffer);
		WebSocketReceiveResult result = await client.ReceiveAsync(segment, CancellationToken.None);
		int bytesRead = HandleReceiveResult(result);
		buffer = segment.Array;
		return (bytesRead, buffer);
	#else 
		UnityEngine.Debug.Log("TODO: Not implemented!");
		await Task.Run(() => UnityEngine.Debug.Log("Replace this"));
		return(0, null);
	#endif

	}

#if UNITY_STANDALONE
	private int HandleReceiveResult(WebSocketReceiveResult result)
	{
		if(result.CloseStatus != null)
		{
			connected = false;
			client.Dispose();
			UnityEngine.Debug.Log($"Connection closed: {result.CloseStatus} - {result.CloseStatusDescription}.");
		}

		if(result.MessageType != WebSocketMessageType.Binary)
		{
			UnityEngine.Debug.LogError($"Received invalid message type: {result.MessageType}.");
		}

		UnityEngine.Debug.Log($"End of message: {result.EndOfMessage}");

		return result.Count;
	}
#endif
}
