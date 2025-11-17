using System;

public static class MiniGameResultEvents
{
    // Eventos públicos para vitória / derrota
    public static event Action OnVictory;
    public static event Action OnDefeat;

    public static void RaiseVictory()
    {
        OnVictory?.Invoke();
    }

    public static void RaiseDefeat()
    {
        OnDefeat?.Invoke();
    }
}
