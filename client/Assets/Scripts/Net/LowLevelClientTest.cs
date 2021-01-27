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
		UnityEngine.Debug.Log("Starting up!");
		client.Connect();
		UnityEngine.Debug.Log("Finished connect.");
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
		UnityEngine.Debug.Log("Waiting for connection...");
		yield return new WaitUntil(() => client.Connected);
		UnityEngine.Debug.Log("Connection complete");
		Task<string> requestTask = client.RequestAuth();
		UnityEngine.Debug.Log("Waiting for auth...");
		yield return new WaitUntil(() => requestTask.IsCompleted);
		UnityEngine.Debug.Log("Auth complete");
		string key = requestTask.Result;
		UnityEngine.Debug.Log("Key: " + key);
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
