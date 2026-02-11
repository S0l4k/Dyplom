public static class GameState
{
    public static bool DemonInStoryMode = true;   // ✅ true na starcie (blokuje ruch)
    public static bool DemonReadyForChase = false; // ✅ NOWA FLAGA – aktywowana po "I won't do it"
    public static bool ChaseLocked = true;         // ✅ true na starcie (blokuje chase)

    public static bool LoopSequenceActive = false;
    public static bool DemonLoopPhase = false;
    public static bool ReadyForFinalChase = false;
    public static bool CourierArrived = false;
    public static bool FinalChase = false;
    public static bool IsTalking = false;
    public static bool DemonRespawnedInApartment = false;
}