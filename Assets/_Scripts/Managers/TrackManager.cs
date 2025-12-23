using System.Collections.Generic;
using UnityEngine;

public class TrackManager : MonoBehaviour
{
    [Header("Track Settings")]
    [SerializeField] private GameObject[] trackSegmentPrefab; // Ensure this is sorted: easiest (index 0) to hardest
    [SerializeField] private int initialSegments = 5;
    [SerializeField] private float segmentLength = 20f;
    [SerializeField] private GameObject trackHolder;

    [Header("Player Settings")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private GameManager gameManager;

    private Queue<GameObject> trackSegments = new Queue<GameObject>();
    private Vector3 lastSegmentEndPosition;

    private void Start()
    {
        lastSegmentEndPosition = Vector3.zero;

        // Spawn initial segments
        for (int i = 0; i < initialSegments; i++)
        {
            SpawnTrackSegment(true); // Pass 'true' for initial spawn
        }
    }

    private void Update()
    {
        if (playerTransform == null || gameManager.IsGameOver)
        {
            return;
        }

        ManageTrackSegments();
    }

    private void ManageTrackSegments()
    {
        // This check is correct: destroys a segment when the player is one full segment past its start
        if (playerTransform.position.z - segmentLength > trackSegments.Peek().transform.position.z)
        {
            // 1. Destroy the oldest segment
            Destroy(trackSegments.Dequeue());

            // 2. Spawn a new one at the end
            SpawnTrackSegment(false);
        }
    }

    private void SpawnTrackSegment(bool isInitialSpawn = false)
    {
        int prefabIndex;

        if (isInitialSpawn)
        {
            prefabIndex = 0; // Always use the easiest prefab (index 0) for the start
        }
        else
        {
            // CHANGED: Now calls the new random function
            prefabIndex = GetRandomPrefabIndexByScore(gameManager.PlayerScore);
        }

        GameObject newSegment = Instantiate(
            trackSegmentPrefab[prefabIndex],
            lastSegmentEndPosition,
            Quaternion.identity,
            trackHolder.transform
        );

        trackSegments.Enqueue(newSegment);
        lastSegmentEndPosition = newSegment.transform.position + new Vector3(0, 0, segmentLength);
        ActivateSegmentChildren(newSegment);
    }

    private void ActivateSegmentChildren(GameObject segment)
    {
        for (int i = 0; i < segment.transform.childCount; i++)
        {
            segment.transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    // --- THIS IS THE FIXED FUNCTION ---
    // CHANGED: Renamed and logic updated for randomness
    private int GetRandomPrefabIndexByScore(int score)
    {
        // 1. Determine the *maximum* difficulty index allowed based on score
        // (e.g., 0-799 score = max index 0, 800-1599 = max index 1)
        int maxAllowedIndex = score / 800;

        // 2. Clamp that index so it doesn't go higher than our available prefabs
        maxAllowedIndex = Mathf.Clamp(maxAllowedIndex, 0, trackSegmentPrefab.Length - 1);

        // 3. Pick a *random* index between 0 (easiest) and the max allowed index (inclusive)
        // Random.Range(min, max) for integers is *exclusive* of the max, so we add 1
        int randomIndex = Random.Range(0, maxAllowedIndex + 1);

        return randomIndex;
    }
}