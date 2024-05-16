using UnityEngine;

public class SplineFollower : MonoBehaviour
{
    public SplinePath spline;
    public float t;
    public float speed;
    public int currentPoint;

    private void Update()
    {
        t += speed * Time.deltaTime;

        if (t > 1)
        {
            t--;
            currentPoint++;
        }

        // Draw spline between current point and next point
        Vector3 p0 = spline.path[currentPoint].position;
        Vector3 p1 = spline.path[currentPoint].position + spline.path[currentPoint].outTangent * spline.path[currentPoint].forward();
        Vector3 p2 = spline.path[currentPoint + 1].position + spline.path[currentPoint + 1].inTangent * spline.path[currentPoint + 1].forward();
        Vector3 p3 = spline.path[currentPoint + 1].position;

        transform.position = spline.CalculateBezierPoint(p0, p1, p2, p3, t);
        LookAtPath();
    }

    void LookAtPath()
    {
        Vector3 point = spline.GetPointFromTime(spline.GetTimeFromPoint(transform.position) + 0.01f);
        Vector3 position = transform.position;

        // Calculate the direction from the current position to the next point
        Vector3 direction = new Vector3(point.x - position.x, point.y - position.y, 0f);

        // Calculate the rotation needed to look in that direction
        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, direction);

        // Apply the rotation to the transform
        transform.rotation = rotation;
    }

    private void OnDrawGizmos()
    {
        Vector3 closestPoint = spline.GetClosestPoint(transform.position, out int index);

        // Draw actual closest point
        Gizmos.color = new Color32(150, 255, 150, 255);
        Gizmos.DrawSphere(closestPoint, 0.1f);

        // Draw closest point from spline.points
        Gizmos.color = new Color32(255, 102, 0, 255);
        Gizmos.DrawSphere(spline.points[index], 0.1f);

        // Draw point at time
        Gizmos.color = new Color32(0, 0, 255, 255);
        Gizmos.DrawSphere(spline.GetPointFromTime(spline.GetTimeFromPoint(transform.position)), 0.1f);
    }
}
