using System.Runtime.CompilerServices;
using UnityEngine;

namespace MackySoft.XPool.Internal {
    internal static partial class Error {
	    
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static MissingReferenceException InstanceDestroyed () {
			return new MissingReferenceException("The instance was destroyed in callback.");
		}
	}
}