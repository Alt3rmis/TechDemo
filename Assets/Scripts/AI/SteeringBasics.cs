using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityMovementAI
{
    public class SteeringBasics : MonoBehaviour
    {
        [Header("General")]
        public float maxVelocity = 5f;
        public float maxAcceleration = 10f;
        public float turnSpeed = 10f;
        [Header("Arrive")]
        public float targetRadius = 1f; // radius from the target where the agent is considered to be close enough
        public float slowRadius = 3f; // radius from the target where the agent is considered to be close enough to start slowing down
        public float timeToTarget = 0.1f; // time in seconds to achieve the target speed

        [Header("Orientation")]
        public bool smoothing = true;
        public int numSamples = 5;
        Queue<Vector3> velocitySamples = new Queue<Vector3>();

        MovementAIRigidbody rb;

        void Awake()
        {
            rb = GetComponent<MovementAIRigidbody>();
        }

        public void Steer(Vector3 linearAcceleration) // update current game object's velocity
        {
            rb.Velocity += linearAcceleration * Time.deltaTime;
            if(rb.Velocity.magnitude > maxVelocity)
            {
                rb.Velocity = rb.Velocity.normalized * maxVelocity;
            }
        }

        public Vector3 Seek(Vector3 targetPosition, float maxSeekAccel) // return an acceleration towards target
        {
            Vector3 acceleration = targetPosition - transform.position;
            acceleration.Normalize();
            acceleration *= maxSeekAccel;
            return acceleration;
        }

        public Vector3 Seek(Vector3 targetPosition)
        {
            return Seek(targetPosition, maxAcceleration);
        }

        public void LookWhereGoing()
        {
            Vector3 direction = rb.Velocity;
            if(smoothing)
            {
                if(velocitySamples.Count == numSamples)
                {
                    velocitySamples.Dequeue();
                }
                velocitySamples.Enqueue(rb.Velocity);
                direction = Vector3.zero;
                foreach(Vector3 v in velocitySamples)
                {
                    direction += v;
                }
                direction /= velocitySamples.Count;
            }
            LookAtDirection(direction);
        }
        public void LookAtDirection(Vector3 direction)
        {
            direction.Normalize();
            if(direction.sqrMagnitude > 0.001f)
            {
                float toRotation = (Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg);
                float rotation = Mathf.LerpAngle(rb.Rotation.eulerAngles.y, toRotation, Time.deltaTime * turnSpeed);
                rb.Rotation = Quaternion.Euler(0f, rotation, 0f);
            }
        }

        public void LookAtDirection(float toRotation)
        {
            float rotation = Mathf.LerpAngle(rb.Rotation.eulerAngles.y, toRotation, Time.deltaTime * turnSpeed);
            rb.Rotation = Quaternion.Euler(0f, rotation, 0f);
        }
        public void LookAtDirection(Quaternion toRotation)
        {
            LookAtDirection(toRotation.eulerAngles.y);
        }

        public Vector3 Arrive(Vector3 targetPosition)
        {
            Debug.DrawLine(transform.position, targetPosition, Color.cyan, 0f, false); // draw the line to the target
            Vector3 targetVelocity = targetPosition - rb.Position;
            float dist = targetPosition.magnitude; // get the distance to the target
            if(dist < targetRadius)
            {
                rb.Velocity = Vector3.zero;
                return Vector3.zero;
            }

            // calculate the target speed, don't worry about too high of a speed, we'll slow down later
            float targetSpeed;
            if(dist > slowRadius)
            {
                targetSpeed = maxVelocity;
            }
            else
            {
                targetSpeed = maxVelocity * (dist / slowRadius);
            }

            targetVelocity.Normalize();
            targetVelocity *= targetSpeed; // assgin the target velocity

            Vector3 acceleration = targetVelocity - rb.Velocity;
            acceleration /= timeToTarget; // acceleration tries to get to the target velocity

            if(acceleration.magnitude > maxAcceleration)
            {
                acceleration = acceleration.normalized * maxAcceleration;
            }

            return acceleration;
        }

        public bool IsFacing(Vector3 target, float cosinValue)
        {
            Vector3 facing = transform.right.normalized;
            Vector3 toTarget = (target - transform.position).normalized;
            return Vector3.Dot(facing, toTarget) > cosinValue;
        }

        public bool hasArrived(Vector3 targetPosition)
        {
            return (targetPosition - rb.Position).magnitude < targetRadius;
        }

    }

}
