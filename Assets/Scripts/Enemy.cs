using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private Collider hitbox;
    [SerializeField] private Rigidbody body;

    public SplinePath spline; // the spline to drive along
    public float offset;
    public float t;
    public float speed;
    public float sight;
    public float maxDistance;

    public float health;
    public int damage;
    public float timer;
    public int gold;
    public string sound;

    private void Start()
    {
        hitbox = gameObject.GetComponent<Collider>();
        if (!hitbox)
            hitbox = gameObject.AddComponent<BoxCollider>();

        body = gameObject.GetComponent<Rigidbody>();
        if (!body)
            body = gameObject.AddComponent<Rigidbody>();
    }

    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            return;
        }


        WalkOnTheGroundLikeAGoodEnemyWould();

        // Adjust rotation to face the spline path
        IWouldBeEternallyThankfulAndForeverInYourDebtIfYouCouldKindlyShiftYourLineOfSightToThatDesignatedPointAtYourEarliestConvenience();
        
        body.velocity = transform.forward * speed;

        // Update t parameter
        t = spline.GetTimeFromPoint(transform.position);

        if (t >= 1)
        {
            Destroy(gameObject);
        }
    }

    private void IWouldBeEternallyThankfulAndForeverInYourDebtIfYouCouldKindlyShiftYourLineOfSightToThatDesignatedPointAtYourEarliestConvenience()
    {
        // get the point in front of the enemy and the point on the spline closest to that point
        Vector3 point = transform.position + transform.forward * sight;
        Vector3 splinePoint = spline.GetClosestPoint(point);

        float distance = Vector3.Distance(point, splinePoint);

        // if the point is close enough to the spline just continue walking normaly
        if (distance < maxDistance)
            return;

        // get the direction to the spline
        Vector3 directionToSpline = splinePoint - point;

        // Calculate the desired forward direction for the Ai
        Vector3 desiredForward = Vector3.Lerp(transform.forward, directionToSpline.normalized, 0.9f);

        float angle = Vector3.SignedAngle(transform.forward, desiredForward, transform.up) / 180f;

        transform.Rotate(Vector3.up, angle);
    }

    public void WalkOnTheGroundLikeAGoodEnemyWould()
    {
        // Cast rays downward to detect the surface beneath the car
        RaycastHit frontHit;
        RaycastHit backHit;
        Vector3 forwardExtents = hitbox.bounds.extents;
        forwardExtents.Scale(transform.forward);
        bool front = Physics.Raycast(transform.position + forwardExtents, -transform.up, out frontHit, 10f);
        bool back = Physics.Raycast(transform.position - forwardExtents, -transform.up, out backHit, 10f);

        if (front && back)
        {
            // Calculate average normal of the terrain beneath the car
            Vector3 averageNormal = (frontHit.normal + backHit.normal).normalized;

            // Calculate rotation to align with the surface normal
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, averageNormal) * transform.rotation;

            // Smoothly rotate towards the target rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 25f);
        }
    }


    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health > 0)
            return;

        //GameManager.gold += gold;
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        // get the point in front of the enemy and the point on the spline closest to that point
        Vector3 point = transform.position + transform.forward * sight;

        Gizmos.DrawSphere(point, 0.05f);

        Gizmos.color = Color.yellow;

        Gizmos.DrawLine(transform.position, point);

        Vector3 splinePoint = spline.GetClosestPoint(point);
        Gizmos.color = Color.red;

        Gizmos.DrawLine(point, splinePoint);
    }
}

