using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.IO;
using System.Windows;
using Algoraph.Scripts.Saving_and_Loading;
using Windows.Devices.Printers;

namespace Algoraph.Scripts
{
    internal class Saver
    {
        public static string GetJsonData(List<Node> nodes, List<Arc> arcs)
        {
            RawGraphData rawData = new RawGraphData();
            rawData.nodeNames = nodes.Select(n => n.name).ToArray();
            rawData.nodePositions = nodes.Select(n => new double[2] { n.GetLocation().X, n.GetLocation().Y }).ToArray();

            rawData.arcNames = arcs.Select(a => a.name).ToArray();
            rawData.arcsWeights = arcs.Select(a => a.weight).ToArray();
            rawData.arcConns = arcs.Select(a => a.connections.Select(n => n.name).ToArray()).ToArray();

            return JsonSerializer.Serialize(rawData);
        }

        public static void SaveJsonData(string jsonString, string path)
        {
            File.WriteAllText(path, jsonString);
        }

        public static bool LoadNodesArcs(Editor ed, string jsonString, out Arc[]? arcs, out Node[]? nodes)
        {
            RawGraphData? rawData = LoadRawData(jsonString);
            if (rawData == null)
            {
                arcs = null;
                nodes = null;
                return false;
            }
                

            arcs = new Arc[rawData.arcNames.Length];
            nodes = new Node[rawData.nodeNames.Length];

            for (int i = 0; i < rawData.nodeNames.Length; i++)
            {
                double[] pos = rawData.nodePositions[i];
                nodes[i] = new Node(
                    editor: ed,
                    location: new Point(pos[0], pos[1]),
                    connections: new List<Node>(),
                    name: rawData.nodeNames[i]);
            }

            for (int i = 0; i < rawData.arcNames.Length; i++)
            {
                string[] conns = rawData.arcConns[i];
                Node n1 = Array.Find(nodes, n => n.name == conns[0]);
                Node n2 = Array.Find(nodes, n => n.name == conns[1]);
                Arc arc = new Arc(ed, n1, n2, rawData.arcsWeights[i], name: rawData.arcNames[i]);

                n1.AddConnection(n2, arc);
                arcs[i] = arc;
            }
            return true;
        }

        public static RawGraphData? LoadRawData(string jsonString)
        {
            Console.WriteLine("LOADING JSON: "+jsonString);
            return JsonSerializer.Deserialize<RawGraphData>(jsonString);
        }

        public static string LoadJsonData(string path)
        {
            return File.ReadAllText(path);
        }
    }

}

