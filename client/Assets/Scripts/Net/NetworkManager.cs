using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class NetworkManager : MonoBehaviour
{
	public static NetworkManager Singleton { get { return singleton; } }
	private static NetworkManager singleton = null;

	public bool IsConnected { get { return client.Connected; } }
	public UnityEvent OnConnected { get { return onConnected; } }
	public UnityEvent OnDisconnect { get { return onDisconnect; } }
	public UnityEvent OnJoinedLobby { get { return onJoinedLobby; } }

	[SerializeField] private string hostname;
	[SerializeField] private float networkDelta;
	[SerializeField] private UnityEvent onConnected;
	[SerializeField] private UnityEvent onDisconnect;
	[SerializeField] private UnityEvent onJoinedLobby;
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
