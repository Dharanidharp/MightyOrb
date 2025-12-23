using UnityEngine;

[CreateAssetMenu(fileName = "CoinMagnet PowerUp", menuName = "Mighty Orb/PowerUp - Coin Magnet")]
public class CoinMagnetPowerUp : PowerUp
{
    [Header("Magnet Settings")]
    [Tooltip("The radius around the player where coins will be attracted.")]
    public float magnetRadius = 5f; // How far the magnet pulls coins

    [Tooltip("How quickly coins move towards the player.")]
    public float magnetStrength = 10f; // How fast coins move

    public override void Activate(PlayerController player)
    {
        if (player == null) return;

        // We'll tell the player to activate its magnet ability
        player.SetCoinMagnet(true, magnetRadius, magnetStrength);
        Debug.Log($"Coin Magnet activated! Radius: {magnetRadius}, Strength: {magnetStrength}");
        // Optional: Play a "MagnetOn" sound effect
        // SoundManager.Instance.PlayMagnetOnSFX();
    }

    public override void Deactivate(PlayerController player)
    {
        if (player == null) return;

        // Tell the player to deactivate its magnet ability
        player.SetCoinMagnet(false, 0f, 0f); // Pass 0s as radius/strength are no longer relevant
        Debug.Log("Coin Magnet deactivated.");
        // Optional: Play a "MagnetOff" sound effect
        // SoundManager.Instance.PlayMagnetOffSFX();
    }
}