using UnityEngine;

public class EnemyObjectPool : ObjectPool<Enemy>
{
    public PowerUpObjectPool PowerUpObjectPool { get; set; }
    public Ship Ship { get; set; }
    
    public Enemy Spawn(Transform spawnPoint)
    {
        var enemy = Spawn();
        enemy.Spawn(new [] { Ship.transform }, Ship);
        enemy.transform.position = spawnPoint.position;
        return enemy;
    }
}
