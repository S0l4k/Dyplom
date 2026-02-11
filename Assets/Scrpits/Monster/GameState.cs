public static class GameState
{
    public static bool DemonInStoryMode = true;
    public static bool ChaseLocked = true;
    public static bool LoopSequenceActive = false;
    public static bool DemonLoopPhase = false;
    public static bool ReadyForFinalChase = false;
    public static bool CourierArrived = false;
    public static bool FinalChase = false;
    public static bool IsTalking = false;
    public static bool DemonRespawnedInApartment = false;

    // ✅ NOWE FLAGI DLA SEKWENCJI KOŃCOWEJ
    public static bool InteractionsLocked = false;
    public static bool SofaSequenceActive = false;

}