using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MkGames
{
	public class MapProperties : ScriptableObject
	{

		[Header("Dimensions")] [Header("PARAMETROS INDIVIDUAIS")]
		[Range(0.1f, 10f)] public float mapScale = 1;
		[Range(1, 100)] public float baseHeight = 50;
		public AnimationCurve heightCurve = AnimationCurve.Linear(0,0,1,1);
		public int size = 241;
		
		[Header("Noise")] 
		public FastNoise.NoiseType noiseType = FastNoise.NoiseType.SimplexFractal;
		[Range(1, 10)] public int octaves = 1;
		public int noiseSeed = 1;
		[Range(1, 1000)] public float noiseScale = 50;
		public Vector2 noisePosition = new Vector2(0,0);
		
		[Header("Colors")] 
		public bool useMeshColor;
		[Range(0, 1)] public float colorSmoothing = 0.2f;
		
		[Header("Quality")] 
		[Range(0, 6)] public int levelOfDetail = 0;
		[Range(1, 16)] public int textureResolutionFactor = 2;
		[HideInInspector] public List<TerrainTextureType> terrainTextures = new List<TerrainTextureType>();
		
		[Header("Normal Map")]
		public bool useNormalMap;
		[Range(0, 1)] public float normalMapStrength;
		
		[Header("PARAMETROS DO MAPA COMPLETO")][Space]
		[Tooltip("Numero de Tiles com tamanho definido em Parameters.size")] public int fullMapSize = 2;
		public AnimationCurve heightCurveX = AnimationCurve.Linear(0,0,1,1);
		public AnimationCurve heightCurveY = AnimationCurve.Linear(0,0,1,1);

		#region Properties

		private MapParameters mapParameters;
		public MapParameters MapParameters
		{
			get
			{
				mapParameters = new MapParameters(mapScale, baseHeight, heightCurve, size, noiseType, octaves, noiseSeed, 
					noiseScale, noisePosition, useMeshColor, colorSmoothing, levelOfDetail, textureResolutionFactor,
					terrainTextures, useNormalMap, normalMapStrength);
				
				return mapParameters;
			}
			set { mapParameters = value; }
		}

		private FullMapParameters fullMapParameters;
		public FullMapParameters FullMapParameters
		{
			get
			{
				fullMapParameters = new FullMapParameters(fullMapSize, heightCurveX, heightCurveY);
				return fullMapParameters;
			}
			set { fullMapParameters = value; }
		}

		#endregion
		
	}//MapProperties
}//MkGames
