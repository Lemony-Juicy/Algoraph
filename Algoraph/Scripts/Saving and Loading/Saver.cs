﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.IO;
using System.Windows;
using Algoraph.Scripts.Saving_and_Loading;

namespace Algoraph.Scripts
{
    internal class Saver
    {
        public static string HEADER_KEY = "DataSet";
        public static string HEADER_ITEM = "For AlgoRaph";

        public static string GetJsonData(List<Node> nodes, List<Arc> arcs)
        {
            // Converts the nodes and arcs data into a JSON string and returns it
            RawGraphData rawData = new()
            {
                DataSet = HEADER_ITEM,
                nodeNames = nodes.Select(n => n.name).ToArray(),
                nodePositions = nodes.Select(n => new double[2] { n.GetLocation().X, n.GetLocation().Y }).ToArray(),

                arcsWeights = arcs.Select(a => a.weight).ToArray(),
                arcConns = arcs.Select(a => a.connections.Select(n => n.name).ToArray()).ToArray()
            };

            return JsonSerializer.Serialize(rawData);
        }

        public static void SaveJsonData(string jsonString, string path)
        {
            File.WriteAllText(path, jsonString);
        }

        // Returns true if successful, else false
        public static bool LoadNodesArcs(Editor ed, string jsonString, out Arc[]? arcs, out Node[]? nodes)
        {
            // Using the JSON string, this method will create and associate the nodes and arcs together, 
            // creating the objects and storing them in the 'arcs' and 'nodes' variables
            try
            {
                RawGraphData? rawData = LoadRawData(jsonString);
               
                if (rawData == null || rawData.DataSet != HEADER_ITEM)
                {
                    arcs = null;
                    nodes = null;
                    return false;
                }

                arcs = new Arc[rawData.arcsWeights.Length];
                nodes = new Node[rawData.nodeNames.Length];

                for (int i = 0; i < rawData.nodeNames.Length; i++)
                {
                    double[] pos = rawData.nodePositions[i];
                    nodes[i] = new Node(
                        ed: ed,
                        location: new Point(pos[0], pos[1]),
                        connections: new List<Node>(),
                        name: rawData.nodeNames[i]);
                }

                for (int i = 0; i < rawData.arcsWeights.Length; i++)
                {
                    string[] conns = rawData.arcConns[i];
                    Node n1 = Array.Find(nodes, n => n.name == conns[0]);
                    Node n2 = Array.Find(nodes, n => n.name == conns[1]);
                    Arc arc = new(n1, n2, rawData.arcsWeights[i], ed);
                    Node.ConnectNodes(n1, n2, arc);
                    arcs[i] = arc;
                }
                return true;
            }
            catch
            {
                arcs = null;
                nodes = null;
                return false;
            }
        }

        public static RawGraphData? LoadRawData(string jsonString)
        {
            // Using the JsonSerializer, we can deserialize the JSON string into a 
            // RawGraphData object which will make it easier to extract the data needed
            // to create the node and arc objects.
            return JsonSerializer.Deserialize<RawGraphData>(jsonString);
        }

        public static string LoadJsonData(string path)
        {
            return File.ReadAllText(path);
        }
    }
}

