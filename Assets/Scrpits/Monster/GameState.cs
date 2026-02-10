public static class GameState
{
    public static bool DemonInStoryMode = true; // ✅ NOWE: domyślnie tryb story

    // ... reszta istniejących pól ...
    public static bool IsTalking = false;
    public static bool ChaseLocked = true;
    // ...

    public static bool LoopSequenceActive = false;
    public static bool DemonLoopPhase = false; // Demon czeka na dole po 5 loopach

    // NOWA FLAGA: dialog zakończony, czekamy na ponowne wejście w loop
    public static bool ReadyForFinalChase = false;
    public static bool CourierArrived = false;
    public static bool FinalChase = false;
}