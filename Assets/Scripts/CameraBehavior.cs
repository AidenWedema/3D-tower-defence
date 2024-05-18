using System;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public Vector2 lookRotation = Vector2.zero;
    public float mouseSensitivity = 5;
    public Mode mode;
    public bool smoothing;
    [SerializeField] private LayerMask mask;
    [SerializeField] private float distanceFromObstacle = 0.95f;
    [SerializeField] private float distanceFromTarget = 7.5f;
    [SerializeField] private Vector3 desiredPosition;
    private Vector2 input;
    private float smoothTime;
    private float smoothTimeScale = 0.5f;
    private Vector3 vel;

    public enum Mode
    {
        ThirdPerson,
        FirstPerson,
        TopDown,
    }

    private void Start()
    {
        SwitchCameraMode(Mode.ThirdPerson);
    }

    void Update()
    {
        GetInput();
        switch (mode)
        {
            case Mode.ThirdPerson:
                ThirdPerson();
                break;

            case Mode.FirstPerson:
                FirstPerson();
                break; 
            
            case Mode.TopDown:
                TopDown();
                break;
        }
        if (smoothing)
        {
            Smooth();
            return;
        }
        transform.SetPositionAndRotation(desiredPosition, Quaternion.Euler(lookRotation.y, lookRotation.x, 0f));
    }

    void GetInput()
    {
        input = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * mouseSensitivity;

        if (Input.GetKeyDown(KeyCode.Q))
            SwitchCameraMode((int)mode-- <= 0 ? Enum.GetNames(typeof(Mode)).Length - 1: (int)mode--);
        if (Input.GetKeyDown(KeyCode.E))
            SwitchCameraMode((int)mode++ >= Enum.GetNames(typeof(Mode)).Length - 1 ? 0 : (int)mode++);
    }

    void Smooth()
    {
        smoothTime += smoothTimeScale * Time.deltaTime;
        transform.SetPositionAndRotation(Vector3.Lerp(transform.position, desiredPosition, smoothTime),
            Quaternion.Lerp(transform.rotation, Quaternion.Euler(lookRotation.y, lookRotation.x, 0f), smoothTime));
        if (smoothTime >= 1 || Vector3.Distance(transform.position, desiredPosition) < 0.2f)
        {
            smoothing = false;
            smoothTime = 0;
        }
    }

    void SetLookRotation()
    {
        // Look around horizontally
        lookRotation.x += input.x;

        // Look around vertically
        lookRotation.y -= input.y;

        lookRotation.y = Mathf.Clamp(lookRotation.y, -89f, 89f); // Clamp vertical rotation to prevent flipping
    }

    void ThirdPerson()
    {
        SetLookRotation();

        Quaternion rotation = Quaternion.Euler(lookRotation.y, lookRotation.x, 0f);

        // Calculate desired camera position
        desiredPosition = target.position + offset - (rotation * Vector3.forward * distanceFromTarget);

        // Check for obstacles between camera and target
        Ray ray = new Ray(target.position, desiredPosition - target.position);
        if (Physics.Raycast(ray, out RaycastHit hit, Vector3.Distance(desiredPosition, target.position), mask))
        {
            // Adjust camera position to avoid the obstacle
            transform.position = ray.GetPoint(hit.distance * distanceFromObstacle);
            return;
        }
    }

    void FirstPerson()
    {
        SetLookRotation();

        desiredPosition = target.position + offset;
    }

    void TopDown()
    {
        desiredPosition = target.position + offset;
        desiredPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref vel, Time.deltaTime);
    }

    void SwitchCameraMode(int newMode)
    {
        SwitchCameraMode((Mode)newMode);
    }

    void SwitchCameraMode(Mode newMode)
    {
        smoothing = true;
        smoothTime = 0;
        mode = newMode;
        switch (mode)
        {
            case Mode.ThirdPerson:
                offset = Vector3.zero;
                distanceFromTarget = 7.5f;
                offset = new Vector3(0, 1, 0);
                Cursor.lockState = CursorLockMode.Locked;
                break;

            case Mode.FirstPerson:
                offset = new Vector3(0, 0.3f, 0);
                Cursor.lockState = CursorLockMode.Locked;
                break;

            case Mode.TopDown:
                offset = new Vector3(0, 20, -9);
                lookRotation = new Vector2(0, 60);
                Cursor.lockState = CursorLockMode.None;
                break;
        }
    }
}
