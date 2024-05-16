using System.Collections.Generic;
using UnityEngine;

public class SplinePath : MonoBehaviour
{
    public Point[] path = new Point[1];
    public List<Vector3> points = new List<Vector3>();

    public Color32 color = Color.red;

    [System.Serializable]
    public class Point
    {
        public Vector3 position;
        public Vector2 rotation; // Euler angles
        public float inTangent;
        public float outTangent;

        public Vector3 forward()
        {
            // Convert Euler angles to Quaternion
            Quaternion quaternionRotation = Quaternion.Euler(rotation);

            // Transform the forward vector by the rotation
            return quaternionRotation * Vector3.forward;
        }
    }

    // Function to calculate position along spline based on parameter value t
    public Vector3 GetPointFromTime(float t)
    {
        if (points.Count < 2)
            return Vector3.zero; // or some default value

        // Determine the segment index and local parameter value within that segment
        int segmentIndex = Mathf.FloorToInt(t * (points.Count - 1));
        float localT = t * (points.Count - 1) - segmentIndex;

        // Ensure segmentIndex stays within valid range
        segmentIndex = Mathf.Clamp(segmentIndex, 0, points.Count - 2);

        // Interpolate position using the appropriate segment
        return InterpolateSegmentPosition(segmentIndex, localT);
    }

    // Function to interpolate position along a specific segment of the spline
    private Vector3 InterpolateSegmentPosition(int segmentIndex, float t)
    {
        // Perform interpolation using the segment's control points
        // For example, you can use cubic interpolation (LERP for simplicity)
        return Vector3.Lerp(points[segmentIndex], points[segmentIndex + 1], t);
    }
    public float GetTimeFromPoint(Vector3 point)
    {
        return GetTimeFromPoint(point, out Vector3 splinePoint);
    }

    // Function to calculate the time (t) value from a given point on the spline
    public float GetTimeFromPoint(Vector3 point, out Vector3 splinePoint)
    {
        float minDistance = float.MaxValue;
        float minTime = 0f;
        splinePoint = Vector3.zero;

        // Iterate through each segment of the spline
        for (int i = 0; i < path.Length - 1; i++)
        {
            // Calculate the time (t) value at the start and end of the segment
            float startTime = i / (float)(path.Length - 1);
            float endTime = (i + 1) / (float)(path.Length - 1);

            // Sample points along the segment and find the closest point to the given point
            int sampleCount = 10; // Adjust this value based on the desired precision
            for (int j = 0; j <= sampleCount; j++)
            {
                float t = Mathf.Lerp(startTime, endTime, j / (float)sampleCount);
                splinePoint = GetPointFromTime(t);
                float distance = Vector3.Distance(point, splinePoint);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    minTime = t;
                }
            }
        }
        return minTime;
    }

    public int GetClosestPointIndex(Vector3 objectPosition)
    {
        GetClosestPoint(objectPosition, out int point);
        return point;
    }

    public Vector3 GetClosestPoint(Vector3 objectPosition)
    {
        return GetClosestPoint(objectPosition, out int point);
    }

    public Vector3 GetClosestPoint(Vector3 objectPosition, out int point)
    {
        Vector3 closestPoint = Vector3.zero;
        float minDistance = float.MaxValue;
        point = -1;

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 segmentStart = points[i];
            Vector3 segmentEnd = points[i + 1];

            // Calculate closest point on segment to objectPosition
            Vector3 closest = GetClosestPointOnSegment(segmentStart, segmentEnd, objectPosition);

            // Calculate distance to object
            float distance = Vector3.Distance(closest, objectPosition);

            // Check if this is the closest point found so far
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPoint = closest;
                point = i + 1;
            }
        }

        return closestPoint;
    }

    private Vector3 GetClosestPointOnSegment(Vector3 segmentStart, Vector3 segmentEnd, Vector3 objectPosition)
    {
        Vector3 segmentDirection = segmentEnd - segmentStart;
        float segmentLengthSquared = segmentDirection.sqrMagnitude;

        if (segmentLengthSquared == 0)
            return segmentStart;

        // Project objectPosition onto the segment
        float t = Vector3.Dot(objectPosition - segmentStart, segmentDirection) / segmentLengthSquared;
        t = Mathf.Clamp01(t);

        return segmentStart + t * segmentDirection;
    }

    private void OnDrawGizmos()
    {
        DrawPath();
        return;
        Gizmos.color = Color.green;
        foreach (Vector3 point in points)
        {
            Gizmos.DrawSphere(point, 0.05f);
        }
    }

    private void DrawPath()
    {
        if (path.Length < 2)
            return;

        for (int i = 0; i < path.Length - 1; i++)
        {
            Gizmos.color = color;

            // Draw spline between current point and next point
            Vector3 p0 = path[i].position;
            Vector3 p1 = path[i].position + path[i].outTangent * path[i].forward();
            Vector3 p2 = path[i + 1].position + path[i + 1].inTangent * path[i + 1].forward();
            Vector3 p3 = path[i + 1].position;

            int segmentCount = 25;
            float step = 1f / segmentCount;
            for (float t = 0; t < 1; t += step)
            {
                Vector3 point = CalculateBezierPoint(p0, p1, p2, p3, t);
                Vector3 nextPoint = CalculateBezierPoint(p0, p1, p2, p3, t + step);
                Gizmos.DrawLine(point, nextPoint);
            }

            // Draw tangent handles
            Gizmos.color = Color.black;
            DrawTangentHandle(path[i].position, path[i].rotation, path[i].outTangent, true);
            Gizmos.color = Color.black;
            DrawTangentHandle(path[i + 1].position, path[i + 1].rotation, path[i + 1].inTangent, false);
        }
    }

    private void DrawTangentHandle(Vector3 point, Vector3 rotation, float tangentLength, bool isOut)
    {
        Quaternion rot = Quaternion.Euler(rotation);
        Vector3 tangentDirection = rot * Vector3.forward * tangentLength;
        Vector3 tangentPosition = point + tangentDirection;

        // Draw tangent handle line
        Gizmos.DrawLine(point, tangentPosition);

        // Draw tangent handle point
        Gizmos.DrawSphere(tangentPosition, 0.05f);

        // Draw line connecting tangent handle to point
        if (isOut)
            Gizmos.DrawLine(point, tangentPosition);
        else
            Gizmos.DrawLine(point, tangentPosition);
    }

    public float GetLength()
    {
        float length = 0;
        for (int i = 0; i < points.Count - 1; i++)
        {
            length += Vector3.Distance(points[i], points[i + 1]);
        }
        return length;
    }

    public Vector3 CalculateBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * p0;
        p += 3 * uu * t * p1;
        p += 3 * u * tt * p2;
        p += ttt * p3;

        return p;
    }
    public Vector3 CalculateBezierPointAlongSpline(float t)
    {
        // Get the total length of the spline
        float totalLength = GetLength();

        // Calculate the length of each segment
        float segmentLength = totalLength / (path.Length - 1);

        // Find the segment index and local 't' value
        int segmentIndex = Mathf.FloorToInt(t * (path.Length - 1));
        float localT = (t * (path.Length - 1)) - segmentIndex;

        // Get the four control points for this segment
        Vector3 p0 = path[segmentIndex].position;
        Vector3 p1 = p0 + path[segmentIndex].outTangent * path[segmentIndex].forward();
        Vector3 p2 = path[segmentIndex + 1].position + path[segmentIndex + 1].inTangent * path[segmentIndex + 1].forward();
        Vector3 p3 = path[segmentIndex + 1].position;

        // Calculate the position on the Bezier curve
        return CalculateBezierPoint(p0, p1, p2, p3, localT);
    }

}
