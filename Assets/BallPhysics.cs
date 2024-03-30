using UnityEngine;

public class BallPhysics : MonoBehaviour
{

    [SerializeField] float speed = 5f;
    [SerializeField] float maxSpeed = 20f;
    [SerializeField] float accel = .1f;

    float currentSpeed;

    public Vector2 movementDirection;

    Vector2 screenBounds;
    float objectWidth;
    float objectHeight;

    bool isActive;

    Rigidbody2D rb;

    void OnEnable()
    {
        GameManager.OnActivateElements += Activate;
        GameManager.OnDeactivateElements += Deactivate;
        GameManager.OnResetGame += ResetBall;
    }

    void OnDisable()
    {
        GameManager.OnActivateElements -= Activate;
        GameManager.OnDeactivateElements -= Deactivate;
        GameManager.OnResetGame -= ResetBall;
    }

    // Start is called before the first frame update
    void Start()
    {
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        objectWidth = transform.GetComponent<SpriteRenderer>().bounds.extents.x;
        objectHeight = transform.GetComponent<SpriteRenderer>().bounds.extents.y;

        rb = GetComponent<Rigidbody2D>();
    }

    void Activate()
    {
        isActive = true;
    }

    void Deactivate()
    {
        isActive = false;
        ResetPosition();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive) return;

        currentSpeed += accel * Time.deltaTime;
        if (currentSpeed > maxSpeed) currentSpeed = maxSpeed;

        //if (GameManager.isClient) return;

        UpdatePosition();

    }

    void UpdatePosition()
    {
        //Vector3 p = transform.position;
        //p.x += movementDirection.x * currentSpeed * Time.deltaTime;
        //p.y += movementDirection.y * currentSpeed * Time.deltaTime;
        //transform.position = p;

        rb.velocity = movementDirection * currentSpeed;

        //if (GameManager.lanSession)
        //{
        //    if (!GameManager.isClient)
        //    {
        //        var msg = new Net_ObjectPosition(transform.position.x, transform.position.y, 0);
        //        GameManager.serverObject.SendToClient(msg);
        //    }
        //}
    }

    void RadomizeInitial()
    {
        Random.InitState(System.DateTime.Now.Second);
        float initialX = Random.Range(-1f, 1f);
        float initialY = Random.Range(-1f, 1f);
        if (Mathf.Abs(initialX) < 0.6f) initialX = Mathf.Sign(initialX) * 0.6f;
        movementDirection = new Vector2(initialX, initialY);
        movementDirection.Normalize();
    }

    void ResetPosition()
    {
        if (rb == null) return;

        var p = Camera.main.transform.position;
        p.z = -5;
        transform.position = p;
        rb.velocity = Vector2.zero;
        currentSpeed = speed;
        GetComponent<SpriteRenderer>().color = Color.white;

        if (GameManager.lanSession && GameManager.isClient) return;

        RadomizeInitial();


        if (GameManager.lanSession)
        {
            if (!GameManager.isClient)
            {
                var msg = new Net_ObjectPosition(transform.position.x, transform.position.y, 0, movementDirection.x, movementDirection.y);
                GameManager.serverObject.SendToClient(msg);
            }
        }
    }

    void ResetBall()
    {
        ResetPosition();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameManager.isClient) return;

        var contact = collision.GetContact(0);
        var normal = contact.normal;

        if (collision.gameObject.tag == "Player")
        {
            float deltaY = contact.point.y - collision.transform.position.y;
            float deltaX = contact.point.x - collision.transform.position.x;
            float normalizedDeltaY = deltaY / collision.transform.GetComponent<SpriteRenderer>().bounds.extents.y;
            int sign = 1;
            if (deltaX < 0) sign = -1;
            float clampedDeltaY = Mathf.Clamp(normalizedDeltaY, -0.75f, 0.75f);
            normal.y = clampedDeltaY;
            normal.x = sign * Mathf.Sqrt(1 - clampedDeltaY * clampedDeltaY);

            movementDirection = Vector2.Reflect(movementDirection, normal);

            if (deltaX < 0 && movementDirection.x > 0) movementDirection.x = -1;
            if(deltaX > 0 && movementDirection.x < 0) movementDirection.x = 1;

            if(Mathf.Abs(movementDirection.x) < 0.3) movementDirection.x = Mathf.Sign(movementDirection.x) * 0.3f;

        } else
        {
            movementDirection = Vector2.Reflect(movementDirection, normal);
        }


        movementDirection.Normalize();

        if (GameManager.lanSession)
        {
            if (!GameManager.isClient)
            {
                var msg = new Net_ObjectPosition(transform.position.x, transform.position.y, 0, movementDirection.x, movementDirection.y);
                GameManager.serverObject.SendToClient(msg);
            }
        }

        //Debug.Log($"MovementDirection: {movementDirection}, Normal: {normal}");
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (GameManager.isClient) return;

        if (collision.gameObject.tag == "Goal")
        {
            bool right = false;
            if (Mathf.Sign(movementDirection.x) == -1) right = true;
            GameManager.Current.AddScore(right);
            ResetPosition();
        }
    }

    void KeepInsideScreen()
    {
        Vector3 viewPos = transform.position;
        viewPos.x = Mathf.Clamp(viewPos.x, screenBounds.x * -1 + objectWidth, screenBounds.x - objectWidth);
        viewPos.y = Mathf.Clamp(viewPos.y, screenBounds.y * -1 + objectHeight, screenBounds.y - objectHeight);
        transform.position = viewPos;
    }
}
