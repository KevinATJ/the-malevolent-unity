using UnityEngine;
using AI.BehaviorTree;

namespace AI.BehaviorTree
{
    public class TaskChasePlayer : Node
    {
        private AIAgent _ai;
        private Vector3 _lastTargetPosition = Vector3.positiveInfinity;

        public TaskChasePlayer(AIAgent agentData)
        {
            _ai = agentData;
        }

        public override NodeState Evaluate()
        {

            if (_ai.BGMSource.clip != _ai.chaseBGM)
            {
                if (_ai.ghostVoice != null) _ai.ghostVoice.Stop();
                if (_ai.sfxSound != null && _ai.horrorStinger != null) _ai.sfxSound.PlayOneShot(_ai.horrorStinger);
                if (_ai.ghostVoice != null && _ai.Angry != null) _ai.ghostVoice.PlayOneShot(_ai.Angry);

                if (_ai.BGMSource != null)
                {
                    _ai.BGMSource.clip = _ai.chaseBGM;
                    _ai.BGMSource.Play();
                }

                if (_ai.AiIK != null) _ai.AiIK.SetTargetTransform(_ai.playerTransform);
                if (_ai.deadCollider != null) _ai.deadCollider.SetActive(true);
            }


            _ai.navMeshAgent.isStopped = false;

            if (_ai.config != null)
            {
                _ai.navMeshAgent.speed = _ai.config.runSpeed;
            }

            if (Vector3.Distance(_lastTargetPosition, _ai.playerTransform.position) > 1.0f)
            {
                _lastTargetPosition = _ai.playerTransform.position;
                _ai.navMeshAgent.SetDestination(_lastTargetPosition);
            }

            state = NodeState.RUNNING;
            return state;
        }
    }
}