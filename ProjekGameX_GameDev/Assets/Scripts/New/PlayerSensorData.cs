using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSensorData : MonoBehaviour
{
    [Header("Referencias")]
    public PlayerMovement playerMovement;
    public StaminaController staminaController;
    public Flashlight flashlightController;

    [Header("Sensor de Estancamiento")]
    private Vector3 ultimaPosicion;
    private float tiempoEstancado = 0f;
    private float radioDeTolerancia = 1.5f;

    private void Start()
    {
        ultimaPosicion = transform.position;
    }

    private void Update()
    {
        if (AIDirectorBlackboard.Instance == null) return;

        AIDirectorBlackboard.Instance.playerMovementState = playerMovement.state.ToString();
        if (flashlightController != null) AIDirectorBlackboard.Instance.isFlashlightOn = flashlightController.flashlightIsOn;
        AIDirectorBlackboard.Instance.isVulnerable = !staminaController.hasRegenerated;

        CalcularRuido();
        CalcularEstancamiento();
    }

    private void CalcularEstancamiento()
    {
        if (Vector3.Distance(transform.position, ultimaPosicion) < radioDeTolerancia)
        {
            tiempoEstancado += Time.deltaTime;
        }
        else
        {
            tiempoEstancado = 0f;
            ultimaPosicion = transform.position;
        }

        AIDirectorBlackboard.Instance.timeStuck = tiempoEstancado;
    }

    private void CalcularRuido()
    {
        float noiseRadius = 0f;

        switch (playerMovement.state)
        {
            case PlayerMovement.movementState.sprinting:
                noiseRadius = 20f;
                break;
            case PlayerMovement.movementState.walking:
                noiseRadius = 5f;
                break;
            case PlayerMovement.movementState.crouchingWalking:
            case PlayerMovement.movementState.crouchingIdle:
            case PlayerMovement.movementState.StandingIdle:
                noiseRadius = 0f;
                break;
        }

        AIDirectorBlackboard.Instance.noiseRadius = noiseRadius;
    }
}