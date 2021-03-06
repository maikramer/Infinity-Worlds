﻿﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MkGames
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(MapDisplay))]
	public class MapGenerator : MonoBehaviour
	{
		#region Fields
		public const string MapTag = "Map";
		public static string MapParentName = "Map";

		public UnityAction<int> ProgressBarAddAction;
		public ScriptableObject mapProfile;
		public MapDrawMode mapDrawMode;
		[HideInInspector] public List<TerrainTextureType> terrainTextures;
		[SerializeField] [HideInInspector] private GameObject terrain;
		public PhysicMaterial defaultPhysicMaterial;
		public bool OverrideProfile;
		public MapParameters parameters;
		public FullMapParameters fullMapParameters;
		public bool AddCollider;
		public bool AutoGenerate;
		public bool OverrideMesh;
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
					if (!OverrideMesh)
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
					GenerateFullMap(parameters, fullMapParameters);
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#region FullMapGeneration

		public void GenerateFullMap()
		{
			GenerateFullMap(parameters, fullMapParameters);
		}

		public void GenerateFullMap(MapParameters mapParameters, FullMapParameters fullMapParameters)
		{
			if (startedGeneration)
			{
				Debug.LogError("Geracao ja iniciada, aguarde");
				return;
			}
			
			if (mapParameters.useMeshColor)
			{
				mapParameters.textureResolutionFactor = 1;
			}
			
			GameObject mapGameObject;
			if (OverrideMesh)
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

			var fullMapGenerator = mapGameObject.AddComponent<FullMapGenerator>();
			fullMapGenerator.ProgressBarAddAction = ProgressBarAddAction;
			fullMapGenerator.SetGenerator(mapParameters, fullMapParameters);
			fullMapGenerator.StartGeneration();
			
			startedGeneration = true;
		}

		public void EndOfFullMapGeneration()
		{
			startedGeneration = false;
			OnMapReady.Invoke();
			Debug.Log("Geracao Finalizada");
		}

		public void GenerateFullMap(MapParameters _parameters)
		{
			if (startedGeneration)
			{
				Debug.LogError("Geracao ja iniciada, aguarde");
				return;
			}

			startedGeneration = true;

			if (_parameters.useMeshColor)
			{
				_parameters.textureResolutionFactor = 1;
			}

			noiseGen = new Noise();
			var fullHeightMapSize = _parameters.size * fullMapParameters.fullMapSize * _parameters.textureResolutionFactor;
			//noiseGen.OnNoiseIsReady.AddListener(GenerateFullMapGameObject);
			noiseGen.ProgressBarAddAction = ProgressBarAddAction;


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
			if (OverrideMesh)
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
			GameObject fullMapGameObject)
		{
			var fullHeightMapSize = _parameters.size * fullMapParameters.fullMapSize * _parameters.textureResolutionFactor;
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

				if (y % (fullHeightMapSize / 3) == 0)
				{
					ProgressBarAddAction(10);
				}
			}

#if UNITY_EDITOR
			if (Application.isEditor)
				EditorCoroutine.Start(GenerateMapChunks(fullHeightMap, _parameters, fullMapGameObject));
			else
				StartCoroutine(GenerateMapChunks(fullHeightMap, _parameters, fullMapGameObject));
#else
			StartCoroutine(GenerateMapChunks(fullHeightMap, _parameters, fullMap));
#endif
		}

		private IEnumerator GenerateMapChunks(float[,] fullHeightMap, MapParameters _parameters, GameObject fullMapGameObject)
		{
			for (int y = 0; y < fullMapParameters.fullMapSize; y++)
			{
				for (int x = 0; x < fullMapParameters.fullMapSize; x++)
				{
					var position = new Vector2Int(x * (_parameters.size - 1), y * (_parameters.size - 1));
					var heightMap = Noise.SliceNoiseMap(fullHeightMap, position * _parameters.textureResolutionFactor,
						_parameters.size * _parameters.textureResolutionFactor);
					var mapChunk = GenerateMapChunk(new Vector3(position.x, 0, position.y), _parameters, heightMap);
					mapChunk.transform.parent = fullMapGameObject.transform;
					mapChunk.tag = MapTag;
					if (!AddCollider) continue;
					var collider = mapChunk.AddComponent<MeshCollider>();
					collider.material = defaultPhysicMaterial;
				}

				if (fullMapParameters.fullMapSize > 1)
				{
					if (y % (fullMapParameters.fullMapSize / 2) == 0)
					{
						ProgressBarAddAction(15);
						yield return null;
					}
				}
				else
				{
					ProgressBarAddAction(30);
					yield return null;
				}
			}

			var fullMapSize = (_parameters.size - 1) * fullMapParameters.fullMapSize * _parameters.mapScale;
			fullMapGameObject.transform.position = new Vector3(-fullMapSize / 2, 0, -fullMapSize / 2);
			startedGeneration = false;
			OnMapReady.Invoke();
			Debug.Log("Geracao Finalizada");
		}

		#endregion

		#region MapGeneration

		public void GenerateNewMap()
		{
			GenerateMapChunk(Vector3.zero);
		}

		public GameObject GenerateMapChunk(Vector3 position)
		{
			var noiseGen = new Noise();
			var noiseMap = noiseGen.GenerateFastNoiseMap(parameters, parameters.size, parameters.noisePosition);
			return GenerateMapChunk(position, parameters, noiseMap);
		}

		public GameObject GenerateMapChunk(Vector3 position, MapParameters mapParameters, float[,] noiseMap)
		{
			var heightMap = Noise.SimplifyNoiseMap(noiseMap, mapParameters.textureResolutionFactor);

			var meshData = MeshGenerator.GenerateMeshData(heightMap, mapParameters);
			var mesh = meshData.CreateMesh();
			mesh.name = "Map Mesh";

			GameObject meshGameObject;
			if (mapParameters.useMeshColor)
			{
				var simplification = mapParameters.levelOfDetail == 0 ? 1 : mapParameters.levelOfDetail * 2;
				var simpleHeightMap = Noise.SimplifyNoiseMap(heightMap, simplification);
				mesh.colors = GenerateMeshColorMap(simpleHeightMap);
				meshGameObject = MeshGenerator.CreateMeshGameObject(mesh, null, null);
			}
			else
			{
				var texture2D = GenerateTexture(noiseMap);
				texture2D.filterMode = FilterMode.Point;
				if (mapParameters.useNormalMap)
				{
					Texture2D normalMap = TextureGenerator.NormalMap(texture2D, mapParameters.normalMapStrength);
					meshGameObject = MeshGenerator.CreateMeshGameObject(mesh, texture2D, normalMap);
				}
				else
					meshGameObject = MeshGenerator.CreateMeshGameObject(mesh, texture2D, null);
			}

			meshGameObject.transform.position = position * mapParameters.mapScale;
			return meshGameObject;
		}

		#endregion

		#region TextureFunctions

		private Texture2D GenerateTexture(float[,] heightMap)
		{
			var textureSize = heightMap.GetLength(0);
			var colorMap = GenerateColorMap(heightMap, textureSize);
			return TextureGenerator.TextureFromColorMap(colorMap, textureSize, textureSize);
		}

		public Color[] GenerateMeshColorMap(float[,] noiseMap)
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

		public Color[] GenerateColorMap(float[,] noiseMap, int size)
		{
			var colorMap = new Color[size * size];
			for (var x = 0; x < size; x++)
			for (var y = 0; y < size; y++)
			{
				var currentHeight = Mathf.Clamp01(noiseMap[x, y]);
				for (var i = 0; i < terrainTextures.Count; i++)
					if (currentHeight <= terrainTextures[i].height / 100)
					{
						var textureHeight = terrainTextures[i].height / 100;
						float percent;
						if (i > 0)
						{
							var lastTextureHeight = terrainTextures[i - 1].height / 100;
							percent = (currentHeight - lastTextureHeight) / (textureHeight - lastTextureHeight);
						}
						else
							percent = currentHeight / textureHeight;


						var textureColor = terrainTextures[i].color;
						if (percent > 1 - parameters.colorSmoothing)
						{
							var factor = (percent / (1 - parameters.colorSmoothing)) - 1;
							var nextTextureColor = i < terrainTextures.Count - 1 ? terrainTextures[i + 1].color : terrainTextures[i].color;
							var debugcolor = Color.Lerp(textureColor, nextTextureColor, factor);
							colorMap[y * size + x] = debugcolor;
						}
						else
						{
							colorMap[y * size + x] = textureColor;
						}

						break;
					}
			}
			return colorMap;
		}

		#endregion

		#region UnityFunctions

		private void OnEnable()
		{
			startedGeneration = false;
		}

		private void Start()
		{
			if (GameObject.FindGameObjectWithTag(MapTag))
			{
				OnMapReady.Invoke();
			}
		}


		private void OnValidate()
		{
			//Nao Permite que uma altura da lista de textura seja maior do que a textura seguinte
			for (int i = 0; i < terrainTextures.Count - 1; i++)
			{
				if (terrainTextures[i].height > terrainTextures[i + 1].height)
				{
					var tempTT = terrainTextures[i];
					tempTT.height = terrainTextures[i + 1].height;
					terrainTextures[i] = tempTT;
				}
			}

			parameters.terrainTextures = new List<TerrainTextureType>(terrainTextures);

			if (parameters.textureResolutionFactor <= 0)
				parameters.textureResolutionFactor = 1;

			if (parameters.baseHeight <= 0)
				parameters.baseHeight = 1;

			UpdateProfile();
		}

		#endregion

		#region Misc

		public void UpdateProfile()
		{
			if (mapProfile && !OverrideProfile)
			{
				var mapProperties = mapProfile as MapProperties;
				parameters = mapProperties.MapParameters;
				fullMapParameters = mapProperties.FullMapParameters;
			}
		}

		#endregion
	} // Fim de MapGenerator

	#region Types

	[Serializable]
	public struct MapParameters
	{
		#region Constructors

		public MapParameters(float mapScale, float baseHeight, AnimationCurve heightCurve, int size, FastNoise.NoiseType noiseType,
			int octaves, float lacunarity, int noiseSeed, float noiseScale, Vector2 noisePosition, bool useMeshColor,
			float colorSmoothing, int levelOfDetail, int textureResolutionFactor, List<TerrainTextureType> terrainTextures,
			bool useNormalMap, float normalMapStrength)
		{
			if (heightCurve == null) throw new ArgumentNullException(nameof(heightCurve));
			if (terrainTextures == null) throw new ArgumentNullException(nameof(terrainTextures));
			this.mapScale = mapScale;
			this.baseHeight = baseHeight;
			this.heightCurve = heightCurve;
			this.size = size;
			this.noiseType = noiseType;
			this.octaves = octaves;
			this.lacunarity = lacunarity;
			this.noiseSeed = noiseSeed;
			this.noiseScale = noiseScale;
			this.noisePosition = noisePosition;
			this.useMeshColor = useMeshColor;
			this.colorSmoothing = colorSmoothing;
			this.levelOfDetail = levelOfDetail;
			this.textureResolutionFactor = textureResolutionFactor;
			this.terrainTextures = terrainTextures;
			this.useNormalMap = useNormalMap;
			this.normalMapStrength = normalMapStrength;
		}

		#endregion

		[Header("Dimensions")] [Range(0.1f, 10f)] public float mapScale;
		[Range(1, 100)] public float baseHeight;
		public AnimationCurve heightCurve;
		public int size;
		[Header("Noise")] public FastNoise.NoiseType noiseType;
		[Range(1, 10)] public int octaves;
		[Range(0.1f, 5)] public float lacunarity;
		public int noiseSeed;
		[Range(1, 1000)] public float noiseScale;
		public Vector2 noisePosition;
		[Header("Colors")] public bool useMeshColor;
		[Range(0, 1)] public float colorSmoothing;
		[Header("Quality")] [Range(0, 6)] public int levelOfDetail;
		[Range(1, 16)] public int textureResolutionFactor;
		[HideInInspector] public List<TerrainTextureType> terrainTextures;
		[Header("Normal Map")] public bool useNormalMap;
		[Range(0, 1)] public float normalMapStrength;
	}

	[Serializable]
	public struct FullMapParameters
	{
		#region Constructors

		public FullMapParameters(int fullMapSize, AnimationCurve heightCurveX, AnimationCurve heightCurveY)
		{
			if (heightCurveX == null) throw new ArgumentNullException(nameof(heightCurveX));
			if (heightCurveY == null) throw new ArgumentNullException(nameof(heightCurveY));
			this.fullMapSize = fullMapSize;
			this.heightCurveX = heightCurveX;
			this.heightCurveY = heightCurveY;
		}

		#endregion


		[Tooltip("Numero de Tiles com tamanho definido em Parameters.size")] public int fullMapSize;
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

	#endregion
} // Fim do namespace