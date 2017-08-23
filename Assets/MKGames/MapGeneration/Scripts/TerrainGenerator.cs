using UnityEngine;

namespace MkGames
{
	public static class TerrainGenerator
	{
		public static TerrainData CreateTerrain(MapParameters par, int mapSize)
		{
			mapSize = Mathf.ClosestPowerOfTwo(mapSize);
			var noiseGen = new Noise();
			var heightMap = noiseGen.GenerateFastNoiseMap(par, mapSize, par.noisePosition);

			var terrainData = new TerrainData
			{
				size = new Vector3(mapSize / 4, par.baseHeight, mapSize / 4),
				heightmapResolution = mapSize
			};

			for (var x = 0; x < mapSize; x++)
			for (var y = 0; y < mapSize; y++)
				heightMap[x, y] = par.heightCurve.Evaluate(heightMap[x, y]);
			terrainData.SetHeights(0, 0, heightMap);

			return terrainData;
		}
	} //TerrainGenerator
} //MkGames