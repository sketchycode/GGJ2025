
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyWaveConfig
{
    public List<EnemySpawn> Wave;
}

[Serializable]
public class EnemySpawn
{
    public EnemyObjectPool Pool;
    public Transform SpawnPoint;
    public int Count;
}
