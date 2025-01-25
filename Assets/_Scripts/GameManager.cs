using System;
using Unity.Cinemachine;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Player playerPrefab;
    [SerializeField] private CinemachineCamera _camera;
    [SerializeField] private Transform _gameObjectsContainer;
    
    private Player _player;
    
    private void Start()
    {
        // Lock the cursor to the center and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Spawn the player
        _player = Instantiate(playerPrefab, _gameObjectsContainer);
        _camera.Follow = _player.FollowTarget;
        _camera.LookAt = _player.FollowTarget;
    }
}
