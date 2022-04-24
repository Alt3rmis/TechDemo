using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityMovementAI
{
    [RequireComponent(typeof(MovementAIRigidbody))]
    public class Cohesion : MonoBehaviour
    {
        public float facingCosine = 120f;
        float facingCosineVal;
        SteeringBasics steeringBasics;

        void Awake()
        {
            steeringBasics = GetComponent<SteeringBasics>();
            facingCosineVal = Mathf.Cos(facingCosine * Mathf.Deg2Rad);
        }

        public Vector3 GetSteering(ICollection<MovementAIRigidbody> targets)
        {
            Vector3 centerOfMass = Vector3.zero;
            int count = 0;
            foreach (MovementAIRigidbody target in targets)
            {
                if (steeringBasics.IsFacing(target.Position, facingCosineVal))
                {
                    centerOfMass += target.Position;
                    count++;
                }
            }
            if (count > 0)
            {
                centerOfMass /= count;
                return steeringBasics.Arrive(centerOfMass);
            }
            return Vector3.zero;
        }
    }
}

