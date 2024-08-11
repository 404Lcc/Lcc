using UnityEngine;

namespace LccModel
{
	public class Singleton<T> where T : Singleton<T>, new()
	{
		protected static T _Instance = null;

		public static T Instance
		{
			get
			{
				if (null == _Instance)
				{
					_Instance = new T();
					_Instance.OnInit();
				}
				return _Instance;
			}
		}

		public static void DestroyInstance()
		{
			if (_Instance != null)
			{
				_Instance.OnDestory();
				_Instance = null;
			}
		}

		protected virtual void OnInit()
		{

		}

		protected virtual void OnDestory()
		{

		}
	}

	public class SingletonMono<T> : MonoBehaviour where T : class, new()
	{
		protected static T _Instance = null;

		public static T Instance
		{
			get
			{
				if (null == _Instance)
				{
					GameObject go = new GameObject(typeof(T).ToString());
					_Instance = go.AddComponent(typeof(T)) as T;
					MonoBehaviour.DontDestroyOnLoad(go);
					GameObject root = GameObject.Find("SingletonMono");
					go.transform.parent = root.transform;
					return _Instance;
				}

				return _Instance;
			}
		}

		public static bool HaveInstance
		{
			get
			{
				GameObject root = GameObject.Find("SingletonMono");
				if (root == null)
				{
					return false;
				}
				Transform trans = root.transform.Find(typeof(T).ToString());
				return trans != null;
			}
		}
	}
}