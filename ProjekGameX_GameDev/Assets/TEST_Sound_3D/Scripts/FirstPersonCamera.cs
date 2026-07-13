using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    [Header("Configuración de Sensibilidad")]
    public float mouseSensitivity = 100f;

    [Header("Referencias")]
    [Tooltip("Arrastra aquí el objeto padre que representa el cuerpo del jugador")]
    public Transform playerBody;

    private float xRotation = 0f;

    void Start()
    {
        // Bloquea el cursor en el centro de la pantalla y lo oculta
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Capturar la entrada del mouse
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Calcular la rotación vertical (arriba/abajo)
        xRotation -= mouseY;

        // Limitar la rotación vertical entre -90 (mirar arriba) y 90 (mirar abajo)
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Aplicar la rotación a la cámara (eje X local)
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotar el cuerpo del jugador horizontalmente (eje Y)
        playerBody.Rotate(Vector3.up * mouseX);
    }
}