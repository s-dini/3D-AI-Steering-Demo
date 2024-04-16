using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (FieldOfView))]

public class FieldOfViewEditor : Editor
{
    void OnSceneGUI()
    {
        FieldOfView fow = (FieldOfView)target;
        Vector3 center = new Vector3(fow.transform.position.x - 0.1f, fow.transform.position.y, fow.transform.position.z + 2.5f);

        Handles.color = Color.white;
        Handles.DrawWireArc( center, Vector3.up, Vector3.forward, 360, fow.viewRadius);

        Vector3 viewAngleA = fow.DirFromAngle(-fow.viewAngle / 2, false);
        Vector3 viewAngleB = fow.DirFromAngle(fow.viewAngle / 2, false);

        Handles.DrawLine(center, center + viewAngleA * fow.viewAngle);
        Handles.DrawLine(center, center + viewAngleB * fow.viewAngle);
    }
}
