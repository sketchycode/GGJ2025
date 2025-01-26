using UnityEngine;

public class TowerShotObjectPool : ObjectPool<TowerShot>
{
    private readonly Collider[] hitColliders = new Collider[50];
    
    public TowerShot Spawn(TowerShotConfig config)
    {
        var shot = Spawn();
        shot.SpawnInit(config, hitColliders, this);
        return shot;
    }
}