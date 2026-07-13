using UnityEngine;

public class SinusoidalMovement : MonoBehaviour
{
    public float amplitude = 1.0f; // Qué tanta distancia recorre (el tamaño de la onda)
    public float frequency = 2.0f; // Qué tan rápido hace el ciclo completo
    public Vector3 movementAxis = new Vector3(0f, 1f, 0f); // En qué dirección se mueve (arriba/abajo por defecto)

    private Vector3 startPosition;

    void Start()
    {
        // Guardamos la posición inicial al iniciar el juego
        startPosition = transform.localPosition;
    }

    void Update()
    {
        // Mathf.Sin crea la curva sinusoidal basada en el tiempo del juego
        float offset = Mathf.Sin(Time.time * frequency) * amplitude;

        // Sumamos ese desplazamiento a la posición inicial a lo largo del eje elegido
        transform.localPosition = startPosition + (movementAxis.normalized * offset);
    }
}