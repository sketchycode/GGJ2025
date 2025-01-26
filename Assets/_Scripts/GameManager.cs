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
    
    private Player player;

    private int numEnemies = 10;
    
    private void Start()
    {
        // Lock the cursor to the center and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Spawn the player
        player = Instantiate(playerPrefab, gameObjectsContainer);
        player.transform.position = level.PlayerSpawnPoint.position;
        playerCamera.Follow = player.FollowTarget;
        playerCamera.LookAt = player.FollowTarget;
        
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
    }

    private void OnEnemy_Died(Enemy obj)
    {
        var powerUp = Instantiate(powerUpPrefab, gameObjectsContainer);
        powerUp.transform.position = obj.transform.position;
        powerUp.Spawn(player);
        powerUp.DropToGround();
    }
}
