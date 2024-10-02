using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LccHotfix
{
	public struct WindowMode
	{
		public string prefabName;
		public int depth;
		public string rootName;
		public string logicName;
		public string bgTex;
		public int music;
		public int sound;
		public bool openAnim;
		/// <summary>
		/// 打开时显示一个黑色的遮罩
		/// 0：不显示
		/// 1：第一次创建时显示
		/// 2：每次打开都显示
		/// </summary>
		public int showScreenMask;
		public int windowFlag;
		public int rejectFlag;
		public EscapeType escapeType;
		public ReleaseType releaseType;

		public int returnNodeType;
		public string returnNodeName;
		public int returnNodeParam;
	}

	
}
