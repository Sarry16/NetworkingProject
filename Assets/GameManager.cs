using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Current;

    public static bool isClient;
    public static bool lanSession;
    public static BaseServer serverObject;
    public static BaseClient clientObject;

    public static event Action OnActivateElements;
    public static event Action OnDeactivateElements;
    public static event Action OnResetGame;

    [SerializeField] TextMeshProUGUI scoreHolder;
    [SerializeField] StartTextEffect startTextEffect;
    [SerializeField] bool lan;
    [SerializeField] bool client;

    [SerializeField] PlayerControl leftPlayer;
    [SerializeField] PlayerControl rightPlayer;
    [SerializeField] BallPhysics ball;

    [SerializeField] RectTransform modePanel;
    [SerializeField] TMP_InputField port;
    [SerializeField] TMP_InputField ipv4;


    int scoreLeft = 0;
    int scoreRight = 0;

    public bool gameStarted = false;

    bool isSelectingMode = true;

    PlayerInputActions input;

    void OnEnable()
    {
        if (input == null) input = new PlayerInputActions();

        input.Control.End.performed += OnEnd;
        input.Control.End.Enable();
    }

    void OnDisable()
    {
        input.Control.End.performed -= OnEnd;
    }

    // Start is called before the first frame update
    void Start()
    {

        Current = this;

        startTextEffect.OnEffectFinished += StartEffectFinished;

        DeactivateGame();
    }

    // Update is called once per frame
    void Update()
    {
        if (isSelectingMode || modePanel.gameObject.activeInHierarchy) return;

        UpdateConnectionState();

        if (!gameStarted)
        {
            if (lanSession)
            {
                if (isClient)
                {
                    if (clientObject.connected) gameStarted = true;
                } else
                {
                    if (serverObject.connected)
                    {
                        gameStarted = true;
                        StartStartEffect(3);
                    }
                    else
                    {
                        return;
                    }
                }
            }
            else
            {
                gameStarted = true;
                StartStartEffect(3);
            }

        }

        if (isClient) return;

        string last = scoreHolder.text;

        scoreHolder.text = $"{scoreLeft} - {scoreRight}";

        if(scoreHolder.text != last)
        {
            if (lanSession)
            {
                if (!isClient)
                {
                    var msg = new Net_Score(scoreHolder.text);
                    serverObject.SendToClient(msg);
                }
            }
        }
    }

    void PrepareServers()
    {
        if (lanSession)
        {
            if (isClient)
            {
                clientObject = GetComponent<BaseClient>();
                clientObject.enabled = true;
                return;
            }
            else
            {
                serverObject = GetComponent<BaseServer>();
                serverObject.enabled = true;
            }
        }
    }

    void UpdateConnectionState()
    {
        if (!lanSession) return;
        if (!gameStarted) return;
        if (isClient)
        {
            if (!clientObject.connected)
            {
                Error("Lost Connection To Server!!");
                gameStarted = false;
            }
        } else
        {
            if (!serverObject.connected)
            {
                Error("Lost Connection To Server!!");
                gameStarted= false;
            }
        }
    }

    public void ForceUpdateScore(string score)
    {
        scoreHolder.text = score;
    }

    public void AddScore(bool right = false)
    {
        if(right) scoreRight++;
        else scoreLeft++;

        StartStartEffect(1);
    }

    void StartStartEffect(float seconds)
    {
        DeactivateGame();
        startTextEffect.StartEffect(seconds);
    }

    void StartEffectFinished()
    {
        ActivateGame();

        if (lanSession && !isClient)
        {
            var msg = new Net_StartEffect("0",0);
            serverObject.SendToClient(msg);
        }
    }

    public void ForceUpdateStartText(string number, int active)
    {
        startTextEffect.ForceUpdateStartText(number, active);
    }

    void ActivateGame()
    {
        OnActivateElements?.Invoke();

        if (lanSession)
        {
            if (!isClient)
            {
                var msg = new Net_GameState(1);
                serverObject.SendToClient(msg);
            }
        }
    }

    void DeactivateGame()
    {
        OnDeactivateElements?.Invoke();

        if (lanSession)
        {
            if (!isClient)
            {
                var msg = new Net_GameState(0);
                serverObject.SendToClient(msg);
            }
        }
    }

    public void SetState(int active)
    {
        if (active == 1) ActivateGame();
        else DeactivateGame();
    }

    public void ForcePosition(int target, Vector2 pos, Vector2 dir = default(Vector2)) {
        Transform t = null;
        switch (target)
        {
            case 0: // BALL
                t = ball.transform;
                ball.movementDirection = dir;
                break;
            case 1: // LEFT
                t = leftPlayer.transform;
                break;
            case 2: // RIGHT
                t = rightPlayer.transform;
                break;
        }

        if (t == null) return;
        t.position = new Vector3(pos.x, pos.y, t.position.z);
    }

    public void ForceBallColor(Color c)
    {
        ball.GetComponent<SpriteRenderer>().color = c;
    }

    public static void Error(string error)
    {
        ItemObtainedNotification.AddToQueue(error);
    }

    public void LocalPlay()
    {
        lanSession = false;
        isClient = false;
        StartGame();
    }

    public void ClientPlay()
    {
        ushort p;

        if(ushort.TryParse(port.text, out p))
        {
            lanSession = true;
            isClient = true;
            PrepareServers();
            clientObject.Init(ipv4.text, p);
        } else
        {
            Error("Port entered is not valid...");
        }
    }

    public void ServerPlay()
    {
        ushort p;

        if (ushort.TryParse(port.text, out p))
        {
            lanSession = true;
            PrepareServers();
            serverObject.Init(p);
        }
        else
        {
            Error("Port entered is not valid...");
        }
    }

    public void EndGame()
    {
        if (lanSession)
        {
            if (isClient)
            {
                clientObject.Shutdown();
                clientObject.enabled = false;
            } else
            {
                serverObject.Shutdown();
                serverObject.enabled = false;
            }
        }
        isSelectingMode = true;
        gameStarted = false;
        modePanel.gameObject.SetActive(true);
    }

    public void StartGame()
    {
        isSelectingMode = false;
        modePanel.gameObject.SetActive(false);
        ResetGame();
    }

    void ResetGame()
    {
        DeactivateGame();
        scoreLeft = 0;
        scoreRight = 0;
        scoreHolder.text = "0 - 0";
        OnResetGame?.Invoke();
        gameStarted = false;
    }

    void OnEnd(InputAction.CallbackContext context)
    {
        EndGame();
    }
}
