using UnityEngine;
using UnityEngine.AI;

namespace AI.BehaviorTree
{
    public class TaskPatrol : Node
    {
        private AIAgent _ai;
        private Transform _playerTransform;
        private bool _isPatrolling = false;
        private bool _investigatingNoise = false;

        private float _radioMinimo = 8f;
        private float _radioMaximoBase = 25f;
        private float _globalPatrolRadius = 40f;

        private float _lowTensionTimer = 0f;
        private float _lowTensionThreshold = 30f;
        private float _timeToTriggerStalking = 10f;

        public TaskPatrol(AIAgent ai)
        {
            _ai = ai;
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }
        }

        public override NodeState Evaluate()
        {
            if (AIDirectorBlackboard.Instance.currentGhostState != GhostState.Patrolling)
            {
                AIDirectorBlackboard.Instance.currentGhostState = GhostState.Patrolling;

                if (_ai.deadCollider != null) _ai.deadCollider.SetActive(false);

                if (_ai.horrorAmbiance != null)
                {
                    _ai.BGMSource.clip = _ai.horrorAmbiance;
                    _ai.BGMSource.loop = true;
                    _ai.BGMSource.Play();
                }
                else
                {
                    _ai.BGMSource.Stop();
                }

                if (_ai.ghostVoice != null && _ai.humming != null)
                {
                    _ai.ghostVoice.clip = _ai.humming;
                    _ai.ghostVoice.loop = true;
                    _ai.ghostVoice.Play();
                }
            }

            if (AIDirectorBlackboard.Instance != null)
            {
                if (AIDirectorBlackboard.Instance.tensionLevel <= _lowTensionThreshold)
                {
                    _lowTensionTimer += Time.deltaTime;
                }
                else
                {
                    _lowTensionTimer = 0f;
                }

                AIDirectorBlackboard.Instance.lowTensionTimerDebug = _lowTensionTimer;
            }

            bool hayRuidoFresco = AIDirectorBlackboard.Instance != null &&
                                  AIDirectorBlackboard.Instance.soundAge < AIDirectorBlackboard.Instance.soundMaxAge;

            float baseWalkSpeed = _ai.config != null ? _ai.config.walkSpeed : 3.5f;
            float maxRunSpeed = _ai.config != null ? _ai.config.runSpeed : 8.5f;

            if (hayRuidoFresco)
            {
                Vector3 destinoRuido = AIDirectorBlackboard.Instance.lastSoundPosition;

                if (!_investigatingNoise)
                {
                    _ai.navMeshAgent.isStopped = false;
                    _ai.navMeshAgent.speed = maxRunSpeed;
                    _ai.navMeshAgent.SetDestination(destinoRuido);
                    _investigatingNoise = true;
                    _isPatrolling = false;
                }

                if (!_ai.navMeshAgent.pathPending && _ai.navMeshAgent.remainingDistance <= _ai.navMeshAgent.stoppingDistance + 1.0f)
                {
                    AIDirectorBlackboard.Instance.soundAge = AIDirectorBlackboard.Instance.soundMaxAge;
                    _investigatingNoise = false;
                }
            }
            else
            {
                _investigatingNoise = false;

                if (_isPatrolling && _ai.navMeshAgent.velocity.sqrMagnitude == 0f && !_ai.navMeshAgent.pathPending)
                {
                    _isPatrolling = false;
                }

                if (!_isPatrolling || _ai.navMeshAgent.remainingDistance <= _ai.navMeshAgent.stoppingDistance)
                {
                    if (_playerTransform != null)
                    {
                        float multiplicadorVelocidad = 1.0f;
                        Vector3 nuevoDestino = _ai.transform.position;

                        if (AIDirectorBlackboard.Instance != null)
                        {
                            float tension = AIDirectorBlackboard.Instance.tensionLevel;
                            multiplicadorVelocidad = AIDirectorBlackboard.Instance.GetDistanceSpeedMultiplier();

                            if (_lowTensionTimer >= _timeToTriggerStalking)
                            {
                                float radioMaximoActual = Mathf.Lerp(10f, _radioMaximoBase, tension / _lowTensionThreshold);
                                nuevoDestino = ObtenerPuntoEnCascaron(_playerTransform.position, _radioMinimo, radioMaximoActual);
                                AIDirectorBlackboard.Instance.currentPatrolRadius = radioMaximoActual;
                                AIDirectorBlackboard.Instance.patrolModeDebug = "Stalking (Acorralando)";
                            }
                            else
                            {
                                nuevoDestino = ObtenerPuntoEnCascaron(_ai.transform.position, 5f, _globalPatrolRadius);
                                AIDirectorBlackboard.Instance.currentPatrolRadius = _globalPatrolRadius;
                                AIDirectorBlackboard.Instance.patrolModeDebug = "Roaming Global";
                            }
                        }

                        _ai.navMeshAgent.isStopped = false;
                        _ai.navMeshAgent.speed = baseWalkSpeed * multiplicadorVelocidad;
                        _ai.navMeshAgent.SetDestination(nuevoDestino);

                        _isPatrolling = true;

                        if (AIDirectorBlackboard.Instance != null)
                        {
                            AIDirectorBlackboard.Instance.currentGhostDestination = nuevoDestino;
                        }
                    }
                }
            }

            state = NodeState.RUNNING;
            return state;
        }

        private Vector3 ObtenerPuntoEnCascaron(Vector3 centro, float minRadius, float maxRadius)
        {
            Vector2 direccionAleatoria = Random.insideUnitCircle.normalized;
            float distanciaAleatoria = Random.Range(minRadius, maxRadius);
            Vector3 puntoTeorico = centro + new Vector3(direccionAleatoria.x, 0, direccionAleatoria.y) * distanciaAleatoria;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(puntoTeorico, out hit, maxRadius, NavMesh.AllAreas))
            {
                return hit.position;
            }

            return centro;
        }
    }
}