using UnityEditor;


namespace Clatter.Unity.Editor
{
    /// <summary>
    /// Custom Inspector script for ClatterManager.
    /// </summary>
    [CustomEditor(typeof(ClatterManager))]
    public class ClatterManagerEditor : UnityEditor.Editor
    {
        /// <summary>
        /// OnInspectorGUI().
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            ClatterManager c = target as ClatterManager;
            // ReSharper disable once PossibleNullReferenceException
            c.generateRandomSeed = EditorGUILayout.Toggle("Generate Random Seed", c.generateRandomSeed);
            // ReSharper disable once PossibleNullReferenceException
            if (!c.generateRandomSeed)
            {
                c.seed = EditorGUILayout.IntField("Random Seed", c.seed);
            }
            c.auto = EditorGUILayout.Toggle("Auto-Simulate", c.auto);
            c.adjustAudioSettings = EditorGUILayout.Toggle("Auto-adjust audio settings", c.adjustAudioSettings);
        }
    }  
}