using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.Reflection;
using TMPro;
using static UnityEditor.Progress;

[CustomEditor(typeof(AgentMovement))]
public class FieldOfViewEditor : Editor
{
    private int index;
    private int indexTwo;
    private string[] options = new string[] { "Seek", "Pursuit", "Flee", "Evade" };

    void OnEnable()
    {
        // Retrieve the saved behavior index from EditorPrefs
        index = EditorPrefs.GetInt("BehaviorIndex", 0);
    }

    void OnSceneGUI()
    {
        AgentMovement fow = (AgentMovement)target;
        Handles.color = Color.white;

        Vector3 center = new Vector3(fow.transform.position.x - 0.1f, fow.transform.position.y, fow.transform.position.z + 2.5f);

        Handles.DrawWireArc(center, Vector3.up, Vector3.forward, 360, fow.viewRadius);
        Vector3 viewAngleA = fow.DirFromAngle(-fow.viewAngle / 2, false);
        Vector3 viewAngleB = fow.DirFromAngle(fow.viewAngle / 2, false);

        Handles.DrawLine(center, center + viewAngleA.normalized * fow.viewRadius);
        Handles.DrawLine(center, center + viewAngleB.normalized * fow.viewRadius);

        Handles.color = Color.red;
        foreach (Transform visibleTarget in fow.visibleTargets)
        {
            Handles.DrawLine(fow.transform.position, visibleTarget.position);
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        AgentMovement agentMovement = (AgentMovement)target;

        index = EditorGUILayout.Popup("Behavior", index, options);
        SetBehavior();
        

        EditorGUILayout.Space();

        string[] optionsTwo = new string[] { "Static Targets", "Player", "Both" };

        indexTwo = EditorGUILayout.Popup("Target", indexTwo, optionsTwo);

        SetTarget(); 
    }
    void SetBehavior()
    {
        AgentMovement fow = (AgentMovement)target;
        EditorPrefs.SetInt("BehaviorIndex", index);

        switch (index)
        {
            case 0:
                fow.selectedBehaviorOption = "Seek";
                break;

            case 1:
                fow.selectedBehaviorOption = "Pursuit";
                break;

            case 2:
                fow.selectedBehaviorOption = "Flee";
                break;

            case 3:
                fow.selectedBehaviorOption = "Evade";
                break;

            default:
                Debug.LogError("Unrecognized Option");
                break;
        }
    }

    void SetTarget()
    {
        AgentMovement fow = (AgentMovement)target;

        switch (indexTwo)
        {
            case 0:
                fow.staticTargets = true;
                fow.player = false;
                break;

            case 1:
                fow.staticTargets = false;
                fow.player = true;
                break;

            case 2:
                fow.staticTargets = true;
                fow.player = true;
                break;

            default:
                Debug.LogError("Unrecognized Option");
                break;
        }
    }
}