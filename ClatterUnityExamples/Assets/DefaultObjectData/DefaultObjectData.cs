using UnityEngine;
using Clatter.Core;
using Clatter.Unity;


/// <summary>
/// Use a default material instead of setting the floor as a ClatterObject.
/// </summary>
public class DefaultObjectData : MonoBehaviour
{
    private void Awake()
    {
        // Generate the floor.
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.transform.localScale = new Vector3(1, 0.015f, 1);
        floor.name = "floor";
        // Set the default Clatter object data.
        ClatterObject.defaultObjectData = new ClatterObjectData(0, ImpactMaterial.wood_hard_4, 0.5, 0.1, 100);
        // Create the falling object.
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.transform.position = new Vector3(0, 2, 0);
        go.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        Rigidbody mr = go.AddComponent<Rigidbody>();
        mr.mass = 0.1f;
        ClatterObject clatterObject = go.AddComponent<ClatterObject>();
        clatterObject.impactMaterial = ImpactMaterialUnsized.glass;
        clatterObject.autoSetSize = false;
        clatterObject.size = 0;
        clatterObject.bounciness = 0.6f;
        clatterObject.resonance = 0.05;
        clatterObject.amp = 0.2;
        // Add the ClatterManager.
        GameObject clatterManager = new GameObject("ClatterManager");
        clatterManager.AddComponent<ClatterManager>();
    }
}