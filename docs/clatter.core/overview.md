# Clatter.Core

`Clatter.Core` is the core library for Clatter. It can be used both in a Unity and non-Unity context.

New users should start by reading:

- [`AudioObjectData`](AudioObjectData.html) Audio data for a Clatter object.
- [`CollisionEvent`](CollisionEvent.html) A struct for storing collision data.
- [`AudioGenerator`](AudioGenerator.html) Generate audio within a dynamic physics simulation.
- [`WavWriter`](WavWriter.html) Write audio samples to a .wav file.

To create audio in Clatter, you typically need to declare at least 2 [`AudioObjectData`](AudioObjectData.html) objects, at least 1 [`CollisionEvent`](CollisionEvent.html), and an [`AudioGenerator`](AudioGenerator.html). The `AudioGenerator` reads the `CollisionEvent` and generates audio. In programs where speed is not important and you just want to generate simple wav files, you can optionally generate audio by declaring a new [`Impact`](Impact.html) or [`Scrape`](Scrape.html) instead of an `AudioGenerator`.