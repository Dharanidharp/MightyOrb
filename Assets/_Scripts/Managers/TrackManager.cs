using System.Collections.Generic;
using UnityEngine;

public class TrackManager : MonoBehaviour
{
    [Header("Track Settings")]
    [SerializeField] private GameObject[] trackSegmentPrefab; // The prefab of the track segment
    [SerializeField] private int initialSegments = 5;      // Number of track segments spawned at the start
    [SerializeField] private float segmentLength = 20f;    // Length of each track segment
    [SerializeField] private GameObject trackHolder;       // Environment

    [Header("Player Settings")]
    [SerializeField] private Transform playerTransform;    // Reference to the player's transform
    [SerializeField] private GameManager gameManager;      // Reference to the player's score component

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
        if (playerTransform == null)
        {
            return;
        }

        ManageTrackSegments();
    }

    private void ManageTrackSegments()
    {
        // Check if the back end of the first track segment has passed the player.
        if (trackSegments.Peek().transform.position.z + segmentLength < playerTransform.position.z - segmentLength)
        {
            RecycleTrackSegment();
            SpawnTrackSegment();
        }
    }

    private void SpawnTrackSegment()
    {
        GameObject newSegment;

        int prefabIndex = GetPrefabIndexBasedOnScore(gameManager.PlayerScore); // Continually updated based on score
        if (trackSegments.Count < initialSegments)
        {
            newSegment = Instantiate(trackSegmentPrefab[prefabIndex], lastSegmentEndPosition, Quaternion.identity, trackHolder.transform);
        }
        else
        {
            Destroy(trackSegments.Dequeue());
            newSegment = Instantiate(trackSegmentPrefab[prefabIndex], lastSegmentEndPosition, Quaternion.identity, trackHolder.transform);
            newSegment.transform.position = lastSegmentEndPosition;
            // Optionally reset the prefab here if you want to change the segment type
            // This could involve replacing the existing segment with a new prefab
            newSegment = Instantiate(trackSegmentPrefab[prefabIndex], lastSegmentEndPosition, Quaternion.identity, trackHolder.transform);

        }

        trackSegments.Enqueue(newSegment);
        lastSegmentEndPosition = newSegment.transform.position + new Vector3(0, 0, segmentLength);
        ActivateSegmentChildren(newSegment);
    }

    private void RecycleTrackSegment()
    {
        GameObject oldSegment = trackSegments.Dequeue();
        ActivateSegmentChildren(oldSegment);
        oldSegment.transform.position = lastSegmentEndPosition;
        trackSegments.Enqueue(oldSegment);
        lastSegmentEndPosition = oldSegment.transform.position + new Vector3(0, 0, segmentLength);
    }

    private void ActivateSegmentChildren(GameObject segment)
    {
        for (int i = 0; i < segment.transform.childCount; i++)
        {
            segment.transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    private int GetPrefabIndexBasedOnScore(int score)
    {
        // Implement your difficulty scaling logic here.
        // For example, increase difficulty for every 1000 points.
        int index = score / 1000;
        // Ensure the index does not exceed the array bounds.
        return Mathf.Clamp(index, 0, trackSegmentPrefab.Length - 1);
    }
}