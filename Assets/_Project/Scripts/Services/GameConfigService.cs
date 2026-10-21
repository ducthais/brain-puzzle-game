using System;
using UnityEngine;

/// <summary>Game đọc cấu hình ở đây; không cần biết SDK Remote Config.</summary>
public static class GameConfigService
{
    public static GameConfig Current { get; private set; } = new GameConfig();
    public static event Action<GameConfig> Changed;

    public static void Apply(GameConfig config)
    {
        Current = config ?? new GameConfig();
        Changed?.Invoke(Current);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Current = new GameConfig();
        Changed = null;
    }
}
