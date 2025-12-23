using UnityEngine;

// This is the base template for all power-ups.
// It's abstract, so other scripts must inherit from it.
public abstract class PowerUp : ScriptableObject
{
    public new string name; // "new" hides the base Object.name
    public string description;
    public float duration;

    // This is called when the power-up is picked up
    // 'player' is the PlayerController that picked it up.
    public abstract void Activate(PlayerController player);

    // This is called when the duration runs out
    public abstract void Deactivate(PlayerController player);
}