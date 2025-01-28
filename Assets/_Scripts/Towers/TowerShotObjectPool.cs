using System.Collections.Generic;
using UnityEngine;

public class TowerShotObjectPool : ObjectPool<TowerShot>
{
    [SerializeField] private VisualEffectObjectPool impactVfxPool;
    
    private readonly Collider[] hitColliders = new Collider[50];
    
    public TowerShot Spawn(TowerShotConfig config, IReadOnlyList<TowerShotModifier> modifiers)
    {
        var shot = Spawn();
        shot.SpawnInit(config, hitColliders, this, modifiers, impactVfxPool);
        return shot;
    }
}