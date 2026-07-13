using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AIAgentConfig : ScriptableObject
{
    public float maxTime = 1.0f;
    public float maxDistance = 1.0f;
    public float maxSightDistance = 5.0f;
    public float ChaseTime = 10.0f;
    public float MaxChaseTime = 20.0f;
    public float range;
    public float walkSpeed = 2f;
    public float runSpeed = 8f;
    public float maxWaitTime = 3.0f;
}
