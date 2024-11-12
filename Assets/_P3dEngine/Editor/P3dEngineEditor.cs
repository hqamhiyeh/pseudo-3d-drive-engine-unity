using Assets._P3dEngine.Interface;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(P3dEngine))]
public class P3dEngineEditor : Editor
{
    /*// Legend
     *  sp  ->          serialized property
     * rsp  -> relative serialized property
     *  spv ->          serialized property value
     * sprv -> relative serialized property value
     */

    private IP3dEngineEditor _driveEngine;

    void OnEnable()
    {
        // Get 'DriveEngine' script component
        _driveEngine = (P3dEngine)serializedObject.targetObject;

        /*// Print Serialized Properties
        //PrintSerializedPropertiesAll();
        PrintSerializedPropertiesWatched();
        */
    }

    public override void OnInspectorGUI()
    {
        Dictionary<string, object> _editorValues = new();
        SerializedProperty sp;

        // Get values before change
        sp = GetFirstVisbleSerializedProperty();
        while(sp.NextVisible(true)) /* skip 'm_Script' */
            if (IsWatchedSerializedProperty(sp))
                _editorValues.Add(sp.propertyPath, sp.boxedValue);

        // Make all the public and serialized fields visible in Inspector
        base.OnInspectorGUI();

        // Load new values
        serializedObject.Update();

        // Check if values have changed
        sp = GetFirstVisbleSerializedProperty();
        while (sp.NextVisible(true)) /* skip 'm_Script' */
            if (IsWatchedSerializedProperty(sp))
                if (_editorValues.ContainsKey(sp.propertyPath))
                    if (!sp.boxedValue.Equals(_editorValues[sp.propertyPath]))
                        _driveEngine.ApplyEditorValue(sp.propertyPath, sp.boxedValue);

        // Apply any pending property changes
        serializedObject.ApplyModifiedProperties();
    }

    private SerializedProperty GetFirstVisbleSerializedProperty()
    {
        SerializedProperty it = serializedObject.GetIterator();
        it.NextVisible(true);
        return it;
    }

    private bool IsWatchedSerializedProperty(SerializedProperty sp)
    {
        return sp.name[0] == 'm' && sp.name[1] == '_' && sp.boxedValue.GetType().IsValueType;
    }

    private void PrintSerializedPropertiesAll()
    {
        SerializedProperty it = serializedObject.GetIterator();
        while(it.Next(true))
            Debug.Log(it.name);
    }

    private void PrintSerializedPropertiesWatched()
    {
        SerializedProperty sp = GetFirstVisbleSerializedProperty();
        while(sp.NextVisible(true))
            if (IsWatchedSerializedProperty(sp))
                Debug.Log(sp.propertyPath);
    }
}