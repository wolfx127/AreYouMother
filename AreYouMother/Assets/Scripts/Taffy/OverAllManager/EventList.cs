using System.Collections.Generic;
using Taffy.Data;

namespace Taffy.OverAllManager
{
    public struct ChangeSceneHomeToPlayingEvent { }

    public struct InitialPlayingSceneEvent { }

    public struct GetPlayersInfosEvent
    {
        public int HP_playerA;
        public int HP_playerB;
        public int MP_playerA;
        public int MP_playerB;
        public List<Prop> bag_playerA;
        public List<Prop> bag_playerB;
        public GetPlayersInfosEvent(int hp_A, int hp_B, int mp_A, int mp_B, List<Prop> bag_A, List<Prop> bag_B)
        {
            HP_playerA = hp_A;
            HP_playerB = hp_B;
            MP_playerA = mp_A;
            MP_playerB = mp_B;
            bag_playerA = bag_A;
            bag_playerB = bag_B;
        }
    }

    public struct ExitGameEvent { }
}
