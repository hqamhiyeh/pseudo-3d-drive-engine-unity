using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(P3dEngine))]
public class DriveEngineEditor : Editor
{
    private P3dEngine _driveEngine;
    private SerializedProperty _sp_Renderer;
    private SerializedProperty _sp_Renderer__unityRenderers_GameObject;
    private SerializedProperty _sp_Renderer__screen_Position;
    private SerializedProperty _sp_Camera__FOV;

    void OnEnable() {
        // Get 'DriveEngine' script component
        _driveEngine = (P3dEngine)serializedObject.targetObject;

        // Get serialized properties
        _sp_Renderer = serializedObject.FindProperty("<Renderer>k__BackingField");
        _sp_Renderer__unityRenderers_GameObject = _sp_Renderer.FindPropertyRelative("_unityRenderers").FindPropertyRelative("<GameObject>k__BackingField");
        _sp_Renderer__screen_Position = _sp_Renderer.FindPropertyRelative("_screen").FindPropertyRelative("<Position>k__BackingField");
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
        Vector3 new__unityRenderers_GameObject_position = _sp_Renderer__unityRenderers_GameObject.objectReferenceValue != null ? ((GameObject)_sp_Renderer__unityRenderers_GameObject.objectReferenceValue).GetComponent<Transform>().position : Vector3.zero;

        // Check if values have changed
        if (previous__FOV != _sp_Camera__FOV.intValue)
        {
            _driveEngine.Camera.FOV = _sp_Camera__FOV.intValue;
        }
        if (new__unityRenderers_GameObject_position != (Vector3)_sp_Renderer__screen_Position.boxedValue)
        {
            _sp_Renderer__screen_Position.boxedValue = new__unityRenderers_GameObject_position;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void PrintSerializedProperties()
    {
        SerializedProperty it = serializedObject.GetIterator();
        while(it.Next(true))
            Debug.Log(it.name);
    }
}