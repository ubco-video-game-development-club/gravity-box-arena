using System.Runtime.InteropServices;
using System.Threading.Tasks;

public class WebSocketClientJS
{
    [DllImport("__Internal")]
    private static extern void _SendAlert(string msg);
    [DllImport("__Internal")]
    private static extern void _ConnectTwo(string url);
    [DllImport("__Internal")]
    private static extern void _PeePeePooPoo(string url);
    [DllImport("__Internal")]
    private static extern void _Close();
    [DllImport("__Internal")]
    private static extern void _SendData(byte[] arr, int size);

    public void SendAlert(string msg)
    {
        _SendAlert(msg);
    }

    public async Task Connect(string url)
    {
        UnityEngine.Debug.Log("Connect Start");
        _SendAlert(url);
        _PeePeePooPoo(url);
        // NOTE: _PeePeePooPoo doesn't get called here, and never seems to continue
        await Task.Run(() => _PeePeePooPoo(url));
        UnityEngine.Debug.Log("Connect Complete");
    }

    public void Close()
    {
        UnityEngine.Debug.Log("Close");
        _Close();
    }

    public async Task SendData(byte[] arr)
    {
        UnityEngine.Debug.Log("SendData");
        await Task.Run(() => _SendData(arr, arr.Length));
    }
}
