using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class NetworkManager : MonoBehaviour
{
	public const int NETWORK_MANAGER_NET_ID = -1;

	private enum InternalFunction
	{
		Instantiate = 0,
		Message = 1,
	}

	[System.Serializable] public class OnMessageReceivedEvent: UnityEvent<byte, string> { }

	public static NetworkManager Singleton { get { return singleton; } }
	private static NetworkManager singleton = null;

	public bool IsConnected { get { return client.Connected; } }
	public UnityEvent OnConnected { get { return onConnected; } }
	public UnityEvent OnDisconnect { get { return onDisconnect; } }
	public UnityEvent OnJoinedLobby { get { return onJoinedLobby; } }
	public OnMessageReceivedEvent OnMessageReceived { get { return onMessageReceived; } }

	[SerializeField] private string hostname;
	[SerializeField] private float networkDelta;
	[SerializeField] private UnityEvent onConnected;
	[SerializeField] private UnityEvent onDisconnect;
	[SerializeField] private UnityEvent onJoinedLobby;
	[SerializeField] private OnMessageReceivedEvent onMessageReceived;
	[SerializeField] private bool dontDestroyOnLoad = false;
    private LowLevelClient client;
	private string authKey;
	private int lobbyId;
	private Dictionary<int, NetworkObject> trackedObjects;

	void Awake()
	{
		if(singleton != null)
		{
			Destroy(this);
			return;
		}

		singleton = this;
		client = new LowLevelClient(hostname);
		lobbyId = -1;
		trackedObjects = new Dictionary<int, NetworkObject>();

		if(dontDestroyOnLoad) DontDestroyOnLoad(this);

		StartCoroutine(NetworkUpdate());
	}

	public void Connect()
	{
		client.Connect();
		StartCoroutine(AwaitConnected());
	}

	public void Disconnect()
	{
		client.Disconnect();
		onDisconnect.Invoke();
	}

	public void JoinOrCreateLobby()
	{
		if(lobbyId >= 0) 
		{
			Debug.LogWarning("Cannot join a lobby when already in one.");
			return;
		}

		StartCoroutine(JoinOrCreateLobbyRoutine());
	}

	public void NetworkInstantiate(string resource, Vector2 position, float rotation)
	{
		using(MemoryStream mStream = new MemoryStream())
		{
			using(BinaryWriter writer = new BinaryWriter(mStream))
			{
				writer.Write(NETWORK_MANAGER_NET_ID);
				writer.Write((byte)InternalFunction.Instantiate);
				writer.Write(resource);
				writer.Write(position.x);
				writer.Write(position.y);
				writer.Write(rotation);
			}

			byte[] buffer = mStream.ToArray();
			client.SyncData(buffer);
		}
	}

	public void SendMessage(byte type, string message)
	{
		using(MemoryStream mStream = new MemoryStream())
		{
			using(BinaryWriter writer = new BinaryWriter(mStream))
			{
				writer.Write(NETWORK_MANAGER_NET_ID);
				writer.Write((byte)InternalFunction.Message);
				writer.Write(type);
				writer.Write(message);
			}

			byte[] buffer = mStream.ToArray();
			client.SyncData(buffer);
		}
	}

	public void Track(NetworkObject obj) => trackedObjects.Add(obj.Id, obj);
	public void Untrack(NetworkObject obj) => trackedObjects.Remove(obj.Id);

	private IEnumerator NetworkUpdate()
	{
		WaitForSecondsRealtime waitForSeconds = new WaitForSecondsRealtime(networkDelta);

		while(IsConnected)
		{
			using(MemoryStream mStream = new MemoryStream())
			{
				using(BinaryWriter writer = new BinaryWriter(mStream))
				{
					foreach(NetworkObject obj in trackedObjects.Values)
					{
						writer.Write(obj.Id);
						obj.SendData(writer);
					}
				}

				byte[] buffer = mStream.ToArray();
				client.SyncData(buffer);
			}

			using(MemoryStream mStream = new MemoryStream())
			{
				while(client.TryGetData(out string from, out byte[] data))
				{
					mStream.Write(data, 0, data.Length);
				}

				using(BinaryReader reader = new BinaryReader(mStream))
				{
					bool endOfStream = false;
					while(!endOfStream)
					{
						try
						{
							int id = reader.ReadInt32();
							if(id == NETWORK_MANAGER_NET_ID)
							{
								HandleInternalFunctions(reader);
								continue;
							}

							NetworkObject obj = trackedObjects[id];
							obj.ReceiveData(reader);
						} catch(System.Exception)
						{
							endOfStream = true;
						}
					}
				}
			}

			yield return waitForSeconds;
		}
	}

	private void HandleInternalFunctions(BinaryReader reader)
	{
		InternalFunction func = (InternalFunction)reader.ReadByte();
		switch(func)
		{
			case InternalFunction.Instantiate:
				InstantiateInternal(reader);
				break;
			case InternalFunction.Message:
				MessageInternal(reader);
				break;
			default:
				Debug.LogError("Internal function does not exist!");
				break;
		}
	}

	private void MessageInternal(BinaryReader reader)
	{
		byte type = reader.ReadByte();
		string message = reader.ReadString();
		onMessageReceived.Invoke(type, message);
	}

	private void InstantiateInternal(BinaryReader reader)
	{
		string resource = reader.ReadString();
		float x = reader.ReadSingle();
		float y = reader.ReadSingle();
		float r = reader.ReadSingle();

		GameObject obj = Resources.Load(resource) as GameObject;
		Vector3 pos = new Vector3(x, y, 0);
		Quaternion rot = Quaternion.AngleAxis(r, Vector3.forward);
		Instantiate(obj, pos, rot);
	}

	private IEnumerator JoinOrCreateLobbyRoutine()
	{
		Task<int> lobbyTask = client.RequestJoinOrCreateLobby();
		yield return new WaitUntil(() => lobbyTask.IsCompleted);
		lobbyId = lobbyTask.Result;

		onJoinedLobby.Invoke();
	}

	private IEnumerator AwaitConnected()
	{
		yield return new WaitUntil(() => client.Connected);
		Task<string> requestAuthTask = client.RequestAuth();
		yield return new WaitUntil(() => requestAuthTask.IsCompleted);
		authKey = requestAuthTask.Result;

		onConnected.Invoke();
	}
}
