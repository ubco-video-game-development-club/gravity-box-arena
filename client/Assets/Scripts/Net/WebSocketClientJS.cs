using System.Runtime.InteropServices;

public class WebSocketClientJS
{
    [DllImport("__Internal")]
    private static extern void SendAlertInternal(string msg);

    public void SendAlert(string msg)
    {
        SendAlertInternal(msg);
    }
}