using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class LowLevelClientTest : MonoBehaviour
{
	[SerializeField]
	private string hostname;
	private LowLevelClient client;

    void Awake()
    {
        client = new LowLevelClient(hostname);
    }

	void Start()
	{
		client.Connect();
		StartCoroutine(TestRequestAuth());
	}

	void Update()
	{
		if(client.TryGetData(out string key, out byte[] data))
		{
			Debug.Log($"Received data from {key}");
			foreach(byte b in data)
			{
				Debug.Log(b);
			}
		}
	}

	private IEnumerator TestRequestAuth()
	{
		client.SendAlert("Waiting for connection...");
		yield return new WaitUntil(() => client.Connected);
		client.SendAlert("Connection complete");
		Task<string> requestTask = client.RequestAuth();
		client.SendAlert("Waiting for auth...");
		yield return new WaitUntil(() => requestTask.IsCompleted);
		client.SendAlert("Auth complete");
		string key = requestTask.Result;
		client.SendAlert("Key: " + key);
		Debug.Log(key);

		StartCoroutine(TestJoinOrCreateLobby());
	}

	private IEnumerator TestJoinOrCreateLobby()
	{
		Task<int> requestTask = client.RequestJoinOrCreateLobby();
		yield return new WaitUntil(() => requestTask.IsCompleted);
		int id = requestTask.Result;
		Debug.Log(id);

		TestSyncData();
	}

	private void TestSyncData()
	{
		byte[] data = { 1, 2, 3, 4 };
		client.SyncData(data);
	}
}
