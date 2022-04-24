using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityMovementAI
{
    public class Separation : MonoBehaviour
    {
        public float maxAccelerationSep = 25;
        public float maxDistanceSep = 1f;
        MovementAIRigidbody rb;

        void Awake()
        {
            rb = GetComponent<MovementAIRigidbody>();
        }

        public Vector3 GetSteering(ICollection<MovementAIRigidbody> targets)
        {
            Vector3 acceleration = Vector3.zero;
            foreach(MovementAIRigidbody target in targets)
            {
                Vector3 direction = rb.ColliderPosition - target.ColliderPosition;
                float distance = direction.magnitude;

                if(distance < maxDistanceSep)
                {
                    float strength = maxAccelerationSep * (1 - distance / maxDistanceSep);
                    direction.Normalize();
                    direction *= strength;
                    acceleration += direction;
                }
            }
            return acceleration;
        }
    }

}
