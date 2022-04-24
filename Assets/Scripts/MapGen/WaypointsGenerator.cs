using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointsGenerator : MonoBehaviour
{
    public int numberOfWaypoints = 10;
    public Transform waypointPrefab;

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
        Vector3 iPoint = mapGenerator.GetInterestingPoints();
        float perAngle = 2 * Mathf.PI / numberOfWaypoints;
        for (int i = 0; i < numberOfWaypoints; i++)
        {
            float angle = perAngle * i;
            Vector3 point = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * waypointsRadius;
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
            newWaypoints.Add(new Vector3(waypoints[i].x + Random.Range(-50f, 50f), Random.Range(height-20f, height+50f), waypoints[i].z + Random.Range(-50f, 50f)));
        }
        return newWaypoints;
    }
    public List<Transform> GenerateWaypoints()
    {
        waypoints = CalculateWaypoints();
        Debug.Log("waypoints: " + waypoints.Count);
        for(int i = 0; i < waypoints.Count; i++)
        {
            Debug.Log("Generating waypoint " + i);
            // Instantiate(waypointPrefab, waypoints[i], Quaternion.identity);
            waypointsObjects.Add(Instantiate(waypointPrefab, waypoints[i], Quaternion.identity));
        }
        return waypointsObjects;
    }
}
