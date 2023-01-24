using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Clatter.Core;


namespace Clatter.Unity
{
    /// <summary>
    /// A ClatterObject is a MonoBehaviour class wrapper for `Clatter.Core.ClatterObjectData` that automatically converts Unity PhysX collisions into Clatter audio.
    ///
    /// A ClatterObject must have either a Rigidbody or ArticulationBody component and at least one Collider.
    ///
    /// ClatterObject listens for Unity collision events (enter, stay, exit) and converts them into `Clatter.Core.CollisionEvent` data objects:
    /// 
    /// - "Enter" events are always impacts.
    /// - "Exit" events are always "none" audio events that will end an ongoing audio event series.
    /// - "Stay" events can be impacts, scrapes, rolls, or none-events. This is determined by a number of factors; see `areaNewCollision`, `scrapeAngle`, `impactAreaRatio`, and `rollAngularSpeed`.
    /// 
    /// ClatterObjects are automatically initialized and updated by `ClatterManager`; you *can* use a ClatterObject without `ClatterManager` but it's very difficult to do so. Notice that there is no Update or FixedUpdate call because it's assumed that `ClatterManager` will call ClatterObject.OnUpdate() and ClatterObject.OnFixedUpdate().
    /// </summary>
    public class ClatterObject : MonoBehaviour
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
        /// ClatterObject tries to filter duplicate collision events in two ways. First, it will remove any reciprocal pairs of objects, i.e. it will accept a collision between objects 0 and 1 but not objects 1 and 0. Second, it will register only the first collision between objects per main-thread update (multiple collisions can be registered because there are many physics fixed update calls in between). To allow duplicate events, set this field to `false`.
        /// </summary>
        public static bool filterDuplicates = true;
        /// <summary>
        /// The maximum number of contact points that will be evaluated when setting the contact area and speed. A higher number can mean somewhat greater precision but at the cost of performance.
        /// </summary>
        public static int maxNumContacts = 16;
        /// <summary>
        /// Unity physic material dynamic friction values. Key: An `ImpactMaterialUnsized` value. Value: A dynamic friction float value.
        /// </summary>
        public static readonly Dictionary<ImpactMaterialUnsized, float> DynamicFriction = new Dictionary<ImpactMaterialUnsized, float>()
        {
            { ImpactMaterialUnsized.ceramic, 0.47f },
            { ImpactMaterialUnsized.wood_hard, 0.35f },
            { ImpactMaterialUnsized.wood_medium, 0.35f },
            { ImpactMaterialUnsized.wood_soft, 0.35f },
            { ImpactMaterialUnsized.cardboard, 0.45f },
            { ImpactMaterialUnsized.paper, 0.47f },
            { ImpactMaterialUnsized.glass, 0.65f },
            { ImpactMaterialUnsized.fabric, 0.65f },
            { ImpactMaterialUnsized.leather, 0.4f },
            { ImpactMaterialUnsized.stone, 0.7f },
            { ImpactMaterialUnsized.rubber, 0.75f },
            { ImpactMaterialUnsized.plastic_hard, 0.3f },
            { ImpactMaterialUnsized.plastic_soft_foam, 0.45f },
            { ImpactMaterialUnsized.metal, 0.43f }
        };
        /// <summary>
        /// Unity physic material static friction values. Key: An `ImpactMaterialUnsized` value. Value: A static friction float value.
        /// </summary>
        public static readonly Dictionary<ImpactMaterialUnsized, float> StaticFriction = new Dictionary<ImpactMaterialUnsized, float>
        {
            { ImpactMaterialUnsized.ceramic, 0.47f },
            { ImpactMaterialUnsized.wood_hard, 0.37f },
            { ImpactMaterialUnsized.wood_medium, 0.37f },
            { ImpactMaterialUnsized.wood_soft, 0.37f },
            { ImpactMaterialUnsized.cardboard, 0.48f },
            { ImpactMaterialUnsized.paper, 0.5f },
            { ImpactMaterialUnsized.glass, 0.68f },
            { ImpactMaterialUnsized.fabric, 0.67f },
            { ImpactMaterialUnsized.leather, 0.43f },
            { ImpactMaterialUnsized.stone, 0.72f },
            { ImpactMaterialUnsized.rubber, 0.8f },
            { ImpactMaterialUnsized.plastic_hard, 0.35f },
            { ImpactMaterialUnsized.plastic_soft_foam, 0.47f },
            { ImpactMaterialUnsized.metal, 0.47f }
        };
        /// <summary>
        /// The unsized impact material. This will be converted into an `ImpactMaterial` by applying the size field (see below).
        /// </summary>
        [HideInInspector]
        public ImpactMaterialUnsized impactMaterial;
        /// <summary>
        /// If true, the "size bucket" is automatically set based on its volume.
        /// </summary>
        [HideInInspector]
        public bool autoSetSize = true;
        /// <summary>
        /// The "size bucket", on a scale of 0-5. To generate realistic audio, smaller objects should have smaller size bucket values. Ignored if `autoSetSize == true`. For more information, including how to derive size bucket values, see: `Clatter.ImpactMaterial` and `Clatter.ImpactMaterialData`.
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
        /// The physic material dynamic friction value (0-1). To derive friction values from `Clatter.Core.ImpactMaterialUnsized` values, see: ClatterObject.DynamicFriction.
        /// </summary>
        [HideInInspector]
        public float dynamicFriction = 0.1f;
        /// <summary>
        /// The physic material static friction value (0-1). To derive friction values from `Clatter.Core.ImpactMaterialUnsized` values, see: ClatterObject.StaticFriction.
        /// </summary>
        [HideInInspector]
        public float staticFriction = 0.1f;
        /// <summary>
        /// The physic material bounciness value (0-1). This always needs to be set on a per-object basis, as opposed to being derived from a `Clatter.Core.ImpactMaterialUnsized` value.
        /// </summary>
        [HideInInspector]
        public float bounciness = 0.2f;
        /// <summary>
        /// The mode for how the mass is set.
        /// </summary>
        [HideInInspector]
        public MassMode massMode = MassMode.body;
        /// <summary>
        /// If massMode == MassMode.fake_mass, the underlying `Clatter.Core.ClatterObjectData` will use this value when generating audio rather than the true mass of the Rigibody/ArticulationBody.
        /// </summary>
        [HideInInspector]
        public double fakeMass;
        /// <summary>
        /// If massMode == MassMode.volume, hollowness is the portion of the object that is hollow (0-1) as follows: mass = volume * density * (1 - hollowness) where volume is the sum of the bounding box sizes of each Renderer object and density is derived from the impact material (see: `Clatter.Core.ImpactMaterialData`).
        /// </summary>
        [HideInInspector]
        public float hollowness;
        /// <summary>
        /// Invoked when this object is destroyed. Parameters: The ID of this object, i.e. `this.data.id`.
        /// </summary>
        // ReSharper disable once Unity.RedundantHideInInspectorAttribute
        [HideInInspector]
        public UnityEvent<uint> onDestroy = new UnityEvent<uint>();
        /// <summary>
        /// This object's data.
        /// </summary>
        public ClatterObjectData data;
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
        /// The default audio data. This is used whenever an `ClatterObject` collides with a non-`ClatterObject` object.
        /// </summary>
        public static ClatterObjectData defaultClatterObjectData = new ClatterObjectData(0, ImpactMaterial.wood_medium_4, 0.5f, 0.1f, 100, ScrapeMaterial.plywood);


        /// <summary>
        /// Set the underlying ClatterObjectData. This must be called once in order for this object to generate audio.
        /// </summary>
        /// <param name="id">This object's ID.</param>
        public void Initialize(uint id)
        {
            contactPoints = new Vector3[maxNumContacts];
            contactNormals = new Vector3[maxNumContacts];
            double volume = 0;
            Vector3 boundsSize;
            Bounds b = new Bounds(gameObject.transform.position, Vector3.zero);
            foreach (Renderer re in gameObject.GetComponentsInChildren<Renderer>())
            {
                b.Encapsulate(re.bounds);
                boundsSize = re.bounds.size;
                volume += boundsSize.x * boundsSize.y * boundsSize.z;
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
            double mass;
            // Get the mass.
            if (massMode == MassMode.body)
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
                    mass = defaultClatterObjectData.mass;
                }  
            }
            else if (massMode == MassMode.volume)
            {
                // Multiply the volume by the density and then by a hollowness factor.
                mass = volume * ImpactMaterialData.Density[impactMaterial] * (1 - hollowness);
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
            else
            {
                mass = fakeMass;
            }
            // Set the physic material.
            if (autoSetFriction)
            {
                dynamicFriction = DynamicFriction[impactMaterial];
                staticFriction = StaticFriction[impactMaterial];
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
                size = ImpactMaterialData.GetSize(new Vector3d(b.size.x, b.size.y, b.size.z));
            }
            // Convert the material + size to an impact material.
            ImpactMaterial im = ImpactMaterialData.GetImpactMaterial(impactMaterial, size);
            // Set the data.
            if (hasScrapeMaterial)
            {
                data = new ClatterObjectData(id, im, amp, resonance, mass, scrapeMaterial);
            }
            else
            {
                data = new ClatterObjectData(id, im, amp, resonance, mass);
            }
        }
        
        
        /// <summary>
        /// Refresh the recorded set of collisions. This method is not equivalent to Update() and is called automatically by `ClatterManager`.
        /// </summary>
        public void OnUpdate()
        {
            collisionIds.Clear();
        }


        /// <summary>
        /// Update the directional and angular speeds of the underlying `Clatter.Core.ClatterObjectData`. This method is not equivalent to FixedUpdate() and is called automatically by `ClatterManager`.
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
        /// Register an enter event.
        /// </summary>
        /// <param name="collision">The collision.</param>
        /// <param name="type">The collision type.</param>
        private void RegisterCollision(Collision collision, OnCollisionType type)
        {
            // Try to get the other object.
            ClatterObject other = collision.body.GetComponentInChildren<ClatterObject>();
            // Get the greater angular velocity.
            double angularSpeed;
            ClatterObjectData otherData;
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
                otherData = defaultClatterObjectData;
            }
            // Compare the IDs for filter out duplicate events.
            if (filterDuplicates && data.id > otherData.id)
            {
                return;
            }
            ClatterObjectData primary;
            ClatterObjectData secondary;
            ClatterObject primaryMono;
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
            // Ignore low-speed events.
            if (speed < AudioGenerator.minSpeed)
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
            // For scrapes, use the averaged speed.
            if (audioEventType == AudioEventType.scrape)
            {
                // Get the normal speeds.
                double normalSpeeds = 0;
                for (int i = 0; i < numContacts; i++)
                {
                    normalSpeeds += speed * Mathf.Acos(Mathf.Clamp(Vector3.Dot(contactNormals[i], normalizedVelocity), -1, 1));
                }
                // Get the average normal speed.
                speed = normalSpeeds / numContacts;
                // Ignore low-speed events.
                if (speed < AudioGenerator.minSpeed)
                {
                    NoneCollisionEvent(primary, secondary);
                    return;
                }
            }
            // Add the collision.
            ClatterManager.instance.generator.AddCollision(new CollisionEvent(ids, primary, secondary, audioEventType, speed, centroid));
        }


        /// <summary>
        /// Set the collision event as a none-type event.
        /// </summary>
        /// <param name="primary">The primary object.</param>
        /// <param name="secondary">The secondary object.</param>
        private void NoneCollisionEvent(ClatterObjectData primary, ClatterObjectData secondary)
        {
            ClatterManager.instance.generator.AddCollision(new CollisionEvent(primary, secondary, AudioEventType.none, 0, Vector3d.Zero));
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