using System.Collections;
using System.Collections.Generic;
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
    
    public float WaveProgress => currentWaveIndex / (float) waveConfigs.Count;
    public bool InAttackPhase { get; private set; }
    public float BuildPhaseRemainingTime { get; private set; }
    public int EnemiesRemainingCurrentWave { get; private set; }
    
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
        yield return StartWave(waveConfigs[currentWaveIndex], 10f);
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
        crabObjectPool.Ship = ship;
    }
    
    private IEnumerator StartWave(EnemyWaveConfig config, float delay)
    {
        InAttackPhase = false;
        BuildPhaseRemainingTime = delay;
        yield return new WaitForSeconds(delay);

        InAttackPhase = true;
        foreach (var enemySpawn in config.Wave)
        {
            for (int i = 0; i < enemySpawn.Count; i++)
            {
                EnemiesRemainingCurrentWave++;
                var enemy = enemySpawn.Pool.Spawn(enemySpawn.SpawnPoint);
                enemy.Died += OnEnemy_Died;
            }
        }
    }

    private void HandleWaveCompleted()
    {
        currentWaveIndex++;
        if (currentWaveIndex < waveConfigs.Count)
        {
            StartCoroutine(StartWave(waveConfigs[currentWaveIndex], timeBetweenWaves));
        }
        else
        {
            HandleWinGame();
        }
    }

    private void HandleWinGame()
    {
        Debug.Log("Win Game");
    }

    private void HandleLoseGame()
    {
        Debug.Log("Lose Game");
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
