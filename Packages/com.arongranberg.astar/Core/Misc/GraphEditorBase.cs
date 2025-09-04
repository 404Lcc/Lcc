using Pathfinding.Serialization;

namespace Pathfinding {
	[JsonOptIn]
	/// <summary>
	/// Base class for all graph editors.
	/// Defined here only so non-editor classes can use the <see cref="target"/> field
	/// </summary>
	public class GraphEditorBase {
		/// <summary>NavGraph this editor is exposing</summary>
		public NavGraph target;
	}
}
