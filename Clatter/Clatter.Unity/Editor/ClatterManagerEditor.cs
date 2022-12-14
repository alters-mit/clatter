using UnityEditor;


namespace Clatter.Unity.Editor
{
    /// <summary>
    /// Custom Inspector script for ClatterManager.
    /// </summary>
    [CustomEditor(typeof(ClatterManager))]
    public class ClatterManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            ClatterManager i = target as ClatterManager;
            // ReSharper disable once PossibleNullReferenceException
            if (!i.generateRandomSeed)
            {
                i.seed = EditorGUILayout.IntField("Random Seed", i.seed);
            }
        }
    }  
}