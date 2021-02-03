using System.Runtime.InteropServices;
using System.Threading.Tasks;

public class WebSocketClientJS
{
    [DllImport("__Internal")]
    private static extern void _SendAlert(string msg);
    [DllImport("__Internal")]
    private static extern void _Connect(string url);
    [DllImport("__Internal")]
    private static extern void _Close();
    [DllImport("__Internal")]
    private static extern void _SendData(byte[] arr, int size);

    public void SendAlert(string msg)
    {
        _SendAlert(msg);
    }

    public void Connect(string url)
    {
        UnityEngine.Debug.Log("Connect Start");
        _Connect(url);
        UnityEngine.Debug.Log("Connect Complete");
    }

    public void Close()
    {
        UnityEngine.Debug.Log("Close");
        _Close();
    }

    public async Task SendData(byte[] arr)
    {
        UnityEngine.Debug.Log("Start SendData");
        _SendData(arr, arr.Length);
        UnityEngine.Debug.Log("Finish SendData");
        await Task.Delay(5);
    }
}
