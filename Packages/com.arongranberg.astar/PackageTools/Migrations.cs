namespace Pathfinding.Serialization {
	/// <summary>
	/// Helper struct for handling serialization backwards compatibility.
	///
	/// It stores which migrations have been completed as a bitfield.
	/// </summary>
	public struct Migrations {
		/// <summary>Bitfield of all migrations that have been run</summary>
		internal int finishedMigrations;
		/// <summary>
		/// Bitfield of all migrations that the component supports.
		/// A newly created component will be initialized with this value.
		/// </summary>
		internal int allMigrations;
		internal bool ignore;

		/// <summary>A special migration flag which is used to mark that the version has been migrated to the bitfield format, from the legacy linear version format</summary>
		const int MIGRATE_TO_BITFIELD = 1 << 30;

		public bool IsLegacyFormat => (finishedMigrations & MIGRATE_TO_BITFIELD) == 0;
		public int LegacyVersion => finishedMigrations;

		public Migrations(int value) {
			this.finishedMigrations = value;
			allMigrations = MIGRATE_TO_BITFIELD;
			ignore = false;
		}

		public bool TryMigrateFromLegacyFormat (out int legacyVersion) {
			legacyVersion = finishedMigrations;
			if (IsLegacyFormat) {
				this = new Migrations(MIGRATE_TO_BITFIELD);
				return true;
			} else return false;
		}

		public void MarkMigrationFinished (int flag) {
			if (IsLegacyFormat) throw new System.InvalidOperationException("Version must first be migrated to the bitfield format");
			finishedMigrations |= flag;
		}

		public bool AddAndMaybeRunMigration (int flag, bool filter = true) {
			if ((flag & MIGRATE_TO_BITFIELD) != 0) throw new System.ArgumentException("Cannot use the MIGRATE_TO_BITFIELD flag when adding a migration");
			allMigrations |= flag;
			if (filter) {
				var res = (finishedMigrations & flag) != flag;
				MarkMigrationFinished(flag);
				return res;
			} else return false;
		}

		public void IgnoreMigrationAttempt () {
			ignore = true;
		}
	}
}
