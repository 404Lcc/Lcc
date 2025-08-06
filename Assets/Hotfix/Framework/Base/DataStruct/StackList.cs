using System.Collections.Generic;

namespace LccHotfix
{
    public class StackList<T>
    {
        private Stack<T> _stack = new Stack<T>();
        private List<T> _valueList = new List<T>();

        public int Count => _stack.Count;
        public List<T> ValueList => _valueList;
        
        public void Push(T value)
        {
            _stack.Push(value);
            _valueList.Add(value);
        }

        public T Pop()
        {
            if (_valueList.Count == 0)
                return default;
            
            _valueList.RemoveAt(_valueList.Count - 1);
            return _stack.Pop();
        }

        public T Peek()
        {
            return _stack.Peek();
        }

        public bool Contains(T t)
        {
            return _valueList.Contains(t);
        }

        public void Clear()
        {
            _stack.Clear();
            _valueList.Clear();
        }
    }
}