using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MkGames
{
	[CustomEditor(typeof(MapGenerator))]
	public class MapGeneratorEditor : Editor
	{
		private const int progressBarHeight = 30;
		private bool drawProgressBar;
		private float progress;
		private ReorderableList list;

		private void OnEnable() {
			var mapGenerator = (MapGenerator) target;
			mapGenerator.ProgressBarAddAction = AddToProgressBar;
			mapGenerator.OnMapReady.AddListener(HideProgressBar);
			mapGenerator.UpdateProfile();

			HideProgressBar();
			
			list = new ReorderableList(serializedObject, serializedObject.FindProperty("terrainTextures"), true, true, true, true);
			list.drawElementCallback =  
				(rect, index, isActive, isFocused) => {
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
			list.drawHeaderCallback = rect => {  
				EditorGUI.LabelField(rect, "Terrain Textures");
			};
		}

		public override void OnInspectorGUI()
		{
			var mapGenerator = (MapGenerator) target;

			if (DrawDefaultInspector())
			{
				if (mapGenerator.AutoGenerate && mapGenerator.OverrideMesh)
					mapGenerator.GenerateMap();
			}

			if (GUILayout.Button("Generate"))
			{
				if (mapGenerator.OverrideMesh || mapGenerator.mapDrawMode != MapDrawMode.Mesh)
					mapGenerator.GenerateMap();
				else
					mapGenerator.GenerateNewMap();
				
				progress = 0;
				drawProgressBar = true;
			}

			
			if (drawProgressBar)
				DrawProgressBar();
			
			serializedObject.Update();
			list.DoLayoutList();
			serializedObject.ApplyModifiedProperties();
			
			
		}

		public void DrawProgressBar()
		{
			var rect = EditorGUILayout.BeginVertical();
			GUILayout.Space(progressBarHeight + 10);
			EditorGUI.ProgressBar(new Rect(rect.x, rect.y + 5, rect.width - 4, progressBarHeight), progress, "Gerando Mapa");
			EditorGUILayout.EndVertical();
		}

		public void HideProgressBar()
		{
			drawProgressBar = false;
		}

		public void AddToProgressBar(int value)
		{
			progress += (float) value / 100;
			Repaint();
		}
	} // Fim de MapGeneratorEditor
	
	
} // Fim do namespace