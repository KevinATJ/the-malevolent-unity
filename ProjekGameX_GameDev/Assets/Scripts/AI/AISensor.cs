using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[ExecuteInEditMode]
public class AISensor : MonoBehaviour
{
    public float focalDistance = 15f;
    public float focalAngle = 15f;
    public float peripheralDistance = 5f;
    public float peripheralAngle = 45f;
    public float height = 1.0f;
    public Color meshColor = Color.red;
    public int scanFrequency = 30;
    public LayerMask layers;
    public LayerMask occlusionLayers;
    public bool ghostHearsPlayer = false;

    public List<GameObject> objects
    {
        get
        {
            objectsList.RemoveAll(obj => !obj);
            return objectsList;
        }
    }
    private List<GameObject> objectsList = new List<GameObject>();
    Collider[] colliders = new Collider[50];
    int count;
    float scanInterval;
    float scanTimer;
    Mesh mesh;

    void Start()
    {
        scanInterval = 1.0f / scanFrequency;
    }

    void Update()
    {
        scanTimer -= Time.deltaTime;
        if (scanTimer < 0)
        {
            scanTimer += scanInterval;
            Scan();
        }
    }

    private void Scan()
    {
        float radioDeEscucha = AIDirectorBlackboard.Instance != null ? AIDirectorBlackboard.Instance.noiseRadius : 0f;
        float radioVisionActual = focalDistance;

        if (AIDirectorBlackboard.Instance != null &&
           (AIDirectorBlackboard.Instance.ghostSeesPlayer || AIDirectorBlackboard.Instance.soundAge < 3f))
        {
            radioVisionActual = focalDistance * 1.5f;
            radioDeEscucha *= 2f;
        }

        float radioMaximoDeteccion = Mathf.Max(radioVisionActual, radioDeEscucha);

        count = Physics.OverlapSphereNonAlloc(transform.position, radioMaximoDeteccion, colliders, layers, QueryTriggerInteraction.Collide);
        objectsList.Clear();

        bool vioAlJugador = false;
        bool escuchoAlJugador = false;

        for (int i = 0; i < count; ++i)
        {
            GameObject obj = colliders[i].gameObject;
            float distanciaAlObjeto = Vector3.Distance(transform.position, obj.transform.position);

            if (distanciaAlObjeto <= radioVisionActual && IsInSight(obj, distanciaAlObjeto))
            {
                objectsList.Add(obj);
                if (obj.CompareTag("Player") || obj.CompareTag("PlayerHead"))
                {
                    vioAlJugador = true;

                    if (AIDirectorBlackboard.Instance != null)
                    {
                        Vector3 puntoDePrediccion = obj.transform.position + (obj.transform.forward * 3f);
                        AIDirectorBlackboard.Instance.lastSoundPosition = puntoDePrediccion;
                        AIDirectorBlackboard.Instance.soundAge = 0f;
                    }
                }
            }

            if (obj.CompareTag("Player") || obj.CompareTag("PlayerHead"))
            {
                if (IsHeard(obj, distanciaAlObjeto, radioDeEscucha))
                {
                    escuchoAlJugador = true;

                    if (AIDirectorBlackboard.Instance != null)
                    {
                        AIDirectorBlackboard.Instance.lastSoundPosition = obj.transform.position;
                        AIDirectorBlackboard.Instance.soundAge = 0f;
                    }
                }
            }
        }

        if (AIDirectorBlackboard.Instance != null)
        {
            AIDirectorBlackboard.Instance.ghostSeesPlayer = vioAlJugador;
            AIDirectorBlackboard.Instance.ghostHearsPlayer = escuchoAlJugador;
        }

        this.ghostHearsPlayer = escuchoAlJugador;
    }

    public bool IsHeard(GameObject obj, float distanciaLineal, float radioRuidoJugador)
    {
        if (radioRuidoJugador <= 0f) return false;

        Vector3 oidoFantasma = transform.position + Vector3.up * 1.5f;
        Vector3 ruidoJugador = obj.transform.position + Vector3.up * 1.5f;

        if (!Physics.Linecast(oidoFantasma, ruidoJugador, occlusionLayers))
        {
            if (distanciaLineal <= radioRuidoJugador)
            {
                return true;
            }
            return false;
        }

        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(transform.position, obj.transform.position, NavMesh.AllAreas, path))
        {
            float distanciaPorPasillos = 0f;

            for (int i = 1; i < path.corners.Length; i++)
            {
                distanciaPorPasillos += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            }

            float distanciaAtenuada = distanciaPorPasillos * 1.2f;

            if (distanciaAtenuada <= radioRuidoJugador)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsInSight(GameObject obj, float distanciaAlObjeto)
    {
        Vector3 origin = transform.position + Vector3.up * (height / 2);
        Vector3 dest = obj.transform.position;
        Vector3 direction = dest - origin;

        if (direction.y < -height || direction.y > height)
        {
            return false;
        }

        direction.y = 0;
        float deltaAngle = Vector3.Angle(direction, transform.forward);

        float currentFocalAngle = focalAngle;
        float currentPeripheralAngle = peripheralAngle;

        if (AIDirectorBlackboard.Instance != null && AIDirectorBlackboard.Instance.isFlashlightOn)
        {
            currentFocalAngle *= 1.5f;
            currentPeripheralAngle *= 1.5f;
        }

        float maxAllowedDistance = 0f;

        if (deltaAngle <= currentFocalAngle)
        {
            maxAllowedDistance = focalDistance;
        }
        else if (deltaAngle <= currentPeripheralAngle)
        {
            maxAllowedDistance = peripheralDistance;
        }
        else
        {
            return false;
        }

        if (distanciaAlObjeto > maxAllowedDistance)
        {
            return false;
        }

        CharacterController playerController = obj.GetComponent<CharacterController>();
        PlayerMovement playerMovement = obj.GetComponent<PlayerMovement>();

        float playerHeight = 1.8f;
        float headHeight = 1.7f;

        if (playerController != null)
        {
            playerHeight = playerController.height;
        }

        if (playerMovement != null && playerMovement.cameraHolder != null)
        {
            headHeight = playerMovement.cameraHolder.localPosition.y;
        }

        Vector3[] targetPoints = new Vector3[]
        {
            dest,
            dest + (Vector3.up * (playerHeight * 0.5f)),
            dest + (Vector3.up * headHeight)
        };

        foreach (Vector3 targetPoint in targetPoints)
        {
            if (!Physics.Linecast(origin, targetPoint, occlusionLayers))
            {
                return true;
            }
        }

        return false;
    }

    Mesh CreateWedgeMesh()
    {
        Mesh mesh = new Mesh();
        int segments = 10;
        int numTriangles = (segments * 4) + 2 + 2;
        int numVertices = numTriangles * 3;
        Vector3[] vertices = new Vector3[numVertices];
        int[] triangles = new int[numVertices];

        Vector3 bottomCenter = Vector3.zero;
        Vector3 bottomLeft = Quaternion.Euler(0, -peripheralAngle, 0) * Vector3.forward * focalDistance;
        Vector3 bottomRight = Quaternion.Euler(0, peripheralAngle, 0) * Vector3.forward * focalDistance;
        Vector3 topCenter = bottomCenter + Vector3.up * height;
        Vector3 topLeft = bottomLeft + Vector3.up * height;
        Vector3 topRight = bottomRight + Vector3.up * height;

        int vert = 0;
        vertices[vert++] = bottomCenter; vertices[vert++] = bottomLeft; vertices[vert++] = topLeft;
        vertices[vert++] = topLeft; vertices[vert++] = topCenter; vertices[vert++] = bottomCenter;
        vertices[vert++] = bottomCenter; vertices[vert++] = topCenter; vertices[vert++] = topRight;
        vertices[vert++] = topRight; vertices[vert++] = bottomRight; vertices[vert++] = bottomCenter;

        float currentAngle = -peripheralAngle;
        float deltaAngle = (peripheralAngle * 2) / segments;

        for (int i = 0; i < segments; i++)
        {
            bottomLeft = Quaternion.Euler(0, currentAngle, 0) * Vector3.forward * focalDistance;
            bottomRight = Quaternion.Euler(0, currentAngle + deltaAngle, 0) * Vector3.forward * focalDistance;
            topLeft = bottomLeft + Vector3.up * height;
            topRight = bottomRight + Vector3.up * height;

            vertices[vert++] = bottomLeft; vertices[vert++] = bottomRight; vertices[vert++] = topRight;
            vertices[vert++] = topRight; vertices[vert++] = topLeft; vertices[vert++] = bottomLeft;
            vertices[vert++] = topCenter; vertices[vert++] = topLeft; vertices[vert++] = topRight;
            vertices[vert++] = bottomCenter; vertices[vert++] = bottomRight; vertices[vert++] = bottomLeft;
            currentAngle += deltaAngle;
        }

        for (int i = 0; i < numVertices; i++) triangles[i] = i;

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    private void OnValidate()
    {
        mesh = CreateWedgeMesh();
        scanInterval = 1.0f / scanFrequency;
    }

    private void OnDrawGizmos()
    {
        if (mesh)
        {
            Gizmos.color = meshColor;
            Gizmos.DrawMesh(mesh, transform.position, transform.rotation);
        }
    }
}