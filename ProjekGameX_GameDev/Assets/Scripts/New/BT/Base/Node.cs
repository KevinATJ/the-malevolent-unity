using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AI.BehaviorTree
{
    public enum NodeState
    {
        RUNNING,
        SUCCESS,
        FAILURE
    }

    [System.Serializable]
    public abstract class Node
    {
        protected NodeState state;

        public Node parent;
        protected List<Node> children = new List<Node>();

        public NodeState State { get { return state; } }

        public Node() { parent = null; }
        public Node(List<Node> children)
        {
            foreach (Node child in children)
            {
                Attach(child);
            }
        }
        public void Attach(Node child)
        {
            child.parent = this;
            children.Add(child);
        }
        public abstract NodeState Evaluate();
    }
}