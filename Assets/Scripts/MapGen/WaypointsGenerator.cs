using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointsGenerator : MonoBehaviour
{
    public int numberOfWaypoints = 10;
    public Transform waypointPrefab;

    public Transform interestingPoint;
    public int waypointsRadius = 500;
    MapGenerator mapGenerator;

    List<Vector3> waypoints;

    List<Transform> waypointsObjects;

    public void Awake()
    {
        
    }
    List<Vector3> CalculateWaypoints()
    {
        mapGenerator = GetComponent<MapGenerator>();
        waypoints = new List<Vector3>();
        waypointsObjects = new List<Transform>();
        Vector3 iPoint;
        if(interestingPoint == null)
        {
            iPoint = mapGenerator.GetInterestingPoints();
        } else{
            iPoint = interestingPoint.position;
        }
            
        float perAngle = 2 * Mathf.PI / numberOfWaypoints;
        for (int i = 0; i < numberOfWaypoints; i++)
        {
            float angle = perAngle * i;
            Vector3 point = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * waypointsRadius;
            waypoints.Add(iPoint + point);
        }
        waypoints = RandomizeWaypoints(waypoints, 100f);
        return waypoints;
    }

    List<Vector3> RandomizeWaypoints(List<Vector3> waypoints, float height)
    {
        List<Vector3> newWaypoints = new List<Vector3>();
        for (int i = 0; i < waypoints.Count; i++)
        {
            newWaypoints.Add(new Vector3(waypoints[i].x + Random.Range(-100f, 100f), Random.Range(height-50f, height+150f), waypoints[i].z + Random.Range(-100f, 100f)));
        }
        return newWaypoints;
    }
    public List<Transform> GenerateWaypoints()
    {
        ClearWaypoints();
        waypoints = CalculateWaypoints();
        for(int i = 0; i < waypoints.Count; i++)
        {
            waypointsObjects.Add(Instantiate(waypointPrefab, waypoints[i], Quaternion.identity));
        }
        return waypointsObjects;
    }

    public void ClearWaypoints()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Waypoint");
        foreach(GameObject obj in objects)
        {
            DestroyImmediate(obj.gameObject);
        }
    }
}
