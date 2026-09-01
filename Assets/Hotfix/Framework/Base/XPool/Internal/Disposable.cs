using System;

namespace MackySoft.XPool.Internal {
	internal static class Disposable {

		public static IDisposable Combine (IDisposable disposable1,IDisposable disposable2) {
			return new Binary(disposable1,disposable2);
		}

		class Binary : IDisposable {

			readonly IDisposable m_Disposable1;
			readonly IDisposable m_Disposable2;

			public Binary (IDisposable disposable1,IDisposable disposable2) {
				m_Disposable1 = disposable1;
				m_Disposable2 = disposable2;
			}

			public void Dispose () {
				m_Disposable1.Dispose();
				m_Disposable2.Dispose();
			}
		}
	}
}