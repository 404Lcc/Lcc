using System;
using System.Runtime.CompilerServices;

namespace MackySoft.XPool.Internal {
    internal static partial class Error {

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ArgumentNullException ArgumentNullException (string paramName) {
			return new ArgumentNullException(paramName);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static InvalidOperationException EmptyCollection () {
			return new InvalidOperationException("Collection is empty.");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ArgumentOutOfRangeException ArgumentOutOfRangeOfCollection (string paramName) {
			return new ArgumentOutOfRangeException(paramName,"Parameter is out of range of collection.");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ArgumentOutOfRangeException RequiredNonNegative (string paramName) {
			return new ArgumentOutOfRangeException(paramName,"Must be a non-negative value.");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ArgumentOutOfRangeException RequiredGreaterThanZero (string paramName) {
			return new ArgumentOutOfRangeException(paramName,"The value must be greater than 0.");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static NullReferenceException FactoryMustReturnNotNull () {
			return new NullReferenceException("Factory method must return not null.");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ArgumentException InvalidOffLength () {
			return new ArgumentException("Parameter is invalid off length.");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ArgumentOutOfRangeException ArgumentOutOfRangeCount (string paramName) {
			return new ArgumentOutOfRangeException(paramName);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ArgumentException TemporaryIsDisposed (string paramName) {
			return new ArgumentException(paramName,"A temporary object is already disposed.");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static InvalidOperationException CannotSetCallback () {
			return new InvalidOperationException("Cannot set the callback because the pool is active.");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static NotSupportedException FunctionIsNotSupported () {
			return new NotSupportedException("This function is not supported.");
		}

	}
}