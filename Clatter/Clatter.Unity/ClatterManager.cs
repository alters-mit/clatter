using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Clatter.Core;


namespace Clatter.Unity
{
    /// <summary>
    /// Singleton manager class for Clatter in Unity. To initialize Clatter in Unity, you must add a ClatterManager to the scene plus at least two GameObjects with an `AudioProducingObject` component.
    ///
    /// Internally, ClatterManager stores data for each `AudioProducingObject` in the scene and has a `Clatter.Core.AudioGenerator`. When a collision occurs between two `AudioProducingObject` components, the collision will be converted into a `Clatter.Core.CollisionEvent` and announced to the ClatterManager, which will in turn feed it to the AudioGenerator (see AudioGenerator.AddCollision(collisionEvent)). ClatterManager then converts the generated audio into `ImpactSound` or `ScrapeSound` components, which will play the sound and then self-destruct. 
    ///
    /// ClatterManager *can* automatically update like any other MonoBehaviour class i.e. via Update() and FixedUpdate(). It might be convenient, especially to handle potential script execution order bugs, to manually update ClatterManager instead by setting `auto == false`. 
    ///
    /// In Clatter, audio is generated from both fixed values and random values; see the constructor for `Clatter.Core.Modes` and Modes.AdjustPowers(). In most cases, you'll want the audio to be truly random. If you want to replicate the exact same audio every time you run your program, uncheck "Generate Random Seed" and then enter a seed.
    ///
    /// The following example illustrates how to create a Clatter scene in Unity purely from code. If you're planning on adding `AudioProducingObject`s and a ClatterManager to the scene via the Unity Editor, most of this code isn't needed. Likewise, we've set `auto == false` in this code, which doesn't really do anything in this example other than showcase how to manually update a ClatterManager.
    ///
    /// {code_example:Marbles}
    /// 
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
        /// If true, automatically update by calling Awake(), Update(), and FixedUpdate() (like an ordinary MonoBehaviour object). If this is false, you must manually update instead by calling instance.Awake(), instance.OnUpdate(), and instance.OnFixedUpdate().
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
        /// Initialize the ClatterManager. If `auto == true`, this method is automatically called in Awake(). This method will find all `AudioProducingObject` components in the scene and initialize them; see: AudioProducingObject.Initialize(id). It will also initialize the internal AudioGenerator. 
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
                // Initialize the audio object data with a unique ID.
                o.Initialize(nextID);
                nextID++;
                // Remember this object.
                objects.Add(o.data.id, o);
                o.onDestroy.AddListener(RemoveObject);
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
        /// Call this once per frame to update ClatterManager. If `auto == true`, this method is automatically called in Update(). This method updates the internal `Clatter.Core.AudioGenerator` (see AudioGenerator.Update()) as well as each `AudioProducingObject` the scene (see AudioProducingObject.OnUpdate()).
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
        /// Call this once per physics frame to update ClatterManager. If `auto == true`, this method is automatically called in FixedUpdate(). This method updates each `AudioProducingObject` the scene (see AudioProducingObject.OnFixedUpdate()).
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