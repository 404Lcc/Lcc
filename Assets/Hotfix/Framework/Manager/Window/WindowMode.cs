namespace LccHotfix
{
	public struct WindowMode
	{
		//资源名
		public string prefabName;
		//层级
		public int depth;
		//根节点名
		public string rootName;
		//节点逻辑名
		public string logicName;
		//背景图
		public string bgTex;
		//窗口音乐
		public int sound;
		//打开窗口是否播放动画
		public bool openAnim;
		/// <summary>
		/// 打开时显示一个黑色的遮罩
		/// 0：不显示
		/// 1：第一次创建时显示
		/// 2：每次打开都显示
		/// </summary>
		public int showScreenMask;
        //窗口类型
        //0=全屏界面
		//2=主要节点，当root的所有主节点被关闭时，root会被关闭
		//4=顶层节点，不会被全屏遮挡
		//3=（主要节点&&顶层节点）
        public int windowFlag;
        //窗口互斥类型
        public int rejectFlag;
		//回退响应类型
		public EscapeType escapeType;
		//释放类型
		public ReleaseType releaseType;

		//返回节点类型
		public int returnNodeType;
		//返回节点名称
		public string returnNodeName;
		//返回节点参数
		public int returnNodeParam;
	}
}