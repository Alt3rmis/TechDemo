using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityMovementAI
{
    
    public class SequenceArrive : MonoBehaviour
    {
        MovementAIRigidbody rb;
        SteeringBasics steeringBasics;

        public List<Transform> mrbList = new List<Transform>();
        public bool canLoop;

        void Awake()
        {
            rb = GetComponent<MovementAIRigidbody>();
            steeringBasics = GetComponent<SteeringBasics>();
        }

        public Vector3 GetSteering(MovementAIRigidbody target)
        {
            Vector3 acceleration = steeringBasics.Arrive(target.Position);
            return acceleration;
        }

        public Vector3 GetSteering(Transform transform)
        {
            Vector3 acceleration = steeringBasics.Arrive(transform.position);
            return acceleration;
        }

        public void sendTargets(List<Transform> objects, bool cl)
        {
            mrbList = objects;
            canLoop = cl;
        }
    }
}
