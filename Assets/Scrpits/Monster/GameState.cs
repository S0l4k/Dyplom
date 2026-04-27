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
    public static bool IsInFlashback = false;
    public static bool FoodOrdered = false;                    // Czy gracz zamówił jedzenie?
    public static bool ApartmentExplorationUnlocked = false;   // Czy odblokowano quest "rozglądnij się"?
    public static int FlashbacksCompleted = 0;                 // Ile z 2 flashbacków ukończono (0/1/2)
    public static readonly int TotalFlashbacksRequired = 2;    // Ile flashbacków trzeba zrobić

    // ✅ Helper: czy wszystkie flashbacki są ukończone?
    public static bool AllFlashbacksCompleted => FlashbacksCompleted >= TotalFlashbacksRequired;

}