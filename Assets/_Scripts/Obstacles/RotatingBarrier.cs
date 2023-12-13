using System.Collections;
using UnityEngine;

public class RotatingBarrier : Obstacle
{
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0, 100, 0); // Example default value

    private void Start()
    {
        StartCoroutine(Rotate());
    }

    // Start rotating once spawned
    public override void ActivateObstacle()
    {
        
    }

    IEnumerator Rotate()
    {
        while (true)
        {
            transform.Rotate(rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
