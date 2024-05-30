using UnityEngine;

// state pattern
public class Player : MonoBehaviour
{
    public State state;
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
    public float maxVelocityChange = 2;
    [HideInInspector]public Vector3 vel;

    void Start()
    {
        SwitchSate(new IdleState());
        cam = Camera.main.transform;
        hitbox = GetComponent<Collider>();
        body = GetComponent<Rigidbody>();

        stamina = maxStamina;
    }

    void Update()
    {
        state.Update(this);
    }

    public bool Grounded()
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

    public Vector3 GetMoveDirection()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = Vector3.Scale(cam.right, new Vector3(1, 0, 1)).normalized;

        Vector3 moveDirection = (camForward * moveZ + camRight * moveX).normalized;
        return moveDirection;
    }

    public void SwitchSate(State newState)
    {
        state = newState;
    }
}

public class IdleState : State
{
    public override void Update(Player player)
    {
        bool grounded = player.Grounded();

        if (!grounded)
        {
            player.SwitchSate(new FallingState());
            return;
        }

        player.stamina = Mathf.Min(player.stamina + Time.deltaTime, player.maxStamina);
        if (player.stamina >= player.maxStamina / 3)
            player.noRunningAlowed = false;

        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            player.SwitchSate(new RunningState());

        if (Input.GetButtonDown("Jump"))
            player.SwitchSate(new JumpingState());
    }
}

public class RunningState : State
{
    public override void Update(Player player)
    {

        if (!player.Grounded())
        {
            player.SwitchSate(new FallingState());
            return;
        }

        if (!Input.anyKey)
        {
            player.SwitchSate(new IdleState());
            return;
        }

        if (Input.GetButtonDown("Jump"))
            player.SwitchSate(new JumpingState());

        Vector3 move = player.GetMoveDirection();
        if (Input.GetKey(KeyCode.LeftShift) && player.stamina > 0 && !player.noRunningAlowed)
        {
            move *= player.runSpeed;
            player.stamina -= Time.deltaTime;
            if (player.stamina <= 0)
                player.noRunningAlowed = true;
        }
        else
        {
            move *= player.moveSpeed;
            player.stamina = Mathf.Min(player.stamina + Time.deltaTime, player.maxStamina);
            if (player.stamina >= player.maxStamina / 3)
                player.noRunningAlowed = false;
        }

        player.body.velocity = new Vector3(move.x, player.body.velocity.y, move.z);
        player.transform.rotation = Quaternion.Euler(0, player.cam.rotation.eulerAngles.y, 0);
    }
}

public class JumpingState : State
{
    public override void Update(Player player)
    {
        player.body.velocity = new Vector3(player.body.velocity.x, player.jumpForce, player.body.velocity.z);
        player.vel = player.body.velocity;
        player.SwitchSate(new FallingState());
    }
}

public class FallingState : State
{
    public override void Update(Player player)
    {
        if (player.Grounded())
        {
            player.SwitchSate(new IdleState());
            return;
        }

        Vector3 move = player.GetMoveDirection() * player.moveSpeed;

        move.x = Mathf.Clamp(move.x, -player.maxVelocityChange, player.maxVelocityChange);
        move.z = Mathf.Clamp(move.z, -player.maxVelocityChange, player.maxVelocityChange);
        move = player.vel + move;

        player.body.velocity = new Vector3(move.x, player.body.velocity.y, move.z);
    }
}