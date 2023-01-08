using System.Collections;
using UnityEngine;
using Clatter.Core;
using Clatter.Unity;


/// <summary>
/// Manually create a scrape sound within a Unity context.
/// </summary>
public class ScrapeGenerator : MonoBehaviour
{
    private ScrapeSound sound;


    private void Awake()
    {
        AudioObjectData primary = new AudioObjectData(0, ImpactMaterial.glass_1, 0.2, 0.1, 0.5);
        AudioObjectData secondary = new AudioObjectData(1, ImpactMaterial.stone_4, 0.2, 0.2, 100, ScrapeMaterial.ceramic);
        StartCoroutine(Generate(primary, secondary, 1, 0.1, 10, 0));
    }


    private IEnumerator Generate(AudioObjectData primary, AudioObjectData secondary, double speed, double deceleration, double duration, int seed)
    {
        // Create a genearator.
        AudioGenerator generator = new AudioGenerator(new AudioObjectData[] { primary, secondary }, seed);
        // Listen to the start of the scrape.
        generator.onScrapeStart += OnScrapeStart;
        // Listen to the ongoing scrape.
        generator.onScrapeOngoing += OnScrapeOngoing;
        // Get the number of scrape events.
        int count = Scrape.GetNumScrapeEvents(duration);
        Vector3d position = Vector3d.Zero;
        // Generate the scrape sound.
        for (int i = 0; i < count; i++)
        {
            // Add a collision to the AudioGenerator and update.
            generator.AddCollision(new CollisionEvent(primary, secondary, AudioEventType.scrape, speed, position));
            generator.Update();
            // Update the speed.
            speed -= deceleration;
            // Update the position.
            position.Z += speed;
            yield return new WaitForEndOfFrame();
        }
        // Gracefully end.
        generator.End();
        sound.End();

#if UNITY_EDITOR

        UnityEditor.EditorApplication.isPlaying = false;

#else

        Application.Quit();

#endif

    }


    private void OnScrapeStart(Samples samples, Vector3d position, int audioSourceId)
    {
        // Create the scrape sound.
        sound = Sound.Create<ScrapeSound>(samples, position, audioSourceId);
    }



    private void OnScrapeOngoing(Samples samples, Vector3d position, int audioSourceId)
    {
        sound.UpdateAudio(samples, position);
    }
}