using System.Collections.Generic;
using MackySoft.XPool.Collections.ObjectModel;

namespace MackySoft.XPool.Collections {
	public class ListPool<T> : CollectionPoolBase<List<T>> {

		public static readonly ListPool<T> Shared = new ListPool<T>();

		public ListPool () : base(CollectionPoolUtility.kDefaultCapacity,() => new List<T>(),list => list.Clear()) {
		}
	}

	public class QueuePool<T> : CollectionPoolBase<Queue<T>> {

		public static readonly QueuePool<T> Shared = new QueuePool<T>();

		public QueuePool () : base(CollectionPoolUtility.kDefaultCapacity,() => new Queue<T>(),queue => queue.Clear()) {
		}
	}

	public class StackPool<T> : CollectionPoolBase<Stack<T>> {

		public static readonly StackPool<T> Shared = new StackPool<T>();

		public StackPool () : base(CollectionPoolUtility.kDefaultCapacity,() => new Stack<T>(),stack => stack.Clear()) {
		}
	}

	public class HashSetPool<T> : CollectionPoolBase<HashSet<T>> {

		public static readonly HashSetPool<T> Shared = new HashSetPool<T>();

		public HashSetPool () : base(CollectionPoolUtility.kDefaultCapacity,() => new HashSet<T>(),hashSet => hashSet.Clear()) {
		}
	}

	public class DictionaryPool<TKey,TValue> : CollectionPoolBase<Dictionary<TKey,TValue>> {

		public static readonly DictionaryPool<TKey,TValue> Shared = new DictionaryPool<TKey,TValue>();

		public DictionaryPool () : base(CollectionPoolUtility.kDefaultCapacity,() => new Dictionary<TKey, TValue>(),dictionary => dictionary.Clear()) {
		}
	}
}