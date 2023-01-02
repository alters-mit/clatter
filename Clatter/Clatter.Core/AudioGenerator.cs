using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;


namespace Clatter.Core
{
    /// <summary>
    /// An AudioGenerator can generate audio within a dynamic physics simulation.
    ///
    /// AudioGenerator is not *required* for audio generation but is usually your best option, for two reasons:
    ///
    /// 1. AudioGenerator automatically converts `CollisionEvent` data into audio data.
    /// 2. AudioGenerator is multi-threaded; concurrent collisions generate audio on separate threads.
    ///
    /// AudioGenerator is structured like a UnityEngine MonoBehaviour object but it isn't a subclass of MonoBehaviour and won't update like one; Update() needs to be called manually.
    ///
    /// This is a minimal example of how to process multiple concurrent collisions with an AudioGenerator and, using `WavWriter`, write .wav files. Note that we're making a few implausible assumptions:
    ///
    /// - All of the objects are randomly generated. In a real simulation, you'll probably want more control over the objects' audio values.
    /// - All of the collisions have a centroid of (0, 0, 0). In a real simulation, the collisions should probably be spatialized.
    /// - All of the collisions events are impacts. In a real simulation, we could add a `ScrapeMaterial` to a "floor" object to start generating scrape audio.
    ///
    /// {code_example:AudioObjectDataConstructorScrapeMaterial}
    /// 
    /// </summary>
    public class AudioGenerator
    {
        /// <summary>
        /// Invoked when audio samples are generated.
        /// </summary>
        /// <param name="samples">The audio samples.</param>
        /// <param name="centroid">The position of the audio source.</param>
        /// <param name="audioSourceId">The audio source ID.</param>
        public delegate void AudioEvent(Samples samples, Vector3d centroid, int audioSourceId);


        /// <summary>
        /// The default maximum number of events.
        /// </summary>
        private const int DEFAULT_MAX_NUM_EVENTS = 200;

        
        /// <summary>
        /// Invoked when impact audio is generated.
        /// </summary>
        public AudioEvent onImpact;
        /// <summary>
        /// Invoked when audio is generated for a new scrape event.
        /// </summary>
        public AudioEvent onScrapeStart;
        /// <summary>
        /// Invoked when audio is generated for an ongoing scrape event.
        /// </summary>
        public AudioEvent onScrapeOngoing;
        /// <summary>
        /// Invoked when a scrape ends.
        /// </summary>
        public Action<int> onScrapeEnd;
        /// <summary>
        /// The random number generator.
        /// </summary>
        private readonly Random rng;
        /// <summary>
        /// An array of collision events. See numEvents for the actual number of events..
        /// </summary>
        private CollisionEvent[] collisionEvents = new CollisionEvent[DEFAULT_MAX_NUM_EVENTS];
        /// <summary>
        /// A cached array of threads for audio events.
        /// </summary>
        private Thread[] audioThreads = new Thread[DEFAULT_MAX_NUM_EVENTS];
        /// <summary>
        /// Booleans indicating whether the threads are alive.
        /// </summary>
        private bool[] threadDeaths = new bool[DEFAULT_MAX_NUM_EVENTS];
        /// <summary>
        /// An array of falses used to clear threadDeaths.
        /// </summary>
        private bool[] falses = new bool[DEFAULT_MAX_NUM_EVENTS];
        /// <summary>
        /// A dictionary of ongoing impacts. Key = Collision object ID pairs. Value = An impact event.
        /// </summary>
        private readonly Dictionary<ulong, Impact> impacts = new Dictionary<ulong, Impact>();
        /// <summary>
        /// A dictionary of ongoing scrapes. Key = Collision object ID pairs. Value = A scrape event.
        /// </summary>
        private readonly Dictionary<ulong, Scrape> scrapes = new Dictionary<ulong, Scrape>();
        /// <summary>
        /// The number of events on this frame.
        /// </summary>
        private int numEvents;
        /// <summary>
        /// If true, the application has quit.
        /// </summary>
        private bool destroyed;


        /// <summary>
        /// (constructor)
        /// </summary>
        /// <param name="audioObjects">The objects that will generate audio.</param>
        /// <param name="seed">The random seed. If null, the seed is random.</param>
        public AudioGenerator(IEnumerable<AudioObjectData> audioObjects, int? seed = null)
        {
            if (seed == null)
            {
                rng = new Random();
            }
            else
            {
                rng = new Random((int)seed);
            }
            // Load the audio materials.
            foreach (AudioObjectData a in audioObjects)
            {
                // Load the impact material.
                ImpactMaterialData.Load(a.impactMaterial);
                // Load the scrape material.
                if (a.hasScrapeMaterial)
                {
                    ScrapeMaterialData.Load(a.scrapeMaterial);
                }
            }
        }


        /// <summary>
        /// Call this once per frame to process collision events and generate audio.
        /// </summary>
        public void Update()
        {
            // Iterate through all valid events.
            for (int i = 0; i < numEvents; i++)
            {
                // Get a thread-safe index.
                int index = i;
                // Generate impact audio.
                if (collisionEvents[index].type == AudioEventType.impact)
                {
                    // Set the previous area.
                    collisionEvents[index].primary.hasPreviousArea = true;
                    collisionEvents[index].primary.previousArea = collisionEvents[index].area;
                    // Start a new impact.
                    if (!impacts.ContainsKey(collisionEvents[index].ids))
                    {
                        impacts.Add(collisionEvents[index].ids,
                            new Impact(collisionEvents[index].primary, collisionEvents[index].secondary, rng));
                    }
                    // Start an impact audio thread.
                    audioThreads[index] = new Thread(() => GetAudio(index, impacts));
                    audioThreads[index].Start();
                }
                // Generate scrape audio.
                else if (collisionEvents[index].type == AudioEventType.scrape && collisionEvents[index].secondary.hasScrapeMaterial)
                {
                    // Set the previous area.
                    collisionEvents[index].primary.hasPreviousArea = true;
                    collisionEvents[index].primary.previousArea = collisionEvents[index].area;
                    // Start a new scrape.
                    if (!scrapes.ContainsKey(collisionEvents[index].ids))
                    {
                        scrapes.Add(collisionEvents[index].ids,
                            new Scrape(collisionEvents[index].secondary.scrapeMaterial, collisionEvents[index].primary, collisionEvents[index].secondary, rng));
                    }
                    // Start an impact audio thread.
                    audioThreads[index] = new Thread(() => GetAudio(index, scrapes));
                    audioThreads[index].Start();
                }
                else if (collisionEvents[index].type == AudioEventType.none)
                {
                    collisionEvents[index].primary.hasPreviousArea = false;
                }
            }
            // Wait for the threads to finish.
            bool threadsDone = false;
            while (!threadsDone)
            {
                threadsDone = true;
                // Iterate through each thread.
                for (int i = 0; i < numEvents; i++)
                {
                    // Ignore null threads.
                    if (audioThreads[i] == null)
                    {
                        continue;
                    }
                    // Kill the thread.
                    if (destroyed)
                    {
                        audioThreads[i].Join();
                        threadDeaths[i] = true;
                    }
                    // This thread is dead.
                    else if (threadDeaths[i])
                    {
                        // Remove the thread.
                        audioThreads[i] = null;
                        // Announce impact audio.
                        if (collisionEvents[i].type == AudioEventType.impact && impacts[collisionEvents[i].ids].state != EventState.end)
                        {
                            // Play impact samples at the centroid using a new random audio source ID.
                            onImpact?.Invoke(impacts[collisionEvents[i].ids].samples, collisionEvents[i].centroid, rng.Next());
                        }
                        // Announce scrape audio.
                        else if (collisionEvents[i].type == AudioEventType.scrape)
                        {
                            // Start a new scrape.
                            if (scrapes[collisionEvents[i].ids].state == EventState.start)
                            {
                                onScrapeStart?.Invoke(scrapes[collisionEvents[i].ids].samples, collisionEvents[i].centroid, scrapes[collisionEvents[i].ids].audioSourceId);
                            }
                            // Continue an ongoing scrape.
                            else if (scrapes[collisionEvents[i].ids].state == EventState.ongoing)
                            {
                                onScrapeOngoing?.Invoke(scrapes[collisionEvents[i].ids].samples, collisionEvents[i].centroid, scrapes[collisionEvents[i].ids].audioSourceId);
                            }
                        }
                    }
                    // This thread is alive.
                    else
                    {
                        threadsDone = false;
                    }
                }
            }
            // Remove any impact events that have ended.
            ulong[] impactKeys = impacts.Keys.ToArray();
            for (int i = 0; i < impactKeys.Length; i++)
            {
                if (impacts[impactKeys[i]].state == EventState.end)
                {
                    impacts.Remove(impactKeys[i]);
                }
            }
            // Update or remove scrape events.
            ulong[] scrapeKeys = scrapes.Keys.ToArray();
            for (int i = 0; i < scrapeKeys.Length; i++)
            {
                // Mark the scrape as ongoing.
                if (scrapes[scrapeKeys[i]].state == EventState.start)
                {
                    scrapes[scrapeKeys[i]].state = EventState.ongoing;
                }
                // Mark the scrape as ended.
                else if (scrapes[scrapeKeys[i]].state == EventState.end)
                {
                    // Announce that the scrape has ended.
                    onScrapeEnd?.Invoke(scrapes[scrapeKeys[i]].audioSourceId);
                    // Remove the event.
                    scrapes.Remove(scrapeKeys[i]);
                }
            }
            // Reset the collision events index for the next frame.
            numEvents = 0;
            // Reset the thread life states.
            Buffer.BlockCopy(falses, 0, threadDeaths, 0, falses.Length);
        }


        /// <summary>
        /// Register a new collision audio event.
        /// </summary>
        /// <param name="collisionEvent">The event.</param>
        public void AddCollision(CollisionEvent collisionEvent)
        {
            // Resize the arrays if needed.
            if (numEvents >= collisionEvents.Length)
            {
                Array.Resize(ref collisionEvents, numEvents * 2);
                Array.Resize(ref audioThreads, numEvents * 2);
                Array.Resize(ref threadDeaths, numEvents * 2);
                Array.Resize(ref falses, numEvents * 2);
            }
            // Add the event.
            collisionEvents[numEvents] = collisionEvent;
            // Increment the total number of events on this frame.
            numEvents++;
        }


        /// <summary>
        /// Call this to announce to kill lingering threads.
        /// </summary>
        public void End()
        {
            destroyed = true;
        }


        /// <summary>
        /// Generate audio.
        /// </summary>
        /// <param name="collisionIndex">The index of the collision event.</param>
        /// <param name="audioEvents">The audio events.</param>
        private void GetAudio<T>(int collisionIndex, Dictionary<ulong, T> audioEvents)
            where T : Core.AudioEvent
        {
            // Try to generate audio.
            try
            {
                if (!audioEvents[collisionEvents[collisionIndex].ids].GetAudio(collisionEvents[collisionIndex].speed, rng))
                {
                    audioEvents[collisionEvents[collisionIndex].ids].state = EventState.end;
                }
            }
            // Mark this thread as done.
            finally
            {
                threadDeaths[collisionIndex] = true;
            }
        }
    }
}
