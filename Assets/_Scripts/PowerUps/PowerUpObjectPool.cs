using UnityEngine;

public class PowerUpObjectPool : ObjectPool<PowerUp>
{   
    private readonly Collider[] hitColliders = new Collider[50];
    
    public Player Player { get; set; }

    public PowerUp Spawn(Transform position)
    {
        var powerUp = Spawn();
        powerUp.transform.position = position.position;
        powerUp.Spawn(Player, hitColliders, this);
        powerUp.DropToGround(false);
        return powerUp;
    }
}
