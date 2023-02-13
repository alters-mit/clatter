using UnityEngine;
using UnityEngine.UI;
using Clatter.Core;
using Clatter.Unity;


// Listen for collisions and display their info in the UI. This example sets ClatterManager.auto to false in order to prevent script execution bugs.
public class CollisionListener : MonoBehaviour
{
    // The UI text.
    public Text text;
    // The ClatterManager.
    public ClatterManager clatterManager;


    private void Awake()
    {
        clatterManager.generator.onImpact += OnCollision;
        clatterManager.generator.onScrapeStart += OnCollision;
        clatterManager.generator.onScrapeOngoing += OnCollision;
        clatterManager.OnAwake();
    }


    private void OnCollision(CollisionEvent collisionEvent, Samples samples, Vector3d position, int audioSourceId)
    {
        if (collisionEvent.type != AudioEventType.none)
        {
            text.text = collisionEvent.primary.id + " " + collisionEvent.secondary.id + " " + collisionEvent.type;
        }
    }


    private void Update()
    {
        clatterManager.OnUpdate();
    }


    private void FixedUpdate()
    {
        clatterManager.OnFixedUpdate();
    }
}