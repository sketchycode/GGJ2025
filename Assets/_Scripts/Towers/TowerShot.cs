using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

public class TowerShot : MonoBehaviour
{
    [SerializeField] private LayerMask collisionLayer;
    
    private TowerShotConfig config;
    private Collider[] hitColliders;
    private TowerShotObjectPool shotPool;
    private IReadOnlyList<TowerShotModifier> modifiers;
    private VisualEffectObjectPool impactVfxPool;

    public void SpawnInit(TowerShotConfig config, Collider[] reusableColliders, TowerShotObjectPool shotPool, IReadOnlyList<TowerShotModifier> modifiers, VisualEffectObjectPool impactVfxPool)
    {
        this.config = Instantiate(config);
        hitColliders = reusableColliders;
        this.shotPool = shotPool;
        this.modifiers = modifiers;
        this.impactVfxPool = impactVfxPool;
    }

    public void Target(Vector3 target)
    {
        var startPos = transform.position;
        LeanTween
            .value(gameObject, 0, 1, GetTravelTime(target))
            .setOnUpdate(v =>
            {
                var xzPos = Vector3.Lerp(startPos.ToXZ(), target.ToXZ(), v);
                var yPos = LeanTween.easeInExpo(startPos.y, target.y, v);
                transform.position = new Vector3(xzPos.x, yPos, xzPos.z);
            })
            .setOnComplete(_ => DoDamage());
    }

    private float GetModifiedSpeed()
    {
        return config.Speed * modifiers.Aggregate(1f, (acc, modifier) => acc * modifier.SpeedModifier);
    }

    private float GetModifiedDamage()
    {
        return config.Damage * modifiers.Aggregate(1f, (acc, modifier) => acc * modifier.DamageModifier);
    }

    private float GetModifiedRadius()
    {
        return config.Radius * modifiers.Aggregate(1f, (acc, modifier) => acc * modifier.SplashRadiusModifier);
    }

    private float GetTravelTime(Vector3 target)
    {
        var distance = (transform.position.ToXZ() - target.ToXZ()).magnitude;
        return distance / GetModifiedSpeed();
    }

    private void DoDamage()
    {
        var hitCount = Physics.OverlapSphereNonAlloc(transform.position, GetModifiedRadius(), hitColliders, collisionLayer);
        var damage= GetModifiedDamage();
        for (int i = 0; i < hitCount && i < hitColliders.Length; i++)
        {
            var hitCollider = hitColliders[i];

            var enemy = hitCollider.GetComponentInParent<Enemy>();
            enemy.TakeDamage(damage);
        }
        impactVfxPool.Spawn(transform);
        shotPool.Despawn(this);
    }

    private void DamageEnemy(Enemy enemy, float damage)
    {
        if (enemy == null) return;
        
        enemy.TakeDamage(damage);
    }
}
