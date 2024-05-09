using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FollowerScript))]
public class FollowerFOVEditor : Editor
{
    private int index;
    private int indexTwo;
    private string[] options = new string[] { "Seek", "Pursuit", "Flee", "Evade", "Follow" };


    void OnEnable()
    {
        // Retrieve the saved behavior index from EditorPrefs
        index = EditorPrefs.GetInt("BehaviorIndex", 0);
    }

    void OnSceneGUI()
    {
        FollowerScript fow = (FollowerScript)target;
        Handles.color = Color.white;

        Vector3 center = new Vector3(fow.transform.position.x - 0.1f, fow.transform.position.y, fow.transform.position.z + 2.5f);

        Handles.DrawWireArc(center, Vector3.up, Vector3.forward, 360, fow.viewRadius);
        Vector3 viewAngleA = fow.DirFromAngle(-fow.viewAngle / 2, false);
        Vector3 viewAngleB = fow.DirFromAngle(fow.viewAngle / 2, false);

        Handles.DrawLine(center, center + viewAngleA.normalized * fow.viewRadius);
        Handles.DrawLine(center, center + viewAngleB.normalized * fow.viewRadius);

        Handles.color = Color.red;
        
        /*foreach (Transform visibleTarget in fow.visibleTargets)
        {
            Handles.DrawLine(fow.transform.position, visibleTarget.position);
        }*/
    }


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        FollowerScript agentMovement = (FollowerScript)target;

        index = EditorGUILayout.Popup("Behavior", index, options);
        SetBehavior();

        EditorGUILayout.Space();
    }

    void SetBehavior()
    {
        FollowerScript fow = (FollowerScript)target;
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

            case 4:
                fow.selectedBehaviorOption = "Follow";
                break;

            default:
                Debug.LogError("Unrecognized Option");
                break;
        }
    }
}