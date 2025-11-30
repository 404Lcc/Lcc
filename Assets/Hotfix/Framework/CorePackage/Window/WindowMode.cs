namespace LccHotfix
{
	public struct WindowMode
	{
		//资源名
		public string prefabName;

		//根节点名
		public string rootName;

		//节点逻辑名
		public string logicName;

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