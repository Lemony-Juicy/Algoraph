using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Linq;
using System.IO;
using System.Windows;

namespace Algoraph.Scripts
{

    class GraphStateData
    {
        public string[] nodeNames { get; set; }
        public double[][] nodePositions { get; set; }
        public double[][] adjMat { get; set; }

        public GraphStateData(Node[] nodes) 
        {
            nodeNames = nodes.Select(n => n.name).ToArray();
            nodePositions = nodes.Select(n => new double[2] { n.GetLocation().X, n.GetLocation().Y }).ToArray();
            SetAdjMat(nodes);
        }

        void SetAdjMat(Node[] nodes)
        {
            adjMat = new double[nodes.Length][];
            for (int i = 0; i < nodes.Length; i++)
            {
                List<Node> conns = nodes[i].nodeConnections;
                double[] matConns = new double[nodes.Length];
                Array.Fill(matConns, 0);
                foreach (Node connNode in conns)
                {
                    double weight = Grapher.GetConnectingArc(nodes[i], connNode).weight;
                    matConns[Array.IndexOf(nodes, connNode)] = weight;
                }
                adjMat[i] = matConns;
            }
        }

        public Node[] GetNodesArray(Editor ed)
        {
            Node[] nodes = new Node[nodeNames.Length];

            for (int i = 0; nodeNames.Length > 0; i++)
            {
                nodes[i] = new Node(
                    editor: ed,
                    location: new Point(nodePositions[i][0], nodePositions[i][1]),
                    connections: new List<Node>());
            }

            for (int i = 0; nodeNames.Length > 0; i++)
            {
                for (int j = 0; j < adjMat[i].Length; j++)
                {
                    nodes[j].AddConnection(nodes[i], new Arc(ed, nodes[j], nodes[i], (uint)adjMat[i][j]));
                }
            }

            return nodes;
        }
    }

    internal class Saver
    {
        public static string? path = null;

        public static void Save(List<Node> nodes)
        {
            Console.WriteLine("Initiating save");

            GraphStateData nd = new GraphStateData(nodes.ToArray());
            string json = JsonSerializer.Serialize(nd);
            if (path != null)
            {
                File.WriteAllText(path, json);
            }
            else
            {
                throw new Exception("Path is null");
            }  
        }

        public static GraphStateData? Load()
        {

            if (path != null)
            {
                string jsonString = File.ReadAllText(path);
                return JsonSerializer.Deserialize<GraphStateData>(jsonString);
            }
            else
            {
                throw new Exception("Path is null");
            }
        }
    }

}

