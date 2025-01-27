using UnityEngine;

public class EnemyObjectPool : ObjectPool<Enemy>
{
    [SerializeField] private PowerUpObjectPool powerUpObjectPool;
    
    public Ship Ship { get; set; }
    public Transform EndGoal { get; set; }
    
    public Enemy Spawn(Transform spawnPoint)
    {
        var enemy = Spawn();
        enemy.Spawn(Ship, EndGoal, powerUpObjectPool, this);
        enemy.transform.position = spawnPoint.position;
        return enemy;
    }
}
