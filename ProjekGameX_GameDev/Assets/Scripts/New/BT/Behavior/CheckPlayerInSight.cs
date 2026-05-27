using System.Collections.Generic;
using System.Collections;
using UnityEngine;


namespace AI.BehaviorTree
{
    public class CheckPlayerInSight : Node
    {
        public override NodeState Evaluate()
        {
            if (AIDirectorBlackboard.Instance != null && AIDirectorBlackboard.Instance.ghostSeesPlayer)
            {
                state = NodeState.SUCCESS;
                return state;
            }

            state = NodeState.FAILURE;
            return state;
        }
    }
}