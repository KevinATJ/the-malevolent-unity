using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[ExecuteInEditMode]
public class AISensor : MonoBehaviour
{
    public float distance = 10f;
    public float angle = 30f;
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
        float radioVisionActual = distance;

        if (AIDirectorBlackboard.Instance != null &&
           (AIDirectorBlackboard.Instance.ghostSeesPlayer || AIDirectorBlackboard.Instance.soundAge < 3f))
        {
            radioVisionActual = distance * 2f;
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

            if (distanciaAlObjeto <= radioVisionActual && IsInSight(obj))
            {
                objectsList.Add(obj);
                if (obj.CompareTag("PlayerHead"))
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
                Debug.DrawLine(oidoFantasma, ruidoJugador, Color.green, 0.5f);
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
                Debug.DrawLine(path.corners[i - 1] + Vector3.up, path.corners[i] + Vector3.up, Color.yellow, 0.5f);
            }

            // Atenuación del 20% por rebotar en las paredes
            float distanciaAtenuada = distanciaPorPasillos * 1.2f;

            if (distanciaAtenuada <= radioRuidoJugador)
            {
                return true;
            }
        }

        Debug.DrawLine(oidoFantasma, ruidoJugador, Color.red, 0.5f);
        return false;
    }

    public bool IsInSight(GameObject obj)
    {
        Vector3 origin = transform.position;
        Vector3 dest = obj.transform.position;
        Vector3 direction = dest - origin;

        if (direction.y < 0 || direction.y > height)
        {
            return false;
        }

        direction.y = 0;
        float deltaAngle = Vector3.Angle(direction, transform.forward);

        if (deltaAngle > angle)
        {
            return false;
        }

        origin.y += height / 2;
        dest.y = origin.y;

        if (Physics.Linecast(origin, dest, occlusionLayers))
        {
            return false;
        }

        return true;
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
        Vector3 bottomLeft = Quaternion.Euler(0, -angle, 0) * Vector3.forward * distance;
        Vector3 bottomRight = Quaternion.Euler(0, angle, 0) * Vector3.forward * distance;
        Vector3 topCenter = bottomCenter + Vector3.up * height;
        Vector3 topLeft = bottomLeft + Vector3.up * height;
        Vector3 topRight = bottomRight + Vector3.up * height;

        int vert = 0;
        vertices[vert++] = bottomCenter; vertices[vert++] = bottomLeft; vertices[vert++] = topLeft;
        vertices[vert++] = topLeft; vertices[vert++] = topCenter; vertices[vert++] = bottomCenter;
        vertices[vert++] = bottomCenter; vertices[vert++] = topCenter; vertices[vert++] = topRight;
        vertices[vert++] = topRight; vertices[vert++] = bottomRight; vertices[vert++] = bottomCenter;

        float currentAngle = -angle;
        float deltaAngle = (angle * 2) / segments;

        for (int i = 0; i < segments; i++)
        {
            bottomLeft = Quaternion.Euler(0, currentAngle, 0) * Vector3.forward * distance;
            bottomRight = Quaternion.Euler(0, currentAngle + deltaAngle, 0) * Vector3.forward * distance;
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
        Gizmos.color = Color.green;
        foreach (var obj in objects)
        {
            Gizmos.DrawSphere(obj.transform.position, 0.2f);
        }
    }
}