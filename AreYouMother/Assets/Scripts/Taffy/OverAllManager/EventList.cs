using System.Collections.Generic;
using Taffy.Data;
using UnityEngine;

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
        public int bagSize_playerA;
        public int bagSize_playerB;
        public int ATK_A;
        public int ATK_B;
        public int DEF_A;
        public int DEF_B;
        public Prop tempWeapon_A;
        public Prop tempWeapon_B;
        public Prop tempDefense_A;
        public Prop tempDefense_B;
        public GetPlayersInfosEvent(int hp_A, int hp_B, int mp_A, int mp_B, List<Prop> bag_A, List<Prop> bag_B, int bagSize_A, int bagSize_B, 
            int atk_A, int atk_B, int def_A, int def_B,Prop wa,Prop wb,Prop da,Prop db)
        {
            HP_playerA = hp_A;
            HP_playerB = hp_B;
            MP_playerA = mp_A;
            MP_playerB = mp_B;
            bag_playerA = bag_A;
            bag_playerB = bag_B;
            bagSize_playerA = bagSize_A;
            bagSize_playerB = bagSize_B;
            ATK_A = atk_A;
            ATK_B = atk_B;
            DEF_A = def_A;
            DEF_B = def_B;
            tempWeapon_A = wa;
            tempWeapon_B = wb;
            tempDefense_A = da;
            tempDefense_B = db;
        }
    }

    public struct ExitGameEvent { }

    public struct GiveContainer_AEvent
    {
        public ContainerData containerData;
        public GiveContainer_AEvent(ContainerData containerData)
        {
            this.containerData = containerData;
        }
    }

    public struct GiveContainer_BEvent
    {
        public ContainerData containerData;
        public GiveContainer_BEvent(ContainerData containerData)
        {
            this.containerData = containerData;
        }
    }

    public struct Evacuate_AEvent { }
    public struct Evacuate_BEvent { }
    
    public struct ChangeScenePlayingToHomeEvent { }
    public struct AllSuccessEvacuateEvent { }
    public struct Only_A_SuccessEvacuateEvent { }
    public struct Only_B_SuccessEvacuateEvent { }
    public struct FailEvacuateEvent { }

    public struct GiveBagsEvent
    {
        public List<Prop> bagA;
        public List<Prop> bagB;
        public GiveBagsEvent(List<Prop> bagA, List<Prop> bagB)
        {
            this.bagA = bagA;
            this.bagB = bagB;
        }
    }

}
