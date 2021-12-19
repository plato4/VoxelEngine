using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace VoxelEngine
{
    public enum MeshGenerationMethod
    {
        SingleThreaded = 0,
        MultiThreaded = 1,
    }
    public enum ColliderType
    {
        Concave = 0,
        Convex = 1,
        Boxes = 2
    }

    public class Volume : MonoBehaviour
    {
        private Color32[] voxels;
        public Vector3Int volumeSize = new Vector3Int(16, 16, 16);
        public Vector3Int chunkSize = new Vector3Int(8, 8, 8);
        public Vector3 voxelScale = new Vector3(1, 1, 1);
        public GameObject baseChunkPrefab;
        public Material material;
        const int MAX_CHUNK_SIZE = 16;

        public bool fillVoxelState = true;

        private Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();
        private Dictionary<Vector3Int, bool> dirtyChunkRegister = new Dictionary<Vector3Int, bool>();


        public ColliderType colliderType = ColliderType.Concave;
        public MeshGenerationMethod meshGenerationMethod = MeshGenerationMethod.MultiThreaded;
        public MeshingAlgorithm meshingAlgorithm = MeshingAlgorithm.Culled;

        private bool instantiated = false;

        void Start()
        {
            if (!this.instantiated)
                Instantiate();
        }

        private void Instantiate()
        {
            if (this.material == null) { Debug.LogError("No material assigned"); }
            this.CreateVoxelArray();
            this.CreateChunks();
            this.UpdateAllChunks();
            this.GenerateTriggerCollider();
            this.instantiated = true;
        }

        private void CreateVoxelArray()
        {
            if (volumeSize.x < 1) volumeSize.x = 1;
            if (volumeSize.y < 1) volumeSize.y = 1;
            if (volumeSize.z < 1) volumeSize.z = 1;
            this.voxels = new Color32[volumeSize.x *
                                      volumeSize.y *
                                      volumeSize.z];

            for (int i = 0; i < this.voxels.Length; i++) this.voxels[i] = new Color32(0, 0, 0, 0);
        }

        public static Volume CreateVolume(Vector3Int chunkSize, Vector3Int volumeSize, Vector3 position, Material material, 
            ColliderType colliderType = ColliderType.Concave, MeshGenerationMethod meshGenerationMethod = MeshGenerationMethod.MultiThreaded,
            MeshingAlgorithm meshingAlgorithm = MeshingAlgorithm.Culled)
        {
            GameObject go = new GameObject("Volume");
            Volume v = go.AddComponent<Volume>();
            v.chunkSize = chunkSize;
            v.volumeSize = volumeSize;
            go.transform.position = position;
            v.material = material;
            v.meshingAlgorithm = meshingAlgorithm;
            v.colliderType = colliderType;
            v.meshGenerationMethod = meshGenerationMethod;
            return v;
        }
        void Update()
        {
            if (this.dirtyChunkRegister.Count > 0)
            {
                foreach(var chunkIndex in this.dirtyChunkRegister)
                {
                    this.UpdateChunk(chunkIndex.Key);
                }
                dirtyChunkRegister.Clear();
            }
        }

        private void UpdateAllChunks()
        {
            foreach (var chunk in this.chunks)
            {
                this.UpdateChunk(chunk.Key);
            }
        }

        private void UpdateChunk(Vector3Int chunkIndex)
        {
            Chunk c;
            if (this.chunks.TryGetValue(chunkIndex, out c))
            {
                c.Generate(this.GetChunkVoxels(chunkIndex), this.chunkSize, this.voxelScale, this.material, this.meshGenerationMethod, this.meshingAlgorithm, this.colliderType);
            }
        }
        private void GenerateTriggerCollider()
        {
            BoxCollider boxCollider = this.GetComponent<BoxCollider>();
            if (boxCollider == null) boxCollider = this.gameObject.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(this.volumeSize.x * this.voxelScale.x,
                                           this.volumeSize.y * this.voxelScale.y,
                                           this.volumeSize.z * this.voxelScale.z);
            boxCollider.center = boxCollider.size / 2f;
            boxCollider.isTrigger = true;
        }

        private void CreateChunks()
        {
            this.chunkSize.Clamp(Vector3Int.one, Vector3Int.one*MAX_CHUNK_SIZE);
            foreach (var chunk in this.chunks)
            {
                Destroy(chunk.Value.gameObject);
                
            }
            this.chunks.Clear();

            Vector3Int chunkDimensions
                        = new Vector3Int((int)Mathf.CeilToInt((float)this.volumeSize.x / this.chunkSize.x),
                                         (int)Mathf.CeilToInt((float)this.volumeSize.y / this.chunkSize.y),
                                         (int)Mathf.CeilToInt((float)this.volumeSize.z / this.chunkSize.z));

            for (int x = 0; x < chunkDimensions.x; x++)
            {
                for (int y = 0; y < chunkDimensions.y; y++)
                {
                    for (int z = 0; z < chunkDimensions.z; z++)
                    {
                        this.CreateChunkAt(new Vector3Int(x, y, z));
                    }
                }
            }
        }

        private void CreateChunkAt(Vector3Int index)
        {
            // TGameObject chunkGoODO: IF STATIC USE STATIC OBJECT
            GameObject chunkGo;


            // for injecting static game object
            if (this.baseChunkPrefab == null)
                chunkGo = new GameObject();
            else
                chunkGo = GameObject.Instantiate(this.baseChunkPrefab);
            chunkGo.name = "Chunk: " + index.ToString();
            chunkGo.transform.parent = this.transform;
            chunkGo.transform.localPosition = this.IndexToWorldPosition(index);
            chunkGo.AddComponent<MeshFilter>();
            chunkGo.AddComponent<MeshRenderer>();
            var c = chunkGo.AddComponent<Chunk>();
            c.index = index;
            this.chunks.Add(index, c);
        }

        private Vector3 IndexToWorldPosition(Vector3Int index)
        {
            Vector3 worldPos = new Vector3();
            worldPos.x = index.x * this.chunkSize.x * this.voxelScale.x;
            worldPos.y = index.y * this.chunkSize.y * this.voxelScale.y;
            worldPos.z = index.z * this.chunkSize.z * this.voxelScale.z;
            return worldPos;
        }

        public void FillVoxels(Color32 color, bool fillVoxelState)
        {
            
            //TODO test volume with maximal vert output
            if (fillVoxelState == false) color.a = 0;
            for (int x = 0; x < this.volumeSize.x; x++)
            {
                for (int y = 0; y < this.volumeSize.y; y++)
                {
                    for (int z = 0; z < this.volumeSize.z; z++)
                    {
                        this.SetVoxel(new Vector3Int(x, y, z), color);
                    }
                }
            }
        }

        public bool SetVoxel(Vector3Int index, Color32 color)
        {
            //TODO CREATE VOXEL ARRAY AND MESH
            if (this.voxels == null | this.chunks.Count == 0) this.Instantiate();

            var flatIndex = Tools.GetFlatIndexFromXYZ(this.volumeSize, index);
            if (flatIndex < 0 || flatIndex > this.voxels.Length - 1) return false;
            this.voxels[flatIndex] = color;

            Vector3Int chunkIndex = this.GetChunkIndexFromVoxelIndex(index);

            if (!this.dirtyChunkRegister.ContainsKey(chunkIndex))
                 this.dirtyChunkRegister.Add(chunkIndex, true);

            return true;
        }

        public bool SetVoxelAtWorld(Vector3 position, Color32 color)
        {
            return this.SetVoxel(this.GetVoxelIndexAtWorld(position), color);
        }

        public bool GetVoxelAtWorld(Vector3 position, out Color32 color)
        {
            return this.GetVoxel(this.GetVoxelIndexAtWorld(position), out color);
        }

        public Vector3Int GetVoxelIndexAtWorld(Vector3 position)
        {
            var pOffset = position - this.transform.position;
            return new Vector3Int((int)(pOffset.x / this.voxelScale.x),
                                  (int)(pOffset.y / this.voxelScale.y),
                                  (int)(pOffset.z / this.voxelScale.z));
        }

        public bool GetVoxel(Vector3Int index, out Color32 color)
        {
            if (this.voxels == null | this.chunks.Count == 0) this.Instantiate();

            var flatIndex = Tools.GetFlatIndexFromXYZ(this.volumeSize, index);
            if (flatIndex < 0 || flatIndex > this.voxels.Length - 1)
            {
                color = new Color32(0, 0, 0, 0);
                return false;
            }
            color = this.voxels[flatIndex];
            return true;
        }

        private Vector3Int GetChunkIndexFromVoxelIndex(Vector3Int index)
        {
            return new Vector3Int((int)index.x / this.chunkSize.x,
                                  (int)index.y / this.chunkSize.y,
                                  (int)index.z / this.chunkSize.z);

        }

        private Color32[] GetChunkVoxels(Vector3Int index)
        {
            Vector3Int indexStart = new Vector3Int(index.x * this.chunkSize.x,
                                                   index.y * this.chunkSize.y,
                                                   index.z * this.chunkSize.z);
            Vector3Int indexEnd = new Vector3Int(indexStart.x + this.chunkSize.x,
                                                 indexStart.y + this.chunkSize.y,
                                                 indexStart.z + this.chunkSize.z);

            indexEnd.Clamp(Vector3Int.zero, this.volumeSize);

            Color32[] voxels = new Color32[this.chunkSize.x * this.chunkSize.y * this.chunkSize.z];

            for (int x = indexStart.x; x < indexEnd.x; x++)
            {
                for (int y = indexStart.y; y < indexEnd.y; y++)
                {
                    for (int z = indexStart.z; z < indexEnd.z; z++)
                    {
                        Vector3Int local_index = new Vector3Int(x - indexStart.x, y - indexStart.y, z - indexStart.z);

                        //Color32 col;
                        //this.GetVoxel(new Vector3Int(x, y, z), out col);

                        //Direct access for speed
                        var fastFlatIndex = Tools.GetFlatIndexFromXYZ(this.volumeSize, new Vector3Int(x, y, z));

                        Color32 col = this.voxels[fastFlatIndex];
                        var flatIndex = Tools.GetFlatIndexFromXYZ(this.chunkSize, local_index);
                        voxels[flatIndex] = col;
                    }
                }
            }
            return voxels;
        }
    }
}
