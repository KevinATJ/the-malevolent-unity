using UnityEngine;
using TMPro;

public class AIDirectorBlackboard : MonoBehaviour
{
    public static AIDirectorBlackboard Instance;

    [Header("Scanning Settings")]
    public float campTimerThreshold = 15f;

    [Header("Director Global Settings")]
    public bool isDirectorActive = true;
    public bool showDebugUI = true;

    [Header("Player Sensors")]
    public float tensionLevel = 0f;
    public string playerMovementState = "Idle";
    public bool isFlashlightOn = false;
    public float noiseRadius = 0f;
    public bool isVulnerable = false;
    public float timeStuck = 0f;

    [Header("Spatial Tension")]
    public float distanceToGhost = 20f;
    public float maxTensionDistance = 20f;

    [Header("Tension Weights")]
    [Range(0f, 1f)] public float weightVision = 0.50f;
    [Range(0f, 1f)] public float weightDistance = 0.30f;
    [Range(0f, 1f)] public float weightActions = 0.20f;

    [Header("Threat Amplifier")]
    [Range(0f, 1f)] public float threatAmplifier = 0.50f;

    [Header("Ghost Sensors & Memory")]
    public bool ghostSeesPlayer = false;
    public bool ghostHearsPlayer = false;
    public Vector3 lastSoundPosition;
    public float soundAge = 0f;
    public float soundMaxAge = 10f;

    [Header("Objective & Aggression")]
    public float aggressionLevel = 0f;
    public int remainingObjectives = 0;
    public Vector3 currentPriorityZone;

    [Header("Ghost Telemetry (Read Only)")]
    public float currentPatrolRadius = 25f;
    public Vector3 currentGhostDestination = Vector3.zero;

    [Header("Debug UI")]
    public TextMeshProUGUI debugTextPanel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        UpdateSoundAge();
        CalculateTension();
        UpdateDebugUI();
    }

    private void UpdateSoundAge()
    {
        if (soundAge < soundMaxAge)
        {
            soundAge += Time.deltaTime;
        }
    }

    private void CalculateTension()
    {
        float targetTension = 0f;

        if (ghostSeesPlayer)
        {
            targetTension = 100f;
        }

        else
        {
            float distanceFactor = Mathf.Clamp01(1.0f - (distanceToGhost / maxTensionDistance));

            float actionFactor = 0f;
            if (isFlashlightOn) actionFactor += 0.3f;
            if (playerMovementState == "sprinting") actionFactor += 0.4f;
            if (isVulnerable) actionFactor += 0.3f;
            if (timeStuck > campTimerThreshold) actionFactor += 0.5f;

            actionFactor = Mathf.Clamp01(actionFactor);

            float baseTension = (weightDistance * distanceFactor) +
                                (weightActions * actionFactor);

            float normalizedThreat = aggressionLevel / 100f;
            float finalTensionNormalized = baseTension * (1.0f + (threatAmplifier * normalizedThreat));

            targetTension = Mathf.Clamp(finalTensionNormalized * 100f, 0f, 100f);
        }

        float changeSpeed = (targetTension > tensionLevel) ? 100f : 5f;
        tensionLevel = Mathf.MoveTowards(tensionLevel, targetTension, changeSpeed * Time.deltaTime);
    }

    private void UpdateDebugUI()
    {
        if (debugTextPanel == null) return;

        // Toggle visibility
        debugTextPanel.gameObject.SetActive(showDebugUI);
        if (!showDebugUI) return;

        string tensionColor = tensionLevel > 70 ? "#FF0000" : (tensionLevel > 30 ? "#FFFF00" : "#00FF00");
        string aggroColor = aggressionLevel > 70 ? "#FF0000" : (aggressionLevel > 30 ? "#FFFF00" : "#00FF00");
        string visionStatus = ghostSeesPlayer ? "<b><color=#FF0000>>> ¡AVISTADO! <<</color></b>" : "<color=#888888>Sin contacto</color>";
        string hearingStatus = ghostHearsPlayer ? "<b><color=#FF4444>>> ¡OYO RUIDO! <<</color></b>" : "<color=#888888>Silencio</color>";
        string linternaStatus = isFlashlightOn ? "<color=yellow>Encendida</color>" : "<color=#888888>Apagada</color>";
        string estancamientoTexto = timeStuck > campTimerThreshold ? $"<color=#FF4444>{timeStuck:F1}s (Camp!)</color>" : $"{timeStuck:F1}s";
        string ageColor = soundAge < 3f ? "#FF4500" : (soundAge < 7f ? "#FFFF00" : "#888888");

        // Color del radio para ver gráficamente la asfixia
        string radioColor = currentPatrolRadius < 12f ? "#FF0000" : (currentPatrolRadius < 18f ? "#FFFF00" : "#00FF00");

        string directorStatus = isDirectorActive ? "<color=#00FF00>ACTIVO (Dinámico)</color>" : "<color=#FF0000>INACTIVO (Estático)</color>";

        debugTextPanel.text =
            $"<align=center><b><color=#FFD700>SISTEMA IA DIRECTOR</color></b></align>\n" +
            $"<align=center><i>Estado: {directorStatus}</i></align>\n\n" +

            $"<b>Tensión:</b> <color={tensionColor}>{tensionLevel:F0}%</color> | <b>Agresividad:</b> <color={aggroColor}>{aggressionLevel:F0}%</color>\n" +
            $"<b>Objetos Restantes:</b> {remainingObjectives}\n\n" +

            $"<b><color=#00FF7F>> DATOS JUGADOR</color></b>\n" +
            $"  Movimiento: <i>{playerMovementState}</i>\n" +
            $"  Linterna: {linternaStatus} | Ruido: <b>{noiseRadius:F1}m</b>\n" +
            $"  Tiempo Estancado: {estancamientoTexto}\n\n" +

            $"<b><color=#FF4500>> SENTIDOS ENEMIGO</color></b>\n" +
            $"  Visión: {visionStatus}\n" +
            $"  Audición: {hearingStatus}\n\n" +

            $"<b><color=#00BFFF>> TÁCTICA & NAVEGACIÓN</color></b>\n" +
            $"  Último Ruido: {lastSoundPosition}\n" +
            $"  Edad Pista: <color={ageColor}>{soundAge:F1}s</color>\n" +
            $"  Distancia Real: {distanceToGhost:F1}m\n" +
            $"  Radio Máximo (Tether): <b><color={radioColor}>{currentPatrolRadius:F1}m</color></b>\n" +
            $"  Destino NavMesh: {currentGhostDestination}";
    }
}