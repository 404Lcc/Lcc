using System;

namespace LccHotfix
{
	public class UIRootBase : UILogicBase
	{
		public override void OnChildClosed(WNode child)
		{
		}

		public override bool OnEscape(ref EscapeType escapeType)
		{
			escapeType = EscapeType.AUTO_CLOSE;
			return true;
		}
	}
}