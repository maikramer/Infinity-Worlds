using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MkGames
{
	[ExecuteInEditMode]
	public class MapGenerator : MonoBehaviour
	{
		#region Fields

		public static string MapParentName = "Map";

		public MapDrawMode mapDrawMode;
		[HideInInspector] public List<TerrainTextureType> terrainTextures;
		[SerializeField] [HideInInspector] private GameObject terrain;
		public PhysicMaterial defaultPhysicMaterial;
		public MapParameters parameters;
		public FullMapParameters fullMapParameters;
		public bool AddCollider;
		public bool AutoGenerate;
		public bool Override;
		public UnityEvent OnMapReady;

		private Noise noiseGen;
		private bool startedGeneration;

		private GameObject meshGameObject;

		#endregion

		public void GenerateMap()
		{
			Texture2D texture2D;
			float[,] noiseMap;
			Noise noiseGen;
			var display = FindObjectOfType<MapDisplay>();
			switch (mapDrawMode)
			{
				case MapDrawMode.HeightMap:
					noiseGen = new Noise();
					noiseMap = noiseGen.GenerateFastNoiseMap(parameters, parameters.size, parameters.noisePosition);
					texture2D = TextureGenerator.TextureFromHeightMap(noiseMap);
					display.DrawMap(texture2D, parameters.size);
					break;

				case MapDrawMode.ColorMap:
					noiseGen = new Noise();
					noiseMap = noiseGen.GenerateFastNoiseMap(parameters, parameters.size, parameters.noisePosition);
					texture2D = GenerateTexture(noiseMap);
					display.DrawMap(texture2D, parameters.size);
					break;

				case MapDrawMode.Mesh:
					if (!Override)
						GenerateNewMap();
					else
					{
						if (meshGameObject)
							DestroyImmediate(meshGameObject);
						meshGameObject = GenerateMapChunk(Vector3.zero);
					}
					break;

				case MapDrawMode.Terrain:
					var terrainData = TerrainGenerator.CreateTerrain(parameters, parameters.size);
					if (terrainData)
						if (!terrain)
							terrain = Terrain.CreateTerrainGameObject(terrainData);
						else
							terrain.GetComponent<Terrain>().terrainData = terrainData;

					break;

				case MapDrawMode.FullMap:
					GenerateFullMap();
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void GenerateFullMap()
		{
			GenerateFullMap(parameters);
		}

		public void GenerateFullMap(MapParameters _parameters)
		{
			if (startedGeneration)
			{
				Debug.LogError("Geracao ja iniciada, aguarde");
				return;
			}

			startedGeneration = true;

			if (_parameters.UseMeshColor)
			{
				_parameters.textureResolutionFactor = 1;
			}

			noiseGen = new Noise();
			var fullHeightMapSize = _parameters.size * fullMapParameters.size * _parameters.textureResolutionFactor;
			noiseGen.OnNoiseIsReady.AddListener(GenerateFullMapGameObject);

#if UNITY_EDITOR
			if (Application.isEditor)
				EditorCoroutine.Start(noiseGen.IEGenerateFastNoiseMap(_parameters, fullHeightMapSize, _parameters.noisePosition));
			else
				StartCoroutine(noiseGen.IEGenerateFastNoiseMap(_parameters, fullHeightMapSize, _parameters.noisePosition));
#else
			StartCoroutine(noiseGen.IEGenerateFastNoiseMap(_parameters, fullHeightMapSize, _parameters.noisePosition));
#endif
		}

		private void GenerateFullMapGameObject(float[,] fullHeightMap, MapParameters _parameters)
		{
			GameObject mapGameObject;
			if (Override)
			{
				mapGameObject = GameObject.Find(MapParentName);
				if (mapGameObject)
					DestroyImmediate(mapGameObject);
				mapGameObject = new GameObject(MapParentName);
			}
			else
			{
				mapGameObject = new GameObject();
				mapGameObject.name = MapParentName + " " + mapGameObject.GetInstanceID();
			}
			
#if UNITY_EDITOR
			if (Application.isEditor)
				EditorCoroutine.Start(ApplyFullMapHeightModifier(fullHeightMap, _parameters, mapGameObject));
			else
				StartCoroutine(ApplyFullMapHeightModifier(fullHeightMap, _parameters, mapGameObject));
#else
			StartCoroutine(ApplyFullMapHeightModifier(fullHeightMap, _parameters, mapGameObject));
#endif
		}

		private IEnumerator ApplyFullMapHeightModifier(float[,] fullHeightMap, MapParameters _parameters,
			GameObject fullMap)
		{
			var progress = 40;
			var fullHeightMapSize = _parameters.size * fullMapParameters.size * _parameters.textureResolutionFactor;
			for (int y = 0; y < fullHeightMapSize; y++)
			{
				for (int x = 0; x < fullHeightMapSize; x++)
				{
					var curveX = fullMapParameters.heightCurveX.Evaluate((float) x / fullHeightMapSize);
					var curveY = fullMapParameters.heightCurveY.Evaluate((float) y / fullHeightMapSize);

					fullHeightMap[x, y] = Mathf.Clamp01(fullHeightMap[x, y] * curveX * curveY);
				}
				if (y % 10 == 0)
					yield return null;

				if (y % fullHeightMapSize / 2 == 0)
				{
					progress += 15;
					Debug.Log("Progresso : " + progress + "%");
				}
			}

#if UNITY_EDITOR
			if (Application.isEditor)
				EditorCoroutine.Start(GenerateMapChunks(fullHeightMap, _parameters, fullMap));
			else
				StartCoroutine(GenerateMapChunks(fullHeightMap, _parameters, fullMap));
#else
			StartCoroutine(GenerateMapChunks(fullHeightMap, _parameters, fullMap));
#endif
		}

		private IEnumerator GenerateMapChunks(float[,] fullHeightMap, MapParameters _parameters, GameObject fullMap)
		{
			int progress = 70;
			for (int y = 0; y < fullMapParameters.size; y++)
			{
				for (int x = 0; x < fullMapParameters.size; x++)
				{
					var position = new Vector2Int(x * (_parameters.size - 1), y * (_parameters.size - 1));
					var heightMap = Noise.SliceNoiseMap(fullHeightMap, position * _parameters.textureResolutionFactor,
						_parameters.size * _parameters.textureResolutionFactor);
					var mapChunk = GenerateMapChunk(new Vector3(position.x, 0, position.y), _parameters, heightMap);
					mapChunk.transform.parent = fullMap.transform;
					mapChunk.tag = "Map";
					if (!AddCollider) continue;
					var collider = mapChunk.AddComponent<MeshCollider>();
					collider.material = defaultPhysicMaterial;
				}

				if (y % fullMapParameters.size / 2 == 0)
				{
					progress += 15;
					Debug.Log("Progresso : " + progress + "%");
					yield return null;
				}
			}

			var fullMapSize = (_parameters.size - 1) * fullMapParameters.size * _parameters.mapScale;
			fullMap.transform.position = new Vector3(-fullMapSize / 2, 0, -fullMapSize / 2);
			startedGeneration = false;
			OnMapReady.Invoke();
			Debug.Log("Geracao Finalizada");
		}

		public void GenerateNewMap()
		{
			GenerateMapChunk(Vector3.zero);
		}

		public GameObject GenerateMapChunk(Vector3 position, MapParameters mapParameters, float[,] noiseMap)
		{
			var heightMap = Noise.SimplifyNoiseMap(noiseMap, mapParameters.textureResolutionFactor);

			var meshData = MeshGenerator.GenerateMeshData(heightMap, mapParameters);
			var mesh = meshData.CreateMesh();
			mesh.name = "Map Mesh";

			GameObject meshGameObject;
			if (mapParameters.UseMeshColor)
			{
				var simplification = mapParameters.levelOfDetail == 0 ? 1 : mapParameters.levelOfDetail * 2;
				var simpleHeightMap = Noise.SimplifyNoiseMap(heightMap, simplification);
				mesh.colors = GenerateMeshColorMap(simpleHeightMap);
				meshGameObject = MeshGenerator.CreateMeshGameObject(mesh, null);
			}
			else
			{
				var texture2D = GenerateTexture(noiseMap);
				texture2D.filterMode = FilterMode.Point;
				meshGameObject = MeshGenerator.CreateMeshGameObject(mesh, texture2D);
			}

			meshGameObject.transform.position = position * mapParameters.mapScale;
			return meshGameObject;
		}

		public GameObject GenerateMapChunk(Vector3 position)
		{
			var noiseGen = new Noise();
			var noiseMap = noiseGen.GenerateFastNoiseMap(parameters, parameters.size, parameters.noisePosition);
			return GenerateMapChunk(position, parameters, noiseMap);
		}

		private Texture2D GenerateTexture(float[,] heightMap)
		{
			var textureSize = heightMap.GetLength(0);
			var colorMap = GenerateColorMap(heightMap, textureSize);
			return TextureGenerator.TextureFromColorMap(colorMap, textureSize, textureSize);
		}

		private Color[] GenerateMeshColorMap(float[,] noiseMap)
		{
			int size = noiseMap.GetLength(0);
			var colorMap = new Color[size * size];
			for (var x = 0; x < size; x++)
			for (var y = 0; y < size; y++)
			{
				var currentHeight = noiseMap[x, y];
				for (var i = 0; i < terrainTextures.Count; i++)
					if (currentHeight <= terrainTextures[i].height / 100)
					{
						colorMap[x * size + y] = terrainTextures[i].color;
						break;
					}
			}
			return colorMap;
		}

		private Color[] GenerateColorMap(float[,] noiseMap, int size)
		{
			var colorMap = new Color[size * size];
			for (var x = 0; x < size; x++)
			for (var y = 0; y < size; y++)
			{
				var currentHeight = noiseMap[x, y];
				for (var i = 0; i < terrainTextures.Count; i++)
					if (currentHeight <= terrainTextures[i].height / 100)
					{
						colorMap[y * size + x] = terrainTextures[i].color;
						break;
					}
			}
			return colorMap;
		}

		private void OnEnable()
		{
			startedGeneration = false;
		}


		private void OnValidate()
		{
			parameters.terrainTextures = new List<TerrainTextureType>(terrainTextures);

			if (parameters.textureResolutionFactor <= 0)
				parameters.textureResolutionFactor = 1;

			if (parameters.baseHeight <= 0)
				parameters.baseHeight = 1;
		}

		public void SortRegions()
		{
			//Array.Sort(regions);
		}
	} // Fim de MapGenerator

	[Serializable]
	public struct MapParameters
	{
		[Header("Dimensions")] [Range(0.1f, 10f)] public float mapScale;
		[Range(1, 100)] public float baseHeight;
		public AnimationCurve heightCurve;
		public int size;
		[Header("Noise")] public FastNoise.NoiseType noiseType;
		public int noiseSeed;
		[Range(1, 100)] public float noiseScale;
		public Vector2 noisePosition;
		[Header("Colors")] public bool UseMeshColor;
		[Header("Quality")] [Range(0, 6)] public int levelOfDetail;
		[Range(1, 16)] public int textureResolutionFactor;
		[HideInInspector] public List<TerrainTextureType> terrainTextures;
		[Header("Normal Map")] public bool useNormalMap;
		[Range(0, 1)] public float normalMapStrength;
	}

	[Serializable]
	public struct FullMapParameters
	{
		[Tooltip("Numero de Tiles com tamanho definido em Parameters.size")] public int size;
		public AnimationCurve heightCurveX;
		public AnimationCurve heightCurveY;
	}

	[Serializable]
	public struct TerrainTextureType : IComparable, ICloneable
	{
		public string name;
		[Range(0, 100)] public float height;
		public Color color;

		public int CompareTo(object obj)
		{
			var otherTT = (TerrainTextureType) obj;
			return height.CompareTo(otherTT.height);
		}

		public object Clone()
		{
			var copy = new TerrainTextureType
			{
				name = name,
				height = height,
				color = color
			};
			return copy;
		}
	}

	[Serializable]
	public enum MapDrawMode
	{
		HeightMap,
		ColorMap,
		Mesh,
		Terrain,
		FullMap
	}
} // Fim do namespace