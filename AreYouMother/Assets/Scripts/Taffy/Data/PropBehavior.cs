using System;
using System.Collections.Generic;
using Taffy.Home;
using Taffy.OverAllManager;
using Taffy.Play.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Taffy.Data
{
    public interface IPropBehavior
    {
        public void Execute(Prop prop = null, Char Player = ' ', int propValue = 0);
    }

    public static class PropBehaviorTable
    {
        public static Dictionary<PropType,IPropBehavior> table = new Dictionary<PropType,IPropBehavior>();

        public static void BuildTable()
        {
            table[PropType.Default] = new Default_propType();
            table[PropType.Treasure] =  new Treasure_propType();
            table[PropType.Close_Attack] = new Close_Attack_propType();
            table[PropType.Remote_Attack] = new Remote_Attack_propType();
            table[PropType.Defend] = new Defend_propType();
            table[PropType.AddBlood] = new AddBlood_propType();
            table[PropType.AddSkill] = new AddSkill_propType();
            table[PropType.AddMaxBlood] = new AddMaxBlood_propType();
            table[PropType.AddMaxSkill] = new AddMaxSkill_propType();
            table[PropType.AddFavorability] = new AddFavorability_propType();
            table[PropType.Poison] = new Poison_propType();
            table[PropType.Detoxify] = new Detoxify_propType();
        }
    }


    public class Default_propType : IPropBehavior
    {
        public void Execute(Prop prop = null, char Player = ' ', int propValue = 0)
        {
            
        }
    }

    public class Treasure_propType :  IPropBehavior
    {
        public void Execute(Prop prop = null, char Player = ' ', int propValue = 0)
        {
            
        }
    }

    public class Close_Attack_propType : IPropBehavior
    {
        public void Execute(Prop prop = null, char Player = ' ', int propValue = 0)
        {
            
        }
    }
    
    public class Remote_Attack_propType : IPropBehavior
    {
        public void Execute(Prop prop = null, char Player = ' ', int propValue = 0)
        {
            
        }
    }
    
    public class Defend_propType : IPropBehavior
    {
        public void Execute(Prop prop = null, char Player = ' ', int propValue = 0)
        {
            
        }
    }
    
    public class AddBlood_propType : IPropBehavior
    {
        public void Execute(Prop prop = null,Char Player = ' ',int propValue = 0)
        {
            if (OverAllStates.isInPlay)
            {
                PlayerCurrentStateController pcsc = PlayerCurrentStateController.Instance;
                if (Player == 'A')
                {
                    pcsc.Cure_A(propValue);
                }
                else if (Player == 'B')
                {
                    pcsc.Cure_B(propValue);
                }

                if (prop != null) prop = null;
            }
        }
    }
    
    public class AddSkill_propType : IPropBehavior
    {
        public void Execute(Prop prop = null, char Player = ' ', int propValue = 0)
        {
            
        }
    }
    
    public class AddMaxBlood_propType : IPropBehavior
    {
        public void Execute(Prop prop = null, char Player = ' ', int propValue = 0)
        {
            
        }
    }
    
    public class AddMaxSkill_propType : IPropBehavior
    {
        public void Execute(Prop prop = null, char Player = ' ', int propValue = 0)
        {
            
        }
    }
    
    public class AddFavorability_propType : IPropBehavior
    {
        public void Execute(Prop prop = null, char Player = ' ', int propValue = 0)
        {
            
        }
    }
    
    public class Poison_propType : IPropBehavior
    {
        public void Execute(Prop prop = null, char Player = ' ', int propValue = 0)
        {
            
        }
    }
    
    public class Detoxify_propType : IPropBehavior
    {
        public void Execute(Prop prop = null, char Player = ' ', int propValue = 0)
        {
            
        }
    }
    
}

