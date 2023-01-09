# Clatter.Unity

`Clatter.Unity` is a library of helpful extensions for using Clatter in Unity. 

**To add Clatter to a Unity project, you must add *both* Clatter.Core.dll and Clatter.Unity.dll to a directory in Assets.**

See the sidebar for API documentation. New users should start by reading:

- [`AudioProducingObject`](AudioProducingObject.md) An AudioProducingObject is a MonoBehaviour class for `Clatter.Core.AudioObjectData`.
- [`ClatterManager`](ClatterManager.md) Singleton manager class for Clatter in Unity.

For example implementation, see the ClatterUnityExample Unity project:

- Impact is a minimal example of how to generate impact audio.
- Scrape is a minimal example of how to generate scrape audio.
- Marbles is an example of how to instantiate Clatter purely from code.
- ScrapeManual is an example of how to generate audio in Unity without a [`ClatterManager`](ClatterManager.md).