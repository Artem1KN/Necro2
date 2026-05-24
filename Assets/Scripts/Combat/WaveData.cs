using System;
using UnityEngine;

/// <summary>
/// Конфигурация одной волны врагов внутри комнаты-арены.
/// </summary>
[CreateAssetMenu(fileName = "NewWaveData", menuName = "Necro2/Combat/Wave Data")]
public class WaveData : ScriptableObject
{
    [Serializable]
    public struct EnemySpawn
    {
        [Tooltip("Префаб врага. На корне должен висеть EnemyBase.")]
        public GameObject enemyPrefab;

        [Tooltip("Сколько штук этого врага заспавнить в волне.")]
        [Min(1)] public int count;

        [Tooltip("Интервал между спавном экземпляров (сек).")]
        [Min(0f)] public float spawnInterval;

        [Tooltip("Индекс точки спавна в RoomCombat.spawnPoints. -1 = случайная точка для каждого экземпляра.")]
        public int spawnPointIndex;
    }

    [Header("Wave")]
    public string waveName = "Wave";

    [Tooltip("Задержка перед стартом волны после очистки предыдущей (сек). Для первой волны — задержка после входа в комнату.")]
    [Min(0f)] public float delayBeforeStart = 0.5f;

    [Tooltip("Группы врагов, которых нужно заспавнить в этой волне.")]
    public EnemySpawn[] spawns;
}
