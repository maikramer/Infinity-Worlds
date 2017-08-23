using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MkGames
{
	[CustomEditor(typeof(MusicPlayer))]
	public class MusicPlayerEditor : Editor
	{
		private const int numberOfElements = 5;
		private ReorderableList musicList;

		public string clipName;
		public AudioClip audioClip;
		public string sceneName;
		public bool loop;
		public bool playAtStart;

		private void OnEnable()
		{
			musicList =
				new ReorderableList(serializedObject, serializedObject.FindProperty("trackList"), true, true, true, true)
				{
					elementHeight = numberOfElements * (EditorGUIUtility.singleLineHeight + 2)
				};
			musicList.drawElementCallback =
				(Rect rect, int index, bool isActive, bool isFocused) =>
				{
					var element = musicList.serializedProperty.GetArrayElementAtIndex(index);
					rect.y += 2;
					Rect clipNameRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
					EditorGUI.PropertyField(clipNameRect,
						element.FindPropertyRelative("clipName"), new GUIContent("Clip Name"));

					rect.y += EditorGUIUtility.singleLineHeight + 2;
					Rect audioClipRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
					EditorGUI.PropertyField(audioClipRect,
						element.FindPropertyRelative("audioClip"), new GUIContent("Audio Clip"));

					rect.y += EditorGUIUtility.singleLineHeight + 2;
					Rect sceneNameRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
					EditorGUI.PropertyField(sceneNameRect,
						element.FindPropertyRelative("sceneName"), new GUIContent("Scene Name"));

					rect.y += EditorGUIUtility.singleLineHeight + 2;
					Rect loopRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
					EditorGUI.PropertyField(loopRect,
						element.FindPropertyRelative("loop"), new GUIContent("Loop Track"));

					rect.y += EditorGUIUtility.singleLineHeight + 2;
					Rect playAtStartRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
					EditorGUI.PropertyField(playAtStartRect,
						element.FindPropertyRelative("playAtStart"), new GUIContent("Play at Start"));
				};
			musicList.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, "Audio Clips"); };
		}

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			serializedObject.Update();
			musicList.DoLayoutList();
			serializedObject.ApplyModifiedProperties();
		}
	} //MusicPlayerEditor
} //MkGames