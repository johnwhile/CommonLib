
using Common.Maths;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    /// <summary>
    /// calculate the tile hash code untill leaf nodes (Morton-order-encoding)
    /// see: http://msdn.microsoft.com/en-us/library/bb259689.aspx
    /// </summary>
    public struct TileCoord16
    {
        // some useful precomputed values to speed up
        public const byte MAXDEPTH = 16;
        public const ushort MAXCOORD = 65535;
        public static byte[] MinDepth;

        // the coordinates
        public ushort x, y;

        public uint hash;


        static TileCoord16()
        {
            // convert the coordinate value into minimum coerent depth
            // conversion : 0->1 ; 1->2 ; 2,3 -> 3 ; 4,5,6,7 -> 4
            // tablebyindex : 1,2,3,3,4,4,4,4,...
            // formula is c = [ 2^(n-1)-1 , ... , 2^n] where n is the depth
            MinDepth = new byte[MAXCOORD + 1];
            MinDepth[0] = 1;

            int count = 1;
            for (int c = 1, n = 2; c <= MAXCOORD; )
            {
                for (int i = 0; i < count; i++) MinDepth[c++] = (byte)n;
                count *= 2;
                n++;
            }
        }


        public TileCoord16(int x, int y) :
            this((ushort)x, (ushort)y) { }

        public TileCoord16(ushort x, ushort y)
        {
            this.x = x;
            this.y = y;
            this.hash = EncodeTileHash(x, y);
        }

        public TileCoord16(uint hash)
        {
            this.hash = hash;
            DecodeTileHash(hash, out x, out y);
        }

        public void GetSplitted(out TileCoord16 c0, out TileCoord16 c1, out TileCoord16 c2, out TileCoord16 c3)
        {
            int x2 = x * 2;
            int y2 = y * 2;
            c0 = new TileCoord16(x2, y2);
            c1 = new TileCoord16(x2 + 1, y2);
            c2 = new TileCoord16(x2, y2 + 1);
            c3 = new TileCoord16(x2 + 1, y2 + 1);
        }

        public int GetQuadIndex()
        {
            return (int)(hash & 3);
        }
        
        public TileCoord16 GetParentTileCoord()
        {
            return new TileCoord16(x >> 1, y >> 1);
        }
        public void GetHierarchy(ref byte[] hierarchy, int depth)
        {
            DecodeTileHashHierarchy(hash, ref hierarchy, depth);
        }

         
        public static TileCoord16 GetTileCoord(uint hash)
        {
            TileCoord16 coord = new TileCoord16();
            TileCoord16.DecodeTileHash(hash, out coord.x, out coord.y);
            return coord;
        }

        /// <summary>
        /// Encode the coordinate to the hash tile value
        /// </summary>
        /// <param name="depth">the quadtree depth in relation of currect coord</param>
        /// <remarks>
        /// if x and y are a values of n bits : x(n-1)...x(0)
        /// f(x,y) = {y(n-1) x(n-1) .... y(0) x(0)}  where n = (depth-1) necessary bits.
        /// example for depth 4  f(x,y) = {y2,x2,y1,x1,y0,x0}
        /// 
        /// the algorithm don't require depth number because it stop when tilex and tiley are zero
        /// </remarks>
        public static uint EncodeTileHash(ushort tilex, ushort tiley)
        {
            int hash = 0;
            for (int i = 0; tilex > 0 || tiley > 0; i++)
            {
                hash |= (((tiley & 1) << 1) | (tilex & 1)) << (i * 2);
                tilex >>= 1;
                tiley >>= 1;
            }
            return (uint)hash;
        }

        /// <summary>
        /// Decode the hash tile value to tile coordinate, depth values isn't necessary
        /// </summary>
        /// <remarks>
        /// the algorithm don't require depth number because it stop when hash are zero
        /// </remarks>
        public static void DecodeTileHash(uint hash , out ushort tilex, out ushort tiley)
        {
            tilex = tiley = 0;
            int i = 0;
            uint x = 0;
            uint y = 0;
            while (hash > 0)
            {
                x |= ((hash & 1) << i);
                y |= ((hash & 2) << i);
                hash >>= 2;
                i++;
            }
            tilex = (ushort)x;
            tiley = (ushort)(y >> 1); // because (hash && 2)
        }
        
        /// <summary>
        /// Expand the TileHash value into array of quad indices, for performance reason i use a
        /// preinitialized array, the array must have enought space (equal to number of levels)
        /// </summary>
        /// <param name="hierarchy">walking child index for each depth , ChildIdx = hierarchy[Depth] , where Depth = 0 is always zero</param>
        /// <param name="depth">VERY IMPORTANT :  is the limit untill algorithm extract the hierarchy indices</param>
        public static void DecodeTileHashHierarchy(uint hash, ref byte[] hierarchy , int depth)
        {
            int min = getMinDepth(hash);
            if (depth < min)
                throw new ArgumentOutOfRangeException("hash contain a coordinate out of maximum targetDepth");
            hierarchy[0] = 0;
            int i = depth - 1;
            while (hash > 0)
            {
                hierarchy[i] = (byte)(hash & 3);
                hash >>= 2;
                i--;
            }
        }

        /// <summary>
        /// The depth can be derived using maxium not-zero bit of tilecoord
        /// </summary>
        /// <remarks>
        /// if you see the document u can understand that diagonal values of tile table can
        /// be used to derive minimum coerent depth used for these tiles coords
        /// max 0 :  depth 1
        /// max 1 :  depth 2
        /// max 2,3 : depth 3
        /// max 4,5,6,7 : depth 4
        /// depth = max bit of max(x,y)
        /// </remarks>
        static byte getMinDepth(ushort tile)
        {
            byte depth = 1;
            //int depth = MinDepth[max];
            while (depth <= MAXDEPTH && tile != 0)
            {
                depth++;
                tile /= 2;
            }
            return depth; 
        }
        /// <summary>
        /// The depth can be derived using maxium not-zero bit of maximum tilecoord, 
        /// </summary>
        static byte getMinDepth(ushort tilex, ushort tiley)
        {
            if (tilex > tiley)
                return getMinDepth(tilex);
            else
                return getMinDepth(tiley);
        }
        /// <summary>
        /// The depth can be derived using maxium not-zero bit of hash value, 
        /// </summary>
        static byte getMinDepth(uint hash)
        {
            byte depth = 1;
            //int depth = MinDepth[max];
            while (depth < 17 && hash != 0)
            {
                depth++;
                hash /= 4;
            }
            return depth;
        }

        public override bool Equals(object obj)
        {
            if (obj is TileCoord16)
            {
                TileCoord16 other = (TileCoord16)obj;
                return other.x == x && other.y == y;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (int)hash;
        }

        public override string ToString()
        {
            return string.Format("[{0},{1}] {2}", x, y, EncodeTileHash(x, y));
        }



        public static Vector2f operator *(TileCoord16 left, Vector2f right)
        {
            return new Vector2f(left.x * right.x, left.y * right.y);
        }
        public static Vector2f operator *(Vector2f left, TileCoord16 right)
        {
            return right * left;
        }
    }

    /// <summary>
    /// </summary>
    public struct TileCoord32
    {
        // some useful precomputed values to speed up
        public const byte MAXDEPTH = 32;
        public const uint MAXCOORD = uint.MaxValue;

        // the coordinates
        public uint x, y;
        public ulong hash;

        static TileCoord32()
        {

        }

        public TileCoord32(int x, int y) :
            this((uint)x, (uint)y) { }

        public TileCoord32(uint x, uint y)
        {
            this.x = x;
            this.y = y;
            this.hash = EncodeTileHash(x, y);
        }

        public TileCoord32(ulong hash)
        {
            this.hash = hash;
            DecodeTileHash(hash, out x, out y);
        }

        public void GetSplitted(out TileCoord32 c0, out TileCoord32 c1, out TileCoord32 c2, out TileCoord32 c3)
        {
            uint x2 = x * 2;
            uint y2 = y * 2;
            c0 = new TileCoord32(x2, y2);
            c1 = new TileCoord32(x2 + 1, y2);
            c2 = new TileCoord32(x2, y2 + 1);
            c3 = new TileCoord32(x2 + 1, y2 + 1);
        }

        public int GetQuadIndex()
        {
            return (int)(hash & 3);
        }

        public TileCoord32 GetParentTileCoord()
        {
            return new TileCoord32(x >> 1, y >> 1);
        }
        public void GetHierarchy(ref byte[] hierarchy, int depth)
        {
            DecodeTileHashHierarchy(hash, ref hierarchy, depth);
        }


        public static TileCoord32 GetTileCoord(ulong hash)
        {
            TileCoord32 coord = new TileCoord32();
            TileCoord32.DecodeTileHash(hash, out coord.x, out coord.y);
            return coord;
        }

        public static ulong EncodeTileHash(uint tilex, uint tiley)
        {
            ulong hash = 0;
            for (int i = 0; tilex > 0 || tiley > 0; i++)
            {
                hash |= (((tiley & 1) << 1) | (tilex & 1)) << (i * 2);
                tilex >>= 1;
                tiley >>= 1;
            }
            return hash;
        }

        public static void DecodeTileHash(ulong hash, out uint tilex, out uint tiley)
        {
            int i = 0;
            tilex = 0;
            tiley = 0;
            while (hash > 0)
            {
                tilex |= ( (uint)(hash & 1) << i);
                tiley |= ( (uint)(hash & 2) << (i - 1));
                hash >>= 2;
                i++;
            }
        }


        public static void DecodeTileHashHierarchy(ulong hash, ref byte[] hierarchy, int depth)
        {
            int min = getMinDepth(hash);
            if (depth < min)
                throw new ArgumentOutOfRangeException("hash contain a coordinate out of maximum targetDepth");
            hierarchy[0] = 0;
            int i = depth - 1;
            while (hash > 0)
            {
                hierarchy[i] = (byte)(hash & 3);
                hash >>= 2;
                i--;
            }
        }

        static byte getMinDepth(uint tile)
        {
            byte depth = 1;
            //int depth = MinDepth[max];
            while (depth <= MAXDEPTH && tile != 0)
            {
                depth++;
                tile /= 2;
            }
            return depth;
        }

        static byte getMinDepth(uint tilex, uint tiley)
        {
            if (tilex > tiley)
                return getMinDepth(tilex);
            else
                return getMinDepth(tiley);
        }

        static byte getMinDepth(ulong hash)
        {
            byte depth = 1;
            //int depth = MinDepth[max];
            while (depth < 17 && hash != 0)
            {
                depth++;
                hash /= 4;
            }
            return depth;
        }

        public override bool Equals(object obj)
        {
            if (obj is TileCoord32)
            {
                TileCoord32 other = (TileCoord32)obj;
                return other.x == x && other.y == y;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (int)hash;
        }

        public override string ToString()
        {
            return string.Format("[{0},{1}] {2}", x, y, EncodeTileHash(x, y));
        }



        public static Vector2f operator *(TileCoord32 left, Vector2f right)
        {
            return new Vector2f(left.x * right.x, left.y * right.y);
        }
        public static Vector2f operator *(Vector2f left, TileCoord32 right)
        {
            return right * left;
        }
    }


}
