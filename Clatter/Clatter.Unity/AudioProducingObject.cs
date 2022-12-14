﻿using System;
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
        [SerializeField]
        private ImpactMaterial impactMaterial;
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
        [SerializeField]
        [Range(0, 1)]
        private float amp = 0.1f;
        /// <summary>
        /// The resonance value.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        private float resonance = 0.1f;
        /// <summary>
        /// The object's Rigidbody. Can be null.
        /// </summary>
        private Rigidbody r;
        /// <summary>
        /// The object's ArticulationBody. Can be null.
        /// </summary>
        private ArticulationBody articulationBody;
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
        /// Set the underlying AudioObjectData. This must be called once in order for this object to generate audio.
        /// </summary>
        /// <param name="id">This object's ID.</param>
        public void Initialize(uint id)
        {
            // Set the bodies.
            r = GetComponent<Rigidbody>();
            articulationBody = GetComponent<ArticulationBody>();
            // Get the mass.
            float mass;
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
            // Get the size from the volume.
            if (autoSetSize)
            {
                Bounds b = new Bounds(gameObject.transform.position, Vector3.zero);
                // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
                foreach (Renderer re in gameObject.GetComponentsInChildren(typeof(Renderer)))
                {
                    b.Encapsulate(re.bounds);
                }
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
            ImpactMaterialSized im = (ImpactMaterialSized)Enum.Parse(typeof(ImpactMaterialSized), impactMaterial + "_" + size);
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
                otherData = AudioObjectData.floor;
            }
            // Get the number of contacts.
            int numContacts = collision.contactCount;
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
                centroid.x += contactPoints[i].x;
                centroid.y += contactPoints[i].y;
                centroid.z += contactPoints[i].z;
            }
            centroid /= numContacts;
            // Get the square root magnitude to be computationally fast.
            double sqrtMagnitude = 0;
            Vector3d maxPoint = Vector3d.Zero;
            Vector3d p = Vector3d.Zero;
            for (int i = 0; i < numContacts; i++)
            {
                p.x = contactPoints[i].x;
                p.y = contactPoints[i].y;
                p.z = contactPoints[i].z;
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
            CollisionEvent audioEvent = new CollisionEvent(data, otherData, angularSpeed, normalSpeed, area, centroid, type);
            // Add the collision.
            ClatterManager.instance.generator.AddCollision(audioEvent);
        }


        /// <summary>
        /// Update the object. This must be called from another script.
        /// </summary>
        public void OnFixedUpdate()
        {
            // Update the velocity.
            if (r != null)
            {
                data.speed = r.velocity.magnitude;
                data.angularSpeed = r.angularVelocity.magnitude;
            }
            else if (articulationBody != null)
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
            onDestroy?.Invoke(data.id);
        }
    }
}