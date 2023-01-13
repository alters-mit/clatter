using UnityEditor;
using Clatter.Core;


namespace Clatter.Unity.Editor
{
    /// <summary>
    /// Custom Inspector script for AudioProducingObject.
    /// </summary>
    [CustomEditor(typeof(AudioProducingObject))]
    public class AudioProducingObjectEditor : UnityEditor.Editor
    {
        /// <summary>
        /// OnInspectorGUI().
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            AudioProducingObject s = target as AudioProducingObject;
            // Show the audio materials.
            EditorGUILayout.LabelField("Audio Materials", EditorStyles.boldLabel);
            // ReSharper disable once PossibleNullReferenceException
            s.impactMaterial = (ImpactMaterialUnsized)EditorGUILayout.EnumPopup("Impact Material", s.impactMaterial);
            s.autoSetSize = EditorGUILayout.Toggle("Auto-Set Size", s.autoSetSize);
            if ( !s.autoSetSize)
            {
                s.size = EditorGUILayout.IntSlider("Size", s.size, 0, 5);
            }
            s.hasScrapeMaterial = EditorGUILayout.Toggle("Has Scrape Material", s.hasScrapeMaterial);
            if (s.hasScrapeMaterial)
            {
                s.scrapeMaterial = (ScrapeMaterial)EditorGUILayout.EnumPopup("Scrape Material", s.scrapeMaterial);
            }
            // Show the audio values.
            EditorGUILayout.LabelField("Audio Values", EditorStyles.boldLabel);
            s.amp = EditorGUILayout.Slider("Amp", (float)s.amp, 0f, 1f);
            s.resonance = EditorGUILayout.Slider("Resonance", (float)s.resonance, 0f, 1f);
            // Show the physics values.
            EditorGUILayout.LabelField("Physics Values", EditorStyles.boldLabel);
            s.autoSetFriction = EditorGUILayout.Toggle("Auto-Set Friction", s.autoSetFriction);
            if (!s.autoSetFriction)
            {
                s.dynamicFriction = EditorGUILayout.Slider("Dynamic Friction", s.dynamicFriction, 0, 1);
                s.staticFriction = EditorGUILayout.Slider("Static Friction", s.staticFriction, 0, 1);
            }
            s.bounciness = EditorGUILayout.Slider("Bounciness", s.bounciness, 0, 1);
            s.massMode = (MassMode)EditorGUILayout.EnumPopup("Mass Mode", s.massMode);
            if (s.massMode == MassMode.fake_mass)
            {
                s.fakeMass = EditorGUILayout.DoubleField("Fake Mass", s.fakeMass);
            }
            else if (s.massMode == MassMode.volume)
            {
                s.hollowness = EditorGUILayout.Slider("Hollowness", s.hollowness, 0, 1);
            }
            // Show the events.
            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
            SerializedProperty onDestroy = serializedObject.FindProperty("onDestroy");
            EditorGUILayout.PropertyField(onDestroy, true);
            serializedObject.ApplyModifiedProperties();
        }
    }
}