using System;

namespace LccHotfix
{
	public interface IUILogic
	{
		/// <summary>
		/// 访问所属node
		/// </summary>
		WNode WNode { set; get; }
		/// <summary>
		/// 出生
		/// WindowManager调用
		/// </summary>
		void OnStart();
		/// <summary>
		/// 更新
		/// WindowManager调用
		/// </summary>
		void OnUpdate();
		/// <summary>
		/// 打开前的准备
		/// 可以在这里请求数据
		/// </summary>
		void OnSwitch(Action<bool> callback);
		/// <summary>
		/// 打开
		/// </summary>
		/// <param name="paramsList"></param>
		void OnOpen(object[] paramsList);
		/// <summary>
		/// 再次已经打开的界面，传参，刷新
		/// </summary>
		/// <param name="paramsList"></param>
		void OnReset(object[] paramsList);
		/// <summary>
		/// 返回
		/// </summary>
		void OnResume();
		/// <summary>
		/// 暂停
		/// </summary>
		void OnPause();
		/// <summary>
		/// 关闭
		/// </summary>
		object OnClose();
		/// <summary>
		/// 卸载
		/// </summary>
		void OnRemove();
		/// <summary>
		/// Escape退出结果
		/// </summary>
		/// <returns></returns>
		bool OnEscape(ref EscapeType escapeType);
		/// <summary>
		/// 子节点打开
		/// </summary>
		void OnChildOpened(WNode child);

		/// <summary>
		/// 子节点关闭
		/// </summary>
		/// <param name="child"></param>
		/// <returns>通过子节点状态，返回是否关闭自己</returns>
		bool OnChildClosed(WNode child);
		/// <summary>
		/// 子节点请求退出
		/// </summary>
		/// <param name="child"></param>
		/// <returns></returns>
		bool OnChildRequireEscape(WNode child);


	}
}