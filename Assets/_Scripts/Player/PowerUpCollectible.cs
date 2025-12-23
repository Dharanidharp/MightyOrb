using UnityEngine;

public class PowerUpCollectible : MonoBehaviour
{
    // You will drag your "Invincibility PowerUp" asset here
    // in the Unity Inspector.
    [SerializeField] private PowerUp powerUp;

    [Header("Effects")]
    [SerializeField] private float rotationSpeed = 100f;

    private void Update()
    {
        // Simple rotation to make it look nice
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that hit us is the Player
        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null && powerUp != null)
        {
            // We found the player!
            // 1. Tell the player to start the power-up
            player.StartPowerUp(powerUp);

            // 2. Hide this collectible
            // We use SetActive(false) so it can be re-activated
            // by the TrackManager's 'ActivateSegmentChildren'
            gameObject.SetActive(false);
        }
    }
}