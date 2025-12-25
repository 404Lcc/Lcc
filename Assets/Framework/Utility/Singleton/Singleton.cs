using UnityEngine;

namespace LccModel
{
	public class Singleton<T> where T : Singleton<T>, new()
	{
		protected static T _instance = null;

		public static T Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new T();
					_instance.OnInit();
				}

				return _instance;
			}
		}

		public static void DestroyInstance()
		{
			if (_instance != null)
			{
				_instance.OnDestory();
				_instance = null;
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
		protected static T _instance = null;

		public static T Instance
		{
			get
			{
				if (!Application.isPlaying)
				{
					return null;
				}

				if (_instance == null)
				{
					_instance = FindObjectOfType(typeof(T)) as T;
				}

				GameObject root = GameObject.Find("SingletonMono");
				if (root == null)
				{
					root = new GameObject("SingletonMono");
					DontDestroyOnLoad(root);
				}

				if (_instance == null)
				{
					GameObject go = new GameObject(typeof(T).ToString());
					_instance = go.AddComponent(typeof(T)) as T;
					go.transform.parent = root.transform;
				}

				return _instance;
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