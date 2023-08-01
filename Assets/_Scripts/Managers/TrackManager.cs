using System.Collections.Generic;
using UnityEngine;

public class TrackManager : MonoBehaviour
{
    [Header("Track Settings")]
    [SerializeField] private GameObject trackSegmentPrefab; // The prefab of the track segment
    [SerializeField] private int initialSegments = 5;      // Number of track segments spawned at the start
    [SerializeField] private float segmentLength = 10f;    // Length of each track segment
    [SerializeField] private GameObject trackHolder; // Environment

    [Header("Player Settings")]
    [SerializeField] private Transform playerTransform;    // Reference to the player's transform

    private Queue<GameObject> trackSegments = new Queue<GameObject>(); // Queue holding the active track segments
    private Vector3 lastSegmentEndPosition;                            // End position of the last segment in the track

    private void Start()
    {
        // Initialize the end position of the last segment to the starting position (origin)
        lastSegmentEndPosition = Vector3.zero;

        // Spawn initial segments
        for (int i = 0; i < initialSegments; i++)
        {
            SpawnTrackSegment();
        }
    }

    private void Update()
    {
        // Check if the back end of the first track segment has passed the player.
        if (trackSegments.Peek().transform.position.z + segmentLength < playerTransform.position.z - segmentLength)
        {
            RecycleTrackSegment();
            SpawnTrackSegment();
        }
    }

    /// <summary>
    /// Spawns a new track segment. 
    /// If there are enough segments, it recycles the oldest segment.
    /// Else, it instantiates a new segment.
    /// </summary>
    private void SpawnTrackSegment()
    {
        GameObject newSegment;
        if (trackSegments.Count < initialSegments)
        {
            newSegment = Instantiate(trackSegmentPrefab, lastSegmentEndPosition, Quaternion.identity, trackHolder.transform);
        }
        else
        {
            newSegment = trackSegments.Dequeue();
            newSegment.transform.position = lastSegmentEndPosition;
        }

        // Add the new segment to the active segments queue
        trackSegments.Enqueue(newSegment);

        // Update the end position of the last segment in the track for the next segment
        lastSegmentEndPosition = newSegment.transform.position + new Vector3(0, 0, segmentLength);
    }

    /// <summary>
    /// Recycles the oldest track segment by moving it to the front.
    /// </summary>
    private void RecycleTrackSegment()
    {
        GameObject oldSegment = trackSegments.Dequeue();
        oldSegment.transform.position = lastSegmentEndPosition;
        trackSegments.Enqueue(oldSegment);

        // Update the end position of the last segment in the track for the next segment
        lastSegmentEndPosition = oldSegment.transform.position + new Vector3(0, 0, segmentLength);
    }
}
