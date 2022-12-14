﻿using System;
using System.Collections;
using UnityEngine;
using Clatter.Core;


namespace Clatter.Unity
{
    /// <summary>
    /// Abstract base class for playing audio generated by collision events.
    /// </summary>
    public abstract class Sound : MonoBehaviour
    {
        /// <summary>
        /// Invoked when the audio ends.
        /// </summary>
        public Action<int> onEnd;
        /// <summary>
        /// The audio source.
        /// </summary>
        private AudioSource source;
        /// <summary>
        /// The samples.
        /// </summary>
        private Samples samples;
        /// <summary>
        /// If true, the audio source is playing.
        /// </summary>
        private bool playing = true;
        /// <summary>
        /// The unique ID of this audio source.
        /// </summary>
        private int id;
        /// <summary>
        /// If true, we are writing audio data to disk.
        /// </summary>
        private bool writing;
        /// <summary>
        /// The wav writer.
        /// </summary>
        private WavWriter writer;
        /// <summary>
        /// The coroutine used to listen for when audio clips are done playing.
        /// </summary>
        private IEnumerator playAudio;
        /// <summary>
        /// A cached audio clip.
        /// </summary>
        private AudioClip clip;


        /// <summary>
        /// Create a new sound.
        /// </summary>
        /// <param name="samples">The audio samples.</param>
        /// <param name="position">The position of the audio source.</param>
        /// <param name="id">The audio source ID.</param>
        public static T Create<T>(Samples samples, Vector3d position, int id)
            where T : Sound
        {
            // Create the object.
            GameObject go = new GameObject();
            // Set the name.
            go.name = id.ToString();
            // Set the position.
            go.transform.position = position.ToVector3();
            // Add the audio source.
            T sound = go.AddComponent<T>();
            sound.id = id;
            sound.samples = samples;
            sound.source = go.AddComponent<AudioSource>();
            sound.source.spatialize = true;
            // Set the audio clip.
            sound.SetAudioClip(sound.samples.ToFloats());
            // Listen for when the audio clip ends.
            sound.playAudio = sound.PlayAudio();
            sound.StartCoroutine(sound.playAudio);
            return sound;
        }


        /// <summary>
        /// Self-destruct.
        /// </summary>
        public void End()
        {
            // Stop playing audio.
            if (playAudio != null)
            {
                StopCoroutine(playAudio);
            }
            // Flag this sound as not playing audio.
            playing = false;
            // Stop writing.
            if (writing)
            {
                EndWrite();
            }
            // Announce that this audio ended.
            onEnd?.Invoke(id);
        }


        /// <summary>
        /// Begin to write a .wav file.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        public void StartWrite(string path)
        {
            writer = new WavWriter(path);
            writing = true;
        }
        
        
        /// <summary>
        /// Create and set the audio clip.
        /// </summary>
        /// <param name="data">The audio data.</param>
        protected void SetAudioClip(float[] data)
        {
            // Flag this sound as playing.
            playing = true;
            // Create the clip.
            source.clip = AudioClip.Create("audio", data.Length, 1, Globals.framerateInt, false);
            // Write audio data to disk.
            if (writing)
            {
                writer.Write(data, data.Length, 1);

            }
            // Set the audio data.
            source.clip.SetData(data, 0);
            // Play the clip.
            source.Play();
        }


        protected abstract void OnAudioClipEnd();


        /// <summary>
        /// Wait until the audio clip is done playing and then do something.
        /// </summary>
        private IEnumerator PlayAudio()
        {
            while (playing)
            {
                clip = source.clip; 
                if (clip == null)
                {
                    OnAudioClipEnd();
                }
                else
                {
                    // Wait until the clip ends.
                    yield return new WaitForSeconds(clip.length);
                    // Destroy the clip.
                    Destroy(clip);
                    // Do something when the clip ends.
                    OnAudioClipEnd();
                }
            }
        }


        /// <summary>
        /// Stop writing a .wav file.
        /// </summary>
        private void EndWrite()
        {
            writing = false;
            writer.End();
        }
    }
}