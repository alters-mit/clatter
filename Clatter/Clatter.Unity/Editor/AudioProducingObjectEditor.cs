using UnityEditor;


namespace Clatter.Unity.Editor
{
    /// <summary>
    /// Custom Inspector script for AudioProducingObject.
    /// </summary>
    [CustomEditor(typeof(AudioProducingObject))]
    public class AudioProducingObjectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            AudioProducingObject s = target as AudioProducingObject;
            // ReSharper disable once PossibleNullReferenceException
            if ( !s.autoSetSize)
            {
                s.size = EditorGUILayout.IntSlider("Size", s.size, 0, 5);
            }
            if (s.hasScrapeMaterial)
            {
                s.scrapeMaterial = (Core.ScrapeMaterial)EditorGUILayout.EnumPopup("Scrape Material", s.scrapeMaterial);
            }
            if (!s.autoSetFriction)
            {
                s.dynamicFriction = EditorGUILayout.Slider("Dynamic Friction", s.dynamicFriction, 0, 1);
                s.staticFriction = EditorGUILayout.Slider("Static Friction", s.staticFriction, 0, 1);
            }

            if (s.autoSetMass)
            {
                s.hollowness = EditorGUILayout.Slider("Hollowness", s.hollowness, 0, 1);
            }
        }
    }
}