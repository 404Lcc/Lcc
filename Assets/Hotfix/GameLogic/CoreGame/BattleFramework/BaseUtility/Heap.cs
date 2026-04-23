using System.Collections;
using System.Collections.Generic;
namespace CoreGame
{
    public class HeapItem : System.IComparable
    {
        public int _heap_index = -1;
        public int _insertion_index = -1;
        public virtual int CompareTo(object obj)
        {
            // < 0 : This instance precedes obj in the sort order. 
            //== 0 : This instance occurs in the same position in the sort order as obj. 
            // > 0 : This instance follows obj in the sort order.
            HeapItem item = obj as HeapItem;
            if (item == null)
                return 1;
            return _insertion_index - item._insertion_index;
        }
        public bool IsInHeap()
        {
            return _heap_index >= 0;
        }
    }

    public class Heap<T> where T : HeapItem
    {
        public enum CheckPriorityMethod
        {
            CPM_CUSTOM = 0,
            CPM_GREATER,
            CPM_LESS,
        }
        public delegate bool CheckPriorityFunc(T higher, T lower);
        public delegate int CompareFunc(T higher, T lower);

        List<T> m_data = new List<T>();
        int m_item_ever_enqueued = 0;
        CheckPriorityFunc m_check_priority_func = null;
        CompareFunc m_compare_func = null;

        public Heap(CompareFunc compare_func)
        {
            m_compare_func = compare_func;
            m_check_priority_func = CheckPriorityByComparer;
        }

        public void Destruct()
        {
            Clear();
            m_check_priority_func = null;
            m_compare_func = null;
        }

        public Heap(CheckPriorityMethod cpm, CompareFunc compare_func = null)
        {
            switch (cpm)
            {
            case CheckPriorityMethod.CPM_CUSTOM:
                m_compare_func = compare_func;
                m_check_priority_func = CheckPriorityByComparer;
                break;
            case CheckPriorityMethod.CPM_GREATER:
                m_check_priority_func = CheckPriorityByGreater;
                break;
            case CheckPriorityMethod.CPM_LESS:
                m_check_priority_func = CheckPriorityByLess;
                break;
            default:
                break;
            }
        }

        public void Build()
        {
            int last_phi = Parent(m_data.Count - 1);
            for (int hi = last_phi; hi >= 0; --hi)
                CascadeDown(hi);
        }

        public void Clear()
        {
            for (int hi = 0; hi < m_data.Count; ++hi)
                m_data[hi]._heap_index = -1;
            m_data.Clear();
        }

        public int Size()
        {
            return m_data.Count;
        }

        public bool Empty()
        {
            return m_data.Count == 0;
        }

        public T Peek()
        {
            if (m_data.Count > 0)
                return m_data[0];
            return null;
        }

        public T GetAt(int hi)
        {
            if (hi >= 0 && hi < m_data.Count)
                return m_data[hi];
            return null;
        }

        public bool Contains(T item)
        {
            return item._heap_index >= 0;
        }

        public void Enqueue(T item)
        {
            int hi = item._heap_index;
            if (hi >= 0)
            {
                UpdatePriorityByIndex(hi);
                return;
            }
            else
            {
                hi = m_data.Count;
                item._heap_index = hi;
                item._insertion_index = m_item_ever_enqueued++;
                m_data.Add(item);
                CascadeUp(hi);
            }
        }

        public T Dequeue()
        {
            int size = m_data.Count;
            if (size == 0)
                return null;
            T item = m_data[0];
            item._heap_index = -1;
            if (size == 1)
            {
                m_data.RemoveAt(0);
            }
            else
            {
                T temp = m_data[size - 1];
                m_data[0] = temp;
                temp._heap_index = 0;
                m_data.RemoveAt(size - 1);
                CascadeDown(0);
            }
            return item;
        }

        public void UpdatePriority(T item)
        {
            int hi = item._heap_index;
            if (hi >= 0)
                UpdatePriorityByIndex(hi);
        }

        public void UpdatePriorityByIndex(int hi)
        {
            if (hi < 0 || hi >= m_data.Count)
                return;
            int phi = Parent(hi);
            if (phi >= 0 && m_check_priority_func(m_data[hi], m_data[phi]))
                CascadeUp(hi);
            else
                CascadeDown(hi);
        }

        public void Remove(T item)
        {
            RemoveByIndex(item._heap_index);
        }

        public void RemoveByIndex(int hi)
        {
            if (hi < 0 || hi >= m_data.Count)
                return;
            m_data[hi]._heap_index = -1;
            int size = m_data.Count;
            if (hi == (size - 1))
            {
                m_data.RemoveAt(size - 1);
            }
            else
            {
                T temp = m_data[size - 1];
                m_data[hi] = temp;
                temp._heap_index = hi;
                m_data.RemoveAt(size - 1);
                UpdatePriorityByIndex(hi);
            }
        }

        public bool Check(T must_less_priority_than_this)
        {
            for (int hi = 0; hi < m_data.Count; ++hi)
            {
                T item = m_data[hi];
                if (item._heap_index != hi)
                    return false;
                if (must_less_priority_than_this != null && m_check_priority_func(item, must_less_priority_than_this))
                    return false;
                int pi = Parent(hi);
                if (pi >= 0)
                {
                    if (m_check_priority_func(item, m_data[pi]))
                        return false;
                }
            }
            return true;
        }

        #region 私有方法
        int Parent(int i)
        {
            return (i - 1) >> 1;
        }

        int LeftChild(int i)
        {
            return (i << 1) + 1;
        }

        int RightChild(int i)
        {
            return (i << 1) + 2;
        }

        void CascadeUp(int hi)
        {
            int phi = Parent(hi);
            while (phi >= 0)
            {
                if (m_check_priority_func(m_data[hi], m_data[phi]))
                {
                    Swap(hi, phi);
                    hi = phi;
                    phi = Parent(hi);
                }
                else
                    break;
            }
        }

        void CascadeDown(int hi)
        {
            int lhi = -1;
            int rhi = -1;
            int largest = hi;
            while (true)
            {
                lhi = LeftChild(hi);
                rhi = RightChild(hi);
                if (lhi < m_data.Count && m_check_priority_func(m_data[lhi], m_data[largest]))
                    largest = lhi;
                if (rhi < m_data.Count && m_check_priority_func(m_data[rhi], m_data[largest]))
                    largest = rhi;
                if (largest == hi)
                    break;
                Swap(hi, largest);
                hi = largest;
            }
        }

        void Swap(int i, int j)
        {
            T temp_i = m_data[i];
            T temp_j = m_data[j];
            m_data[i] = temp_j;
            m_data[j] = temp_i;
            temp_i._heap_index = j;
            temp_j._heap_index = i;
        }

        bool CheckPriorityByGreater(T higher, T lower)
        {
            int result = higher.CompareTo(lower);
            if (result > 0)
                return true;
            else if (result < 0)
                return false;
            else
                return higher._insertion_index < lower._insertion_index;
        }

        bool CheckPriorityByLess(T higher, T lower)
        {
            int result = higher.CompareTo(lower);
            if (result < 0)
                return true;
            else if (result > 0)
                return false;
            else
                return higher._insertion_index < lower._insertion_index;
        }

        bool CheckPriorityByComparer(T higher, T lower)
        {
            if (m_compare_func != null)
            {
                int result = m_compare_func(higher, lower);
                if (result < 0)
                    return true;
                else if (result > 0)
                    return false;
                else
                    return higher._insertion_index < lower._insertion_index;
            }
            return higher._insertion_index < lower._insertion_index;
        }
        #endregion
    }
}