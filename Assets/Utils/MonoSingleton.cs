using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
	private static T _instance;

	public static bool HasInstance => _instance != null;

	public static T Instance
	{
		get
		{
			if(_instance == null)
			{
				_instance = Object.FindFirstObjectByType<T>();
				if(_instance == null)
				{
					_instance = new GameObject(typeof(MonoSingleton<T>).ToString(), typeof(T)).GetComponent<T>();
					DontDestroyOnLoad(_instance);
				}
				
			}

			return _instance;
		}
	}
}
