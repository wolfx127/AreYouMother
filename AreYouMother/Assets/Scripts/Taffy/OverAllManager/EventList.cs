namespace Taffy.OverAllManager
{
    public struct ChangeSceneHomeToPlayingEvent { }

    public struct InitialPlayingSceneEvent { }

    public struct GetPlayersHPandMPEvent
    {
        public int HP_playerA;
        public int HP_playerB;
        public int MP_playerA;
        public int MP_playerB;
        public GetPlayersHPandMPEvent(int hp_A, int hp_B, int mp_A, int mp_B)
        {
            HP_playerA = hp_A;
            HP_playerB = hp_B;
            MP_playerA = mp_A;
            MP_playerB = mp_B;
        }
    }

    public struct ExitGameEvent { }
}
