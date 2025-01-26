using UnityEngine;

public interface IPowerUppable
{
    Transform AttachPoint { get; }
    void CollectPowerUp(PowerUp powerUp);
}