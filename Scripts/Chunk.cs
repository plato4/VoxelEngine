using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace VoxelEngine
{
    [RequireComponent (typeof(MeshCollider))]
    [RequireComponent (typeof(MeshFilter))]
    [RequireComponent (typeof(MeshRenderer))]
    public class Chunk : MonoBehaviour
    {
        public Vector3Int index;

        // classic meshing
        private List<Vector3> verts = new List<Vector3>();
        private List<Color32> colors = new List<Color32>();
        private List<int> inds = new List<int>();

        // threaded meshing
        private Mutex accessMeshDataMutex = new Mutex();

        private Material material;

        private enum MeshingState
        {
            Idle = 0,
            Generating = 1,
            Generated = 2
        }
        private MeshingState meshingState = MeshingState.Idle;

        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (this.meshingState == MeshingState.Generated)
            {
                this.meshingState = MeshingState.Idle;
                this.ApplyMesh(this.verts, this.colors, this.inds, this.material);
            }
        }

        internal void Generate(Color32[] voxels, Vector3Int chunkSize, Vector3 voxelScale, Material material, 
            MeshGenerationMethod meshGenerationMethod, MeshingAlgorithm meshingAlgorithm, ColliderType colliderType)
        {
            this.material = material;

            if (meshGenerationMethod == MeshGenerationMethod.SingleThreaded)
            {
                Meshing.GenerateMesh(voxels, chunkSize, voxelScale, material, ref this.verts, ref this.colors, ref this.inds, meshingAlgorithm);
                this.meshingState = MeshingState.Generated;
                //throw new NotImplementedException();
            }
            else if (meshGenerationMethod == MeshGenerationMethod.MultiThreaded)
            {
                Task.Run(() =>
                {
                    this.meshingState = MeshingState.Generating;
                    List<Vector3> _verts = new List<Vector3>();
                    List<Color32> _colors = new List<Color32>();
                    List<int> _inds = new List<int>();
                    Meshing.GenerateMesh(voxels, chunkSize, voxelScale, material, ref _verts, ref _colors, ref _inds, meshingAlgorithm);

                    accessMeshDataMutex.WaitOne();
                    this.verts = _verts;
                    this.colors = _colors;
                    this.inds = _inds;
                    accessMeshDataMutex.ReleaseMutex();

                    this.meshingState = MeshingState.Generated;
                });
            }
        }

        private void ApplyMesh(List<Vector3> verts, List<Color32> colors, List<int> inds, Material material)
        {
            if (verts.Count > 65535) { Debug.LogError("Vertices on mesh over 65535, mesh most likely not generated"); };
            MeshFilter mf = this.GetComponent<MeshFilter>();
            if (mf.sharedMesh == null) mf.mesh = new Mesh();

            accessMeshDataMutex.WaitOne();
            mf.sharedMesh.Clear();
            mf.sharedMesh.SetVertices(verts);
            mf.sharedMesh.SetColors(colors);
            mf.sharedMesh.SetTriangles(inds, 0);
            accessMeshDataMutex.ReleaseMutex();

            //TODO move this into thread might save a few ms
            mf.sharedMesh.RecalculateNormals();
            this.GetComponent<MeshRenderer>().sharedMaterial = this.material;
            this.GetComponent<MeshCollider>().sharedMesh = this.GetComponent<MeshFilter>().sharedMesh;
        }
    }
}
