using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

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
    [SerializeField] private PowerUpObjectPool frostFlowerPowerUpObjectPool;
    [SerializeField] private EnemyObjectPool gooblinObjectPool;
    
    [Header("Debug Stuff")]
    [SerializeField] private TowerShotConfig shotConfig;
    [SerializeField] private TowerShotModifier shotModifier;
    
    private Player player;
    private Ship ship;

    private int numEnemies = 5;
    
    private void Start()
    {
        InitializeGameScene();
        ConfigureObjectPools();

        TempSetupStuff();
    }

    private void InitializeGameScene()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SetupPlayer();
        SetupShip();
    }

    private void SetupPlayer()
    {
        player = Instantiate(playerPrefab, gameObjectsContainer);
        player.transform.position = level.PlayerSpawnPoint.position;
        playerCamera.Follow = player.FollowTarget;
        playerCamera.LookAt = player.FollowTarget;
    }

    private void SetupShip()
    {
        ship = Instantiate(shipPrefab, gameObjectsContainer);
        ship.transform.position = level.ShipSpawnPoint.position;
    }

    private void ConfigureObjectPools()
    {
        towerObjectPool.OverrideParentTransform = gameObjectsContainer;
        towerShotObjectPool.OverrideParentTransform = gameObjectsContainer;
        frostFlowerPowerUpObjectPool.OverrideParentTransform = gameObjectsContainer;
        gooblinObjectPool.OverrideParentTransform = gameObjectsContainer;
        
        frostFlowerPowerUpObjectPool.Player = player;
        gooblinObjectPool.Ship = ship;
    }
    
    private void TempSetupStuff()
    {
        foreach (var spawnPoint in level.SpawnPoints)
        {
            for (int i = 0; i < numEnemies; i++)
            {
                gooblinObjectPool.Spawn(spawnPoint);
            }
        }
        
        var tower = towerObjectPool.Spawn(shotConfig);
        tower.Install(level.InstallPoints[0]);
    }
}
