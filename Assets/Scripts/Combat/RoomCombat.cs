using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Компонент на trigger-volume комнаты-арены. При входе игрока запускает последовательность волн.
/// Слушает EnemyBase.onDeath, считает живых, переходит к следующей волне когда комната очищена.
/// </summary>
[RequireComponent(typeof(Collider))]
public class RoomCombat : MonoBehaviour
{
    [Header("Config")]
    public RoomCombatData data;

    [Header("Spawn points")]
    [Tooltip("Точки спавна врагов. WaveData.EnemySpawn.spawnPointIndex ссылается на индекс в этом массиве.")]
    public Transform[] spawnPoints;

    [Header("Doors (optional)")]
    [Tooltip("Объекты, которые активируются на время боя (закрытые двери). Деактивируются после очистки комнаты.")]
    public GameObject[] doorsToClose;

    [Header("Trigger")]
    [Tooltip("Слой(и) игрока. Trigger срабатывает только на этот слой.")]
    public LayerMask playerLayer = ~0;

    public event Action OnRoomStarted;
    public event Action<int> OnWaveStarted;
    public event Action<int> OnWaveCleared;
    public event Action OnRoomCleared;

    public enum RoomState { Idle, Active, Cleared }

    private RoomState state = RoomState.Idle;
    private int currentWaveIndex = -1;
    private int aliveEnemyCount;
    private readonly List<EnemyBase> trackedEnemies = new List<EnemyBase>();

    public RoomState CurrentState => state;
    public int CurrentWaveIndex => currentWaveIndex;
    public int AliveEnemyCount => aliveEnemyCount;
    public int TotalWaves => data != null && data.waves != null ? data.waves.Length : 0;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            Debug.LogWarning($"[RoomCombat:{name}] Collider must be set as Trigger.", this);
        }
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
        CloseDoors(true);
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
        CloseDoors(false);
        CombatManager.NotifyRoomCleared(this);
        OnRoomCleared?.Invoke();

        if (data.oneShot)
        {
            var col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }
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
        var enemy = instance.GetComponent<EnemyBase>();
        if (enemy == null)
        {
            Debug.LogWarning($"[RoomCombat:{name}] Spawned prefab '{spawn.enemyPrefab.name}' has no EnemyBase. It won't count toward wave clear.", instance);
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

    private void CloseDoors(bool closed)
    {
        if (!data.lockDoorsDuringCombat || doorsToClose == null) return;
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
            Gizmos.DrawLine(p.position, p.position + p.forward * 1f);
        }
    }
}
