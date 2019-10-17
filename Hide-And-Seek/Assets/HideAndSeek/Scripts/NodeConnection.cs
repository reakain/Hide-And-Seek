using UnityEngine;
using System.Collections;

namespace Pathfinding
{
    public class NodeConnection
    {
        public Node Parent;
        public Node Node;
        public bool Valid;

        //Debug
        private LineRenderer Debug;
        GameObject go;

        public NodeConnection(Node parent, Node node, bool valid)
        {
            Valid = valid;
            Node = node;
            Parent = parent;

            if (Node != null && Node.BadNode)
                Valid = false;
            if (Parent != null && Parent.BadNode)
                Valid = false;
        }

        //Debug
        public void DrawLine()
        {
            if (Valid)
            {
                if (Parent != null && Node != null)
                {
                    go = GameObject.Instantiate(Resources.Load("Line")) as GameObject;
                    Debug = go.GetComponent<LineRenderer>();

                    Debug.SetPosition(0, Parent.Position);
                    Debug.SetPosition(1, Node.Position);
                    Debug.startWidth = 0.06f;
                    Debug.endWidth = 0.00f;
                    if (Valid)
                    {
                        Debug.startColor = Color.green;
                        Debug.endColor = Color.green;
                    }
                    else
                    {
                        Debug.startColor = Color.red;
                        Debug.endColor = Color.red;
                    }
                }
            }
        }
    }
}