using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(P3dEngine))]
public class DriveEngineEditor : Editor
{
    private P3dEngine _driveEngine;
    //private SerializedProperty _sp__unityScreen;
    //private SerializedProperty _sp_Viewport_Position;
    private SerializedProperty _sp_Camera__FOV;

    void OnEnable() {
        // Get 'DriveEngine' script component
        _driveEngine = (P3dEngine)serializedObject.targetObject;

        // Get serialized properties
        //_sp__unityScreen = serializedObject.FindProperty("_unityScreen");
        //_sp_Viewport_Position = serializedObject.FindProperty("<Viewport>k__BackingField").FindPropertyRelative("<Position>k__BackingField");
        _sp_Camera__FOV = serializedObject.FindProperty("<Camera>k__BackingField").FindPropertyRelative("_FOV");

        //PrintSerializedProperties();
    }

    public override void OnInspectorGUI() {
        // Get values before change
        int previous__FOV = _sp_Camera__FOV.intValue;

        // Make all the public and serialized fields visible in Inspector
        base.OnInspectorGUI();

        // Load changed values
        serializedObject.Update();

        // Get new values
        //Vector3 new__unityScreen_position = _sp__unityScreen.objectReferenceValue != null ? ((GameObject)_sp__unityScreen.objectReferenceValue).GetComponent<Transform>().position : Vector3.zero;

        // Check if values have changed
        if (previous__FOV != _sp_Camera__FOV.intValue)
        {
            _driveEngine.Camera.FOV = _sp_Camera__FOV.intValue;
        }
        //if (new__unityScreen_position != (Vector3)_sp_Viewport_Position.boxedValue)
        //{
        //    _sp_Viewport_Position.boxedValue = new__unityScreen_position;
        //}

        serializedObject.ApplyModifiedProperties();
    }

    private void PrintSerializedProperties()
    {
        SerializedProperty it = serializedObject.GetIterator();
        while(it.Next(true))
            Debug.Log(it.name);
    }
}