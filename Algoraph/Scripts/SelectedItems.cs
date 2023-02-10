using System.Collections.Generic;
using System.Windows.Input;

namespace Algoraph.Scripts
{
    internal class SelectedNodes
    {
        public List<Node> nodes { get; private set; }
        Cursor? previousMouseMode = null;
        Editor ed;

        public SelectedNodes(Editor ed)
        {
            nodes = new List<Node>();
            this.ed = ed;
        }

        public void ClearItems()
        {
            foreach (Node node in nodes.ToArray())
                node.Uncheck();
            nodes.Clear();
        }

        public void AddItem(Node? node)
        {
            if (node == null || nodes.Contains(node)) return;

            nodes.Add(node);
        }

        void RemoveItem(Node? node)
        {
            if (node == null) return;

            nodes.Remove(node);
        }

        public void OnCheck(Node? node, SelectedArcs selectedArcs)
        {
            selectedArcs.ClearItems();
            AddItem(node);
        }

        public void OnUncheck(Node? node)
        {
            RemoveItem(node);
        }

        public void OnLeave()
        {
            if (previousMouseMode == Cursors.Cross && nodes.Count == 0)
                ed.CursorCrossMode();
            else
                ed.CursorArrowMode();
        }

        public void OnEnter()
        {
            previousMouseMode = ed.mainPanel.Cursor;
            ed.CursorHandMode();
        }
    }



    internal class SelectedArcs
    {
        public List<Arc> arcs { get; private set; }
        Cursor? previousMouseMode = null;
        Editor ed;

        public SelectedArcs(Editor ed)
        {
            arcs = new List<Arc>();
            this.ed = ed;
        }

        public void ClearItems()
        {
            foreach (Arc arc in arcs.ToArray())
                arc.Uncheck();
            arcs.Clear();
        }

        public void AddItem(Arc? arc)
        {
            if (arc == null || arcs.Contains(arc)) return;
            arc.Check();
            arcs.Add(arc);
        }

        public void AddItems(Arc[] arcs)
        {
            foreach (Arc arc in arcs)
                AddItem(arc);
        }

        public void RemoveItem(Arc? arc)
        {
            if (arc == null) return;

            arc.Uncheck();
            arcs.Remove(arc);
        }

        public void OnCheck(Arc? arc, SelectedNodes selectedNodes)
        {
            selectedNodes.ClearItems();
            AddItem(arc);
        }

        public void OnUncheck(Arc? arc) => RemoveItem(arc);
        

        public void OnLeave()
        {
            if (previousMouseMode == Cursors.Cross && arcs.Count == 0)
                ed.CursorCrossMode();
            else
                ed.CursorArrowMode();
        }

        public void OnEnter()
        {
            previousMouseMode = ed.mainPanel.Cursor;
            ed.CursorHandMode();
        }
    }
}

