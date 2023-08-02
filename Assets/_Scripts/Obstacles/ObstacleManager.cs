using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> staticObstacles; // Assign in inspector
    [SerializeField] private List<GameObject> dynamicObstacles; // Assign in inspector

    // Spawn a static obstacle at a given position
    public void SpawnStaticObstacle(Vector3 position)
    {
        GameObject obs = Instantiate(staticObstacles[Random.Range(0, staticObstacles.Count)], position, Quaternion.identity);
        obs.transform.SetParent(transform);
    }

    // Spawn a dynamic obstacle at a given position and activate it
    public void SpawnDynamicObstacle(Vector3 position)
    {
        GameObject obs = Instantiate(dynamicObstacles[Random.Range(0, dynamicObstacles.Count)], position, Quaternion.identity);
        obs.GetComponent<Obstacle>().ActivateObstacle();
        obs.transform.SetParent(transform);
    }

    // You can add further methods for logic like determining spawn rates, obstacle variety based on player progress, etc.
}
