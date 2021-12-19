using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VoxelEngine
{
    public static class Tools
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetFlatIndexFromXYZ(int xSize, int ySize, int x, int y, int z)
        {
            return x + xSize * (y + ySize * z);
            //return GetFlatIndexFromXYZ(new Vector3Int(xSize, ySize, 0), new Vector3Int(x, y, z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetFlatIndexFromXYZ(Vector3Int size, Vector3Int pos)
        {
            return GetFlatIndexFromXYZ(size.x, size.y, pos.x, pos.y, pos.z);
        }
    }
}
