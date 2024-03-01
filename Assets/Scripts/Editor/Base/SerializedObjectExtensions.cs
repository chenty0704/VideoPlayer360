using UnityEditor;

public static class SerializedObjectExtensions {
    /// <summary>
    /// Finds a serialized property for the backing field of a C# property.
    /// </summary>
    /// <param name="obj">A serialized object.</param>
    /// <param name="path">The path to a C# property.</param>
    /// <returns>The serialized property for the backing field of the C# property.</returns>
    public static SerializedProperty FindBackingFieldProperty(this SerializedObject obj, string path) {
        return obj.FindProperty($"<{path}>k__BackingField");
    }
}
