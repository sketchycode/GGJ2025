using Unity.Cinemachine;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Instanced References")]
    [SerializeField] private CinemachineCamera playerCamera;
    [SerializeField] private Transform gameObjectsContainer;
    [SerializeField] private Transform enemySpawnPoint;
    [SerializeField] private Level level;
    
    [Header("Prefabs")]
    [SerializeField] private Player playerPrefab;
    [SerializeField] private PowerUp powerUpPrefab;
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private Ship shipPrefab;
    
    [Header("Internal References")]
    [SerializeField] private TowerObjectPool towerObjectPool;
    [SerializeField] private TowerShotObjectPool towerShotObjectPool;
    [SerializeField] private PowerUpObjectPool powerUpObjectPool;
    
    [Header("Debug Stuff")]
    [SerializeField] private TowerShotConfig shotConfig;
    [SerializeField] private TowerShotModifier shotModifier;
    
    private Player player;

    private int numEnemies = 5;
    
    private void Start()
    {
        // Lock the cursor to the center and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        towerObjectPool.OverrideParentTransform = gameObjectsContainer;
        towerShotObjectPool.OverrideParentTransform = gameObjectsContainer;
        powerUpObjectPool.OverrideParentTransform = gameObjectsContainer;
        
        // Spawn the player
        player = Instantiate(playerPrefab, gameObjectsContainer);
        player.transform.position = level.PlayerSpawnPoint.position;
        playerCamera.Follow = player.FollowTarget;
        playerCamera.LookAt = player.FollowTarget;
        
        powerUpObjectPool.Player = player;
        
        var ship = Instantiate(shipPrefab, gameObjectsContainer);
        ship.transform.position = level.ShipSpawnPoint.position;

        foreach (var spawnPoint in level.SpawnPoints)
        {
            for (int i = 0; i < numEnemies; i++)
            {
                var enemy = Instantiate(enemyPrefab, gameObjectsContainer);
                enemy.transform.position = spawnPoint.position;
                enemy.Spawn(new[] { ship.transform }, player, ship);
                enemy.Died += OnEnemy_Died;
            }
        }

        var tower = towerObjectPool.Spawn(shotConfig);
        tower.Install(level.InstallPoints[0]);
    }

    private void OnEnemy_Died(Enemy obj)
    {
        var powerUp = powerUpObjectPool.Spawn(obj.transform);
    }
}
