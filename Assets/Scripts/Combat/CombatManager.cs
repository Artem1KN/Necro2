using System;
using UnityEngine;

/// <summary>
/// Глобальный координатор боевых комнат. RoomCombat сообщает ему о старте/очистке,
/// HUD и другие системы подписываются на события.
/// Создаётся автоматически при первом обращении — отдельный GameObject в сцене не нужен.
/// </summary>
public class CombatManager : MonoBehaviour
{
    private static CombatManager _instance;

    public static CombatManager Instance
    {
        get
        {
            if (_instance != null) return _instance;
            var go = new GameObject("[CombatManager]");
            _instance = go.AddComponent<CombatManager>();
            DontDestroyOnLoad(go);
            return _instance;
        }
    }

    public RoomCombat ActiveRoom { get; private set; }

    public static event Action<RoomCombat> RoomActivated;
    public static event Action<RoomCombat> RoomCleared;

    public static void NotifyRoomStarted(RoomCombat room)
    {
        Instance.ActiveRoom = room;
        RoomActivated?.Invoke(room);
    }

    public static void NotifyRoomCleared(RoomCombat room)
    {
        if (Instance.ActiveRoom == room) Instance.ActiveRoom = null;
        RoomCleared?.Invoke(room);
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }
}
