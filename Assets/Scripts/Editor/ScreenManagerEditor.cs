using UnityEditor;

/// <summary>
/// The custom editor for screen manager.
/// </summary>
[CustomEditor(typeof(ScreenManager))]
internal class ScreenManagerEditor : Editor {
    public override void OnInspectorGUI() {
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        if (!EditorGUI.EndChangeCheck()) return;

        var screenManager = (ScreenManager)target;
        while (screenManager.transform.childCount > 0) DestroyImmediate(screenManager.transform.GetChild(0).gameObject);
        if (screenManager.InstantiateInEditMode) screenManager.InstantiateScreens();
    }
}
