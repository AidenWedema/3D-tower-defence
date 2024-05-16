using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;

[CustomEditor(typeof(SplinePath))]
public class SplinePathEditor : Editor
{
    private SplinePath splinePath;
    private int selectedPointIndex = -1;

    private void OnEnable()
    {
        splinePath = (SplinePath)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Add Point"))
        {
            AddPoint();
        }
        if (GUILayout.Button("Generate"))
        {
            CreateSpline();
        }
    }

    private void OnSceneGUI()
    {
        if (splinePath == null || splinePath.path == null)
            return;

        for (int i = 0; i < splinePath.path.Length; i++)
        {
            DrawPointHandles(i);
        }
    }

    private void DrawPointHandles(int index)
    {
        Vector3 pointPosition = splinePath.path[index].position;
        Handles.color = Color.yellow;
        Handles.DrawWireDisc(pointPosition, Vector3.up, 0.5f);

        EditorGUI.BeginChangeCheck();
        Vector3 newPosition = Handles.PositionHandle(pointPosition, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(splinePath, "Move Point");
            splinePath.path[index].position = newPosition;
        }

        if (selectedPointIndex == index)
        {
            DrawControlHandles(index);
        }
    }

    private void DrawControlHandles(int index)
    {
        Handles.color = Color.gray;

        Vector3 handlePosition = splinePath.path[index].position + splinePath.path[index].inTangent * splinePath.path[index].forward();
        EditorGUI.BeginChangeCheck();
        Vector3 newInPosition = Handles.PositionHandle(handlePosition, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(splinePath, "Move In Point");
            splinePath.path[index].inTangent = Vector3.Distance(newInPosition, splinePath.path[index].position);
        }

        handlePosition = splinePath.path[index].position + splinePath.path[index].outTangent * splinePath.path[index].forward();
        EditorGUI.BeginChangeCheck();
        Vector3 newOutPosition = Handles.PositionHandle(handlePosition, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(splinePath, "Move Out Point");
            splinePath.path[index].outTangent = Vector3.Distance(newOutPosition, splinePath.path[index].position);
        }
    }

    private void AddPoint()
    {
        Undo.RecordObject(splinePath, "Add Point");
        int oldLength = splinePath.path.Length;
        Array.Resize(ref splinePath.path, oldLength + 1);
        splinePath.path[oldLength] = new SplinePath.Point();
        splinePath.path[oldLength].position = splinePath.path[oldLength - 1].position + splinePath.path[oldLength - 1].forward();
        EditorUtility.SetDirty(splinePath);
    }

    private void CreateSpline()
    {
        if (splinePath.path.Length < 2)
        {
            Debug.LogWarning("Not enough points to generate spline");
            return;
        }

        Undo.RecordObject(splinePath, "CreateSpline");
        
        for (int i = 0; i < splinePath.path.Length - 1; i++)
        {
            // Draw spline between current point and next point
            Vector3 p0 = splinePath.path[i].position;
            Vector3 p1 = splinePath.path[i].position + splinePath.path[i].outTangent * splinePath.path[i].forward();
            Vector3 p2 = splinePath.path[i + 1].position + splinePath.path[i + 1].inTangent * splinePath.path[i + 1].forward();
            Vector3 p3 = splinePath.path[i + 1].position;

            float segmentCount = 25;
            float step = 1f / segmentCount;
            for (float t = 0; t < 1; t += step)
            {
                Vector3 point = CalculateCubicBezierPoint(p0, p1, p2, p3, t);
                splinePath.points.Add(point);
            }
        }
        EditorUtility.SetDirty(splinePath);
    }

    private Vector3 CalculateCubicBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
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
}
