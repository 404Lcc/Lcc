using Unity.Mathematics;

namespace andywiecko.BurstTriangulator {
	/// <summary>
	/// A signed 128-bit integer.
	/// </summary>
	struct I128 {
		ulong hi;
		ulong lo;

		public bool IsNegative => (hi & 0x8000000000000000UL) != 0;

		public I128(ulong hi, ulong lo) => (this.hi, this.lo) = (hi, lo);
		public static I128 operator + (I128 a, I128 b) {
			var lo = a.lo + b.lo;
			var hi = a.hi + b.hi + (lo < a.lo ? 1UL : 0);
			return new(hi, lo);
		}

		public static I128 operator - (I128 a, I128 b) {
			var lo = a.lo - b.lo;
			var hi = a.hi - b.hi - (lo > a.lo ? 1UL : 0);
			return new(hi, lo);
		}

		public static I128 operator -(I128 a) => new I128(~a.hi, ~a.lo) + new I128(0, 1);

		/// <summary>
		/// Multiplies two 64-bit signed integers, without any possibility of overflow.
		/// </summary>
		public static I128 Multiply (long slhs, long srhs) {
			// From https://stackoverflow.com/a/58381061
			var negative = (slhs < 0) ^ (srhs < 0);
			ulong lhs = (ulong)math.abs(slhs);
			ulong rhs = (ulong)math.abs(srhs);

			// First calculate all of the cross products.
			ulong lo_lo = (lhs & 0xFFFFFFFFUL) * (rhs & 0xFFFFFFFFUL);
			ulong hi_lo = (lhs >> 32)          * (rhs & 0xFFFFFFFFUL);
			ulong lo_hi = (lhs & 0xFFFFFFFFUL) * (rhs >> 32);
			ulong hi_hi = (lhs >> 32)          * (rhs >> 32);

			// Now add the products together. These will never overflow.
			ulong cross = (lo_lo >> 32) + (hi_lo & 0xFFFFFFFFUL) + lo_hi;
			ulong upper = (hi_lo >> 32) + (cross >> 32)          + hi_hi;

			var res = new I128(upper, (cross << 32) | (lo_lo & 0xFFFFFFFFUL));
			res = negative ? -res : res;
			return res;
		}
	}
}
