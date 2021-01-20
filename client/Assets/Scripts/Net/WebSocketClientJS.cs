using System.Runtime.InteropServices;

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
        SendAlert("Connect");
        _Connect(url);
    }

    public void Close()
    {
        SendAlert("Close");
        _Close();
    }

    public void SendData(byte[] arr)
    {
        SendAlert("SendData");
        _SendData(arr, arr.Length);
    }
}