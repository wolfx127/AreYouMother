using System.Collections.Generic;
using Taffy.Data.PropData;
using Taffy.Play.Container;

namespace TaffyFrame.EventBus
{
    public struct ChangeSceneHomeToPlayingEvent { }

    public struct GetPlayersInfosEvent_A
    {
        public int HP;
        public int MP;
        public List<Prop> bag;
        public int bagSize;
        public Prop weapon;
        public Prop defense;

        public GetPlayersInfosEvent_A(int HP, int MP, List<Prop> bag, int bagSize, Prop weapon, Prop defense)
        {
            this.HP = HP;
            this.MP = MP;
            this.bag = bag;
            this.bagSize = bagSize;
            this.weapon = weapon;
            this.defense = defense;
        }
    }
    

    public struct GetPlayersInfosEvent_B
    {
        public int HP;
        public int MP;
        public List<Prop> bag;
        public int bagSize;
        public Prop weapon;
        public Prop defense;

        public GetPlayersInfosEvent_B(int HP, int MP, List<Prop> bag, int bagSize, Prop weapon, Prop defense)
        {
            this.HP = HP;
            this.MP = MP;
            this.bag = bag;
            this.bagSize = bagSize;
            this.weapon = weapon;
            this.defense = defense;
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

    public struct DealerUpdateEvent { }

}
