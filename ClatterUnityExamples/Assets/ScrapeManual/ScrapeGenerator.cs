using System.Collections;
using UnityEngine;
using Clatter.Core;
using Clatter.Unity;


/// <summary>
/// Manually create a scrape sound within a Unity context.
/// </summary>
public class ScrapeGenerator : MonoBehaviour
{
    /// <summary>
    /// The current speed.
    /// </summary>
    public double speed = 1;
    /// <summary>
    /// Decelerate by this value per physics frame.
    /// </summary>
    public double deceleration = 0.01;
    /// <summary>
    /// The duration of the scrape in seconds. The actually duration will be shorter due to deceleration.
    /// </summary>
    public double duration = 10;
    /// <summary>
    /// The number of scrapes we'll generate.
    /// </summary>
    private int totalNumScrapes;
    /// <summary>
    /// The current scrape count.
    /// </summary>
    private int scrapeCount;
    /// <summary>
    /// The start position.
    /// </summary>
    private Vector3d position;
    /// <summary>
    /// The primary (moving) object.
    /// </summary>
    private AudioObjectData primary;
    /// <summary>
    /// The secondary (non-moving scrape surface) object.
    /// </summary>
    private AudioObjectData secondary;
    /// <summary>
    /// The audio generator.
    /// </summary>
    private AudioGenerator generator;
    /// <summary>
    /// The scrape sound.
    /// </summary>
    private ScrapeSound sound;


    private void Awake()
    {
        // Instantiate the objects and the audio generator.
        primary = new AudioObjectData(0, ImpactMaterial.glass_1, 0.2, 0.1, 0.5);
        secondary = new AudioObjectData(1, ImpactMaterial.stone_4, 0.2, 0.2, 100, ScrapeMaterial.ceramic);
        generator = new AudioGenerator(new AudioObjectData[] { primary, secondary });
        // Listen to the scrape events.
        generator.onScrapeStart += OnScrapeStart;
        generator.onScrapeOngoing += OnScrapeOngoing;
        generator.onScrapeEnd += OnScrapeEnd;
        // Get the number of scrape events.
        totalNumScrapes = Scrape.GetNumScrapeEvents(duration);
        position = Vector3d.Zero;
    }


    private void Update()
    {
        // Generate audio.
        if (scrapeCount < totalNumScrapes)
        {
            // Add a collision to the AudioGenerator and update.
            generator.AddCollision(new CollisionEvent(primary, secondary, AudioEventType.scrape, speed, position));
            generator.Update();
            // Update the speed.
            speed -= deceleration;
            // Update the position.
            position.Z += speed;
            // Increment the scrape counter.
            scrapeCount++;

        }
        // Quit.
        else
        {
            generator.End();
            sound.End();

#if UNITY_EDITOR

            UnityEditor.EditorApplication.isPlaying = false;

#else

            Application.Quit();

#endif

        }
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


    private void OnScrapeEnd(int audioSourceId)
    {
        sound.End();
    }
}