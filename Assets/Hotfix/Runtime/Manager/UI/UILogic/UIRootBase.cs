using System;

namespace LccHotfix
{
	public class UIRootBase : UILogicBase
	{

		public override bool OnChildClosed(WNode child)
		{
			return ((WRootNode)WNode).DefaultChildCheck();
		}

		public override bool OnEscape(ref EscapeType escapeType)
		{
			escapeType = EscapeType.AUTO_CLOSE;
			return true;
		}
	}
}