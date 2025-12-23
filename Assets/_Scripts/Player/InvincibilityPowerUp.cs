using UnityEngine;

// This attribute lets us right-click in the Project to create this as an asset.
[CreateAssetMenu(fileName = "Invincibility PowerUp", menuName = "Mighty Orb/PowerUp - Invincibility")]
public class InvincibilityPowerUp : PowerUp
{
    // This is what happens when we pick it up
    public override void Activate(PlayerController player)
    {
        if (player == null) return;

        // We'll tell the player to set its internal "IsInvincible" flag
        player.SetInvincibility(true);
        // You could also tell the SoundManager to play a "PowerUp!" sound here
        // SoundManager.Instance.PlayPowerUpActivateSFX();
    }

    // This is what happens when it wears off
    public override void Deactivate(PlayerController player)
    {
        if (player == null) return;

        // We tell the player it's no longer invincible
        player.SetInvincibility(false);
        // SoundManager.Instance.PlayPowerUpDeactivateSFX();
    }
}