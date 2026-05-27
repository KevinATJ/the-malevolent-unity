using UnityEngine;
using UnityEngine.AI;

namespace AI.BehaviorTree
{
    public class TaskKillPlayer : Node
    {
        private AIAgent _ai;
        private float _killDistance = 1.5f;
        private bool _isKilling = false;

        public TaskKillPlayer(AIAgent agentData)
        {
            _ai = agentData;
        }

        public override NodeState Evaluate()
        {

            if (_isKilling)
            {
                state = NodeState.SUCCESS;
                return state;
            }

            if (Vector3.Distance(_ai.transform.position, _ai.playerTransform.position) <= _killDistance)
            {
                _isKilling = true;

                if (_ai.navMeshAgent.isOnNavMesh)
                {
                    _ai.navMeshAgent.isStopped = true;
                }

                if (_ai.animator != null) _ai.animator.SetTrigger("Attack");

                if (_ai.playerDead != null)
                {
                    _ai.playerDead.PlayerDie(null, null);
                }

                state = NodeState.SUCCESS;
                return state;
            }

            state = NodeState.FAILURE;
            return state;
        }
    }
}