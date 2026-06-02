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
    public static bool FridgeDemonDialogCompleted = false;
    public static bool IsInspecting = false;
    public static bool TriggerSeizureEffect = false;
    public static bool InteractionsLocked = false;
    public static bool SofaSequenceActive = false;
    public static bool IsInFlashback = false;
    public static bool FoodOrdered = false;                    
    public static bool ApartmentExplorationUnlocked = false;  
    public static int FlashbacksCompleted = 0;                
    public static readonly int TotalFlashbacksRequired = 2;    


    public static bool AllFlashbacksCompleted => FlashbacksCompleted >= TotalFlashbacksRequired;
    public static void ResetAll()
    {
        DemonInStoryMode = true;
        ChaseLocked = true;
        LoopSequenceActive = false;
        FinalChase = false;
        ReadyForFinalChase = false;
        IsTalking = false;
        IsInFlashback = false;
        ApartmentExplorationUnlocked = false;
        FlashbacksCompleted = 0;
        CourierArrived = false;
        DemonRespawnedInApartment = false;
        SofaSequenceActive = false;
        InteractionsLocked = false;

       
    }
}