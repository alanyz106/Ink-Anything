using System.Collections.Generic;

namespace Ink_Anything.Helpers
{
    public class TextUndoStack
    {
        private readonly List<TextUndoEntry> _history = new List<TextUndoEntry>();
        private int _currentIndex = -1;

        public bool CanUndo => _currentIndex >= 0;
        public bool CanRedo => _currentIndex + 1 < _history.Count;

        public void CommitAdd(TextElementData data)
        {
            TrimFuture();
            _history.Add(new TextUndoEntry(TextAction.Add, data));
            _currentIndex = _history.Count - 1;
        }

        public void CommitRemove(TextElementData data)
        {
            TrimFuture();
            _history.Add(new TextUndoEntry(TextAction.Remove, data));
            _currentIndex = _history.Count - 1;
        }

        public void CommitMove(TextElementData data, double oldX, double oldY, double newX, double newY)
        {
            TrimFuture();
            var entry = new TextUndoEntry(TextAction.Move, data);
            entry.OldX = oldX;
            entry.OldY = oldY;
            entry.NewX = newX;
            entry.NewY = newY;
            _history.Add(entry);
            _currentIndex = _history.Count - 1;
        }

        public void CommitResize(TextElementData data, double oldFontSize, double newFontSize)
        {
            TrimFuture();
            var entry = new TextUndoEntry(TextAction.Resize, data);
            entry.OldFontSize = oldFontSize;
            entry.NewFontSize = newFontSize;
            _history.Add(entry);
            _currentIndex = _history.Count - 1;
        }

        public TextUndoEntry Undo()
        {
            if (!CanUndo) return null;
            var entry = _history[_currentIndex];
            entry.IsUndone = true;
            _currentIndex--;
            return entry;
        }

        public TextUndoEntry Redo()
        {
            if (!CanRedo) return null;
            _currentIndex++;
            var entry = _history[_currentIndex];
            entry.IsUndone = false;
            return entry;
        }

        public void Clear()
        {
            _history.Clear();
            _currentIndex = -1;
        }

        private void TrimFuture()
        {
            if (_currentIndex + 1 < _history.Count)
            {
                _history.RemoveRange(_currentIndex + 1, _history.Count - 1 - _currentIndex);
            }
        }
    }

    public class TextUndoEntry
    {
        public TextAction Action { get; set; }
        public TextElementData Data { get; set; }
        public bool IsUndone { get; set; }
        public double OldX { get; set; }
        public double OldY { get; set; }
        public double NewX { get; set; }
        public double NewY { get; set; }
        public double OldFontSize { get; set; }
        public double NewFontSize { get; set; }

        public TextUndoEntry(TextAction action, TextElementData data)
        {
            Action = action;
            Data = data;
        }
    }

    public enum TextAction
    {
        Add,
        Remove,
        Move,
        Resize
    }
}
