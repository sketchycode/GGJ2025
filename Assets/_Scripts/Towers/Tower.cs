using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tower : MonoBehaviour, IPowerUppable
{
    [SerializeField] private float cooldownSeconds;
    [SerializeField] private LayerMask collisionLayer;
    [SerializeField] private Transform gunTransform;

    private TowerShotObjectPool shotPool;
    private TowerShotConfig shotConfig;
    private Collider[] hitColliders;
    private Enemy currentTarget;
    private readonly List<TowerShotModifier> modifiers = new();

    private float modifiedRange;
    private float modifiedCooldown;

    public bool IsPowerUppable => true;
    public Transform AttachPoint => gunTransform;

    public void Spawn(TowerShotConfig shotConfig, TowerShotObjectPool shotPool, Collider[] reusableColliders)
    {
        this.shotConfig = shotConfig;
        this.shotPool = shotPool;
        hitColliders = reusableColliders;
        
        modifiedRange = shotConfig.Radius;
        modifiedCooldown = cooldownSeconds;
    }

    public void Install(Transform installPoint)
    {
        transform.position = installPoint.position - (Vector3.up * 15);
        LeanTween
            .move(gameObject, installPoint.position, 4f)
            .setEase(LeanTweenType.easeOutExpo)
            .setOnComplete(_ => StartCoroutine(BeginShooting()));
    }

    public void CollectPowerUp(PowerUp powerUp)
    {
        powerUp.DragToTower(this, () => ApplyPowerUp(powerUp));
    }

    private void ApplyPowerUp(PowerUp powerUp)
    {
        modifiers.Add(powerUp.Modifier);
        
        modifiedRange = shotConfig.Radius * modifiers.Aggregate(1f, (acc, modifier) => acc * modifier.RangeModifier);
        modifiedCooldown = cooldownSeconds + modifiers.Aggregate(1f, (acc, modifier) => acc * modifier.CooldownModifier);
    }

    private IEnumerator BeginShooting()
    {
        while (true)
        {
            CheckForEnemiesInRange();
            if (currentTarget == null)
            {
                yield return null;
            }
            else
            {
                FireAtEnemy();
                yield return new WaitForSeconds(modifiedCooldown);
            }
        }
    }

    private void CheckForEnemiesInRange()
    {
        var minDistance = float.MaxValue;
        var hadTarget = currentTarget != null;
        var foundBubbled = currentTarget?.IsBubbled ?? false;
        
        if (hadTarget && foundBubbled) return;
        
        var hitCount = Physics.OverlapSphereNonAlloc(transform.position, modifiedRange, hitColliders, collisionLayer);

        for (int i = 0; i < hitCount && i < hitColliders.Length; i++)
        {
            var hitCollider = hitColliders[i];

            var enemy = hitCollider.GetComponentInParent<Enemy>();
            if (foundBubbled && !enemy.IsBubbled) continue;
            
            var distance= (hitCollider.transform.position - transform.position).sqrMagnitude;

            if (!foundBubbled && enemy.IsBubbled)
            {
                foundBubbled = true;
                minDistance = distance; // override min distance if this is first bubbled enemy
                currentTarget = enemy;
                continue;
            }
            
            if (distance < minDistance && !hadTarget)
            {
                minDistance = distance;
                currentTarget = enemy;
            }
        }
    }

    private void FireAtEnemy()
    {
        if (currentTarget == null) return;

        if ((currentTarget.transform.position - transform.position).sqrMagnitude > shotConfig.Radius * shotConfig.Radius)
        {
            currentTarget = null;
            return;
        }

        var shot = shotPool.Spawn(shotConfig, modifiers);
        shot.transform.position = gunTransform.position;
        shot.Target(currentTarget.transform.position);
    }
}
