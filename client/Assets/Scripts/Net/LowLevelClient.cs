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

#if UNITY_EDITOR
	private ClientWebSocket client;
#endif

	private Queue<Message> messageQueue;
	private string hostname;
	private bool connected;
	private string key;

	public LowLevelClient(string hostname) 
	{
		this.hostname = hostname;

	#if UNITY_EDITOR
		client = new ClientWebSocket();
	#else
		UnityEngine.Debug.LogError("TODO: Not implemented!");
	#endif

		messageQueue = new Queue<Message>();

	}

	public async void Connect()
	{

	#if UNITY_EDITOR
		Uri host = new Uri(hostname);
		await client.ConnectAsync(host, CancellationToken.None);
	#else
		UnityEngine.Debug.LogError("TODO: Not implemented!");
	#endif

		connected = true;
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

	#if UNITY_EDITOR
		client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnected.", CancellationToken.None);
		client.Dispose();
	#else 
		UnityEngine.Debug.LogError("TODO: Not implemented!");
	#endif

	}

	private async Task<Message> GetMessageOfType(RequestHeader header)
	{
		return await Task.Run(() => {
			while(messageQueue.Count < 1) ; //Wait until there are messages in the queue

			Message msg = messageQueue.Peek();
			while(msg.header != header) //Wait until the message we want is available
			{
				if(messageQueue.Count < 1) continue;
				msg = messageQueue.Peek();
			}

			return messageQueue.Dequeue(); //Return it
		});
	}

	private async void HandleIncomingMessages()
	{
		while(connected)
		{
			(int read, byte[] data) = await ReceiveData();
			if(read < 1) continue;

			RequestHeader header = (RequestHeader)data[0];
			byte[] buffer = new byte[read - 1];
			for(int i = 1; i < data.Length && i <= buffer.Length; i++) { buffer[i - 1] = data[i]; }

			Message message = new Message();
			message.header = header;
			message.data = buffer;
			messageQueue.Enqueue(message);
		}
	}

	private async Task SendData(byte[] buffer)
	{
		if(!connected) return;

	#if UNITY_EDITOR
		ArraySegment<byte> segment = new ArraySegment<byte>(buffer);
		await client.SendAsync(segment, WebSocketMessageType.Binary, true, CancellationToken.None);
	#else 
		UnityEngine.Debug.Log("TODO: Not implemented!");
	#endif

	}

	private async Task<(int, byte[])> ReceiveData()
	{
		if(!connected) return (-1, null);

	#if UNITY_EDITOR
		byte[] buffer = new byte[32];
		ArraySegment<byte> segment = new ArraySegment<byte>(buffer);
		WebSocketReceiveResult result = await client.ReceiveAsync(segment, CancellationToken.None);
		int bytesRead = HandleReceiveResult(result);
		buffer = segment.Array;
		return (bytesRead, buffer);
	#else 
		UnityEngine.Debug.Log("TODO: Not implemented!");
	#endif

	}

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
}
