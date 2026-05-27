using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AI.BehaviorTree;

public class GhostBT : AI.BehaviorTree.Tree
{
    private AIAgent _aiAgent;

    private void Awake()
    {
        _aiAgent = GetComponent<AIAgent>();
    }

    protected override Node SetupTree()
    {
        Node verAlJugador = new CheckPlayerInSight();
        Node matarJugador = new TaskKillPlayer(_aiAgent);
        Node perseguirJugador = new TaskChasePlayer(_aiAgent);

        Node escucharRuido = new CheckNoise();
        Node investigar = new TaskInvestigate(_aiAgent);

        Node patrullar = new TaskPatrol(_aiAgent);

        Selector accionCaceria = new Selector(new List<Node> { matarJugador, perseguirJugador });
        Sequence ramaCaceria = new Sequence(new List<Node> { verAlJugador, accionCaceria });

       // Sequence ramaInvestigacion = new Sequence(new List<Node> { escucharRuido, investigar });

        Selector root = new Selector(new List<Node> { ramaCaceria, patrullar });

        return root;
    }
}