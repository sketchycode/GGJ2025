using UnityEngine;

public class TowerShot : MonoBehaviour
{
    [SerializeField] private LayerMask collisionLayer;
    
    private TowerShotConfig config;
    private Collider[] hitColliders;
    private TowerShotObjectPool shotPool;

    public void SpawnInit(TowerShotConfig config, Collider[] reusableColliders, TowerShotObjectPool shotPool)
    {
        this.config = ScriptableObject.Instantiate(config);
        hitColliders = reusableColliders;
        this.shotPool = shotPool;
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

    private float GetTravelTime(Vector3 target)
    {
        var distance = (transform.position.ToXZ() - target.ToXZ()).magnitude;
        return distance / config.Speed;
    }

    private void DoDamage()
    {
        var hitCount = Physics.OverlapSphereNonAlloc(transform.position, config.Radius, hitColliders, collisionLayer);

        for (int i = 0; i < hitCount && i < hitColliders.Length; i++)
        {
            var hitCollider = hitColliders[i];

            var enemy = hitCollider.GetComponentInParent<Enemy>();
            DamageEnemy(enemy);
        }
        
        shotPool.Despawn(this);
    }

    private void DamageEnemy(Enemy enemy)
    {
        if (enemy == null) return;
        
        enemy.TakeDamage(config.Damage);
    }
}
