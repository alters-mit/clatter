# Clatter.Unity

`Clatter.Unity` is a library of helpful extensions for using Clatter in Unity.

[URLS]

To add Clatter to a Unity project, you must add *both* Clatter.Core.dll and Clatter.Unity.dll to a directory in Assets.

New users should start by reading:

- [`ClatterObject`](ClatterObject.html) A ClatterObject is a MonoBehaviour wrapper class for [`ClatterObjectData`](ClatterObjectData.html).
- [`ClatterManager`](ClatterManager.html) Singleton manager class for Clatter in Unity. This is a a MonoBehaviour wrapper class for [`AudioGenerator`](AudioGenerator.html). It also manages each `ClatterObject` in the scene.

For example implementation, see the ClatterUnityExample Unity project. **NOTE:** You need to manually add Clatter.Core.dll and Clatter.Unity.dll to ClatterUnityExample/Assets/

- Impact is a minimal example of how to generate impact audio.
- Scrape is a minimal example of how to generate scrape audio.
- Marbles is an example of how to instantiate Clatter purely from code.
- ScrapeManual is an example of how to generate audio in Unity without a [`ClatterManager`](ClatterManager.html).
- DefaultObjectData is an example of how to use the default Clatter object data (see: [`ClatterObject.defaultObjectData`](ClatterObject.html)).