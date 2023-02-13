using UnityEngine;
using UnityEngine.UI;
using Clatter.Core;
using Clatter.Unity;


/// <summary>
/// Listen for collisions and display their info in the UI. This example sets ClatterManager.auto to false in order to prevent script execution bugs.
/// </summary>
public class CollisionListener : MonoBehaviour
{
    /// <summary>
    /// The UI text.
    /// </summary>
    public Text text;
    /// <summary>
    /// The ClatterManager.
    /// </summary>
    public ClatterManager clatterManager;


    private void Awake()
    {
        clatterManager.OnAwake();
        clatterManager.generator.onImpact += OnCollision;
        clatterManager.generator.onScrapeStart += OnCollision;
        clatterManager.generator.onScrapeOngoing += OnCollision;
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