using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    [SerializeField] private Transform _playerSpawnPoint;
    [SerializeField] private Transform _shipSpawnPoint;
    [SerializeField] private Transform _endGoalPoint;

    public Transform PlayerSpawnPoint => _playerSpawnPoint;
    public Transform ShipSpawnPoint => _shipSpawnPoint;
    public Transform EndGoalPoint => _endGoalPoint;
}
