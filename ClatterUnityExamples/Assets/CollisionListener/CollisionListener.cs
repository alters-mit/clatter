using UnityEngine;
using UnityEngine.UI;
using Clatter.Core;
using Clatter.Unity;


public class CollisionListener : MonoBehaviour
{
    /// <summary>
    /// The UI text.
    /// </summary>
    public Text text;
    /// <summary>
    /// The sphere.
    /// </summary>
    public AudioProducingObject sphere;
    /// <summary>
    /// The surface.
    /// </summary>
    public AudioProducingObject surface;
    /// <summary>
    /// The ClatterManager.
    /// </summary>
    public ClatterManager clatterManager;


    private void Awake()
    {
        sphere.onCollision.AddListener(OnCollision);
        surface.onCollision.AddListener(OnCollision);
        clatterManager.OnAwake();
    }


    private void OnCollision(CollisionEvent collisionEvent)
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