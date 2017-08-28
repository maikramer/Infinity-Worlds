using System.Collections;

#if UNITY_EDITOR	
using UnityEditor;

namespace MkGames
{
	public class EditorCoroutine {


		public static EditorCoroutine Start( IEnumerator _routine )
		{
			EditorCoroutine coroutine = new EditorCoroutine(_routine);
			coroutine.Start();
			return coroutine;
		}

		readonly IEnumerator routine;
		EditorCoroutine( IEnumerator _routine )
		{
			routine = _routine;
		}

		void Start()
		{
			EditorApplication.update += Update;
		}
		public void Stop()
		{
			EditorApplication.update -= Update;
		}

		void Update()
		{
			if (!routine.MoveNext())
			{
				Stop();
			}
		}
	}//EditorCoroutine
}//MkGames

#else

namespace MkGames
{
	public class EditorCoroutine {
		public static void Start( IEnumerator _routine )
		{
			return;
		}
	}//EditorCoroutine
}//MkGames

#endif
