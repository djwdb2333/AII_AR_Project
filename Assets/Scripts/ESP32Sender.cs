using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System.Text;

public class ESP32Client : MonoBehaviour
{
    private ClientWebSocket ws;
    public string serverUrl = "ws://192.168.4.1:81";
    private CancellationTokenSource cancelTokenSource;

    public static ESP32Client Instance;

    private bool esp32Available = false;  // 是否已成功连接

    private async void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            await ConnectToServer();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async Task ConnectToServer()
    {
        ws = new ClientWebSocket();
        cancelTokenSource = new CancellationTokenSource();

        try
        {
            Debug.Log("Trying to connect to ESP32...");
            await ws.ConnectAsync(new Uri(serverUrl), cancelTokenSource.Token);
            esp32Available = true;
            Debug.Log("Connected to ESP32 WebSocket server");
        }
        catch (Exception e)
        {
            esp32Available = false;
            Debug.LogWarning("ESP32 not available. WebSocket connect failed: " + e.Message);
        }
    }

    public async void SendVibrationDuration(int durationMs)
    {
        if (!esp32Available)
        {
            // 不连 ESP32 时忽略请求
            return;
        }

        if (ws == null || ws.State != WebSocketState.Open)
        {
            Debug.LogWarning("WebSocket lost. Skipping vibration.");
            esp32Available = false;
            return;
        }

        string msg = $"vibrate:{durationMs}";
        byte[] data = Encoding.UTF8.GetBytes(msg);

        try
        {
            await ws.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, CancellationToken.None);
            Debug.Log("Sent to ESP32: " + msg);
        }
        catch (Exception e)
        {
            Debug.LogError("Send failed: " + e.Message);
            esp32Available = false;
        }
    }
    
        public async void SendVibrationStrength(int strengthByte)
    {
        if (ws == null || ws.State != WebSocketState.Open) return;

        string msg = $"strength:{strengthByte}";
        byte[] data = Encoding.UTF8.GetBytes(msg);

        try
        {
            await ws.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, CancellationToken.None);
            Debug.Log("Sent to ESP32: " + msg);
        }
        catch (Exception e)
        {
            Debug.LogError("Send failed: " + e.Message);
        }
    }

    private void OnApplicationQuit()
    {
        if (ws != null)
        {
            ws.Dispose();
            ws = null;
        }

        if (cancelTokenSource != null)
        {
            cancelTokenSource.Cancel();
            cancelTokenSource.Dispose();
        }
    }
}