using System.Collections;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [SerializeField] private float cooldownSeconds;
    [SerializeField] private LayerMask collisionLayer;
    [SerializeField] private Transform gunTransform;

    private TowerShotObjectPool shotPool;
    private TowerShotConfig shotConfig;
    private Collider[] hitColliders;
    private Enemy currentTarget;

    public void Spawn(TowerShotConfig shotConfig, TowerShotObjectPool shotPool, Collider[] reusableColliders)
    {
        this.shotConfig = shotConfig;
        this.shotPool = shotPool;
        hitColliders = reusableColliders;
    }

    public void Install(Transform installPoint)
    {
        transform.position = installPoint.position - (Vector3.up * 10);
        LeanTween
            .move(gameObject, installPoint.position, 1f)
            .setEase(LeanTweenType.easeOutExpo)
            .setOnComplete(_ => StartCoroutine(BeginShooting()));
    }

    private IEnumerator BeginShooting()
    {
        while (true)
        {
            if (currentTarget == null)
            {
                CheckForEnemiesInRange();
                yield return null;
            }
            else
            {
                FireAtEnemy();
                yield return new WaitForSeconds(cooldownSeconds);
            }
        }
    }

    private void CheckForEnemiesInRange()
    {
        Debug.Log($"searching for enemies in range of {shotConfig.Radius}");
        var minDistance = float.MaxValue;
        var foundBubbled = false;
        currentTarget = null;
        
        var hitCount = Physics.OverlapSphereNonAlloc(transform.position, shotConfig.Radius, hitColliders, collisionLayer);

        for (int i = 0; i < hitCount && i < hitColliders.Length; i++)
        {
            var hitCollider = hitColliders[i];

            var enemy = hitCollider.GetComponentInParent<Enemy>();
            if (enemy == null) continue;
            if (foundBubbled && !enemy.IsBubbled) continue;
            
            var distance = (hitCollider.transform.position - transform.position).sqrMagnitude;

            if (!foundBubbled && enemy.IsBubbled)
            {
                foundBubbled = true;
                minDistance = distance; // override min distance if this is first bubbled enemy
                currentTarget = enemy;
                continue;
            }
            
            if (distance < minDistance)
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
            Debug.Log("target out of range");
            currentTarget = null;
            return;
        }

        Debug.Log($"firing at enemy at {currentTarget.transform.position}");
        var shot = shotPool.Spawn(shotConfig);
        shot.transform.position = gunTransform.position;
        shot.Target(currentTarget.transform.position);
    }
}
