using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public interface IPositionProvider
    {
        Vector3 position { get; }
    }

    /// <summary>
    /// X-Z平面的KdTree，用于查找圆内范围和k近邻
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class KdTree<T> where T : IPositionProvider
    {
        private const int AXIS_X = 0;
        private const int AXIS_Z = 1;

        private KdTreeNode<T> _root;
        private int _count;

        public int Count => _count;

        public void Build(IList<T> items)
        {
            Clear();

            if (items == null || items.Count == 0)
            {
                return;
            }

            _count = items.Count;
            var work = new T[_count];
            for (int i = 0; i < _count; i++)
            {
                work[i] = items[i];
            }

            _root = BuildRecursive(work, 0, _count, 0);
        }

        public void Clear()
        {
            _root = null;
            _count = 0;
        }

        public List<T> FindKNearest(Vector3 query, int k, System.Func<T, bool> predicate = null)
        {
            var result = new List<T>(Mathf.Max(k, 0));
            if (_root == null || k <= 0)
            {
                return result;
            }

            var heap = new KnnMaxHeap<T>(k);
            FindKNearestRecursive(_root, query, k, heap, predicate);

            heap.SortAscendingInto(result);
            return result;
        }

        public List<T> FindInRange(Vector3 center, float radius, System.Func<T, bool> predicate = null)
        {
            var result = new List<T>();
            if (_root == null || radius < 0f)
            {
                return result;
            }

            float radiusSq = radius * radius;
            FindInRangeRecursive(_root, center, radius, radiusSq, result, predicate);
            return result;
        }

        public List<TDerived> FindKNearest<TDerived>(Vector3 query, int k, System.Func<TDerived, bool> predicate = null)
            where TDerived : T
        {
            System.Func<T, bool> combined = item =>
            {
                if (item is TDerived derived)
                {
                    return predicate == null || predicate(derived);
                }

                return false;
            };
            var baseResult = FindKNearest(query, k, combined);
            return CastList<TDerived>(baseResult);
        }

        public List<TDerived> FindInRange<TDerived>(Vector3 center, float radius, System.Func<TDerived, bool> predicate = null)
            where TDerived : T
        {
            System.Func<T, bool> combined = item =>
            {
                if (item is TDerived derived)
                {
                    return predicate == null || predicate(derived);
                }

                return false;
            };
            var baseResult = FindInRange(center, radius, combined);
            return CastList<TDerived>(baseResult);
        }

        private static List<TDerived> CastList<TDerived>(List<T> source) where TDerived : T
        {
            var result = new List<TDerived>(source.Count);
            for (int i = 0; i < source.Count; i++)
            {
                result.Add((TDerived)source[i]);
            }

            return result;
        }

        private static KdTreeNode<T> BuildRecursive(T[] items, int start, int end, int depth)
        {
            int length = end - start;
            if (length <= 0)
            {
                return null;
            }

            int axis = depth % 2;
            int mid = start + (length / 2);
            QuickselectByAxis(items, start, end, axis, mid);

            var node = new KdTreeNode<T>
            {
                Point = items[mid],
                Axis = axis,
            };
            node.Left = BuildRecursive(items, start, mid, depth + 1);
            node.Right = BuildRecursive(items, mid + 1, end, depth + 1);
            return node;
        }

        private static void QuickselectByAxis(T[] items, int start, int end, int axis, int mid)
        {
            while (true)
            {
                int left = start;
                int right = end - 1;
                if (left >= right)
                {
                    return;
                }

                int pivotIndex = (left + right) >> 1;
                float pivot = GetAxis(items[pivotIndex], axis);
                Swap(items, pivotIndex, right);
                int store = left;
                for (int i = left; i < right; i++)
                {
                    if (GetAxis(items[i], axis) < pivot)
                    {
                        Swap(items, i, store);
                        store++;
                    }
                }

                Swap(items, store, right);

                if (mid == store)
                {
                    return;
                }

                if (mid < store)
                {
                    end = store;
                }
                else
                {
                    start = store + 1;
                }
            }
        }

        private static void Swap(T[] items, int a, int b)
        {
            var tmp = items[a];
            items[a] = items[b];
            items[b] = tmp;
        }

        private static float GetAxis(T obj, int axis)
        {
            if (obj == null) return 0f;
            var p = obj.position;
            return axis == AXIS_X ? p.x : p.z;
        }

        private static void FindKNearestRecursive(KdTreeNode<T> node, Vector3 query, int k, KnnMaxHeap<T> heap, System.Func<T, bool> predicate)
        {
            if (node == null || node.Point == null)
            {
                return;
            }

            if (predicate == null || predicate(node.Point))
            {
                float distSq = SqrDistanceXZ(node.Point.position, query);
                if (heap.Count < k)
                {
                    heap.Push(node.Point, distSq);
                }
                else if (distSq < heap.TopDistanceSq)
                {
                    heap.ReplaceTop(node.Point, distSq);
                }
            }

            float queryAxis = GetAxisQuery(query, node.Axis);
            float nodeAxis = GetAxis(node.Point, node.Axis);
            float axisDelta = queryAxis - nodeAxis;
            if (axisDelta < 0f) axisDelta = -axisDelta;

            KdTreeNode<T> first;
            KdTreeNode<T> second;
            if (queryAxis < nodeAxis)
            {
                first = node.Left;
                second = node.Right;
            }
            else
            {
                first = node.Right;
                second = node.Left;
            }

            if (first != null && (heap.Count < k || axisDelta * axisDelta < heap.TopDistanceSq))
            {
                FindKNearestRecursive(first, query, k, heap, predicate);
            }

            if (second != null && (heap.Count < k || axisDelta * axisDelta < heap.TopDistanceSq))
            {
                FindKNearestRecursive(second, query, k, heap, predicate);
            }
        }

        private static void FindInRangeRecursive(KdTreeNode<T> node, Vector3 center, float radius, float radiusSq, List<T> result, System.Func<T, bool> predicate)
        {
            if (node == null || node.Point == null)
            {
                return;
            }

            if (predicate == null || predicate(node.Point))
            {
                float distSq = SqrDistanceXZ(node.Point.position, center);
                if (distSq <= radiusSq)
                {
                    InsertSortedByDistSq(result, node.Point, distSq);
                }
            }

            float queryAxis = GetAxisQuery(center, node.Axis);
            float nodeAxis = GetAxis(node.Point, node.Axis);
            float axisDelta = queryAxis - nodeAxis;
            if (axisDelta < 0f) axisDelta = -axisDelta;

            if (axisDelta <= radius)
            {
                FindInRangeRecursive(node.Left, center, radius, radiusSq, result, predicate);
                FindInRangeRecursive(node.Right, center, radius, radiusSq, result, predicate);
            }
            else
            {
                if (queryAxis < nodeAxis)
                {
                    FindInRangeRecursive(node.Left, center, radius, radiusSq, result, predicate);
                }
                else
                {
                    FindInRangeRecursive(node.Right, center, radius, radiusSq, result, predicate);
                }
            }
        }

        private static void InsertSortedByDistSq(List<T> result, T obj, float distSq)
        {
            int i = result.Count - 1;
            while (i >= 0 && _rangeDistSq[i] > distSq)
            {
                i--;
            }

            result.Insert(i + 1, obj);
            _rangeDistSq.Insert(i + 1, distSq);
        }

        private static readonly List<float> _rangeDistSq = new List<float>();

        private static float GetAxisQuery(Vector3 query, int axis)
        {
            return axis == AXIS_X ? query.x : query.z;
        }

        private static float SqrDistanceXZ(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x;
            float dz = a.z - b.z;
            return dx * dx + dz * dz;
        }

        private class KnnMaxHeap<TItem> where TItem : IPositionProvider
        {
            private readonly TItem[] _points;
            private readonly float[] _dists;
            public int Count { get; private set; }
            public float TopDistanceSq => _dists[0];

            public KnnMaxHeap(int capacity)
            {
                _points = new TItem[capacity];
                _dists = new float[capacity];
            }

            public void Push(TItem point, float distSq)
            {
                int i = Count;
                _points[i] = point;
                _dists[i] = distSq;
                Count++;
                SiftUp(i);
            }

            public void ReplaceTop(TItem point, float distSq)
            {
                _points[0] = point;
                _dists[0] = distSq;
                SiftDown(0);
            }

            public void SortAscendingInto(List<TItem> output)
            {
                int n = Count;
                var pts = new TItem[n];
                var dts = new float[n];
                for (int i = 0; i < n; i++)
                {
                    pts[i] = _points[i];
                    dts[i] = _dists[i];
                }

                System.Array.Sort(dts, pts, 0, n);
                for (int i = 0; i < n; i++)
                {
                    output.Add(pts[i]);
                }
            }

            private void SiftUp(int i)
            {
                while (i > 0)
                {
                    int parent = (i - 1) >> 1;
                    if (_dists[i] <= _dists[parent]) break;
                    Swap(i, parent);
                    i = parent;
                }
            }

            private void SiftDown(int i)
            {
                int n = Count;
                while (true)
                {
                    int left = i * 2 + 1;
                    int right = left + 1;
                    int largest = i;
                    if (left < n && _dists[left] > _dists[largest]) largest = left;
                    if (right < n && _dists[right] > _dists[largest]) largest = right;
                    if (largest == i) break;
                    Swap(i, largest);
                    i = largest;
                }
            }

            private void Swap(int a, int b)
            {
                var tp = _points[a];
                _points[a] = _points[b];
                _points[b] = tp;
                var td = _dists[a];
                _dists[a] = _dists[b];
                _dists[b] = td;
            }
        }

        private class KdTreeNode<TNode> where TNode : IPositionProvider
        {
            public TNode Point;
            public int Axis;
            public KdTreeNode<TNode> Left;
            public KdTreeNode<TNode> Right;
        }
    }
}