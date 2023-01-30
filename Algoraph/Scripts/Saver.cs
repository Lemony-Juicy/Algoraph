using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Linq;
using System.IO;
using System.Windows;

namespace Algoraph.Scripts
{

    class NodesArcsTuple
    {
        public Arc[] arcs { get; set; }
        public Node[] nodes { get; set; }

        public NodesArcsTuple(Arc[] arcs, Node[] nodes)
        {
            this.arcs = arcs;
            this.nodes = nodes;
        }
    }


    class GraphStateData
    {
        public string[]? nodeNames { get; set; }
        public double[][]? nodePositions { get; set; }

        public string[]? arcNames { get; set; }
        public uint[]? arcsWeights { get; set; }

        // EG: {["N4", "N5"], ["N2", "N1"], ...}
        public string[][]? arcConns { get; set; }
    }

    class GraphStateDataMethods
    {
        public static void SetData(Node[] nodes, Arc[] arcs, GraphStateData data)
        {
            data.nodeNames = nodes.Select(n => n.name).ToArray();
            data.nodePositions = nodes.Select(n => new double[2] { n.GetLocation().X, n.GetLocation().Y }).ToArray();

            data.arcNames = arcs.Select(a=> a.name).ToArray();
            data.arcsWeights = arcs.Select(a => a.weight).ToArray();
            data.arcConns = arcs.Select(a => a.connections.Select(n => n.name).ToArray()).ToArray();
        }


        static public NodesArcsTuple GetNodeArcTuple(Editor ed, GraphStateData data)
        {
            Arc[] arcs = new Arc[data.arcNames.Length];
            Node[] nodes = new Node[data.nodeNames.Length];
            for (int i = 0; i < data.nodeNames.Length; i++)
            {
                double[] pos = data.nodePositions[i];
                nodes[i] = new Node(
                    editor: ed,
                    location: new Point(pos[0], pos[1]),
                    connections: new List<Node>());
                Console.WriteLine(nodes[i].ChangeName(data.nodeNames[i])); 
            }

            for (int i = 0; i < data.arcNames.Length; i++)
            {
                string[] conns = data.arcConns[i];
                Node n1 = Array.Find(nodes, n => n.name == conns[0]);
                Node n2 = Array.Find(nodes, n => n.name == conns[1]);
                Arc arc = new Arc(ed, n1, n2, data.arcsWeights[i]);

                n1.AddConnection(n2, arc);
                arcs[i] = arc;
            }

            return new NodesArcsTuple(arcs, nodes);
        }
    }

    internal class Saver
    {
        public static string? path = null;

        public static void Save(List<Node> nodes, List<Arc> arcs)
        {
            GraphStateData data = new GraphStateData();
            GraphStateDataMethods.SetData(nodes.ToArray(), arcs.ToArray(), data);
            string json = JsonSerializer.Serialize(data);
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

