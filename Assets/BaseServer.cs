using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class BaseServer : MonoBehaviour
{
    public NetworkDriver driver;
    protected NetworkConnection connection;

    public bool connected;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        UpdateServer();
    }

    private void OnDestroy()
    {
        Shutdown();
    }

    public virtual void Init(ushort port)
    {
        driver = NetworkDriver.Create();
        NetworkEndPoint endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = port;
        if(driver.Bind(endpoint) != 0) {
            GameManager.Error("There was an error binding to port " + endpoint.Port);
            driver.Dispose();
            return;
        } else
        {
            driver.Listen();
        }

        connection = default(NetworkConnection);
    }

    public virtual void UpdateServer()
    {
        if (!driver.IsCreated) return;
        driver.ScheduleUpdate().Complete();
        AcceptNewConnection();
        UpdateMessagePump();
        CleanupConnection();
    }

    public virtual void Shutdown()
    {
        if (!driver.IsCreated) return;

        var status = driver.GetConnectionState(connection);
        if (status != NetworkConnection.State.Disconnected)
        {
            driver.Disconnect(connection);
            driver.ScheduleUpdate().Complete();
        }

        driver.Dispose();
        connection = default(NetworkConnection);
        connected = false;
    }

    void AcceptNewConnection()
    {
        if (connection != default(NetworkConnection)) return;
        NetworkConnection c = default(NetworkConnection);
        c = driver.Accept();
        if (c == default(NetworkConnection)) return;
        Debug.Log("Accepted Connection From Client");
        connection = c;
        connected = true;
        GameManager.Current.StartGame();
    }

    void CleanupConnection()
    {
        if (!connected) return;

        var status = driver.GetConnectionState(connection);
        if (status == NetworkConnection.State.Disconnected)
        {
            GameManager.Error("Lost Connection To Client...");
            connected = false;
            GameManager.Current.EndGame();
        }
    }

    protected virtual void UpdateMessagePump()
    {
        DataStreamReader stream;

        NetworkEvent.Type cmd;
        while((cmd = driver.PopEventForConnection(connection, out stream)) != NetworkEvent.Type.Empty)
        {
            if(cmd == NetworkEvent.Type.Data) {
                OpCode code = (OpCode) stream.ReadByte();
                NetMessage.DeserializeMessage(code, ref stream);
            } else if(cmd == NetworkEvent.Type.Disconnect)
            {
                GameManager.Error("Ended Connection With Client...");
                connected = false;
                GameManager.Current.EndGame();
                connection = default(NetworkConnection);
            }
        }
    }

    public virtual void SendToClient(NetMessage msg)
    {
        if (!driver.IsCreated || !GameManager.Current.gameStarted) return;
        DataStreamWriter writer;
        driver.BeginSend(connection, out writer);
        msg.Serialize(ref writer);
        driver.EndSend(writer);
    }
}
