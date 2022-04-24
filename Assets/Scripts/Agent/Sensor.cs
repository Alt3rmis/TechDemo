using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityMovementAI
{
    public class Sensor : MonoBehaviour
    {
        HashSet<MovementAIRigidbody> targets = new HashSet<MovementAIRigidbody>();

        static bool IsNull(MovementAIRigidbody r)
        {
            return (r == null||r.Equals(null));
        }
        public HashSet<MovementAIRigidbody> Targets
        {
            get
            {
                targets.RemoveWhere(IsNull);
                return targets;
            }
        }

        void TryToAdd(Component other)
        {
            MovementAIRigidbody r = other.GetComponent<MovementAIRigidbody>();
            if (r != null)
            {
                targets.Add(r);
            }
        }

        void TryToRemove(Component other)
        {
            MovementAIRigidbody r = other.GetComponent<MovementAIRigidbody>();
            if (r != null)
            {
                targets.Remove(r);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            TryToAdd(other);
        }

        void OnTriggerExit(Collider other)
        {
            TryToRemove(other);
        }
    }
}
