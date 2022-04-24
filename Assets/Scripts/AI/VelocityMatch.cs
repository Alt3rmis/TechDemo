using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityMovementAI
{
    public class VelocityMatch : MonoBehaviour
    {
        public float facingCosine = 90;
        public float timeToTarget = 0.1f;
        public float maxAcceleration = 10f;
        float facingCosineVal;
        MovementAIRigidbody rb;
        SteeringBasics steeringBasics;

        void Awake()
        {
            rb = GetComponent<MovementAIRigidbody>();
            steeringBasics = GetComponent<SteeringBasics>();
            facingCosineVal = Mathf.Cos(facingCosine * Mathf.Deg2Rad);
        }

        public Vector3 getSteering(ICollection<MovementAIRigidbody> tragets)
        {
            Vector3 acceleration = Vector3.zero;
            int count = 0;
            foreach(MovementAIRigidbody target in tragets)
            {
                if(steeringBasics.IsFacing(target.Position, facingCosineVal))
                {
                    Vector3 a = target.Velocity - rb.Velocity;
                    a /= timeToTarget;
                    acceleration += a;
                    count++;
                }
            }
            if(count > 0)
            {
                acceleration /= count;
                if(acceleration.magnitude > maxAcceleration)
                {
                    acceleration.Normalize();
                    acceleration *= maxAcceleration;
                }
            }
            return acceleration;
        }
    }

}
