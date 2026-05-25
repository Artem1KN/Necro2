using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWaveData", menuName = "Necro2/Combat/Wave Data")]
public class WaveData : ScriptableObject
{
    [Serializable]
    public struct EnemySpawn
    {
        [Tooltip("Enemy prefab. Root must contain an EnemyBase component.")]
        public GameObject enemyPrefab;

        [Tooltip("How many instances to spawn during this wave.")]
        [Min(1)] public int count;

        [Tooltip("Interval between spawn instances (seconds).")]
        [Min(0f)] public float spawnInterval;

        [Tooltip("Index into RoomCombat.spawnPoints. -1 = random point per instance.")]
        public int spawnPointIndex;
    }

    [Header("Wave")]
    public string waveName = "Wave";

    [Tooltip("Delay before this wave starts (seconds). For the first wave, measured from player entering the room.")]
    [Min(0f)] public float delayBeforeStart = 0.5f;

    [Tooltip("Enemy groups spawned in this wave.")]
    public EnemySpawn[] spawns;
}
