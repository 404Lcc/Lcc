namespace Pathfinding.Util {
	/// <summary>Calculates checksums of byte arrays</summary>
	internal class Checksum {
		/// <summary>Calculate checksum for the byte array.</summary>
		/// <param name="arr">Byte array to calculate checksum for. May be null.</param>
		/// <param name="hash">Initial hash value. Default is 0. Can be used to chain checksums together.</param>
		public static uint GetChecksum (byte[] arr, uint hash = 0) {
			// Sort of implements the Fowler–Noll–Vo hash function
			const int prime = 16777619;

			hash ^= 2166136261U;
			if (arr == null) return hash - 1;

			for (int i = 0; i < arr.Length; i++)
				hash = (hash ^ arr[i]) * prime;

			return hash;
		}
	}
}
