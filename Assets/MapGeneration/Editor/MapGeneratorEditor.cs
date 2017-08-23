using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MkGames
{
	[CustomEditor(typeof(MapGenerator))]
	public class MapGeneratorEditor : Editor
	{
		private ReorderableList list;

		private void OnEnable() {
			list = new ReorderableList(serializedObject, serializedObject.FindProperty("terrainTextures"), true, true, true, true);
			list.drawElementCallback =  
				(Rect rect, int index, bool isActive, bool isFocused) => {
					var element = list.serializedProperty.GetArrayElementAtIndex(index);
					rect.y += 2;
					EditorGUI.PropertyField(
						new Rect(rect.x, rect.y, 70, EditorGUIUtility.singleLineHeight),
						element.FindPropertyRelative("name"), GUIContent.none);
					EditorGUI.PropertyField(
						new Rect(rect.x + 75, rect.y, rect.width - 75 - 30, EditorGUIUtility.singleLineHeight),
						element.FindPropertyRelative("height"), GUIContent.none);
					EditorGUI.PropertyField(
						new Rect(rect.x + rect.width - 30, rect.y, 30, EditorGUIUtility.singleLineHeight),
						element.FindPropertyRelative("color"), GUIContent.none);
				};
			list.drawHeaderCallback = (Rect rect) => {  
				EditorGUI.LabelField(rect, "Terrain Textures");
			};
		}

		public override void OnInspectorGUI()
		{
			var mapGenerator = (MapGenerator) target;

			if (DrawDefaultInspector())
				if (mapGenerator.AutoGenerate && mapGenerator.Override)
					mapGenerator.GenerateMap();

			if (GUILayout.Button("Generate"))
				if (mapGenerator.Override || mapGenerator.mapDrawMode != MapDrawMode.Mesh)
					mapGenerator.GenerateMap();
				else
					mapGenerator.GenerateNewMap();
			
			serializedObject.Update();
			list.DoLayoutList();
			serializedObject.ApplyModifiedProperties();
		}
	} // Fim de MapGeneratorEditor
} // Fim do namespace