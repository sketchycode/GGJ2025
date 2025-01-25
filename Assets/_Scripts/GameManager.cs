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
    
    private Player _player;
    
    private void Start()
    {
        // Lock the cursor to the center and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Spawn the player
        _player = Instantiate(playerPrefab, gameObjectsContainer);
        playerCamera.Follow = _player.FollowTarget;
        playerCamera.LookAt = _player.FollowTarget;
        
        var powerUp = Instantiate(powerUpPrefab, gameObjectsContainer);
        powerUp.transform.position = new Vector3(0, 0, 1);
        powerUp.Spawn(_player);
    }
}
