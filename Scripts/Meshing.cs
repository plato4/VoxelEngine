using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;

namespace VoxelEngine
{
    public enum MeshingAlgorithm
    {
        All = 0,
        Culled = 1,
        Greedy = 2,
        Marching = 3,
    }

    internal class Meshing
    {

        internal static void GenerateMesh(Color32[] voxels, Vector3Int chunkSize, Vector3 voxelScale, Material material, ref List<Vector3> verts, ref List<Color32> colors, ref List<int> inds, MeshingAlgorithm meshingAlgorithm)
        {
            if (meshingAlgorithm == MeshingAlgorithm.Culled)
                GenerateMeshCulled(voxels, chunkSize, voxelScale, material, ref verts, ref colors, ref inds);
            else if (meshingAlgorithm == MeshingAlgorithm.Greedy)
                GenerateMeshGreedy(voxels, chunkSize, voxelScale, material, ref verts, ref colors, ref inds);
        }

        private static void GenerateMeshGreedy(Color32[] voxels, Vector3Int chunkSize, Vector3 voxelScale, Material material, ref List<Vector3> verts, ref List<Color32> colors, ref List<int> inds)
        {
            
        }

        private static void GenerateMeshCulled(Color32[] voxels, Vector3Int chunkSize, Vector3 voxelScale, Material material, ref List<Vector3> verts, ref List<Color32> colors, ref List<int> ind)
        {
            verts.Clear();
            colors.Clear();
            ind.Clear();

            Vector3[] vertList = {
                                    new Vector3(0, 0, 0), new Vector3(1, 0, 0),
                                    new Vector3(1, 0, 1), new Vector3(0, 0, 1),
                                    new Vector3(0, 1, 0), new Vector3(1, 1, 0),
                                    new Vector3(1, 1, 1), new Vector3(0, 1, 1)
                                 };
            int verticesIndex = 0;
            for (int x = 0; x < chunkSize.x; x++)
            {
                for (int y = 0; y < chunkSize.y; y++)
                {
                    for (int z = 0; z < chunkSize.z; z++)
                    {
                        var flattened_index = Tools.GetFlatIndexFromXYZ(chunkSize, new Vector3Int(x, y, z));
                        if (voxels[flattened_index].a == 0) continue;

                        var voxelColor = voxels[flattened_index];

                        bool left = false;
                        flattened_index = Tools.GetFlatIndexFromXYZ(chunkSize, new Vector3Int(x - 1, y, z));
                        if (x > 0) left = voxels[flattened_index].a != 0;

                        bool right = false;
                        flattened_index = Tools.GetFlatIndexFromXYZ(chunkSize, new Vector3Int(x + 1, y, z));
                        if (x < chunkSize.x - 1) right = voxels[flattened_index].a != 0;

                        bool back = false;
                        flattened_index = Tools.GetFlatIndexFromXYZ(chunkSize, new Vector3Int(x, y, z - 1));
                        if (z > 0) back = voxels[flattened_index].a != 0;

                        bool front = false;
                        flattened_index = Tools.GetFlatIndexFromXYZ(chunkSize, new Vector3Int(x, y, z + 1));
                        if (z < chunkSize.z - 1) front = voxels[flattened_index].a != 0;

                        bool top = false;
                        flattened_index = Tools.GetFlatIndexFromXYZ(chunkSize, new Vector3Int(x, y + 1, z));
                        if (y < chunkSize.y - 1) top = voxels[flattened_index].a != 0;

                        bool bottom = false;
                        flattened_index = Tools.GetFlatIndexFromXYZ(chunkSize, new Vector3Int(x, y - 1, z));
                        if (y > 0) bottom = voxels[flattened_index].a != 0;

                        if (left && right && top && bottom && front && back) continue;

                        Vector3 vOffset = new Vector3(x, y, z);

                        if (!top)
                        {
                            verts.Add(Vector3.Scale((vertList[7] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                            verts.Add(Vector3.Scale((vertList[6] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                            verts.Add(Vector3.Scale((vertList[5] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                            verts.Add(Vector3.Scale((vertList[7] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                            verts.Add(Vector3.Scale((vertList[5] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                            verts.Add(Vector3.Scale((vertList[4] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                        }

                        if (!right)
                        {
                            verts.Add(Vector3.Scale((vertList[5] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                            verts.Add(Vector3.Scale((vertList[6] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                            verts.Add(Vector3.Scale((vertList[2] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                            verts.Add(Vector3.Scale((vertList[1] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                            verts.Add(Vector3.Scale((vertList[5] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                            verts.Add(Vector3.Scale((vertList[2] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                        }

                        if (!left)
                        {
                            verts.Add(Vector3.Scale((vertList[7] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                            verts.Add(Vector3.Scale((vertList[4] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                            verts.Add(Vector3.Scale((vertList[0] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                            verts.Add(Vector3.Scale((vertList[3] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                            verts.Add(Vector3.Scale((vertList[7] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                            verts.Add(Vector3.Scale((vertList[0] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                        }

                        if (!front)
                        {
                            verts.Add(Vector3.Scale((vertList[6] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                            verts.Add(Vector3.Scale((vertList[7] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                            verts.Add(Vector3.Scale((vertList[3] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                            verts.Add(Vector3.Scale((vertList[2] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                            verts.Add(Vector3.Scale((vertList[6] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                            verts.Add(Vector3.Scale((vertList[3] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                        }

                        if (!back)
                        {
                            verts.Add(Vector3.Scale((vertList[0] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                            verts.Add(Vector3.Scale((vertList[4] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                            verts.Add(Vector3.Scale((vertList[5] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                            verts.Add(Vector3.Scale((vertList[5] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                            verts.Add(Vector3.Scale((vertList[1] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                            verts.Add(Vector3.Scale((vertList[0] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                        }

                        if (!bottom)
                        {
                            verts.Add(Vector3.Scale((vertList[3] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                            verts.Add(Vector3.Scale((vertList[0] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                            verts.Add(Vector3.Scale((vertList[1] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                            verts.Add(Vector3.Scale((vertList[2] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                            verts.Add(Vector3.Scale((vertList[3] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                            verts.Add(Vector3.Scale((vertList[1] + vOffset), voxelScale));
                            ind.Add(verticesIndex++);
                            colors.Add(voxelColor);
                        }
                    }
                }
            }
        }
    }
}
