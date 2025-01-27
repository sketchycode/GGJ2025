using UnityEngine;

public interface IPowerUppable
{
    bool IsPowerUppable { get; }
    Transform AttachPoint { get; }
    void CollectPowerUp(PowerUp powerUp);
}