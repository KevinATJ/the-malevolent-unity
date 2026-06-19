using UnityEngine;
using UnityEngine.AI;

namespace AI.BehaviorTree
{
    public class TaskTeleport : Node
    {
        private AIAgent _ai;
        private Transform _playerTransform;
        private Camera _mainCamera;
        private LayerMask _occlusionLayers;

        private float _staticTimer = 0f;
        private float _staticCooldown = 40f;
        private float _minDistStatic = 30f;

        private float _minDistDynamic = 15f;

        public TaskTeleport(AIAgent ai)
        {
            _ai = ai;
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            _mainCamera = Camera.main;
            _occlusionLayers = LayerMask.GetMask("Default", "Obstacle");
        }

        public override NodeState Evaluate()
        {
            if (AIDirectorBlackboard.Instance == null || _playerTransform == null) return NodeState.FAILURE;

            if (AIDirectorBlackboard.Instance.ghostSeesPlayer)
            {
                _staticTimer = 0f;
                AIDirectorBlackboard.Instance.tpIsReadyToJump = false;
                AIDirectorBlackboard.Instance.tpConditionLabel = "Cancelado";
                AIDirectorBlackboard.Instance.tpConditionValue = "Jugador a la vista";
                AIDirectorBlackboard.Instance.tpDistanceStatus = "<color=red>Fallo (En Persecución)</color>";
                return NodeState.FAILURE;
            }

            if (AIDirectorBlackboard.Instance.dynamicTensionEnabled)
            {
                return ExecuteDynamicTP();
            }
            else
            {
                return ExecuteStaticTP();
            }
        }

        private NodeState ExecuteStaticTP()
        {
            var db = AIDirectorBlackboard.Instance;
            _staticTimer += Time.deltaTime;

            float distLineal = Vector3.Distance(_ai.transform.position, _playerTransform.position);
            bool tiempoCumplido = _staticTimer >= _staticCooldown;

            db.tpConditionLabel = "Temporizador";
            db.tpConditionValue = $"{_staticTimer:F0}s / {_staticCooldown}s";
            db.tpIsReadyToJump = tiempoCumplido;

            if (tiempoCumplido)
            {
                if (distLineal >= _minDistStatic)
                {
                    if (TryBuscarPuntoOculto(out Vector3 puntoSeguro, out string direccionElegida))
                    {
                        _ai.navMeshAgent.Warp(puntoSeguro);
                        _staticTimer = 0f;
                        db.tpLastEvent = $"TP Fuerza Bruta ({direccionElegida}) - {Time.time:F1}s";
                        return NodeState.SUCCESS;
                    }
                }
                else
                {
                    db.tpDistanceStatus = $"<color=yellow>Pausado: Dist ({distLineal:F1}m) < Mín ({_minDistStatic}m)</color>";
                }
            }
            else
            {
                db.tpDistanceStatus = $"Esperando Reloj | Dist: {distLineal:F1}m";
            }

            return NodeState.FAILURE;
        }

        private NodeState ExecuteDynamicTP()
        {
            var db = AIDirectorBlackboard.Instance;

            int objetosRecogidosActuales = db.objectsPickedSinceLastTP;
            bool objetosCumplidos = objetosRecogidosActuales >= db.targetObjectsForNextTP;

            db.tpConditionLabel = "Items Recogidos";
            db.tpConditionValue = $"{objetosRecogidosActuales} / {db.targetObjectsForNextTP} Items";
            db.tpIsReadyToJump = objetosCumplidos;

            float distReal = CalcularDistanciaRuta(_ai.transform.position, _playerTransform.position);

            if (objetosCumplidos)
            {
                if (distReal >= _minDistDynamic)
                {
                    if (TryBuscarPuntoOculto(out Vector3 puntoEmboscada, out string direccionElegida))
                    {
                        _ai.navMeshAgent.Warp(puntoEmboscada);
                        db.objectsPickedSinceLastTP = 0;
                        db.targetObjectsForNextTP = Random.Range(1, 4);
                        db.tpLastEvent = $"Emboscada ({direccionElegida}) - {Time.time:F1}s";
                        return NodeState.SUCCESS;
                    }
                    else
                    {
                        db.tpDistanceStatus = $"<color=yellow>Buscando punto ciego... (Dist: {distReal:F0}m)</color>";
                    }
                }
                else
                {
                    db.tpDistanceStatus = $"<color=yellow>Pausado: Ruta ({distReal:F1}m) < Mín ({_minDistDynamic}m)</color>";
                }
            }
            else
            {
                db.tpDistanceStatus = $"Esperando Items | Dist: {distReal:F1}m";
            }

            return NodeState.FAILURE;
        }

        private bool TryBuscarPuntoOculto(out Vector3 resultado, out string nombreDireccion)
        {
            bool prefiereAdelante = Random.value > 0.5f;

            Vector3 fwd = _playerTransform.forward;
            Vector3 bck = -_playerTransform.forward;

            Vector3[] dirAdelante = new Vector3[] { fwd, Quaternion.Euler(0, 70, 0) * fwd, Quaternion.Euler(0, -70, 0) * fwd };
            Vector3[] dirAtras = new Vector3[] { bck, Quaternion.Euler(0, 70, 0) * bck, Quaternion.Euler(0, -70, 0) * bck };

            Vector3[] direccionesAProbar = prefiereAdelante ?
                new Vector3[] { dirAdelante[0], dirAdelante[1], dirAdelante[2], dirAtras[0], dirAtras[1], dirAtras[2] } :
                new Vector3[] { dirAtras[0], dirAtras[1], dirAtras[2], dirAdelante[0], dirAdelante[1], dirAdelante[2] };

            for (int i = 0; i < direccionesAProbar.Length; i++)
            {
                if (TryCalcularPuntoSeguro(_playerTransform.position, direccionesAProbar[i], 30f, out Vector3 puntoEvaluado))
                {
                    if (!EsVisibleParaElJugador(puntoEvaluado))
                    {
                        resultado = puntoEvaluado;

                        bool usoPuntoFrontal = prefiereAdelante ? (i < 3) : (i >= 3);
                        nombreDireccion = usoPuntoFrontal ? "Frente" : "Espalda";
                        return true;
                    }
                }
            }

            resultado = Vector3.zero;
            nombreDireccion = "Ninguna";
            return false;
        }

        private bool TryCalcularPuntoSeguro(Vector3 origen, Vector3 direccion, float distanciaMaxima, out Vector3 resultado)
        {
            Vector3 destinoTeorico = origen + (direccion * distanciaMaxima);
            resultado = Vector3.zero;

            if (Physics.Raycast(origen, direccion, out RaycastHit hit, distanciaMaxima, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                float distanciaSegura = Mathf.Max(0f, hit.distance - 1.0f);
                destinoTeorico = origen + (direccion * distanciaSegura);
            }

            if (NavMesh.SamplePosition(destinoTeorico, out NavMeshHit navHit, 2.0f, NavMesh.AllAreas))
            {
                if (Vector3.Distance(origen, navHit.position) > 20f)
                {
                    resultado = navHit.position;
                    return true;
                }
            }

            return false;
        }

        private bool EsVisibleParaElJugador(Vector3 punto)
        {
            Vector3 viewportPoint = _mainCamera.WorldToViewportPoint(punto);

            if (viewportPoint.z > 0 && viewportPoint.x > -0.4f && viewportPoint.x < 1.4f && viewportPoint.y > -0.4f && viewportPoint.y < 1.4f)
            {
                Vector3 camPos = _mainCamera.transform.position;
                Vector3 direccionHaciaPunto = punto - camPos;
                float distancia = direccionHaciaPunto.magnitude;

                if (Physics.Raycast(camPos, direccionHaciaPunto, distancia, _occlusionLayers, QueryTriggerInteraction.Ignore))
                {
                    return false;
                }
                return true;
            }

            return false;
        }

        private float CalcularDistanciaRuta(Vector3 inicio, Vector3 fin)
        {
            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(inicio, fin, NavMesh.AllAreas, path))
            {
                float total = 0f;
                for (int i = 1; i < path.corners.Length; i++)
                    total += Vector3.Distance(path.corners[i - 1], path.corners[i]);
                return total;
            }
            return Vector3.Distance(inicio, fin);
        }
    }
}