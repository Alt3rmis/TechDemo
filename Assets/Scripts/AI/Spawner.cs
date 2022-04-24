using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityMovementAI
{
    public class Spawner : MonoBehaviour
    {
        public Transform spawnObject;
        public Vector2 objectSizeRange = new Vector2(1, 2);
        public int numberOfObjects = 10;
        public bool randomizeOrientation = false;
        public float boundaryPadding = 1f;
        public float spaceBetweenObjects = 1f;

        // public MovementAIRigidbody[] thingsToAvoid;
        public List<Transform> thingsToTrack;

        public Transform spawnPoint;
        public float spawnOffset = 1f; // offset from the spawn point

        public bool canLoop = true;

        public Camera cam;

        [System.NonSerialized]

        public List<MovementAIRigidbody> spawnedObjects = new List<MovementAIRigidbody>();

        bool CanPlaceObject(float halfSize, Vector3 pos)
        {
            // check if overlap with current spawned objects
            foreach (MovementAIRigidbody obj in spawnedObjects)
            {
                if (Vector3.Distance(obj.Position, pos) < halfSize + obj.Radius)
                {
                    return true;
                }
            }
            return true;
        }
        bool TryToCreateObject()
        {
            float size = Random.Range(objectSizeRange.x, objectSizeRange.y);
            float halfSize = size / 2;

            Vector3 pos = new Vector3();
            pos.x = spawnPoint.position.x + Random.Range(-spawnOffset, spawnOffset);
            pos.y = spawnPoint.position.y + Random.Range(-spawnOffset, spawnOffset);
            pos.z = spawnPoint.position.z + Random.Range(-spawnOffset, spawnOffset);
            
            if (CanPlaceObject(halfSize, pos))
            {
                Transform t = Instantiate(spawnObject, pos, Quaternion.identity) as Transform;
                Rigidbody rb = t.GetComponent<Rigidbody>();
                rb.useGravity = false;
                MovementAIRigidbody mrb = t.GetComponent<MovementAIRigidbody>();
                mrb.canFly = true;
                t.localScale = new Vector3(size, size, size);
                if (randomizeOrientation)
                {
                    t.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                }
                spawnedObjects.Add(t.GetComponent<MovementAIRigidbody>());
                
                return true;
            }
            return false;
        }
        void Awake()
        {
            if(!spawnPoint)
            {
                spawnPoint = transform;
            }
            SequenceArrive sa = spawnObject.GetComponent<SequenceArrive>();
            if (sa)
            {
                sa.sendTargets(thingsToTrack, canLoop); // send the spawned objects to the seq arrive script
            }
        }
        void Start()
        {
            MovementAIRigidbody rb = spawnObject.GetComponent<MovementAIRigidbody>();
            rb.SetUp();
            // should find size of map?
            int count = 0;
            for (int i = 0 ; i < numberOfObjects; i++)
            {
                for (int j = 0; j < 10; j++) // try create obj 10 times
                {
                    if (TryToCreateObject())
                    {
                        break;
                    }
                }
                Debug.Log("Created object:"+count);
                count++;
            }
            InitCamera();
        }

        void InitCamera()
        {
            Instantiate(cam);
        }
    }
}

