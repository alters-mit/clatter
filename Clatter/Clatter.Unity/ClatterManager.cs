using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Clatter.Core;


namespace Clatter.Unity
{
    /// <summary>
    /// Singleton manager class for Clatter in Unity. To initialize Clatter in Unity, you must add a ClatterManager to the scene, at least two GameObjects with an `ClatterObject` component, and an AudioListener.
    ///
    /// Internally, ClatterManager stores data for each `ClatterObject` in the scene and has a `Clatter.Core.AudioGenerator`. When a collision occurs between two `ClatterObject` components, the collision will be converted into a `Clatter.Core.CollisionEvent` and announced to the ClatterManager, which will in turn feed it to the AudioGenerator (see AudioGenerator.AddCollision(collisionEvent)). ClatterManager then converts the generated audio into `ImpactSound` or `ScrapeSound` components, which will play the sound and then self-destruct. 
    ///
    /// ClatterManager *can* automatically update like any other MonoBehaviour class i.e. via Update() and FixedUpdate(). It might be convenient, especially to handle potential script execution order bugs, to manually update ClatterManager instead by setting `auto == false`. Either way, ClatterManager will call ClatterObject.Initialize(id) and ClatterObject.OnFixedUpdate() for each `ClatterObject` in the scene. 
    ///
    /// In Clatter, audio is generated from both fixed values and random values; see the constructor for `Clatter.Core.Modes` and Modes.AdjustPowers(). In most cases, you'll want the audio to be truly random. If you want to replicate the exact same audio every time you run your program, uncheck "Generate Random Seed" and then enter a seed.
    ///
    /// For a minimal example scene, see the "Impact" scene in the ClatterUnityExamples project.
    ///
    /// ## Code Examples
    ///
    /// Create a Clatter scene in Unity purely from code. See the "Marbles" scene in ClatterUnityExamples.
    ///
    /// {code_example:Marbles}
    ///
    /// Create a Clatter scene that listens for collision events. This example sets auto to False and manually updates ClatterManager from a separate script. This way, we can be certain that ClatterManager.OnAwake() will be invoked *after* the listener script awakens. See "CollisionListener" in ClatterUnityExamples.
    ///
    /// {code_example:CollisionListener}
    /// 
    /// </summary>
    public class ClatterManager : MonoBehaviour
    {
        /// <summary>
        /// The optimal DSP buffer size.
        /// </summary>
        private const int DSP_BUFFER_SIZE = 256;
        
        
        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static ClatterManager instance;
        /// <summary>
        /// If true, automatically update by calling Awake(), Update(), and FixedUpdate() (like an ordinary MonoBehaviour object). If this is false, you must manually update instead by calling instance.Awake(), instance.OnUpdate(), and instance.OnFixedUpdate().
        /// </summary>
        public bool auto = true;
        /// <summary>
        /// If true, generate a new random seed.
        /// </summary>
        public bool generateRandomSeed = true;
        /// <summary>
        /// The random seed. Ignored if generateRandomSeed == true.
        /// </summary>
        public int seed;
        /// <summary>
        /// If true, adjust the global audio settings for better-quality audio.
        /// </summary>
        public bool adjustAudioSettings = true;
        /// <summary>
        /// The audio generator.
        /// </summary>
        public AudioGenerator generator;
        /// <summary>
        /// Invoked when this is destroyed.
        /// </summary>
        public Action onDestroy;
        /// <summary>
        /// The objects as an array.
        /// </summary>
        private ClatterObject[] objectsArray;
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
        private readonly Dictionary<uint, ClatterObject> objects = new Dictionary<uint, ClatterObject>();
        /// <summary>
        /// A queue of IDs of sounds that ended on this frame. We need this because we can't destroy GameObjects on the audio filter read thread.
        /// </summary>
        private readonly Queue<int> endedSounds = new Queue<int>();
        /// <summary>
        /// The next object ID.
        /// </summary>
        private uint nextId;


        /// <summary>
        /// Initialize the ClatterManager. If `auto == true`, this method is automatically called in Awake(). This method will find all `ClatterObject` components in the scene and initialize them; see: ClatterObject.Initialize(id). It will also initialize the internal AudioGenerator. 
        /// </summary>
        public void OnAwake()
        {
            // Set the audio for best quality.
            if (adjustAudioSettings)
            {
                AudioConfiguration audioConfiguration = AudioSettings.GetConfiguration();
                audioConfiguration.sampleRate = Globals.framerateInt;
                audioConfiguration.dspBufferSize = DSP_BUFFER_SIZE;
                AudioSettings.Reset(audioConfiguration);
            }
            // Load the default materials.
            ImpactMaterialData.Load(ClatterObject.defaultObjectData.impactMaterial);
            if (ClatterObject.defaultObjectData.hasScrapeMaterial)
            {
                ScrapeMaterialData.Load(ClatterObject.defaultObjectData.scrapeMaterial);
            }
            // Set the singleton instance.
            instance = this;
            // Find all of the audio-producing objects.
            objectsArray = FindObjectsOfType<ClatterObject>();
            IEnumerable<ClatterObjectData> objectData = objectsArray.Select(o => o.data);
            objects.Clear();
            // Add all of the objects.
            nextId = 0;
            foreach (ClatterObject o in objectsArray)
            {
                // Initialize the audio object data with a unique ID.
                o.Initialize(nextId);
                nextId++;
                // Remember this object.
                objects.Add(o.data.id, o);
                o.onDestroy.AddListener(RemoveObject);
            }
            // End ongoing audio.
            EndAllAudio();
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
            // Remove additional state information.
            endedSounds.Clear();
            sounds.Clear();
            scrapeSounds.Clear();
        }


        /// <summary>
        /// Call this once per frame to update ClatterManager. If `auto == true`, this method is automatically called in Update(). This method updates the internal `Clatter.Core.AudioGenerator` (see AudioGenerator.Update()) as well as each `ClatterObject` the scene; see ClatterObject.OnUpdate().
        /// </summary>
        public void OnUpdate()
        {
            // Clear the collision data.
            for (int i = 0; i < objectsArray.Length; i++)
            {
                objectsArray[i].OnUpdate();
            }
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
        /// Call this once per physics frame to update ClatterManager. If `auto == true`, this method is automatically called in FixedUpdate(). This method updates each `ClatterObject` the scene; see: ClatterObject.OnFixedUpdate().
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
            objectsArray = objectsArray.Where(o => o.data.id != id).ToArray();
        }


        /// <summary>
        /// Generate impact audio.
        /// </summary>
        /// <param name="collisionEvent">The collision event that generated the audio.</param>
        /// <param name="samples">The audio samples.</param>
        /// <param name="position">The position of the audio source.</param>
        /// <param name="audioSourceId">The ID of the audio source.</param>
        private void OnImpact(CollisionEvent collisionEvent, Samples samples, Vector3d position, int audioSourceId)
        {
            // Do the impact.
            OnAudioStart<ImpactSound>(samples, position, audioSourceId);
        }


        /// <summary>
        /// Start to generate scrape audio.
        /// </summary>
        /// <param name="collisionEvent">The collision event that generated the audio.</param>
        /// <param name="samples">The audio samples.</param>
        /// <param name="position">The position of the audio source.</param>
        /// <param name="audioSourceId">The ID of the audio source.</param>
        private void OnScrapeStart(CollisionEvent collisionEvent, Samples samples, Vector3d position, int audioSourceId)
        {
            // Start a scrape sound.
            ScrapeSound sound = OnAudioStart<ScrapeSound>(samples, position, audioSourceId);
            // Remember the scrape sound.
            scrapeSounds.Add(audioSourceId, sound);
        }
        
        
        /// <summary>
        /// Update scrape audio.
        /// </summary>
        /// <param name="collisionEvent">The collision event that generated the audio.</param>
        /// <param name="samples">The audio samples.</param>
        /// <param name="position">The position of the audio source.</param>
        /// <param name="audioSourceId">The ID of the audio source.</param>
        private void OnScrapeOngoing(CollisionEvent collisionEvent, Samples samples, Vector3d position, int audioSourceId)
        {
            // Continue.
            if (scrapeSounds.ContainsKey(audioSourceId))
            {
                scrapeSounds[audioSourceId].UpdateAudio(samples, position);          
            }
            // Restart.
            else
            {
                OnScrapeStart(collisionEvent, samples, position, audioSourceId);
            }
        }


        /// <summary>
        /// End scrape audio.
        /// </summary>
        /// <param name="audioSourceId">The ID of the audio source.</param>
        private void OnScrapeEnd(int audioSourceId)
        {
            if (scrapeSounds.ContainsKey(audioSourceId))
            {
                scrapeSounds[audioSourceId].End();   
            }
        }


        /// <summary>
        /// Start a new sound.
        /// </summary>
        /// <param name="samples">The audio samples.</param>
        /// <param name="position">The position of the audio source.</param>
        /// <param name="audioSourceId">The ID of the audio source.</param>
        private T OnAudioStart<T>(Samples samples, Vector3d position, int audioSourceId)
            where T : Sound
        {
            T sound = Sound.Create<T>(samples, position, audioSourceId);
            // Remember the audio source.
            sounds.Add(audioSourceId, sound);
            // Remember to destroy the audio source.
            sound.onEnd += OnSoundEnd;
            return sound;
        }


        /// <summary>
        /// End any ongoing audio.
        /// </summary>
        private void EndAllAudio()
        {
            // Stop the generator.
            if (generator != null)
            {
                generator.End();       
            }
            // Destroy all remaining audio sources.
            foreach (int id in sounds.Keys)
            {
                if (sounds[id] != null)
                {
                    sounds[id].End();
                    Destroy(sounds[id].gameObject);             
                }
            }
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
            EndAllAudio();
            // Destroy all remaining audio-producing objects.
            foreach (uint id in objects.Keys)
            {
                if (objects[id] != null && objects[id].gameObject != null)
                {
                    Destroy(objects[id].gameObject);
                }
            }
            onDestroy?.Invoke();
        }
    }
}