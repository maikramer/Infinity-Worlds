using UnityEditor;
using UnityEngine;

namespace MkGames
{
	public static class ScriptableObjectUtility
	{
		public static void CreateAsset<T>() where T : ScriptableObject
		{
			var asset = ScriptableObject.CreateInstance<T>();
			ProjectWindowUtil.CreateAsset(asset, "New " + typeof(T).Name + ".asset");
		}
	}
}