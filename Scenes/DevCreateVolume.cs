using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevCreateVolume : MonoBehaviour
{
    public Material volumeMaterial;
    public VoxelEngine.MeshingAlgorithm alg;
    public VoxelEngine.MeshGenerationMethod method;
    public VoxelEngine.Volume v;
    // Start is called before the first frame update
    void Start()
    {
        this.v = VoxelEngine.Volume.CreateVolume(new Vector3Int(18, 18, 18), new Vector3Int(32, 32, 32), this.transform.position, this.volumeMaterial, VoxelEngine.ColliderType.Concave, method, alg);


    }

    // Update is called once per frame
    void Update()
    {
        for (int x = 0; x < this.v.volumeSize.x; x++)
        {
            for (int y = 0; y < this.v.volumeSize.y; y++)
            {
                for (int z = 0; z < this.v.volumeSize.z; z++)
                {
                    var a = 128;
                    if (UnityEngine.Random.Range(0f, 1f) < 0.5f) a = 0;
                    this.v.SetVoxel(new Vector3Int(x, y, z), new Color32(255, 255, 255, (byte)a));
                }
            }
        }

    }
}
