using System.Collections.Generic;

namespace Algoraph.Scripts
{
    internal class UndoStack
    {
        int stackSize;
        List<string> undoList;

        public UndoStack(int stackSize = 100)
        {
            this.stackSize = stackSize;
            undoList = new List<string>();
        }

        public void Push(string jsonData)
        {
            undoList.Add(jsonData);
            if (undoList.Count > stackSize) 
            {
                undoList.RemoveAt(0);
            }
        }

        public string? Pop()
        {
            int index = undoList.Count - 1;

            if (undoList.Count == 0) 
            { 
                return null;
            }
            string jsonData = undoList[index];
            undoList.RemoveAt(index);
            return jsonData;
        }

        public void LoadPrevious(Editor ed)
        {
            string? jsonData = Pop();
            if (jsonData != null)
            {
                ed.LoadJsonData(jsonData);
                ed.MarkAsChanged();
            }  
        }
    }
}
