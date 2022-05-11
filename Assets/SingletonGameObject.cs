using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonGameObject<T> : MonoBehaviour where T : Component
{

	private static T _instance;

	public static T Instance
	{
		get
		{
			if (_instance == null)
				_instance = FindObjectOfType<T>();
			if (_instance == null)
			{
				var go = new GameObject(typeof(T).Name);
				_instance = go.AddComponent<T>();
//				DontDestroyOnLoad(go);
			}
			DontDestroyOnLoad(_instance);
			return _instance;
		}
	}

	protected void OnEnable () {
		if(_instance != null && Instance != _instance) Destroy(_instance.gameObject);
	}
	
	protected void Awake () {
		if(_instance != null && Instance != _instance) Destroy(_instance.gameObject);
	}

}
