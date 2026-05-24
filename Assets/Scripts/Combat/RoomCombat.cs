using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RoomCombat : MonoBehaviour
{
    public enum RoomState { Idle, Active, Cleared }

    [Header("Config")]
    public RoomCombatData data;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Doors")]
    public GameObject[] doorsToClose;

    [Header("Trigger")]
    public LayerMask playerLayer = ~0;

    public event Action OnRoomStarted;
    public event Action<int> OnWaveStarted;
    public event Action<int> OnWaveCleared;
    public event Action OnRoomCleared;

    private RoomState state = RoomState.Idle;
    private int currentWaveIndex = -1;
    private int aliveEnemyCount;
    private readonly List<EnemyBase> trackedEnemies = new();

    public RoomState CurrentState => state;
    public int CurrentWaveIndex => currentWaveIndex;
    public int AliveEnemyCount => aliveEnemyCount;
    public int TotalWaves => data != null && data.waves != null ? data.waves.Length : 0;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (!col.isTrigger)
            Debug.LogWarning($"[RoomCombat:{name}] Collider must be Trigger.", this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (state != RoomState.Idle) return;
        if (data == null || data.waves == null || data.waves.Length == 0) return;
        if (((1 << other.gameObject.layer) & playerLayer.value) == 0) return;

        StartCoroutine(RunCombat());
    }

    private IEnumerator RunCombat()
    {
        state = RoomState.Active;
        SetDoorsClosed(true);
        CombatManager.NotifyRoomStarted(this);
        OnRoomStarted?.Invoke();

        for (int i = 0; i < data.waves.Length; i++)
        {
            var wave = data.waves[i];
            if (wave == null) continue;

            currentWaveIndex = i;

            if (wave.delayBeforeStart > 0f)
                yield return new WaitForSeconds(wave.delayBeforeStart);

            OnWaveStarted?.Invoke(i);
            yield return StartCoroutine(SpawnWave(wave));

            while (aliveEnemyCount > 0)
                yield return null;

            OnWaveCleared?.Invoke(i);
        }

        state = RoomState.Cleared;
        SetDoorsClosed(false);
        CombatManager.NotifyRoomCleared(this);
        OnRoomCleared?.Invoke();

        if (data.oneShot && TryGetComponent<Collider>(out var col))
            col.enabled = false;
    }

    private IEnumerator SpawnWave(WaveData wave)
    {
        if (wave.spawns == null) yield break;

        foreach (var spawn in wave.spawns)
        {
            if (spawn.enemyPrefab == null) continue;

            for (int i = 0; i < spawn.count; i++)
            {
                SpawnOne(spawn);
                if (spawn.spawnInterval > 0f)
                    yield return new WaitForSeconds(spawn.spawnInterval);
            }
        }
    }

    private void SpawnOne(WaveData.EnemySpawn spawn)
    {
        Transform point = PickSpawnPoint(spawn.spawnPointIndex);
        Vector3 pos = point != null ? point.position : transform.position;
        Quaternion rot = point != null ? point.rotation : Quaternion.identity;

        GameObject instance = Instantiate(spawn.enemyPrefab, pos, rot);
        if (!instance.TryGetComponent<EnemyBase>(out var enemy))
        {
            Debug.LogWarning($"[RoomCombat:{name}] Prefab '{spawn.enemyPrefab.name}' has no EnemyBase. Wave will not count it.", instance);
            return;
        }

        aliveEnemyCount++;
        trackedEnemies.Add(enemy);
        enemy.onDeath += () => HandleEnemyDeath(enemy);
    }

    private Transform PickSpawnPoint(int requestedIndex)
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return null;
        if (requestedIndex >= 0 && requestedIndex < spawnPoints.Length)
            return spawnPoints[requestedIndex];
        return spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
    }

    private void HandleEnemyDeath(EnemyBase enemy)
    {
        if (!trackedEnemies.Remove(enemy)) return;
        aliveEnemyCount = Mathf.Max(0, aliveEnemyCount - 1);
    }

    private void SetDoorsClosed(bool closed)
    {
        if (data == null || !data.lockDoorsDuringCombat || doorsToClose == null) return;
        foreach (var door in doorsToClose)
            if (door != null) door.SetActive(closed);
    }

    private void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;
        Gizmos.color = Color.cyan;
        foreach (var p in spawnPoints)
        {
            if (p == null) continue;
            Gizmos.DrawWireSphere(p.position, 0.5f);
            Gizmos.DrawLine(p.position, p.position + p.forward);
        }
    }
}
