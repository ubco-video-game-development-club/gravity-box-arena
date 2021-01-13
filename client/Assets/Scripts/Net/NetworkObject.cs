using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class NetworkObject : MonoBehaviour
{
	private static int objectCount = 0;

	[System.Serializable] public class OnSendDataEvent: UnityEvent<BinaryWriter> { }
	[System.Serializable] public class OnReceiveDataEvent: UnityEvent<BinaryReader> { }

	public OnSendDataEvent OnSendData { get { return onSendData; } }
	public OnReceiveDataEvent OnReceiveData { get { return onReceiveData; } }
	public int Id { get { return id; } }

	[SerializeField] private OnSendDataEvent onSendData;
	[SerializeField] private OnReceiveDataEvent onReceiveData;
	private int id;

	void Awake()
	{
		id = objectCount++;
	}

    void OnEnable()
	{
		NetworkManager.Singleton.Track(this);
	}

	void OnDisable()
	{
		NetworkManager.Singleton.Untrack(this);
	}

	public void SendData(BinaryWriter writer) => onSendData.Invoke(writer);
	public void ReceiveData(BinaryReader reader) => onReceiveData.Invoke(reader);
}
