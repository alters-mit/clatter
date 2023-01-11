using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Clatter.Core;


namespace Clatter.Unity
{
    /// <summary>
    /// An AudioProducingObject is a MonoBehaviour class wrapper for `Clatter.Core.AudioObjectData` that automatically converts Unity PhysX collisions into Clatter audio.
    ///
    /// An AudioProducingObject must have either a Rigidbody or ArticulationBody component and at least one Collider.
    ///
    /// AudioProducingObject listens for Unity collision events (enter, stay, exit) and converts them into `Clatter.Core.CollisionEvent` data objects:
    /// 
    /// - "Enter" events are always impacts.
    /// - "Exit" events are always "none" audio events that will end an ongoing audio event series.
    /// - "Stay" events can be impacts, scrapes, rolls, or none-events. This is determined by a number of factors; see `areaNewCollision`, `scrapeAngle`, `impactAreaRatio`, and `rollAngularSpeed`.
    /// 
    /// AudioProducingObjects are automatically initialized and updated by `ClatterManager`; you *can* use an AudioProducingObject without `ClatterManager` but it's very difficult to do so. Notice that there is no Update or FixedUpdate call because it's assumed that `ClatterManager` will call AudioProducingObject.OnUpdate() and AudioProducingObject.OnFixedUpdate().
    /// </summary>
    public class AudioProducingObject : MonoBehaviour
    {
        /// <summary>
        /// Types of OnCollision callbacks, e.g. OnCollisionEnter.
        /// </summary>
        private enum OnCollisionType : byte
        {
            enter = 0,
            stay = 1,
            exit = 2
        }
        
        
        /// <summary>
        /// On a collision stay event, if the previous area is None and the current area is greater than this, the audio event is either an impact or a scrape; see scrapeAngle. 
        /// </summary>
        public static double areaNewCollision = 1e-5;
        /// <summary>
        /// On a collision stay event, there is a large new contact area (see areaNewCollision), if the angle between Vector3.up and the normalized relative velocity of the collision is greater than this value, then the audio event is a scrape. Otherwise, it's an impact. 
        /// </summary>
        public static float scrapeAngle = 80;
        /// <summary>
        /// On a collision stay event, if the area of the collision increases by at least this factor, the audio event is an impact.
        /// </summary>
        public static double impactAreaRatio = 5;
        /// <summary>
        /// On a collision stay event, if the angular speed is this or greater, the audio event is a roll; otherwise, it's a scrape.
        /// </summary>
        public static double rollAngularSpeed = 0.5;
        /// <summary>
        /// On a collision stay event, if we think the collision is an impact but any of the contact points are this far away or greater, the audio event is none.
        /// </summary>
        public static float maxContactSeparation = 1e-8f;
        /// <summary>
        /// AudioProducingObject tries to filter duplicate collision events in two ways. First, it will remove any reciprocal pairs of objects, i.e. it will accept a collision between objects 0 and 1 but not objects 1 and 0. Second, it will register only the first collision between objects per main-thread update (multiple collisions can be registered because there are many physics fixed update calls in between). To allow duplicate events, set this field to `false`.
        /// </summary>
        public static bool filterDuplicates = true;
        /// <summary>
        /// The maximum number of contact points that will be evaluated when setting the contact area and speed. A higher number can mean somewhat greater precision but at the cost of performance.
        /// </summary>
        public static int maxNumContacts = 16;
        /// <summary>
        /// The unsized impact material. This will be converted into an `ImpactMaterial` by applying the size field (see below).
        /// </summary>
        [HideInInspector]
        public ImpactMaterialUnsized impactMaterial;
        /// <summary>
        /// If true, the object's "size bucket" is automatically set based on its volume.
        /// </summary>
        [HideInInspector]
        public bool autoSetSize = true;
        /// <summary>
        /// The size of the object (this affects the generated audio). Ignored if `autoSetSize == true`.
        /// </summary>
        [HideInInspector]
        public int size;
        /// <summary>
        /// If true, this object has a scrape material.
        /// </summary>
        [HideInInspector]
        public bool hasScrapeMaterial;
        /// <summary>
        /// The scrape material, if any. Ignored if `hasScrapeMaterial == false`.
        /// </summary>
        [HideInInspector]
        public ScrapeMaterial scrapeMaterial;
        /// <summary>
        /// The audio amplitude.
        /// </summary>
        [HideInInspector]
        public double amp = 0.1;
        /// <summary>
        /// The resonance value.
        /// </summary>
        [HideInInspector]
        public double resonance = 0.1;
        /// <summary>
        /// If true, the friction values are automatically set based on the impact material.
        /// </summary>
        [HideInInspector]
        public bool autoSetFriction = true;
        /// <summary>
        /// The physic material dynamic friction value.
        /// </summary>
        [HideInInspector]
        public float dynamicFriction = 0.1f;
        /// <summary>
        /// The physic material static friction value.
        /// </summary>
        [HideInInspector]
        public float staticFriction = 0.1f;
        /// <summary>
        /// The physic material bounciness value. This always needs to be set on a per-object basis. 
        /// </summary>
        [HideInInspector]
        public float bounciness = 0.2f;
        /// <summary>
        /// If true, the mass of the object is automatically set based on its impact material and volume. If false, use the mass value in the Rigidbody.
        /// </summary>
        [HideInInspector]
        public bool autoSetMass = true;
        /// <summary>
        /// The portion from 0 to 1 of the object that is hollow. This is used to convert volume and density to mass. Ignored if `autoSetMass == false`.
        /// </summary>
        [HideInInspector]
        public float hollowness;
        /// <summary>
        /// The mass of the object. Ignored if `autoSetMass == true`.
        /// </summary>
        [HideInInspector]
        public double mass;
        /// <summary>
        /// Invoked when this object generates a `CollisionEvent`. Parameters: The `CollisionEvent`.
        /// </summary>
        // ReSharper disable once Unity.RedundantHideInInspectorAttribute
        [HideInInspector]
        public UnityEvent<CollisionEvent> onCollision = new UnityEvent<CollisionEvent>();
        /// <summary>
        /// Invoked when this object is destroyed. Parameters: The ID of this object, i.e. `this.data.id`.
        /// </summary>
        // ReSharper disable once Unity.RedundantHideInInspectorAttribute
        [HideInInspector]
        public UnityEvent<uint> onDestroy = new UnityEvent<uint>();
        /// <summary>
        /// This object's data.
        /// </summary>
        public AudioObjectData data;
        /// <summary>
        /// The collision contacts area of a previous collision.
        /// </summary>
        private double previousArea;
        /// <summary>
        /// If true, this object has contacted another object previously and generated contact area.
        /// </summary>
        private bool hasPreviousArea;
        /// <summary>
        /// The object's Rigidbody. Can be null.
        /// </summary>
        private Rigidbody r;
        /// <summary>
        /// The object's ArticulationBody. Can be null.
        /// </summary>
        private ArticulationBody articulationBody;
        /// <summary>
        /// If true, this object has a Rigidbody.
        /// </summary>
        private bool hasRigidbody;
        /// <summary>
        /// If true, this object has an ArticulationBody.
        /// </summary>
        private bool hasArticulationBody;
        /// <summary>
        /// Cached array of collision contacts.
        /// </summary>
        private static ContactPoint[] contacts = Array.Empty<ContactPoint>();
        /// <summary>
        /// A cached array of contact points.
        /// </summary>
        private Vector3[] contactPoints;
        /// <summary>
        /// A cached array of contact normals.
        /// </summary>
        private Vector3[] contactNormals;
        /// <summary>
        /// The object's physic material.
        /// </summary>
        private PhysicMaterial physicMaterial;
        /// <summary>
        /// A set of IDs of collision events from this frame. This is used to filter duplicates.
        /// </summary>
        private HashSet<ulong> collisionIds = new HashSet<ulong>();
        /// <summary>
        /// The default audio data. This is used whenever an `AudioProducingObject` collides with a non-`AudioProducingObject` object.
        /// </summary>
        public static AudioObjectData defaultAudioObjectData = new AudioObjectData(0, ImpactMaterial.wood_medium_4, 0.5f, 0.1f, 100, ScrapeMaterial.plywood);


        /// <summary>
        /// Set the underlying AudioObjectData. This must be called once in order for this object to generate audio.
        /// </summary>
        /// <param name="id">This object's ID.</param>
        public void Initialize(uint id)
        {
            contactPoints = new Vector3[maxNumContacts];
            contactNormals = new Vector3[maxNumContacts];
            // Get the bounds of the object.
            Bounds b = new Bounds(gameObject.transform.position, Vector3.zero);
            foreach (Renderer re in gameObject.GetComponentsInChildren<Renderer>())
            {
                b.Encapsulate(re.bounds);
            }
            // Set the bodies.
            r = GetComponent<Rigidbody>();
            articulationBody = GetComponent<ArticulationBody>();
            // Automatically add a Rigidbody if needed.
            if (r == null && articulationBody == null)
            {
                r = gameObject.AddComponent<Rigidbody>();
                r.mass = 1;
                r.collisionDetectionMode = CollisionDetectionMode.Discrete;
            }
            hasRigidbody = r != null;
            hasArticulationBody = articulationBody != null;
            // Get the mass.
            // Auto-set the mass.
            if (autoSetMass)
            {
                // Get the approximate volume.
                double volume = b.size.x * b.size.y * b.size.z;
                // Multiply the volume by the density and then by a hollowness factor.
                mass = volume * PhysicsValues.Density[impactMaterial] * (1 - hollowness);
                // Set the mass.
                if (r != null)
                {
                    r.mass = (float)mass;
                }
                else if (articulationBody != null)
                {
                    articulationBody.mass = (float)mass;
                }
            }
            // Use the mass value in the Rigidbody or ArticulationBody.
            else
            {
                if (r != null)
                {
                    mass = r.mass;
                }
                else if (articulationBody != null)
                {
                    mass = articulationBody.mass;
                }
                else
                {
                    mass = 0;
                }          
            }
            // Set the physic material.
            if (autoSetFriction)
            {
                dynamicFriction = PhysicMaterialValues.DynamicFriction[impactMaterial];
                staticFriction = PhysicMaterialValues.StaticFriction[impactMaterial];
            }
            physicMaterial = new PhysicMaterial()
            {
                dynamicFriction = dynamicFriction,
                staticFriction = staticFriction,
                bounciness = bounciness
            };
            Collider[] colliders = gameObject.GetComponentsInChildren<Collider>();
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].sharedMaterial = physicMaterial;
            }
            // Get the size from the volume.
            if (autoSetSize)
            {
                size = AudioObjectData.GetSize(new Vector3d(b.size.x, b.size.y, b.size.z));
            }
            // Convert the material + size to an impact material.
            ImpactMaterial im = AudioObjectData.GetImpactMaterial(impactMaterial, size);
            // Set the data.
            if (hasScrapeMaterial)
            {
                data = new AudioObjectData(id, im, amp, resonance, mass, scrapeMaterial);
            }
            else
            {
                data = new AudioObjectData(id, im, amp, resonance, mass);
            }
        }


        /// <summary>
        /// Register an enter event.
        /// </summary>
        /// <param name="collision">The collision.</param>
        /// <param name="type">The collision type.</param>
        private void RegisterCollision(Collision collision, OnCollisionType type)
        {
            // Try to get the other object.
            AudioProducingObject other = collision.body.GetComponentInChildren<AudioProducingObject>();
            // Get the greater angular velocity.
            double angularSpeed;
            AudioObjectData otherData;
            if (other != null)
            {
                angularSpeed = data.angularSpeed > other.data.angularSpeed ?
                    data.angularSpeed : other.data.angularSpeed;
                otherData = other.data;
            }
            else
            {
                angularSpeed = data.angularSpeed;
                // The other object is the floor.
                otherData = defaultAudioObjectData;
            }
            // Compare the IDs for filter out duplicate events.
            if (filterDuplicates && data.id > otherData.id)
            {
                return;
            }
            AudioObjectData primary;
            AudioObjectData secondary;
            AudioProducingObject primaryMono;
            // Set the primary and secondary IDs depending on:
            // 1. Whether this is a secondary object.
            // 2. Which object is has a greater speed.
            if (data.speed > otherData.speed)
            {
                primary = data;
                secondary = otherData;
                primaryMono = this;
            }
            else
            {
                primary = otherData;
                secondary = data;
                primaryMono = other;
            }
            // Get the IDs pair.
            ulong ids = CollisionEvent.GetIds(primary, secondary);
            // This object IDs pair already exists.
            if (filterDuplicates && !collisionIds.Add(ids))
            {
                return;
            }
            // Get the number of contacts.
            int numContacts = collision.contactCount;
            if (numContacts == 0)
            {
                NoneCollisionEvent(primary, secondary);
                return;
            }
            // Resize the array so we can copy the contacts directly into it.
            if (numContacts > contacts.Length)
            {
                Array.Resize(ref contacts, numContacts);
            }
            // Only evaluate up to maxNumContacts.
            numContacts = numContacts > maxNumContacts ? maxNumContacts : numContacts;
            // Get the contacts.
            collision.GetContacts(contacts);
            for (int i = 0; i < numContacts; i++)
            {
                contactPoints[i] = contacts[i].point;
                contactNormals[i] = contacts[i].normal;
            }
            // Get the normalized relative velocity of the collision.
            Vector3 normalizedVelocity = collision.relativeVelocity.normalized;
            double speed = normalizedVelocity.magnitude;
            // Get the normal speeds.
            double normalSpeeds = 0;
            for (int i = 0; i < numContacts; i++)
            {
                normalSpeeds += speed * Mathf.Acos(Mathf.Clamp(Vector3.Dot(contactNormals[i], normalizedVelocity), -1, 1));
            }
            // Get the average normal speed.
            double normalSpeed = normalSpeeds / numContacts;
            // Ignore low-speed events.
            if (normalSpeed < AudioEvent.minSpeed)
            {
                NoneCollisionEvent(primary, secondary);
                return;
            }
            // Get the centroid.
            Vector3d centroid = Vector3d.Zero;
            for (int i = 0; i < numContacts; i++)
            {
                centroid.X += contactPoints[i].x;
                centroid.Y += contactPoints[i].y;
                centroid.Z += contactPoints[i].z;
            }
            centroid /= numContacts;
            // Get the square root magnitude to be computationally fast.
            double sqrtMagnitude = 0;
            Vector3d maxPoint = Vector3d.Zero;
            Vector3d p = Vector3d.Zero;
            for (int i = 0; i < numContacts; i++)
            {
                p.X = contactPoints[i].x;
                p.Y = contactPoints[i].y;
                p.Z = contactPoints[i].z;
                double s = (centroid - p).SqrMagnitude;
                if (s > sqrtMagnitude)
                {
                    sqrtMagnitude = s;
                    p.CopyTo(maxPoint);
                }
            }
            // Get the radius.
            double radius = Vector3d.Distance(centroid, maxPoint);
            // Get the approximate area.
            double area = Math.PI * radius * radius;
            AudioEventType audioEventType;
            // Exits are always none.
            if (type == OnCollisionType.exit)
            {
                audioEventType = AudioEventType.none;
            }
            // Enters are always impacts.
            else if (type == OnCollisionType.enter)
            {
                audioEventType = AudioEventType.impact;
            }
            // Stays can be anything.
            else
            {
                // There is no previous area.
                if (!primaryMono.hasPreviousArea)
                {
                    // The area is big enough to be an impact.
                    if (area > areaNewCollision)
                    {
                        // The angle between the collision and the up angle is high enough that this is a scrape.
                        if (Vector3.Angle(collision.relativeVelocity, Vector3.up) >= scrapeAngle)
                        {
                            audioEventType = AudioEventType.scrape;
                        }
                        // The angle is shallow enough that this is an impact.
                        else
                        {
                            audioEventType = AudioEventType.impact;
                        }
                    }
                    // This is a non-event.
                    else
                    {
                        audioEventType = AudioEventType.none;
                    }
                }
                // There is previous area.
                else if (primaryMono.previousArea > 0)
                {
                    // The ratio between the new area and the previous area is small.
                    if (area / primaryMono.previousArea < impactAreaRatio)
                    {
                        // If the angular speed is fast, this is a roll.
                        if (angularSpeed > rollAngularSpeed)
                        {
                            audioEventType = AudioEventType.roll;
                        }
                        // If the angular speed is slow, this is a scrape.
                        else
                        {
                            audioEventType = AudioEventType.scrape;
                        }          
                    }
                    // This is an impact or a none-event.
                    else
                    {
                        // Check the contact point separations.
                        bool isImpact = true;
                        for (int i = 0; i < numContacts; i++)
                        {
                            if (contacts[i].separation >= maxContactSeparation)
                            {
                                isImpact = false;
                                break;
                            }
                        }
                        // The contact separation is low. This is an impact.
                        if (isImpact)
                        {
                            audioEventType = AudioEventType.impact;                 
                        }
                        // The contact separation is high. This is an none-event.
                        else
                        {
                            audioEventType = AudioEventType.none;    
                        }
                    }
                }
                // This is a non-event.
                else
                {
                    audioEventType = AudioEventType.none;
                }
            }
            // Set the previous area.
            if (audioEventType == AudioEventType.none)
            {
                primaryMono.hasPreviousArea = false;
            }
            else
            {
                primaryMono.hasPreviousArea = true;
                primaryMono.previousArea = area; 
            }
            // Add the collision.
            CollisionEvent collisionEvent = new CollisionEvent(ids, primary, secondary, audioEventType, normalSpeed, centroid);
            ClatterManager.instance.generator.AddCollision(collisionEvent);
            onCollision?.Invoke(collisionEvent);
        }
        
        
        /// <summary>
        /// Refresh the recorded set of collisions. This method is not equivalent to Update() and is called automatically by `ClatterManager`.
        /// </summary>
        public void OnUpdate()
        {
            collisionIds.Clear();
        }


        /// <summary>
        /// Update the directional and angular speeds of the underlying `Clatter.Core.AudioObjectData`. This method is not equivalent to FixedUpdate() and is called automatically by `ClatterManager`.
        /// </summary>
        public void OnFixedUpdate()
        {
            // Update the velocity.
            if (hasRigidbody)
            {
                data.speed = r.velocity.magnitude;
                data.angularSpeed = r.angularVelocity.magnitude;
            }
            else if (hasArticulationBody)
            {
                data.speed = articulationBody.velocity.magnitude;
                data.angularSpeed = articulationBody.angularVelocity.magnitude;
            }
        }


        /// <summary>
        /// Set the collision event as a none-type event.
        /// </summary>
        /// <param name="primary">The primary object.</param>
        /// <param name="secondary">The secondary object.</param>
        private void NoneCollisionEvent(AudioObjectData primary, AudioObjectData secondary)
        {
            CollisionEvent collisionEvent = new CollisionEvent(primary, secondary, AudioEventType.none, 0, Vector3d.Zero);
            ClatterManager.instance.generator.AddCollision(collisionEvent);
            onCollision?.Invoke(collisionEvent);
        }


        private void OnCollisionEnter(Collision collision)
        {
            RegisterCollision(collision, OnCollisionType.enter);
        }


        private void OnCollisionStay(Collision collision)
        {
            RegisterCollision(collision, OnCollisionType.stay);
        }


        private void OnCollisionExit(Collision collision)
        {
            RegisterCollision(collision, OnCollisionType.exit);
        }


        private void OnDestroy()
        {
            Destroy(physicMaterial);
            onDestroy?.Invoke(data.id);
        }
    }
}