using System;
using UnityEngine;

namespace MkGames
{
	public class ThreadedTextureGenerator : ThreadedJob
	{
		public Color[] colorMap;
		public TextureParameters parameters;

		protected override void ThreadFunction()
		{
			var size = parameters.noiseMap.GetLength(0);
			if (parameters.noiseMap == null)
				new Exception("Inicialize os parametros");
			colorMap = new Color[size * size];
			for (var x = 0; x < size; x++)
			for (var y = 0; y < size; y++)
			{
				var currentHeight = parameters.noiseMap[x, y];
				for (var i = 0; i < parameters.mapParameters.terrainTextures.Count; i++)
					if (currentHeight <= parameters.mapParameters.terrainTextures[i].height / 100)
					{
						colorMap[y * size + x] = parameters.mapParameters.terrainTextures[i].color;
						break;
					}
			}
		}
	} //ThreadedTextureGenerator

	public struct TextureParameters
	{
		public float[,] noiseMap;
		public MapParameters mapParameters;

		public TextureParameters(float[,] noiseMap, MapParameters mapParameters)
		{
			if (noiseMap == null) throw new ArgumentNullException(nameof(noiseMap));
			this.noiseMap = noiseMap;
			this.mapParameters = mapParameters;
		}
	}
} //MkGames