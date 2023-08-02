using UnityEngine;

public abstract class Obstacle : MonoBehaviour
{
    // This is where logic for how the obstacle behaves goes
    public abstract void ActivateObstacle();

    // Logic for when the player collides with this obstacle
    public virtual void PlayerCollision()
    {
        // Default action on collision, can be overridden by derived classes
        // For instance, you might reduce player health here.
    }
}
