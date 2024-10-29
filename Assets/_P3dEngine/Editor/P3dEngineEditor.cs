using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(P3dEngine))]
public class P3dEngineEditor : Editor
{
    private IP3dEngineEditor _driveEngine;
    private SerializedProperty _sp_Renderer;
    private SerializedProperty _sp_Renderer__unityDisplay_GameObject;
    private SerializedProperty _sp_Renderer__screen_Position;
    private SerializedProperty _sp_World_Camera__FOV;
    private SerializedProperty _sp__settings_RendererSettings__UseSpriteRenderer;

    void OnEnable() {
        // Get 'DriveEngine' script component
        _driveEngine = (P3dEngine)serializedObject.targetObject;

        // Get serialized properties
        _sp_Renderer =
            serializedObject
            .FindProperty("_renderer");

        _sp_Renderer__unityDisplay_GameObject =
            _sp_Renderer
            .FindPropertyRelative("_unityDisplay")
            .FindPropertyRelative("<GameObject>k__BackingField");

        _sp_Renderer__screen_Position =
            _sp_Renderer
            .FindPropertyRelative("_screen")
            .FindPropertyRelative("<Position>k__BackingField");

        _sp_World_Camera__FOV =
            serializedObject
            .FindProperty("<World>k__BackingField")
            .FindPropertyRelative("<Camera>k__BackingField")
            .FindPropertyRelative("_FOV");

        _sp__settings_RendererSettings__UseSpriteRenderer =
            serializedObject
            .FindProperty("_settings")
            .FindPropertyRelative("<RendererSettings>k__BackingField")
            .FindPropertyRelative("_UseSpriteRenderer");

        //PrintSerializedProperties();
    }

    public override void OnInspectorGUI() {
        // Get values before change
        int previous_World_Camera__FOV = _sp_World_Camera__FOV.intValue;
        bool previous__UseSpriteRenderer = _sp__settings_RendererSettings__UseSpriteRenderer.boolValue;

        // Make all the public and serialized fields visible in Inspector
        base.OnInspectorGUI();

        // Load changed values
        serializedObject.Update();

        // Get new values
        Vector3 new__unityDisplay_GameObject_position = _sp_Renderer__unityDisplay_GameObject.objectReferenceValue != null ? ((GameObject)_sp_Renderer__unityDisplay_GameObject.objectReferenceValue).GetComponent<Transform>().position : Vector3.zero;

        // Check if values have changed
        if (previous_World_Camera__FOV != _sp_World_Camera__FOV.intValue)
        {
            _driveEngine.ApplyEditorValues(IP3dEngineEditor.EditorValuesGroup.Camera);
        }
        if (previous__UseSpriteRenderer != _sp__settings_RendererSettings__UseSpriteRenderer.boolValue)
        {
            _driveEngine.ApplyEditorValues(IP3dEngineEditor.EditorValuesGroup.RendererSettings);
        }
        if (new__unityDisplay_GameObject_position != (Vector3)_sp_Renderer__screen_Position.boxedValue)
        {
            _sp_Renderer__screen_Position.boxedValue = new__unityDisplay_GameObject_position;
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