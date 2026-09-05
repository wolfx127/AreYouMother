using System;
using System.Collections.Generic;
using Taffy.Home;
using Taffy.OverAllManager;
using Taffy.Play.Player;
using UnityEngine;

namespace Taffy.Data.PropData
{
    public interface IPropBehavior
    {
        public event Action Event;
        public void Execute(Prop prop = null, Char Player = ' ', int propValue = 0);
    }

    public static class PropBehaviorTable
    {
        public static Dictionary<PropType,IPropBehavior> table = new Dictionary<PropType,IPropBehavior>();

        public static void BuildTable()
        {
            Debug.Log("[初始化] PropBehaviorTable 开始构建");
            table[PropType.Default] = new Default_propType();
            table[PropType.Cultivate] = new Cultivate_propType();
            table[PropType.Consume] = new Consume_propType();
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
            Debug.Log($"[初始化] PropBehaviorTable 构建完成，共 {table.Count} 个行为");
        }
    }


    public class Default_propType : IPropBehavior
    {
        public event Action Event;

        public void Execute(Prop prop = null, char Player = ' ', int propValue = 0)
        {
            
        }
    }

    public class Cultivate_propType : IPropBehavior
    {
        public event Action Event;

        public void Execute(Prop prop = null, char Player = ' ', int propValue = 0)
        {
            
        }
    }

    public class Consume_propType : IPropBehavior
    {
        public event Action Event;

        public void Execute(Prop prop = null, char Player = ' ', int propValue = 0)
        {
            
        }
    }

    public class Treasure_propType :  IPropBehavior
    {
        public event Action Event;

        public void Execute(PropData.Prop prop = null, char Player = ' ', int propValue = 0)
        {
            
        }
    }

    public class Close_Attack_propType : IPropBehavior
    {
        public event Action Event;
        public void Execute(PropData.Prop prop = null, char Player = ' ', int propValue = 0)
        {
            if(OverAllStates.isInHome)
            {
                if(Player == 'B')
                {
                    OverAllPlayerController.Instance.ATK_B = propValue;
                    Debug.Log($"B的攻击力更新:{OverAllPlayerController.Instance.ATK_B}");
                }
            }

            if (OverAllStates.isInPlay)
            {
                
            }
            
            Event?.Invoke();
        }
    }
    
    public class Remote_Attack_propType : IPropBehavior
    {
        public event Action Event;

        public void Execute(PropData.Prop prop = null, char Player = ' ', int propValue = 0)
        {
            if(OverAllStates.isInHome)
            {
                
                if(Player == 'A')
                {
                    OverAllPlayerController.Instance.ATK_A = propValue;
                    Debug.Log($"A的攻击力更新:{OverAllPlayerController.Instance.ATK_A}");
                }
            }

            if (OverAllStates.isInPlay)
            {
                
            }
            
            Event?.Invoke();
        }
    }
    
    public class Defend_propType : IPropBehavior
    {
        public event Action Event;

        public void Execute(PropData.Prop prop = null, char Player = ' ', int propValue = 0)
        {
            if(OverAllStates.isInHome)
            {
                if(Player == 'A')
                {
                    OverAllPlayerController.Instance.DEF_A = propValue;
                    Debug.Log($"A的防御值更新:{OverAllPlayerController.Instance.DEF_A}");
                }
                else if (Player == 'B')
                {
                    OverAllPlayerController.Instance.DEF_B = propValue;
                    Debug.Log($"B的防御值更新:{OverAllPlayerController.Instance.DEF_B}");
                }
            }

            if (OverAllStates.isInPlay)
            {
                
            }
            
            Event?.Invoke();
        }
    }
    
    public class AddBlood_propType : IPropBehavior
    {
        public event Action Event;

        public void Execute(PropData.Prop prop = null,Char Player = ' ',int propValue = 0)
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
                
                Event?.Invoke();

                if (prop != null) prop = null;
            }
        }
    }
    
    public class AddSkill_propType : IPropBehavior
    {
        public event Action Event;

        public void Execute(PropData.Prop prop = null, char Player = ' ', int propValue = 0)
        {
            
        }
    }
    
    public class AddMaxBlood_propType : IPropBehavior
    {
        public event Action Event;

        public void Execute(Prop prop = null, char Player = ' ', int propValue = 0)
        {
            if (OverAllStates.isInHome)
            {
                if(Player == 'A') OverAllPlayerController.Instance.maxHP_A += propValue;
                else  if (Player == 'B') OverAllPlayerController.Instance.maxHP_B += propValue;
            }
            
            Event?.Invoke();
        }
    }
    
    public class AddMaxSkill_propType : IPropBehavior
    {
        public event Action Event;

        public void Execute(PropData.Prop prop = null, char Player = ' ', int propValue = 0)
        {
            if (OverAllStates.isInHome)
            {
                if(Player == 'A') OverAllPlayerController.Instance.maxMP_A += propValue;
                else  if (Player == 'B') OverAllPlayerController.Instance.maxMP_B += propValue;
            }
            
            Event?.Invoke();
        }
    }
    
    public class AddFavorability_propType : IPropBehavior
    {
        public event Action Event;

        public void Execute(PropData.Prop prop = null, char Player = ' ', int propValue = 0)
        {
            if(OverAllStates.isInDealer) DealerManager.AddFavoribility(propValue);
            
            Event?.Invoke();
        }
    }
    
    public class Poison_propType : IPropBehavior
    {
        public event Action Event;

        public void Execute(PropData.Prop prop = null, char Player = ' ', int propValue = 0)
        {
            
            
            Event?.Invoke();
        }
    }
    
    public class Detoxify_propType : IPropBehavior
    {
        public event Action Event;

        public void Execute(PropData.Prop prop = null, char Player = ' ', int propValue = 0)
        {
            
            
            Event?.Invoke();
        }
    }
    
}

