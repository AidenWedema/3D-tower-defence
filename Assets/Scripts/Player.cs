using UnityEngine;

public class Player : MonoBehaviour
{
    public Transform cam;
    public Collider hitbox;
    public Rigidbody body;
    public int health = 100;
    public int maxHealth = 100;
    public float moveSpeed = 5;
    public float runSpeed = 12;
    public float jumpForce = 8;
    public float stamina;
    public float maxStamina = 5;
    public bool noRunningAlowed;
    private Vector3 vel;
    [SerializeField] private float maxVelocityChange = 2;

    void Start()
    {
        cam = Camera.main.transform;
        hitbox = GetComponent<Collider>();
        body = GetComponent<Rigidbody>();

        stamina = maxStamina;
    }

    void Update()
    {
        bool grounded = Grounded();

        if (grounded)
        {
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
                Move();
            else
            {
                body.velocity = new Vector3(0, body.velocity.y, 0);
                stamina = Mathf.Min(stamina + Time.deltaTime, maxStamina);
                if (stamina >= maxStamina / 3)
                    noRunningAlowed = false;
            }
            transform.rotation = Quaternion.Euler(0, cam.rotation.eulerAngles.y, 0);
        }
        else
            AirMove();

        // Jump if grounded and space is pressed
        if (Input.GetButtonDown("Jump") && grounded)
            Jump();
    }

    public void Move()
    {
        Vector3 move = GetMoveDirection();
        if (Input.GetKey(KeyCode.LeftShift) && stamina > 0 && !noRunningAlowed)
        {
            move *= runSpeed;
            stamina -= Time.deltaTime;
            if (stamina <= 0)
                noRunningAlowed = true;
        }
        else
        {
            move *= moveSpeed;
            stamina = Mathf.Min(stamina + Time.deltaTime, maxStamina);
            if (stamina >= maxStamina / 3)
                noRunningAlowed = false;
        }

        body.velocity = new Vector3(move.x, body.velocity.y, move.z);
    }

    public void AirMove()
    {
        Vector3 move = GetMoveDirection() * moveSpeed;

        move.x = Mathf.Clamp(move.x, -maxVelocityChange, maxVelocityChange);
        move.z = Mathf.Clamp(move.z, -maxVelocityChange, maxVelocityChange);
        move = vel + move;

        body.velocity = new Vector3(move.x, body.velocity.y, move.z);
    }

    public void Jump()
    {
        body.velocity = new Vector3(body.velocity.x, jumpForce, body.velocity.z);
        vel = body.velocity;
    }

    bool Grounded()
    {
        // Check if there's ground beneath the player
        Collider[] hits = Physics.OverlapBox(transform.position - new Vector3(0, hitbox.bounds.extents.y, 0), new Vector3(hitbox.bounds.extents.x * 0.5f, 0.01f, hitbox.bounds.extents.z * 0.5f), transform.rotation, ~LayerMask.GetMask("Zone"));
        foreach (Collider hit in hits)
        {
            if (hit.transform != transform)
            {
                return true;
            }
        }
        return false;
    }

    Vector3 GetMoveDirection()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = Vector3.Scale(cam.right, new Vector3(1, 0, 1)).normalized;

        Vector3 moveDirection = (camForward * moveZ + camRight * moveX).normalized;
        return moveDirection;
    }
}