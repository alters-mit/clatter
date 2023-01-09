using UnityEngine;
using Clatter.Core;
using Clatter.Unity;
using Random = System.Random;

// Drop lots of marbles, generating impact sounds.
public class Marbles : MonoBehaviour
{
    // The diameter of a marble.
    private const float DIAMETER = 0.013f;
    // The spacing between the marbles.
    private const float SPACING = 0.1f;
    // The padded half-extent of the floor.
    private const float EXTENT = 0.4f;

    
    // The random seed.
    public int seed;


    private void Awake()
    {
        // Generate the floor.
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.transform.localScale = new Vector3(1, 0.015f, 1);
        floor.name = "floor";
        // Generate audio from the floor.
        AudioProducingObject f = floor.AddComponent<AudioProducingObject>();
        f.impactMaterial = ImpactMaterialUnsized.wood_medium;
        f.autoSetSize = false;
        f.size = 4;
        f.amp = 0.5f;
        f.resonance = 0.1f;
        f.autoSetMass = false;
        f.data = AudioProducingObject.defaultAudioObjectData;
        // Add the floor's Rigidbody and set the mass.
        Rigidbody fr = floor.AddComponent<Rigidbody>();
        fr.isKinematic = true;
        fr.mass = 100;
        // Create the random number generator.
        Random rng = new Random(seed);
        // Add marbles in a grid.
        float z = -EXTENT;
        while (z < EXTENT)
        {
            float x = -EXTENT;
            while (x < EXTENT)
            {
                GameObject marble = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                // Set a random y value.
                float y = 1.8f + (float)rng.NextDouble();
                // Set the position.
                marble.transform.position = new Vector3(x, y, z);
                // Set the scale.
                marble.transform.localScale = new Vector3(DIAMETER, DIAMETER, DIAMETER);
                // Set a random color.
                marble.GetComponent<MeshRenderer>().material.color = new Color((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble(), 1);
                // Add the audio data.
                AudioProducingObject ma = marble.AddComponent<AudioProducingObject>();
                ma.impactMaterial = ImpactMaterialUnsized.glass;
                ma.bounciness = 0.6f;
                ma.resonance = 0.25f;
                ma.amp = 0.2f;
                x += SPACING;
            }
            z += SPACING;
        }
        // Add the ClatterManager.
        GameObject go = new GameObject("ClatterManager");
        ClatterManager clatterManager = go.AddComponent<ClatterManager>();
        // Disable auto-updates.
        clatterManager.auto = false;
        // Manually awaken the manager.
        clatterManager.OnAwake();
    }


    private void Update()
    {
        ClatterManager.instance.OnUpdate();
    }


    private void FixedUpdate()
    {
        ClatterManager.instance.OnFixedUpdate();
    }
}