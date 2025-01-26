using UnityEngine;

public class TowerObjectPool : ObjectPool<Tower>
{
    [SerializeField] TowerShotObjectPool shotPool;
    private readonly Collider[] hitColliders = new Collider[50];
    
    public Collider[] HitColliders => hitColliders;
    
    public Tower Spawn(TowerShotConfig shotConfig, Transform spawnPoint)
    {
        var tower = Spawn();
        tower.Spawn(shotConfig, shotPool, hitColliders);
        tower.Install(spawnPoint);
        return tower;
    }
}
