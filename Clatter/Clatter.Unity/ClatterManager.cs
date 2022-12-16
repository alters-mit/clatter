using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Clatter.Core;


namespace Clatter.Unity
{
    /// <summary>
    /// Singleton manager class for Clatter in Unity. Add this to a scene to automatically handle an Clatter simulation.
    /// </summary>
    public class ClatterManager : MonoBehaviour
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static ClatterManager instance;
        /// <summary>
        /// If true, generate a new random seed.
        /// </summary>
        public bool generateRandomSeed = true;
        /// <summary>
        /// If true, automatically update by calling Update() and FixedUpdate(). To manually update instead, call instance.OnUpdate() and instance.OnFixedUpdate().
        /// </summary>
        public bool auto;
        /// <summary>
        /// The random seed. Ignored if generateRandomSeed == true.
        /// </summary>
        [HideInInspector]
        public int seed;
        /// <summary>
        /// The audio generator.
        /// </summary>
        public AudioGenerator generator;
        /// <summary>
        /// The objects as an array.
        /// </summary>
        private AudioProducingObject[] objectsArray;
        /// <summary>
        /// A dictionary of ongoing sounds.
        /// </summary>
        private readonly Dictionary<int, Sound> sounds = new Dictionary<int, Sound>();
        /// <summary>
        /// A dictionary of scrape sounds.
        /// </summary>
        private readonly Dictionary<int, ScrapeSound> scrapeSounds = new Dictionary<int, ScrapeSound>();
        /// <summary>
        /// A dictionary of objects that can generate audio.
        /// </summary>
        private readonly Dictionary<uint, AudioProducingObject> objects = new Dictionary<uint, AudioProducingObject>();
        /// <summary>
        /// A queue of IDs of sounds that ended on this frame. We need this because we can't destroy GameObjects on the audio filter read thread.
        /// </summary>
        private readonly Queue<int> endedSounds = new Queue<int>();
        /// <summary>
        /// The next object ID.
        /// </summary>
        private uint nextID;


        /// <summary>
        /// Call this to manually awaken the manager.
        /// </summary>
        public void OnAwake()
        {
            // Set the singleton instance.
            instance = this;
            // Find all of the audio-producing objects.
            objectsArray = FindObjectsOfType<AudioProducingObject>();
            IEnumerable<AudioObjectData> objectData = objectsArray.Select(o => o.data);
            // Add all of the objects.
            foreach (AudioProducingObject o in objectsArray)
            {
                // Add the object.
                AddAudioObject(o);
            }
            // Set the random number generator.
            if (generateRandomSeed)
            {
                generator = new AudioGenerator(objectData);
            }
            else
            {
                generator = new AudioGenerator(objectData, seed);
            }
            // Subscribe to audio-generating events.
            generator.onImpact += OnImpact;
            generator.onScrapeStart += OnScrapeStart;
            generator.onScrapeOngoing += OnScrapeOngoing;
            generator.onScrapeEnd += OnScrapeEnd;
        }


        /// <summary>
        /// Call this to manually update.
        /// </summary>
        public void OnUpdate()
        {
            generator.Update();
            // Destroy sounds that have ended.
            int id;
            while (endedSounds.Count > 0)
            {
                id = endedSounds.Dequeue();
                if (!sounds.ContainsKey(id))
                {
                    return;
                }
                Destroy(sounds[id].gameObject);
                sounds.Remove(id);
                // Try to remove a scrape.
                if (!scrapeSounds.ContainsKey(id))
                {
                    return;
                }
                scrapeSounds.Remove(id);
            }
        }


        /// <summary>
        /// Call this to manually update.
        /// </summary>
        public void OnFixedUpdate()
        {
            // Update each object.
            for (int i = 0; i < objectsArray.Length; i++)
            {
                objectsArray[i].OnFixedUpdate();
            }
        }


        /// <summary>
        /// Register a new audio object.
        /// </summary>
        /// <param name="o">The audio object.</param>
        public void AddAudioObject(AudioProducingObject o)
        {
            // Set the data with a unique ID.
            o.Initialize(nextID);
            nextID++;
            // Remember this object.
            objects.Add(o.data.id, o);
            o.onDestroy += RemoveObject;
        }


        /// <summary>
        /// Mark a sound as ended.
        /// </summary>
        /// <param name="id">The audio source ID.</param>
        private void OnSoundEnd(int id)
        {
            endedSounds.Enqueue(id);
        }


        /// <summary>
        /// Remove an audio-producing object when it is destroyed.
        /// </summary>
        /// <param name="id">The object ID.</param>
        private void RemoveObject(uint id)
        {
            objects.Remove(id);
        }


        /// <summary>
        /// Generate impact audio.
        /// </summary>
        /// <param name="samples">The audio samples.</param>
        /// <param name="position">The position of the audio source.</param>
        /// <param name="audioSourceID">The ID of the audio source.</param>
        private void OnImpact(Samples samples, Vector3d position, int audioSourceID)
        {
            // Do the impact.
            OnAudioStart<ImpactSound>(samples, position, audioSourceID);
        }


        /// <summary>
        /// Start to generate scrape audio.
        /// </summary>
        /// <param name="samples">The audio samples.</param>
        /// <param name="position">The position of the audio source.</param>
        /// <param name="audioSourceID">The ID of the audio source.</param>
        private void OnScrapeStart(Samples samples, Vector3d position, int audioSourceID)
        {
            // Start a scrape sound.
            ScrapeSound sound = OnAudioStart<ScrapeSound>(samples, position, audioSourceID);
            // Remember the scrape sound.
            scrapeSounds.Add(audioSourceID, sound);
        }


        /// <summary>
        /// Start a new sound.
        /// </summary>
        /// <param name="samples">The audio samples.</param>
        /// <param name="position">The position of the audio source.</param>
        /// <param name="audioSourceID">The ID of the audio source.</param>
        private T OnAudioStart<T>(Samples samples, Vector3d position, int audioSourceID)
            where T : Sound
        {
            T sound = Sound.Create<T>(samples, position, audioSourceID);
            // Remember the audio source.
            sounds.Add(audioSourceID, sound);
            // Remember to destroy the audio source.
            sound.onEnd += OnSoundEnd;
            return sound;
        }


        /// <summary>
        /// Update scrape audio.
        /// </summary>
        /// <param name="samples">The audio samples.</param>
        /// <param name="position">The position of the audio source.</param>
        /// <param name="audioSourceID">The ID of the audio source.</param>
        private void OnScrapeOngoing(Samples samples, Vector3d position, int audioSourceID)
        {
            scrapeSounds[audioSourceID].UpdateAudio(samples, position);
        }


        /// <summary>
        /// End scrape audio.
        /// </summary>
        /// <param name="audioSourceID">The ID of the audio source.</param>
        private void OnScrapeEnd(int audioSourceID)
        {
            scrapeSounds[audioSourceID].End();
        }


        private void Awake()
        {
            if (auto)
            {
                OnAwake();
            }
        }


        private void Update()
        {
            if (auto)
            {
                OnUpdate();
            }
        }


        private void FixedUpdate()
        {
            if (auto)
            {
                OnFixedUpdate();
            }
        }


        private void OnDestroy()
        {
            // Stop the generator.
            generator.End();
            // Destroy all remaining audio sources.
            foreach (int id in sounds.Keys)
            {
                sounds[id].End();
            }
            // Destroy all remaining audio-producing objects.
            foreach (uint id in objects.Keys)
            {
                if (objects[id] != null && objects[id].gameObject != null)
                {
                    Destroy(objects[id].gameObject);
                }
            }
        }
    }
}