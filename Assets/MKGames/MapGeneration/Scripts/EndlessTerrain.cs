using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MkGames
{
	public class EndlessTerrain : MonoBehaviour
	{
		public static float maxViewDist;

		private readonly Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
		private int chunkSize;
		private int chunksVisibleInViewDist;
		public float maxViewDistance = 100;
		public bool canStart;

		[SerializeField] private Transform player;

		private void Awake()
		{
			maxViewDist = maxViewDistance;
		}

		private void Start()
		{
			if (!player)
				player = GameObject.FindGameObjectWithTag("Player").transform;
			chunkSize = FindObjectOfType<MapGenerator>().parameters.size;
			chunksVisibleInViewDist = Mathf.RoundToInt(maxViewDist / chunkSize);
			StartCoroutine(UpdateVisibleChunks());
		}

		private IEnumerator UpdateVisibleChunks()
		{
			while (true)
			{
				var currentchunkCoordX = Mathf.FloorToInt(player.position.x / chunkSize);
				var currentchunkCoordY = Mathf.FloorToInt(player.position.z / chunkSize);


				for (var yOffset = -chunksVisibleInViewDist; yOffset <= chunksVisibleInViewDist; yOffset++)
				for (var xOffset = -chunksVisibleInViewDist; xOffset <= chunksVisibleInViewDist; xOffset++)
				{
					var viewedChunkCoord = new Vector2(currentchunkCoordX + xOffset, currentchunkCoordY + yOffset);

					if (!terrainChunkDictionary.ContainsKey(viewedChunkCoord))
					{
						var newTerrainChunk = new TerrainChunk(viewedChunkCoord, chunkSize);
						while (!newTerrainChunk.IsReady())
							yield return null;
						terrainChunkDictionary.Add(viewedChunkCoord, newTerrainChunk);
					}
				}

				foreach (var terrainChunk in terrainChunkDictionary.Values)
					terrainChunk.UpdateChunkVisibility();

				canStart = true;
				yield return new WaitForSeconds(0.5f);
			}
		}

		public class TerrainChunk
		{
			private readonly MapMesh _mapMesh;
			private readonly GameObject meshGameObject;
			private readonly Transform player;
			private readonly Vector2 position;
			private Vector3 center;

			public TerrainChunk(Vector2 coord, int size)
			{
				player = GameObject.FindGameObjectWithTag("Player").transform;
				meshGameObject = new GameObject();
				_mapMesh = meshGameObject.AddComponent<MapMesh>();
				position = coord * (size - 1);
				var positionVector3 = new Vector3(position.x, 0, position.y);
				meshGameObject.transform.position = positionVector3;
			}

			public void UpdateChunkVisibility()
			{
				if (_mapMesh.bounds.size.magnitude == 0)
					return;

				var distanceFromPlayerToBorder = Mathf.Sqrt(_mapMesh.bounds.SqrDistance(player.position));
				var visible = distanceFromPlayerToBorder < maxViewDist;
				SetActive(visible);
			}

			public void SetActive(bool active)
			{
				meshGameObject.SetActive(active);
			}

			public bool IsReady()
			{
				return _mapMesh.isMeshReady && _mapMesh.isTextureReady && _mapMesh.isColliderReady;
			}
		}
	} //EndlessTerrain
} //MkGames