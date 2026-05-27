using UnityEngine;
using UnityEngine.AI;

namespace AI.BehaviorTree
{
    public class TaskInvestigate : Node
    {
        private AIAgent _ai;
        private bool _isWaiting = false;
        private float _waitTimer = 0f;
        private float _timeToWait = 4.0f;

        private Vector3 _currentTarget = Vector3.positiveInfinity;
        private float _travelTimer = 0f;
        private float _maxTravelTime = 10.0f;

        public TaskInvestigate(AIAgent ai)
        {
            _ai = ai;
        }

        public override NodeState Evaluate()
        {

            if (_ai.BGMSource.clip == _ai.chaseBGM)
            {
                if (_ai.horrorAmbiance != null)
                {
                    _ai.BGMSource.clip = _ai.horrorAmbiance;
                    _ai.BGMSource.Play();
                }
                else
                {
                    _ai.BGMSource.Stop();
                }

                if (_ai.deadCollider != null) _ai.deadCollider.SetActive(false);

                if (_ai.ghostVoice != null) _ai.ghostVoice.Stop();
            }

            Vector3 blackboardTarget = AIDirectorBlackboard.Instance.lastSoundPosition;

            if (Vector3.Distance(_currentTarget, blackboardTarget) > 0.5f)
            {
                _currentTarget = blackboardTarget;
                _ai.navMeshAgent.SetDestination(_currentTarget);

                _ai.navMeshAgent.isStopped = false;
                _ai.navMeshAgent.speed = 4.5f;

                _isWaiting = false;
                _waitTimer = 0f;
                _travelTimer = 0f;
            }

            if (!_isWaiting)
            {
                _travelTimer += Time.deltaTime;
            }

            bool llegoAlDestino = !_ai.navMeshAgent.pathPending && _ai.navMeshAgent.remainingDistance <= _ai.navMeshAgent.stoppingDistance + 1.5f;

            bool seAtasco = _travelTimer >= _maxTravelTime;

            if (llegoAlDestino || seAtasco)
            {
                if (!_isWaiting)
                {
                    _isWaiting = true;
                    _waitTimer = 0f;
                    _ai.navMeshAgent.isStopped = true;
                }

                _waitTimer += Time.deltaTime;

                if (_waitTimer >= _timeToWait)
                {
                    AIDirectorBlackboard.Instance.soundAge = AIDirectorBlackboard.Instance.soundMaxAge;

                    _isWaiting = false;
                    _ai.navMeshAgent.isStopped = false;
                    _currentTarget = Vector3.positiveInfinity;

                    state = NodeState.SUCCESS;
                    return state;
                }
            }
            else
            {
                _ai.navMeshAgent.isStopped = false;
                _ai.navMeshAgent.speed = 4.5f;
            }

            state = NodeState.RUNNING;
            return state;
        }
    }
}