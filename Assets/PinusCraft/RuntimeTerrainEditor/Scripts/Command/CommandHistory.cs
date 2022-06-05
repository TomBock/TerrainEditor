using System.Collections.Generic;

namespace RuntimeTerrainEditor
{    
    public class CommandHistory
    {
        private static LinkedList<ICommand> _undoHistory = new LinkedList<ICommand>();
        private static LinkedList<ICommand> _redoHistory = new LinkedList<ICommand>();

        public static void Register(ICommand command)
        {
            if (_undoHistory.Count > Constants.MAX_UNDO)
            {
                _undoHistory.RemoveLast();    
            }

            _undoHistory.AddFirst(command);
            _redoHistory.Clear();
        }

        public static void Undo()
        {
            if (_undoHistory.Count > 0)
            {
                var cmd = _undoHistory.First;
                cmd.Value.Undo();

                _undoHistory.Remove(cmd);
                _redoHistory.AddFirst(cmd);
            }
        }

        public static void Redo()
        {
            if (_redoHistory.Count > 0)
            {
                var cmd = _redoHistory.First;
                cmd.Value.Execute();

                _redoHistory.Remove(cmd);
                _undoHistory.AddFirst(cmd);
            }
        }

        public static void Clear()
        {
            _undoHistory.Clear();
            _redoHistory.Clear();
        }
    }
}