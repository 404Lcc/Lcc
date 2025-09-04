using Unity.Jobs;

namespace Pathfinding.Sync {
	public interface IProgress {
		float Progress { get; }
	}

	/// <summary>
	/// A promise that T is being calculated asynchronously.
	///
	/// The result can be accessed by calling <see cref="Complete"/>. This will block until the calculation is complete.
	/// </summary>
	public struct Promise<T>: IProgress, System.IDisposable where T : IProgress, System.IDisposable {
		public JobHandle handle;
		T result;

		public Promise(JobHandle handle, T result) {
			this.handle = handle;
			this.result = result;
		}

		public bool IsCompleted {
			get {
				return handle.IsCompleted;
			}
		}

		public float Progress {
			get {
				return result.Progress;
			}
		}

		public T GetValue () {
			return result;
		}

		public T Complete () {
			handle.Complete();
			return result;
		}

		public void Dispose () {
			Complete();
			result.Dispose();
		}
	}
}
