using QFSW.QC.Demo;

namespace HJ
{
    public enum AudioAmbience
    {
        PlayMainAmbience,
        StopMainAmbiences
    }

    public enum AudioEnemies
    {
        // DON'T MODIFY - IN USE
        EnemyHurt,
        EnemyDie,

        // CAN BE MODIFIED - NOT YET IN USE
        EnemyLocationalVocals,
        EnemyMovementIdle,
        EnemyLocomotion,
        EnemyVocalKill,
    }

    public enum AudioEnvironment
    {
        // DON'T MODIFY - IN USE
        Impact,
        DynamicOpen,
        DynamicClose,
        DynamicLocked,
        DynamicUnlock,
        LeverOn,
        LeverOff,
        Jumpscare,

        // CAN BE MODIFIED - NOT YET IN USE
        CabinetClosed,
        CabinetOpen,
        DoorClose,
        DoorLocked,
        DoorOpen,
        EnvDoorLocked,
        EnvGearStart,
        EnvGearFailed
    }

    public enum AudioItems
    {
        // DON'T MODIFY - IN USE
        ItemExamine,
        ItemExamineHint,
        ItemDragStart,
        ItemDragStop,
        ItemPickup,

        // CAN BE MODIFIED - NOT YET IN USE
        ItemDrop,
        ItemDynamiteExplosion,
        ItemKeyPickup,
        ItemLanternEquip,
        ItemLanternFill,
        ItemNote,
        ItemGascanPickup
    }

    public enum AudioMusic
    {
        PlayMainMusic,
        StopMainMusic,
        PlayLevelTransitionMusic
    }

    public enum AudioPlayer
    {
        // DON'T MODIFY - IN USE
        PlayerTakeDamage,

        // CAN BE MODIFIED - NOT YET IN USE
        PlayerFootstep,
        PlayerGrunt,
        PlayerHeartbeat,
        PlayerLandCarpet,
        PlayerLandGrass,
        PlayerLandGravel,
        PlayerLandMetal,
        PlayerLandSand,
        PlayerLandWood,
        Play_Player_Scared,
        Stop_Player_Scared
    }

    public enum AudioUI
    {
        // DON'T MODIFY - IN USE
        UIInvItemSelect,
        UIInvItemMove,
        UIInvItemPut,
        UIInvItemError,


        // CAN BE MODIFIED - NOT YET IN USE
        UIInvBagClose,
        UIInvBagOpen,
        UIInvPut,
        UIInvSelect,
        UILockpickFail,
        UILockpickMove,
        UILockpickStart,
        UILockpickSuccess,
        UIMenuNavigate,
        UIMenuNegative,
        UIMenuObjective,
        UIMenuPositive
    }
    public enum AudioDialog
    {
        PlayIntroCutsceneDialog,
        StopIntroCutsceneDialog,
        Play_Estella_Phrase_BrokeLockpick,
        Play_Estella_Phrase_CantSee,
        Play_Estella_Phrase_LanternLow,
        Play_Estella_Phrase_LooksLikeOldNote,
        Play_Estella_Phrase_MyDadWroteThis,
        Play_Estella_Phrase_NeedToFindOil,
        Play_Estella_Phrase_Newspaper,
        Play_Estella_Phrase_Ronen,
        Play_Estella_Phrase_ThisIsFromMyGrandfather,
        Play_Estella_Phrase_WhatsThat,
        Play_Estella_Phrase_WhatsThis
    }

    public enum AudioState
    {
        MainMenu,
        GameActive,
        GamePaused,
        Forest,
        Lakeside,
        LightHouse,
        Cabin,
        PlayerDead,
        PlayerInDanager,
        PlayerNotInDanger,
        Credits
    }
}
