using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class NetworkManager : MonoBehaviour
{
	public bool IsConnected { get { return client.Connected; } }
	public UnityEvent OnConnected { get { return onConnected; } }
	public UnityEvent OnDisconnect { get { return onDisconnect; } }
	public UnityEvent OnJoinedLobby { get { return onJoinedLobby; } }

	[SerializeField] private string hostname;
	[SerializeField] private UnityEvent onConnected;
	[SerializeField] private UnityEvent onDisconnect;
	[SerializeField] private UnityEvent onJoinedLobby;
    private LowLevelClient client;
	private string authKey;
	private int lobbyId;

	void Awake()
	{
		client = new LowLevelClient(hostname);
		lobbyId = -1;
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
