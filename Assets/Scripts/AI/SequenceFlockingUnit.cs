using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityMovementAI
{
    public class SequenceFlockingUnit : MonoBehaviour
    {
        public float cohesionWeight = 1.5f;
        public float separationWeight = 2f;
        public float velocityMatchWeight = 1f;
        SteeringBasics steeringBasics;
        SequenceArrive arrive;
        Cohesion cohesion;
        Separation separation;
        VelocityMatch velocityMatch;
        Sensor sensor;
        MovementAIRigidbody rb;

        List<bool> hasArrived = new List<bool>();

        void Start()
        {
            rb = GetComponent<MovementAIRigidbody>();
            steeringBasics = GetComponent<SteeringBasics>();
            arrive = GetComponent<SequenceArrive>();
            cohesion = GetComponent<Cohesion>();
            separation = GetComponent<Separation>();
            velocityMatch = GetComponent<VelocityMatch>();
            sensor = transform.Find("Sensor").GetComponent<Sensor>();
            foreach(Transform mrb in arrive.mrbList)
            {
                hasArrived.Add(false);
            }
        }

        Transform GetNextTarget()
        {
            Transform target = null;
            int i = 0;
            while(i < arrive.mrbList.Count)
            {
                if(hasArrived[i])
                {
                    i++;
                }
                else
                {
                    target = arrive.mrbList[i];
                    break;
                }
            }
            if(target == null)
            {
                if(arrive.canLoop)
                {
                    i = 0;
                    while(i < arrive.mrbList.Count)
                    {
                        hasArrived[i] = false;
                        i++;
                    }
                    target = arrive.mrbList[0];
                }
            }
            return target;
        }
        void FixedUpdate()
        {
            Vector3 acceleration = Vector3.zero;
            acceleration += cohesion.GetSteering(sensor.Targets) * cohesionWeight;
            acceleration += separation.GetSteering(sensor.Targets) * separationWeight;
            acceleration += velocityMatch.getSteering(sensor.Targets) * velocityMatchWeight;

            Transform nextTarget = GetNextTarget();
            acceleration += arrive.GetSteering(nextTarget) * velocityMatchWeight; // get next target from target list

            if(nextTarget != null)
            {
                if(steeringBasics.hasArrived(nextTarget.position))
                {
                    hasArrived[arrive.mrbList.IndexOf(nextTarget)] = true;
                }
            }

            steeringBasics.Steer(acceleration);
            steeringBasics.LookWhereGoing();
        }
    }
}
