# Clatter

**Clatter is a C# library that can synthesize plausible sounds from physics-driven events.** Given a collision, the mass of the two objects, their "audio materials", the relative velocity, and so on, Clatter will generate a unique sound. Currently, Clatter is capable of generating impact and scrape sounds.

Clatter is intended for usage with [TDW](https://github.com/threedworld-mit/tdw), a research simulation platform, but can be easily be used game-like projects and other software.

**Features:**

- Unique and powerful audio synthesis capabilities. Unlike most physics driven audio generators, Clatter generates audio out of pre-computed data, rather than relying on modifying or mixing pre-existing audio files.
- Built-in support for Unity.
- An intuitive, highly flexible API.
- Python API bindings.

 **There are three ways to use Clatter:**

1. As a C# library: `Clatter.Core.dll`. The Clatter library can output raw wav data of physics-driven audio sounds or save the data as a .wav file.
2. As a Unity plugin: `Clatter.Core.dll` plus `Clatter.Unity.dll`. The `Clatter.Unity.dll` library includes helpful scripts that automatically listen for collision events and automatically play generated audio.
3. As a command-line executable: `clatter.exe`.

***

# Installation

**TODO**

***

# Documentation

- [**Clatter.Core**](docs/clatter.core/overview.md)
- [**Clatter.Unity**](docs/clatter.unity/overview.md)
- **Clatter CLI**

***

# Getting Started

## Clatter.Core

The simplest way to use Clatter is to call methods in the `Creator` class. This minimal example generates an impact sound:

```csharp
using Clatter.Core;


public class Program
{
    private static void Main(string[] args)
    {
        Creator.SetPrimaryObject(new AudioObjectData(0, ImpactMaterialSized.glass_1, 0.2f, 0.2f, 1));
        Creator.SetSecondaryObject(new AudioObjectData(1, ImpactMaterialSized.stone_4, 0.5f, 0.1f, 100));
        Creator.WriteImpact(1, true, "out.wav");
    }
}
```

## Clatter.Unity

In Unity C#, you can import `ClatterManager` and `AudioProducingObject` to auto-generate sounds when objects collide.

This example, which can be found in `ClatterUnityExamples/`, drops marbles on a surface to generate audio:

```csharp
using UnityEngine;
using Clatter.Core;
using Clatter.Unity;
using Random = System.Random;


/// <summary>
/// Drop lots of marbles, generating impact sounds.
/// </summary>
public class Marbles : MonoBehaviour
{
    /// <summary>
    /// The diameter of a marble.
    /// </summary>
    private const float DIAMETER = 0.013f;
    /// <summary>
    /// The spacing between the marbles.
    /// </summary>
    private const float SPACING = 0.1f;
    /// <summary>
    /// The padded half-extent of the floor.
    /// </summary>
    private const float EXTENT = 0.4f;


    private void Awake()
    {
        // Generate the floor.
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.transform.localScale = new Vector3(1, 0.015f, 1);
        floor.name = "floor";
        // Generate audio from the floor.
        AudioProducingObject f = floor.AddComponent<AudioProducingObject>();
        f.impactMaterial = ImpactMaterial.wood_medium;
        f.autoSetSize = false;
        f.size = 4;
        f.amp = 0.5f;
        f.resonance = 0.1f;
        f.autoSetMass = false;
        f.data = AudioProducingObject.floor;
        // Add the floor's Rigidbody and set the mass.
        Rigidbody fr = floor.AddComponent<Rigidbody>();
        fr.isKinematic = true;
        fr.mass = 100;
        // Create the random number generator.
        Random rng = new Random();
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
                // Add the audio data.
                AudioProducingObject ma = marble.AddComponent<AudioProducingObject>();
                ma.impactMaterial = ImpactMaterial.glass;
                ma.bounciness = 0.6f;
                ma.resonance = 0.25f;
                ma.amp = 0.2f;
                x += SPACING;
            }
            z += SPACING;
        }
        // Add the ClatterManager.
        new GameObject("ClatterManager").AddComponent<ClatterManager>();
    }
}
```

## Command-line Executable

Example call:

```powershell
./clatter.exe --primary_material glass --primary_size 1 --primary_amp 0.2 --primary_resonance 0.2 --primary_mass 1 --secondary_material stone --secondary_size 4 --secondary_amp 0.5 --secondary_resonance 0.1 --secondary_mass 100 --type impact --path out.wav
```

***

# Roadmap

Pre-alpha to-dos:

- [ ] Add documentation.
- [ ] Add releases and build tools.
- [ ] Add Python bindings for `Clatter.Core.dll`.
- [ ] Add command-line tools for OS X and Linux.
- [ ] Add Clatter to TDW.

Clatter is currently in alpha. It is likely still buggy and unstable. The API will likely change often until the first stable release.

- We intend to support *rolling sounds* but this hasn't yet been implemented.
- Softer impact materials such as "fabric" don't sound as good as harder materials. This will be corrected in the near-future.
- Impacts tend to sound best for small objects. We're working on adding better support for larger objects.
- Some scrape materials may sound distorted or "scratchy". We'll fix these.
- Clatter is pure-C# and likely can be further optimized.

# How to Cite Clatter

Please review Clatter's [license](LICENSE.md).

Clatter was developed by Esther Alter. If you are using Clatter in a game, please list it and myself in the credits.

Clatter was originally a C# port of [PyImpact](https://github.com/threedworld-mit/tdw/blob/master/Documentation/lessons/audio/py_impact.md), a component of [ThreeDWorld](https://github.com/threedworld-mit/tdw), a research simulation platform. Clatter has since expanded upon the PyImpact's functionality.

PyImpact was developed by James Traer, Maddie Cusimano, Josh McDermott, Vin Agarwal, and Esther Alter. When using Clatter for research, please cite [Traer,Cusimano  and McDermott, A perceptually inspired generative model of rigid-body  contact sounds, Digital Audio Effects, (DAFx), 2019](http://dafx2019.bcu.ac.uk/papers/DAFx2019_paper_57.pdf) and [Agarwal,  Cusimano, Traer, and McDermott, Object-based synthesis of scraping and  rolling sounds based on non-linear physical constraints, (DAFx), 2021](http://mcdermottlab.mit.edu/bib2php/pubs/makeAbs.php?loc=agarwal21).

```
@article {4500,
	title = {A perceptually inspired generative model of rigid-body contact sounds},
	journal = {Proceedings of the 22nd International Conference on Digital Audio Effects (DAFx-19)},
	year = {2019},
	month = {09/2019},
	abstract = {<p>Contact between rigid-body objects produces a diversity of impact and friction sounds. These sounds can be synthesized with detailed simulations of the motion, vibration and sound radiation of the objects, but such synthesis is computationally expensive and prohibitively slow for many applications. Moreover, detailed physical simulations may not be necessary for perceptually compelling synthesis; humans infer ecologically relevant causes of sound, such as material categories, but not with arbitrary precision. We present a generative model of impact sounds which summarizes the effect of physical variables on acoustic features via statistical distributions fit to empirical measurements of object acoustics. Perceptual experiments show that sampling from these distributions allows efficient synthesis of realistic impact and scraping sounds that convey material, mass, and motion.</p>
},
	author = {James Traer and Maddie Cusimano and Josh H. McDermott}
}
```

```
@inproceedings{agarwal21,
     TITLE= "Object-based synthesis of scraping and rolling sounds based on non-linear physical constraints",
     AUTHOR= "V Agarwal and M Cusimano and J Traer and J H McDermott",
     booktitle= "The 24th International Conference on Digital Audio Effects (DAFx-21)",
     MONTH= "September",
     YEAR= 2021,
     PDF-URL= "http://mcdermottlab.mit.edu/papers/Agarwal_etal_2021_scraping_rolling_synthesis_DAFx.pdf",
}
```