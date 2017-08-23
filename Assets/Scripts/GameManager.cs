using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MkGames
{
	public class GameManager : Singleton<GameManager>
	{
		[SerializeField] private MapGenerator _mapGenerator;
		public UnityEvent OnMapReadyEvent;

		void Start()
		{
			if (!_mapGenerator)
			{
				_mapGenerator = FindObjectOfType<MapGenerator>();
				Utility.NotNullException(_mapGenerator);
			}

			if (GameObject.Find(MapGenerator.MapParentName) == null)
			{
				StartCoroutine(_mapGenerator.GenerateFullMap());
				_mapGenerator.OnMapReady.AddListener(MapIsReady);
			}
		}

		void MapIsReady()
		{
			OnMapReadyEvent.Invoke();
		}
	} //GameManager
} //MkGames