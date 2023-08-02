using UnityEngine;

public class StaticObstacle : Obstacle
{
    // Since this is static, it doesn't need to "activate" any behaviors
    public override void ActivateObstacle()
    {
        // Nothing to activate for static obstacles
    }

    // Additional logic can be added here specific to static obstacles
}
