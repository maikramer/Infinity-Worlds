using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MkGames
{
	public class MeshDraft
	{
		public List<Vector3> vertices = new List<Vector3>();
		public List<int> triangles = new List<int>();
		public List<Vector3> normals;
		public List<Vector2> uvs = new List<Vector2>();
		public List<Color> colors = new List<Color>();

		private readonly int width;

		public MeshDraft(int width)
		{
			this.width = width;
		}

		public void AddVertice(Vector3 vertex)
		{
			vertices.Add(vertex);
		}

		public void AddTriangles()
		{
			if (vertices.Count < 3)
				new UnityException("Numero de vertices menor que 3, impossivel gerar triangulos");

			var height = vertices.Count / width;
			for (int i = 0; i < height - 1; i++)
			{
				for (int j = 0; j < width - 1; j++)
				{
					triangles.Add(width * i + j);
					triangles.Add(width * i + j + 1);
					triangles.Add(width * (i + 1) + j + 1);

					triangles.Add(width * i + j);
					triangles.Add(width * (i + 1) + j + 1);
					triangles.Add(width * (i + 1) + j);
				}
			}
		}

		public void AddUvs()
		{
			var height = vertices.Count / width;
			for (int y = 0; y < height; y++)
			for (int x = 0; x < width; x++)
				uvs.Add(new Vector2((float) y / width, (float) x / height));
		}

		public void AddUvs(ref Mesh mesh)
		{
			var bounds = mesh.bounds;
			var vertices = mesh.vertices;
			var uvs = new Vector2[vertices.Length];
			for (int i = 0; i < vertices.Length; i++)
			{
				uvs[i] = new Vector2(vertices[i].x / bounds.size.x, vertices[i].z / bounds.size.z);
			}
			mesh.uv = uvs;
		}

		public void CalculateNormals()
		{
			normals = new Vector3[vertices.Count].ToList();
			var triangleCount = triangles.Count / 3;
			for (int i = 0; i < triangleCount; i++)
			{
				var vertexIndex = i * 3;
				var vertexA = vertices[triangles[vertexIndex]];
				var vertexB = vertices[triangles[vertexIndex + 1]];
				var vertexC = vertices[triangles[vertexIndex + 2]];
				var vectorAB = vertexB - vertexA;
				var vectorAC = vertexC - vertexA;
				var triangleNormal = Vector3.Cross(vectorAB, vectorAC).normalized;
				normals[triangles[vertexIndex]] = triangleNormal;
				normals[triangles[vertexIndex + 1]] = triangleNormal;
				normals[triangles[vertexIndex + 2]] = triangleNormal;
			}
		}

		public List<MeshDraft> SliceMeshDraft(int segments)
		{
			var slices = new List<MeshDraft>();

			int totalSegments = segments * segments;
			var segmentSize = (width - 1) / segments + 1;
			for (int i = 0; i < totalSegments; i++)
			{
				var slice = new MeshDraft(segmentSize);
				var lin = i / segments;
				var col = i % segments;
				var start = width * (segmentSize - 1) * lin + (segmentSize - 1) * col;
				for (int j = 0; j < segmentSize; j++)
				{
					var indice = start + j * width;
					slice.vertices.AddRange(vertices.GetRange(indice, segmentSize));
				}

				slice.normals = new List<Vector3>();
				for (int j = 0; j < segmentSize; j++)
				{
					var indice = start + j * width;
					slice.normals.AddRange(normals.GetRange(indice, segmentSize));
				}

				for (int j = 0; j < segmentSize; j++)
				{
					var indice = start + j * width;
					slice.uvs.AddRange(uvs.GetRange(indice, segmentSize));
				}


				slice.AddTriangles();

				slices.Add(slice);
			}


			return slices;
		}

		public Mesh ToMesh()
		{
			var mesh = new Mesh();

			mesh.SetVertices(vertices);
			mesh.SetTriangles(triangles, 0);
			mesh.SetUVs(0, uvs);
			mesh.SetNormals(normals);
//			mesh.RecalculateNormals();
			return mesh;
		}
	} //MeshDraft
} //MkGames