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
            if (_ai.BGMSource.clip == _ai.chaseBGM)
            {
                if (_ai.deadCollider != null) _ai.deadCollider.SetActive(false);

                if (_ai.horrorAmbiance != null)
                {
                    _ai.BGMSource.clip = _ai.horrorAmbiance;
                    _ai.BGMSource.Play();
                }
                else
                {
                    _ai.BGMSource.Stop();
                }
            }

            bool hayRuidoFresco = AIDirectorBlackboard.Instance != null &&
                                  AIDirectorBlackboard.Instance.soundAge < AIDirectorBlackboard.Instance.soundMaxAge;

            if (hayRuidoFresco)
            {
                Vector3 destinoRuido = AIDirectorBlackboard.Instance.lastSoundPosition;

                if (!_investigatingNoise)
                {
                    _ai.navMeshAgent.isStopped = false;
                    _ai.navMeshAgent.speed = 4.5f;
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
                        float radioMaximoActual = _radioMaximoBase;

                        if (AIDirectorBlackboard.Instance != null && AIDirectorBlackboard.Instance.isDirectorActive)
                        {
                            float tension = AIDirectorBlackboard.Instance.tensionLevel;
                            radioMaximoActual = Mathf.Lerp(10f, _radioMaximoBase, tension / 100f);
                        }

                        Vector3 nuevoDestino = ObtenerPuntoEnCascaron(_playerTransform.position, _radioMinimo, radioMaximoActual);

                        _ai.navMeshAgent.isStopped = false;
                        _ai.navMeshAgent.speed = 3.5f;
                        _ai.navMeshAgent.SetDestination(nuevoDestino);

                        _isPatrolling = true;

                        if (AIDirectorBlackboard.Instance != null)
                        {
                            AIDirectorBlackboard.Instance.currentPatrolRadius = radioMaximoActual;
                            AIDirectorBlackboard.Instance.currentGhostDestination = nuevoDestino;
                        }
                    }
                }
            }

            state = NodeState.RUNNING;
            return state;
        }

        private Vector3 ObtenerPuntoEnCascaron(Vector3 centroJugador, float minRadius, float maxRadius)
        {
            Vector2 direccionAleatoria = Random.insideUnitCircle.normalized;
            float distanciaAleatoria = Random.Range(minRadius, maxRadius);
            Vector3 puntoTeorico = centroJugador + new Vector3(direccionAleatoria.x, 0, direccionAleatoria.y) * distanciaAleatoria;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(puntoTeorico, out hit, maxRadius, NavMesh.AllAreas))
            {
                return hit.position;
            }

            return centroJugador;
        }
    }
}