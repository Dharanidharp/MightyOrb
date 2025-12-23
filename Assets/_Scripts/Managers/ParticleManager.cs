using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance { get; private set; }

    [Header("Particle Prefabs")]
    [SerializeField] private ParticleSystem coinSparklePrefab;

    [Header("Pool Settings")]
    [SerializeField] private int poolSize = 10;

    private List<ParticleSystem> coinSparklePool;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Optional: if you need it across scenes
        }

        InitializePools();
    }

    private void InitializePools()
    {
        // Initialize the Coin Sparkle pool
        coinSparklePool = new List<ParticleSystem>();
        for (int i = 0; i < poolSize; i++)
        {
            ParticleSystem ps = Instantiate(coinSparklePrefab, transform);
            ps.gameObject.SetActive(false); // Start disabled
            coinSparklePool.Add(ps);
        }
    }

    // Call this to play the coin effect
    public void PlayCoinEffect(Vector3 position)
    {
        ParticleSystem ps = GetPooledSparkle();
        if (ps != null)
        {
            ps.transform.position = position;
            ps.gameObject.SetActive(true); // The particle system will play (due to PlayOnAwake)
                                           // and then disable itself (due to Stop Action: Disable)
        }
    }

    // Finds an available particle system from the pool
    private ParticleSystem GetPooledSparkle()
    {
        // Find the first inactive particle system in the pool
        for (int i = 0; i < coinSparklePool.Count; i++)
        {
            if (!coinSparklePool[i].gameObject.activeInHierarchy)
            {
                return coinSparklePool[i];
            }
        }

        // If no inactive ones are found, create a new one (and add it to the pool)
        Debug.LogWarning("Coin sparkle pool exhausted. Creating new instance.");
        ParticleSystem ps = Instantiate(coinSparklePrefab, transform);
        coinSparklePool.Add(ps);
        return ps;
    }
}