using UnityEngine;

namespace MkGames
{
	public static class TextureGenerator
	{
		public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height)
		{
			var texture = new Texture2D(width, height);

			texture.SetPixels(colorMap);
			texture.Apply();

			return texture;
		}

		public static Texture2D TextureFromHeightMap(float[,] heightMap)
		{
			var width = heightMap.GetLength(0);
			var height = heightMap.GetLength(1);
			var colorMap = new Color[width * height];

			for (var x = 0; x < width; x++)
			for (var y = 0; y < height; y++)
				colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);

			return TextureFromColorMap(colorMap, width, height);
		}
		
		public static Texture2D NormalMap(Texture2D source, float strength)
		{
			strength = Mathf.Clamp(strength, 0.0F, 1.0F);

			var normalTexture = new Texture2D(source.width, source.height, TextureFormat.ARGB32, true);

			for (int y = 0; y < normalTexture.height; y++)
			{
				for (int x = 0; x < normalTexture.width; x++)
				{
					var xLeft = source.GetPixel(x - 1, y).grayscale * strength;
					var xRight = source.GetPixel(x + 1, y).grayscale * strength;
					var yUp = source.GetPixel(x, y - 1).grayscale * strength;
					var yDown = source.GetPixel(x, y + 1).grayscale * strength;
					var xDelta = ((xLeft - xRight) + 1) * 0.5f;
					var yDelta = ((yUp - yDown) + 1) * 0.5f;
					normalTexture.SetPixel(x, y, new Color(xDelta, yDelta, 1.0f, yDelta));
				}
			}
			normalTexture.Apply();
			return normalTexture;
		}
	} // Fim de TextureGenerator
} // Fim do namespace