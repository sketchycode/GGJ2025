using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    [SerializeField] private List<Transform> _spawnPoints;
    [SerializeField] private Transform _playerSpawnPoint;
    [SerializeField] private Transform _shipSpawnPoint;
    [SerializeField] private List<Transform> installPoints;

    public IReadOnlyList<Transform> SpawnPoints => _spawnPoints;
    public Transform PlayerSpawnPoint => _playerSpawnPoint;
    public Transform ShipSpawnPoint => _shipSpawnPoint;
    public IReadOnlyList<Transform> InstallPoints => installPoints;
}
