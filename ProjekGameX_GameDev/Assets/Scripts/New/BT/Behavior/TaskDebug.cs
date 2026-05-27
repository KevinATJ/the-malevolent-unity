using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AI.BehaviorTree
{
    public class TaskDebug : Node
    {
        private string message;

        public TaskDebug(string message)
        {
            this.message = message;
        }

        public override NodeState Evaluate()
        {
            Debug.Log($"[Cerebro BT] {message}");

            state = NodeState.SUCCESS;
            return state;
        }
    }
}