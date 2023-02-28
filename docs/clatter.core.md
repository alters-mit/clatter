# Clatter.Core

`Clatter.Core` is the core library for Clatter. It can be used both in a Unity and non-Unity context.

[URLS]

New users should start by reading:

- [`ClatterObjectData`](ClatterObjectData.html) Audio data for a Clatter object.
- [`CollisionEvent`](CollisionEvent.html) A struct for storing collision data.
- [`AudioGenerator`](AudioGenerator.html) Generate audio within a dynamic physics simulation.
- [`WavWriter`](WavWriter.html) Write audio samples to a .wav file.
- [`ExternalEntryPoint`](ExternalEntryPoint.html) A convenient entry point for non-C# applications. If your project is in Unity/C#, you should ignore this class.

To create audio in Clatter, you typically need to declare at least 2 [`AudioObjectData`](AudioObjectData.html) objects, at least 1 [`CollisionEvent`](CollisionEvent.html), and an [`AudioGenerator`](AudioGenerator.html). The `AudioGenerator` reads the `CollisionEvent` and generates audio. In programs where speed is not important and you just want to generate simple wav files, you can optionally generate audio by declaring a new [`Impact`](Impact.html) or [`Scrape`](Scrape.html) instead of an `AudioGenerator`.

`Clatter.Core` *can* be used in Unity as-is, but it's usually much easier to use [`Clatter.Unity`](clatter.unity.html). `Clatter.Core` doesn't have any MonoBehaviour subclasses, meaning that nothing will update on Update(), Awake(), etc.