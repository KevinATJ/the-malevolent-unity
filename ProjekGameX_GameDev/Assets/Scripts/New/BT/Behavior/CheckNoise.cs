using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.BehaviorTree
{
    public class CheckNoise : Node
    {
        public override NodeState Evaluate()
        {
            if (AIDirectorBlackboard.Instance != null &&
                AIDirectorBlackboard.Instance.soundAge < AIDirectorBlackboard.Instance.soundMaxAge)
            {
                state = NodeState.SUCCESS;
                return state;
            }

            state = NodeState.FAILURE;
            return state;
        }
    }
}