using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class BaseClient : MonoBehaviour
{
    public NetworkDriver driver;
    protected NetworkConnection connection;

    public bool connected;

    // Start is called before the first frame update
    void Start()
    {
        //Init();
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

    public virtual void Init(string dotted, ushort port)
    {
        driver = NetworkDriver.Create();
        NetworkEndPoint endpoint = NetworkEndPoint.Parse(dotted, port);
        if(endpoint == default(NetworkEndPoint))
        {
            GameManager.Error("Incorrect Server, Verify Address Or Port...");
            driver.Dispose();
            return;
        }

        connection = default(NetworkConnection);
        connection = driver.Connect(endpoint);
        if(connection == default(NetworkConnection))
        {
            GameManager.Error("Error Connecting To Server, Connection Failed...");
            driver.Dispose();
            return;
        }
    }

    public virtual void UpdateServer()
    {
        if (!driver.IsCreated) return;
        driver.ScheduleUpdate().Complete();
        UpdateMessagePump();
        CheckAlive();
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
        connection = default (NetworkConnection);
        connected = false;
    }

    void CheckAlive()
    {
        if (!connected) return;

        var status = driver.GetConnectionState(connection);
        if (status == NetworkConnection.State.Disconnected)
        {
            GameManager.Error("Lost Connection To Server...");
            connected = false;
            GameManager.Current.EndGame();
        }
    }

    protected virtual void UpdateMessagePump()
    {
        DataStreamReader stream;

        NetworkEvent.Type cmd;
        while ((cmd = connection.PopEvent(driver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Data)
            {
                OpCode code = (OpCode)stream.ReadByte();
                NetMessage.DeserializeMessage(code, ref stream);
                Debug.Log($"Recieved {code} command");
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                GameManager.Error("Ended Connection With Server...");
                connected = false;
                GameManager.Current.EndGame();
                connection = default(NetworkConnection);
            } else if(cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("Connected to server");
                connected = true;
                GameManager.Current.StartGame();
            }
        }
    }

    public virtual void SendToServer(NetMessage msg)
    {
        if (!driver.IsCreated || !GameManager.Current.gameStarted) return;
        DataStreamWriter writer;
        driver.BeginSend(connection, out writer);
        msg.Serialize(ref writer);
        driver.EndSend(writer);
    }
}
