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
                // When the node is unchecked, an event is called to render it differently
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
            nodes.Remove(node);
        }

        public void OnCheck(Node? node, SelectedArcs selectedArcs)
        {
            // Clearing any selected arcs before selecting a node
            selectedArcs.ClearItems();

            AddItem(node);
        }

        public void OnUncheck(Node? node)
        {
            RemoveItem(node);
        }

        public void OnLeave()
        {
            // Program checks whether any nodes are selected, and mouse is in cross mode
            if (previousMouseMode == Cursors.Cross && nodes.Count == 0)
                ed.CursorCrossMode();
            else
                // We set mouse to arrow mode if there are selected items
                // because the user can click off onto the canvas to deselect
                // any seleted items WITHOUT adding a node to the canvas
                ed.CursorArrowMode();
        }

        public void OnEnter()
        {
            // This is called when the mouse hovers over the node
            // We keep track of the cursor icon before it entered the node
            previousMouseMode = ed.mainPanel.Cursor;
            // We then change the cursor icon to hand mode
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

