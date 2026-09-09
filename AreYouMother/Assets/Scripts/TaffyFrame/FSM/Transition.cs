using System;

namespace TaffyFrame.FSM
{
    public class Transition
    {
        public State to;
        public Func<IBoard, bool> condition;
        public Func<IBoard, bool> trigger;
        public Transition(State to, Func<IBoard, bool> condition, Func<IBoard, bool> trigger = null)
        {
            this.to = to;
            this.condition = condition;
            this.trigger = trigger;
        }
    }
}
