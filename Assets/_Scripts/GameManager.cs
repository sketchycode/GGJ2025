using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Instanced References")]
    [SerializeField] private CinemachineCamera playerCamera;
    [SerializeField] private Transform gameObjectsContainer;
    
    [Header("Prefabs")]
    [SerializeField] private Player playerPrefab;
    [SerializeField] private PowerUp powerUpPrefab;
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private Ship shipPrefab;
    
    [SerializeField] private List<Transform> waypoints;
    
    private Player player;
    
    private void Start()
    {
        // Lock the cursor to the center and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Spawn the player
        player = Instantiate(playerPrefab, gameObjectsContainer);
        playerCamera.Follow = player.FollowTarget;
        playerCamera.LookAt = player.FollowTarget;
        
        var ship = Instantiate(shipPrefab, gameObjectsContainer);
        ship.transform.position = new Vector3(-5, 0, -5);
        
        waypoints.Add(ship.transform);

        var enemy = Instantiate(enemyPrefab, gameObjectsContainer);
        enemy.transform.position = new Vector3(5, 0, 5);
        enemy.Spawn(waypoints.ToArray(), player, ship);
        enemy.Died += OnEnemy_Died;
    }

    private void OnEnemy_Died(Enemy obj)
    {
        var powerUp = Instantiate(powerUpPrefab, gameObjectsContainer);
        powerUp.transform.position = obj.transform.position;
        powerUp.Spawn(player);
        powerUp.DropToGround();
    }
}
