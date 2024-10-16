using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DriveEngine))]
public class DriveEngineEditor : Editor
{
    private DriveEngine driveEngine;
    private SerializedProperty sp_Camera;
    private SerializedProperty sp__FOV;

    void OnEnable() {
        // Get DriveEngine component
        driveEngine = GameObject.Find("Drive Engine").GetComponent<DriveEngine>();

        // Get serialized properties
        sp_Camera = serializedObject.FindProperty("<Camera>k__BackingField");
        sp__FOV = sp_Camera.FindPropertyRelative("_FOV");
        
        /*// Example of how to print list of Serialized Properties
        SerializedObject scriptComponent = new(GameObject.Find("Drive Engine").GetComponent("DriveEngine"));
        SerializedProperty it = scriptComponent.GetIterator();
        while(it.Next(true))
        {
            Debug.Log(it.name);
        }
        */

    }

    public override void OnInspectorGUI() {
        // Get values before change
        int previous__FOV = sp__FOV.intValue;

        // Make all the public and serialized fields visible in Inspector
        base.OnInspectorGUI();

        // Load changed values
        serializedObject.Update();

        // Check if values have changed
        if (previous__FOV != sp__FOV.intValue) {
            driveEngine.Camera.FOV = sp__FOV.intValue;
        }

        serializedObject.ApplyModifiedProperties();
    }
}