using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace MkGames
{
	/// <summary>
	///     Classe geradora de Ruído
	/// </summary>
	public class Noise
	{
		public UnityAction<int> ProgressBarAddAction;
		public NoiseEvent OnNoiseIsReady;
		
		public Noise()
		{
			OnNoiseIsReady = new NoiseEvent();
		}
		
		public float[,] GenerateFastNoiseMap(MapParameters par, int mapSize, Vector2 noisePosition)
		{
			var fastNoise = new FastNoise(par.noiseSeed);

			if (Mathf.Approximately(par.noiseScale, 0))
				par.noiseScale = 0.001f;

			var frequency = 1 / par.noiseScale;
			fastNoise.SetFrequency(frequency);
			fastNoise.SetFractalOctaves(par.octaves);
			fastNoise.SetNoiseType(par.noiseType);

			float textureResolutionFactor = par.textureResolutionFactor;

			mapSize = mapSize * (int) textureResolutionFactor;
			var retNoiseMap = new float[mapSize, mapSize];

			for (var x = 0; x < mapSize; x++)
			{
				for (var y = 0; y < mapSize; y++)
				{
					var sampleX = x / textureResolutionFactor + noisePosition.x;
					var sampleY = y / textureResolutionFactor + noisePosition.y;
					var noiseHeight = fastNoise.GetNoise(sampleX, sampleY);
					retNoiseMap[x, y] = (noiseHeight + 1) / 2;
				}
			}
			

			return retNoiseMap;
		}

		public IEnumerator IEGenerateFastNoiseMap(MapParameters par, int mapSize, Vector2 noisePosition)
		{		
			var fastNoise = new FastNoise(par.noiseSeed);

			if (Mathf.Approximately(par.noiseScale, 0))
				par.noiseScale = 0.001f;

			var frequency = 1 / par.noiseScale;
			fastNoise.SetFrequency(frequency);
			fastNoise.SetFractalOctaves(par.octaves);
			fastNoise.SetFractalLacunarity(par.lacunarity);
			fastNoise.SetNoiseType(par.noiseType);

			float textureResolutionFactor = par.textureResolutionFactor;

			mapSize = mapSize * (int) textureResolutionFactor;
			var result = new float[mapSize, mapSize];

			for (var x = 0; x < mapSize; x++)
			{
				for (var y = 0; y < mapSize; y++)
				{
					var sampleX = x / textureResolutionFactor + noisePosition.x;
					var sampleY = y / textureResolutionFactor + noisePosition.y;
					var noiseHeight = fastNoise.GetNoise(sampleX, sampleY);
					result[x, y] = (noiseHeight + 1)/2;
				}

				if (x % 100 == 0)
					yield return null;
				
				if (x % (mapSize / 2) == 0)
				{
					ProgressBarAddAction.Invoke(15);
				}
			}
			
			ProgressBarAddAction.Invoke(10);
			
			OnNoiseIsReady.Invoke(result);
		}
		
		public static float[,] SliceNoiseMap(float[,] noiseMap, Vector2Int position, int size)
		{
			var slicedMap = new float[size, size];
			for (int y = 0; y < size; y++)
			for (int x = 0; x < size; x++)
			{
				slicedMap[x, y] = noiseMap[x + position.x, y + position.y];
			}

			return slicedMap;
		}
		
		public static float[,] SimplifyNoiseMap(float[,] noiseMap, int simplificationFactor)
		{
			if (simplificationFactor <= 1)
				return noiseMap;
			var actualSize = noiseMap.GetLength(0);
			var newSize = actualSize / simplificationFactor + actualSize % simplificationFactor;
			var simplifiedMap = new float[newSize, newSize];
			for (int y = 0; y < newSize; y++)
			for (int x = 0; x < newSize; x++)
			{
				simplifiedMap[x, y] = noiseMap[x * simplificationFactor, y * simplificationFactor];
			}

			return simplifiedMap;
		}
	} // Fim de Noise

	public class NoiseEvent : UnityEvent<float[,]>{}
} // Fim do namespace