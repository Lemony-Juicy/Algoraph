namespace Algoraph.Scripts.Saving_and_Loading
{
    class RawGraphData
    {
        public string? DataSet { get; set;} 
        public string[]? nodeNames { get; set; }
        public double[][]? nodePositions { get; set; }

        public uint[]? arcsWeights { get; set; }

        // EG: {["N4", "N5"], ["N2", "N1"], ...}
        public string[][]? arcConns { get; set; }
    }
}
