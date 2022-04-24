using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityMovementAI
{
    public class MovementAIRigidbody : MonoBehaviour
    {
        [Header("3D Settings")]
        public bool canFly = true;

        [Header("3D Ground Settings")]
        // public bool stayGrounded = false;
        // public float groundFollowDistance = 0.1f; // 
        // public LayerMask groundMask = Physics.DefaultRaycastLayers;
        // public float slopeLimit = 45f; // The maximum angle of slopes that the AI can walk up
        SphereCollider col;

        public float Radius
        {
            get
            {
                if(col)
                {
                    return Mathf.Max(rb.transform.localScale.x, rb.transform.localScale.y, rb.transform.localScale.z)*col.radius;
                }
                else
                {
                    return -1;
                }
            }
        }

        [System.NonSerialized]
        Rigidbody rb;
        void SetUpRigidbody()
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb)
            {
                this.rb = rb;
            }
            else
            {
                Debug.LogError("No Rigidbody found on " + gameObject.name + "!");
            }
        }

        void SetUpCollider()
        {
            SphereCollider col = rb.GetComponent<SphereCollider>();
            if (col)
            {
                this.col = col;
            }
            else
            {
                Debug.LogError("No SphereCollider found on " + gameObject.name + "!");
            }
        }

        public void SetUp()
        {
            SetUpRigidbody();
            SetUpCollider();
        }
        
        void Awake()
        {
            SetUp();
        }

        public Transform Transform
        {
            get{
                return rb.transform;
            }
        }
        public Vector3 ColliderPosition
        {
            get
            {
                return Transform.TransformPoint(col.center) + rb.position - Transform.position;
            }
        }
        public Vector3 Velocity
        {
            get
            {
                return rb.velocity;
            }
            set
            {
                rb.velocity = value;
            }
        }

        public Vector3 RealVelocity
        {
            get
            {
                return rb.velocity;
            }
            set
            {
                rb.velocity = value;
            }
        }

        public Quaternion Rotation
        {
            get
            {
                return rb.rotation;
            }
            set
            {
                rb.MoveRotation(value);
            }
        }

        public Vector3 Position
        {
            get
            {
                return rb.position;
            }
        }
        int count = 0;
        int countDebug = 0;

        void Start()
        {
            StartCoroutine(DebugDraw());
        }
        IEnumerator DebugDraw()
        {
            yield return new WaitForFixedUpdate();

            Vector3 origin = ColliderPosition;
            Debug.DrawLine(origin, origin + (Velocity.normalized), Color.red, 0f, false);
            Debug.DrawLine(origin, origin + (RealVelocity.normalized), Color.green, 0f, false);
            count++;
            countDebug++;
            StartCoroutine(DebugDraw());
        }

    }

}
