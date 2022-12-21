using System;
using UnityEngine;
using Clatter.Core;


namespace Clatter.Unity
{
    /// <summary>
    /// An AudioProducingObject listens to collisions with other AudioProducingObjects and generates audio event data.
    /// </summary>
    public class AudioProducingObject : MonoBehaviour
    {
        /// <summary>
        /// This object's data.
        /// </summary>
        public AudioObjectData data;
        /// <summary>
        /// Invoked when this object is destroyed.
        /// </summary>
        public Action<uint> onDestroy;
        /// <summary>
        /// If true, auto-size the object based on its volume.
        /// </summary>
        public bool autoSetSize = true;
        /// <summary>
        /// The size of the object (this affects the generated audio).
        /// </summary>
        [HideInInspector]
        public int size;
        /// <summary>
        /// The impact material.
        /// </summary>
        public ImpactMaterial impactMaterial;
        /// <summary>
        /// If true, this object has a scrape material.
        /// </summary>
        public bool hasScrapeMaterial;
        /// <summary>
        /// The scrape material, if any.
        /// </summary>
        [HideInInspector]
        public ScrapeMaterial scrapeMaterial;
        /// <summary>
        /// The audio amplitude.
        /// </summary>
        [Range(0, 1)]
        public float amp = 0.1f;
        /// <summary>
        /// The resonance value.
        /// </summary>
        [Range(0, 1)]
        public float resonance = 0.1f;
        /// <summary>
        /// If true, automatically set the friction values based on the impact material (see: PhysicsValues.cs).
        /// </summary>
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
        [Range(0, 1)]
        public float bounciness = 0.2f;
        /// <summary>
        /// If true, automatically set the mass of the object based on its impact material and volume (see: PhysicsValues.cs). If false, use the mass value in the Rigidbody.
        /// </summary>
        public bool autoSetMass = true;
        /// <summary>
        /// The portion from 0 to 1 of the object that is hollow. This is used to convert volume and density to mass (see: autoSetMass).
        /// </summary>
        [HideInInspector]
        public float hollowness;
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
        private Vector3[] contactPoints = Array.Empty<Vector3>();
        /// <summary>
        /// A cached array of contact normals.
        /// </summary>
        private Vector3[] contactNormals = Array.Empty<Vector3>();
        /// <summary>
        /// The object's physic material.
        /// </summary>
        private PhysicMaterial physicMaterial;
        /// <summary>
        /// The floor audio data.
        /// </summary>
        public static AudioObjectData floor = new AudioObjectData(0, ImpactMaterialSized.wood_medium_4, 0.5f, 0.1f, 100, ScrapeMaterial.plywood);


        /// <summary>
        /// Set the underlying AudioObjectData. This must be called once in order for this object to generate audio.
        /// </summary>
        /// <param name="id">This object's ID.</param>
        public void Initialize(uint id)
        {
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
            float mass;
            // Auto-set the mass.
            if (autoSetMass)
            {
                // Get the approximate volume.
                float volume = b.size.x * b.size.y * b.size.z;
                // Multiply the volume by the density and then by a hollowness factor.
                mass = volume * PhysicsValues.Density[impactMaterial] * (1 - hollowness);
                // Set the mass.
                if (r != null)
                {
                    r.mass = mass;
                }
                else if (articulationBody != null)
                {
                    articulationBody.mass = mass;
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
            // Auto-set friction values.
            if (autoSetFriction)
            {
                dynamicFriction = PhysicsValues.DynamicFriction[impactMaterial];
                staticFriction = PhysicsValues.StaticFriction[impactMaterial];
            }
            // Set the physic material.
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
                float s = b.size.x + b.size.y + b.size.z;
                if (s <= 0.1f)
                {
                    size = 0;
                }
                else if (s <= 0.2f)
                {
                    size = 1;
                }
                else if (s <= 0.5f)
                {
                    size = 2;
                }
                else if (s <= 1)
                {
                    size = 3;
                }
                else if (s <= 3)
                {
                    size = 4;
                }
                else
                {
                    size = 5;
                }
            }
            // Convert the material + size to an impact material.
            ImpactMaterialSized im = ImpactMaterialData.GetImpactMaterialSized(impactMaterial, size);
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
            float angularSpeed;
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
                otherData = floor;
            }
            CollisionEvent collisionEvent;
            // Get the number of contacts.
            int numContacts = collision.contactCount;
            if (numContacts == 0)
            {
                // Get the event.
                collisionEvent = new CollisionEvent(data, otherData, angularSpeed, 0, 0, Vector3d.Zero, OnCollisionType.exit);
                // Add the collision.
                ClatterManager.instance.generator.AddCollision(collisionEvent);
                return;
            }
            // Resize the arrays.
            if (numContacts > contacts.Length)
            {
                Array.Resize(ref contacts, numContacts * 2);
            }
            if (numContacts > contactPoints.Length)
            {
                Array.Resize(ref contactPoints, numContacts * 2);
                Array.Resize(ref contactNormals, numContacts * 2);
            }
            // Get the contacts.
            collision.GetContacts(contacts);
            for (int i = 0; i < numContacts; i++)
            {
                contactPoints[i] = contacts[i].point;
                contactNormals[i] = contacts[i].normal;
            }
            // Get the normalized relative velocity of the collision.
            Vector3 normalizedVelocity = collision.relativeVelocity.normalized;
            float speed = normalizedVelocity.magnitude;
            // Get the normal speeds.
            float normalSpeeds = 0;
            for (int i = 0; i < numContacts; i++)
            {
                normalSpeeds += speed * Mathf.Acos(Mathf.Clamp(Vector3.Dot(contactNormals[i], normalizedVelocity), -1, 1));
            }
            // Get the average normal speed.
            float normalSpeed = normalSpeeds / numContacts;
            // Get the approximate area.
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
            // Get the area.
            double area = Math.PI * radius * radius;
            // Get the event.
            collisionEvent = new CollisionEvent(data, otherData, angularSpeed, normalSpeed, area, centroid, type);
            // Add the collision.
            ClatterManager.instance.generator.AddCollision(collisionEvent);
        }


        /// <summary>
        /// Update the object. This must be called from another script.
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