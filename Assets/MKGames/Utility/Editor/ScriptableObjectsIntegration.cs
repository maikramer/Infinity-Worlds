using UnityEditor;

namespace MkGames
{
	public class ScriptableObjectsIntegration
	{
		[MenuItem("Assets/Create/Map Properties")]
		public static void CreateLaneProps()
		{
			ScriptableObjectUtility.CreateAsset<MapProperties>();
		}
	}
}