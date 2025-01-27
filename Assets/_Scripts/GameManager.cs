using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField] private TowerObjectPool fireTowerPool;
    [SerializeField] private TowerObjectPool frostTowerPool;
    [SerializeField] private TowerShotObjectPool fireTowerShotPool;
    [SerializeField] private TowerShotObjectPool frostTowerShotPool;
    [SerializeField] private PowerUpObjectPool frostPowerUpObjectPool;
    [SerializeField] private PowerUpObjectPool firePowerUpObjectPool;
    [SerializeField] private EnemyObjectPool gooblinObjectPool;
    [SerializeField] private EnemyObjectPool crabObjectPool;
    
    [Header("Wave Management")]
    [SerializeField] private List<EnemyWaveConfig> waveConfigs;
    [SerializeField] private float timeBetweenWaves = 30f;
    
    [Header("Debug Stuff")]
    [SerializeField] private TowerShotConfig shotConfig;
    [SerializeField] private TowerShotModifier shotModifier;
    
    private Player player;
    private Ship ship;
    
    private int currentWaveIndex;

    public int CurrentWave => currentWaveIndex;
    public int MaxWaves => waveConfigs.Count;
    public bool InAttackPhase { get; private set; }
    public float BuildPhaseRemainingTime { get; private set; }
    public int EnemiesRemainingCurrentWave { get; private set; }
    public float ShipHealth => ship.CurrentHealth;
    public float ShipMaxHealth => ship.MaxHealth;
    
    private void Start()
    {
        InitializeGameScene();
        ConfigureObjectPools();
        
        StartCoroutine(StartGame());
    }

    private void Update()
    {
        if (!InAttackPhase)
        {
            BuildPhaseRemainingTime -= Time.deltaTime;
            BuildPhaseRemainingTime = Mathf.Max(BuildPhaseRemainingTime, 0f);
        }
    }

    private IEnumerator StartGame()
    {
        yield return StartNextWave(10f);
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
        ship.Spawn();
        ship.transform.position = level.ShipSpawnPoint.position;
        ship.transform.rotation = level.ShipSpawnPoint.rotation;
        ship.Died += OnShip_Died;
    }

    private void ConfigureObjectPools()
    {
        fireTowerPool.OverrideParentTransform = gameObjectsContainer;
        frostTowerPool.OverrideParentTransform = gameObjectsContainer;
        fireTowerShotPool.OverrideParentTransform = gameObjectsContainer;
        frostTowerShotPool.OverrideParentTransform = gameObjectsContainer;
        frostPowerUpObjectPool.OverrideParentTransform = gameObjectsContainer;
        firePowerUpObjectPool.OverrideParentTransform = gameObjectsContainer;
        gooblinObjectPool.OverrideParentTransform = gameObjectsContainer;
        crabObjectPool.OverrideParentTransform = gameObjectsContainer;
        
        frostPowerUpObjectPool.Player = player;
        firePowerUpObjectPool.Player = player;
        
        gooblinObjectPool.Ship = ship;
        gooblinObjectPool.EndGoal = level.EndGoalPoint;
        crabObjectPool.Ship = ship;
        crabObjectPool.EndGoal = level.EndGoalPoint;
    }
    
    private IEnumerator StartNextWave(float delay)
    {
        InAttackPhase = false;
        BuildPhaseRemainingTime = delay;
        yield return new WaitForSeconds(delay);

        InAttackPhase = true;
        foreach (var enemySpawn in waveConfigs[currentWaveIndex].Wave)
        {
            for (int i = 0; i < enemySpawn.Count; i++)
            {
                EnemiesRemainingCurrentWave++;
                var enemy = enemySpawn.Pool.Spawn(enemySpawn.SpawnPoint);
                enemy.Died += OnEnemy_Died;
            }
        }
        currentWaveIndex++;
    }

    private void HandleWaveCompleted()
    {
        if (currentWaveIndex < waveConfigs.Count)
        {
            StartCoroutine(StartNextWave(timeBetweenWaves));
        }
        else
        {
            HandleWinGame();
        }
    }

    private void HandleWinGame()
    {
        SceneManager.LoadScene(2);
    }

    private void HandleLoseGame()
    {
        SceneManager.LoadScene(3);
    }

    private void OnEnemy_Died(Enemy obj)
    {
        EnemiesRemainingCurrentWave--;

        if (EnemiesRemainingCurrentWave == 0)
        {
            HandleWaveCompleted();
        }
    }

    private void OnShip_Died(Ship obj)
    {
        HandleLoseGame();
    }
}
