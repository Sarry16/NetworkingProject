using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{
    [SerializeField] bool isClientPlayer;
    [SerializeField] float speed = 50f;
    [SerializeField] Color color;
    [SerializeField] bool canControlThisPlayer = true;
    [SerializeField] bool controlWS = true;
    [SerializeField] bool controlArrows = false;

    PlayerInputActions input;
    float movementDirection;

    Vector2 screenBounds;
    float objectWidth;
    float objectHeight;

    bool isActive;

    void OnEnable()
    {
        if (!canControlThisPlayer) return;

        if(input == null) input = new PlayerInputActions();

        input.Movement.ControlArrows.performed += OnMoveInput;
        input.Movement.ControlArrows.canceled += OnMoveInput;

        input.Movement.ControlWS.performed += OnMoveInput;
        input.Movement.ControlWS.canceled += OnMoveInput;

        GameManager.OnActivateElements += Activate;
        GameManager.OnDeactivateElements += Deactivate;
        GameManager.OnResetGame += ResetPlayer;
    }

    void OnDisable()
    {
        if (!canControlThisPlayer) return;

        input.Movement.ControlArrows.performed -= OnMoveInput;
        input.Movement.ControlArrows.canceled -= OnMoveInput;

        input.Movement.ControlWS.performed -= OnMoveInput;
        input.Movement.ControlWS.canceled -= OnMoveInput;

        input.Movement.Disable();

        GameManager.OnActivateElements -= Activate;
        GameManager.OnDeactivateElements -= Deactivate;
        GameManager.OnResetGame -= ResetPlayer;
    }

    // Start is called before the first frame update
    void Start()
    {
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        objectWidth = transform.GetComponent<SpriteRenderer>().bounds.extents.x;
        objectHeight = transform.GetComponent<SpriteRenderer>().bounds.extents.y;

        GetComponent<SpriteRenderer>().color = color;
    }

    void Activate()
    {
        isActive = true;
    }

    void Deactivate()
    {
        isActive = false;

        transform.position = new Vector3(transform.position.x, 0, transform.position.z);

        if (GameManager.lanSession)
        {
            if (GameManager.isClient)
            {
                if (isClientPlayer)
                {
                    var msg = new Net_ObjectPosition(transform.position.x, 0, 2);
                    GameManager.clientObject.SendToServer(msg);
                }
            }
            else
            {
                if (!isClientPlayer)
                {
                    var msg = new Net_ObjectPosition(transform.position.x, 0, 1);
                    GameManager.serverObject.SendToClient(msg);
                }
            }
        }
    }

    void ResetPlayer()
    {
        transform.position = new(transform.position.x, 0, transform.position.z);

        canControlThisPlayer = true;

        if (isClientPlayer)
        {
            controlArrows = true;
            controlWS = false;
        } else
        {
            controlWS = true;
            controlArrows = false;
        }

        if (GameManager.lanSession)
        {
            if (GameManager.isClient)
            {
                if (isClientPlayer)
                {
                    controlArrows = true;
                    controlWS = true;
                }
                else canControlThisPlayer = false;
            }
            else
            {
                if (!isClientPlayer)
                {
                    controlArrows = true;
                    controlWS = true;
                }
                else canControlThisPlayer = false;
            }

        }

        if (!canControlThisPlayer) return;

        input.Movement.Enable();

        if (controlArrows) input.Movement.ControlArrows.Enable();
        else input.Movement.ControlArrows.Disable();

        if (controlWS) input.Movement.ControlWS.Enable();
        else input.Movement.ControlWS.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive) return;

        if(GameManager.lanSession)
        {
            if (GameManager.isClient)
            {
                if (isClientPlayer) UpdatePosition();
            } else
            {
                if(!isClientPlayer) UpdatePosition();
            }

        } else UpdatePosition();
    }

    void UpdatePosition()
    {
        Vector3 last = transform.position;
        var p = transform.position;
        p.y += speed * movementDirection * Time.deltaTime;
        transform.position = p;
        KeepInsideScreen();

        if(transform.position != last)
        {
            if (GameManager.lanSession)
            {
                if (GameManager.isClient)
                {
                    if (isClientPlayer) {
                        var msg = new Net_ObjectPosition(p.x, p.y, 2);
                        GameManager.clientObject.SendToServer(msg);
                    } 
                } else
                {
                    if(!isClientPlayer) {
                        var msg = new Net_ObjectPosition(p.x, p.y, 1);
                        GameManager.serverObject.SendToClient(msg);
                    }
                }
            }
        }
    }

    void OnMoveInput(InputAction.CallbackContext context)
    {
        if (context.performed) movementDirection = context.ReadValue<float>();
        else movementDirection = 0;
    }

    void KeepInsideScreen()
    {
        Vector3 viewPos = transform.position;
        viewPos.x = Mathf.Clamp(viewPos.x, screenBounds.x * -1 + objectWidth, screenBounds.x - objectWidth);
        viewPos.y = Mathf.Clamp(viewPos.y, screenBounds.y * -1 + objectHeight, screenBounds.y - objectHeight);
        transform.position = viewPos;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameManager.lanSession && GameManager.isClient) return;
        if (collision.gameObject.tag == "Ball") {
            collision.gameObject.GetComponent<SpriteRenderer>().color = color;
            if (GameManager.lanSession)
            {
                var msg = new Net_BallColor(color.r, color.g, color.b, color.a);
                GameManager.serverObject.SendToClient(msg);
            }
        }

    }
}
