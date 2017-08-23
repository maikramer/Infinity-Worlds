using UnityEngine;

namespace MkGames
{
	public class ThreadedNoise : ThreadedJob
	{
		public bool isTexture;
		public MapParameters parameters;
		public float[,] retNoiseMap;

		protected override void ThreadFunction()
		{
			var fastNoise = new FastNoise(parameters.noiseSeed);

			if (parameters.noiseScale == 0)
				parameters.noiseScale = 0.001f;

			var frequency = 1 / parameters.noiseScale;
			fastNoise.SetFrequency(frequency);
			fastNoise.SetNoiseType(parameters.noiseType);

			float textureResolutionFactor;
			textureResolutionFactor = isTexture ? parameters.textureResolutionFactor : 1;

			parameters.size = parameters.size * (int) textureResolutionFactor;
			retNoiseMap = new float[parameters.size, parameters.size];

			for (var x = 0; x < parameters.size; x++)
			for (var y = 0; y < parameters.size; y++)
			{
				var sampleX = x / textureResolutionFactor + parameters.noisePosition.x;
				var sampleY = y / textureResolutionFactor + parameters.noisePosition.y;
				var noiseHeight = fastNoise.GetNoise(sampleX, sampleY);
				//Transforma do range(-1, 1) para o range(0,1)
				retNoiseMap[x, y] = (noiseHeight + 1)/2;
			}
		}
	} //ThreadedNoise
} //MkGames