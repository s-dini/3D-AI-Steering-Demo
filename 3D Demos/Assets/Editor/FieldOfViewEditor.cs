using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AgentMovement))]
public class FieldOfViewEditor : Editor
{
    private int index;
    private int indexTwo;
    private string[] options = new string[] { "Seek", "Pursuit", "Flee", "Evade", "Follow" };
    private string[] typeOptions = new string[] { "Solo", "Follower", "Leader" };


    void OnEnable()
    {
        // Retrieve the saved behavior index from EditorPrefs
        index = EditorPrefs.GetInt("BehaviorIndex", 0);
        indexTwo = EditorPrefs.GetInt("TypeIndex", 0);
    }

    void OnSceneGUI()
    {
        AgentMovement fow = (AgentMovement)target;
        Handles.color = Color.white;

        Vector3 center = new Vector3(fow.transform.position.x - 0.1f, fow.transform.position.y, fow.transform.position.z);

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
        AgentMovement agentMovement = (AgentMovement)target;

        index = EditorGUILayout.Popup("Behavior", index, options);
        SetBehavior();


        EditorGUILayout.Space();

        string[] optionsTwo = new string[] { "Static Targets", "Player", "Both" };

        indexTwo = EditorGUILayout.Popup("Type", indexTwo, typeOptions);
        SetType(); 

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

            case 4:
                fow.selectedBehaviorOption = "Follow";
                break;

            default:
                Debug.LogError("Unrecognized Option");
                break;
        }
    }

    void SetType()
    {
        AgentMovement fow = (AgentMovement)target;
        EditorPrefs.SetInt("TypeIndex", indexTwo);

        switch (indexTwo)
        {
            case 0:
                fow.selectedTypeOption = "Solo";
                break;

            case 1:
                fow.selectedTypeOption = "Follower";
                fow.selectedBehaviorOption = "Follow";

                break;

            case 2:
                fow.selectedTypeOption = "Leader";
                break; 
        }
    }
}