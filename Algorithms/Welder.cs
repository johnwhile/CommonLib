
#define ORIGINAL

using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Common.Maths;

namespace Common.Tools
{
    /// <summary>
    /// Code extract from:
    /// Real-Time Collision Detection (The Morgan Kaufmann Series in Interactive 3-D Technology)
    /// http://www.amazon.com/Real-Time-Collision-Detection-Interactive-Technology/dp/1558607323
    /// </summary>
    /// <typeparam name="T">the vertex struct</typeparam>
    public abstract class WeldAlgorithm<T>
    {
        /*
        //  use the hash bucket infinite partitioning where bucket are grid cell in 2D or cube box in 3D.
        //  __|___|___|__
        //  __|___|___|__
        //  __|___|___|__
        //    |   |   |
        // Distant points can use the same cell know as "Hash collision". Increase hash collision increase
        // the number of test for each vertices and reduce performance:
        // H(x,y,z) = (p1*x xor p2*y xor p3*z) % n  where p123 are large primitives 
        //
        // I notice that using large primitive the bucket used is ~ 70% (0.6s), example with  H = x ^ y ^ z; the bucket use is ~5% (6s)
        // in fact a non-uniform distribution increases the number of hash collisions and the cost of resolving them.
        //  ___a________b____
        // |        |        |
        // | 0 2 4  |1  3 6  |
        // |________|________|
        // |        |        |
        // |  5 7   |        |
        // |___c____|____d___|
        //
        // Supposing weld_epsilon = -1 so no collisions, at end of algorithm the result is
        //  the table "first" are { a,4  b,6  c,7  d,-1} contain the first vertex where scan start
        //  the table "next" are {-1,-1, 0, 1, 2,-1, 3, 5}
        //  
        //  if you want scan for example v3 --> get bucket(v3) = b 
        //  start scan with first[b] = v6 (start)
        //  test collision v3,v6
        //  next[v6] = v3
        //  test collision v3,v3 (skip)
        //  next[v3] = v1
        //  test collision v3,v1
        //  next[v1] = -1 (stop)
        //  
        //  if you want scan for example v1 --> get bucket(v1) = b and a
        */
        /// <summary>
        /// index of vertices used to stop the scan for each bucket. See "next" array
        /// </summary>
        protected const int STOP = -1;
        /// <summary>
        /// number of hash buckets to map grid cells into
        /// must be different from 2^p and 10^p for a random p number
        /// http://adtsai.blogspot.it/2008/04/optimising-vertex-welder-adventures.html
        /// O(m(n / m)^2), where n is the total number of vertices and m is the total number of partitions in the octree
        /// but with CellSize the solution is a little complicated
        /// </summary>
        public uint NUM_BUCKETS = 128;
        /// <summary>
        /// grid cell size; must be at least 2 * WELD_EPSILON
        /// TODO : can improve performance using differents CELL_SIZE_X , Y and Z ???
        /// </summary>
        public float CELL_SIZE = 10.0f;
        /// <summary>
        /// The radius of collisions. A problem occour when epsilon don't change the float value : 
        /// if x=1e6 and eps = 0.1 : x+e == x-e and neighbour bucket will never been find.
        /// if epsilon is &lt;0 a incorrect bucket sequence will be tested
        /// </summary>
        public float WELD_EPSILON = 1.0f; // radius around vertex defining welding neighborhood
        /// <summary>
        /// to improve computation of distance we precompute the squared distance.
        /// If you use Manhattan distance, the collision test is using a rectangle
        /// </summary>
        protected float WELD_EPSILON_SQ = 1.0f;
        /// <summary>
        /// contain the reference to group using vertex index.
        /// TODO : improve the use of dinamic list
        /// </summary>
        public Dictionary<int, List<int>> groups;

        // in the ORIGINAL we suppose that the hash function populate 100% of array, but empiricaly i
        // noticed that 78% is maximum with my hash function
#if ORIGINAL
        /// <summary>
        /// contain the first vertex index(int) of linked list of select bucket(uint)
        /// </summary>
        protected int[] first;
#else
        /// <summary>
        /// contain the first vertex index(int) of linked list of select bucket(uint)
        /// </summary>
        protected Dictionary<uint, int> first;
#endif
        /// <summary>
        /// contain the bucket key foreach vertex index
        /// </summary>
        int[] next;
        /// <summary>
        /// the vertices list
        /// </summary>
        protected IList<T> vertices;
        /// <summary>
        /// Contain the octree indices of visited bucked for one vertex welding process.
        /// with CELL &gt; 2xEpsilon, we ensure that one vertex test only 8 neighbours (plus 1 central where vertex are)
        /// </summary>
        protected uint[] visitedbucket;
        /// <summary>
        /// Mark vertex as welded
        /// </summary>
        BitArray marked;
        /// <summary>
        /// debug information about optimization respect the brutal force approach
        /// </summary>
        public int CollisionCounter { get; protected set; }
        /// <summary>
        /// debug information about time in ms
        /// </summary>
        public int TimeCounter { get; protected set; }
        /// <summary>
        /// debug information about real bucket utilized by algorith, the optimal value are equal to num_buckets
        /// </summary>
        public int UsedBucketCounter { get; protected set; }

        #region Collisions On / Off
        delegate bool CollisionFunctionDelegate(int i, int j);
        /// <summary>
        /// Using a delegate we avoid to insert a boolean test for each collisions
        /// </summary>
        CollisionFunctionDelegate CollisionFunction;
        bool collisionsenabled = false;
        /// <summary>
        /// All collisions will be negative, need to check the number of collision with brute force
        /// </summary>
        public bool EnableCollisions 
        {
            get { return collisionsenabled; }
            set
            {
                if (value) CollisionFunction = new CollisionFunctionDelegate(this.TestCollision);
                else CollisionFunction = new CollisionFunctionDelegate(this.NullCollision);
            }
        }
        #endregion
       
        /// <summary>
        /// Initialize helper arrays
        /// </summary>
        public void InitData(IList<T> verts)
        {
            int count = verts.Count;
            //groups = new Dictionary<int, Group>();
            groups = new Dictionary<int, List<int>>();

            marked = new BitArray(count, false);
            next = new int[count];
            for (int i = 0; i < count; i++) next[i] = STOP;
            vertices = verts;
            EnableCollisions = true;

#if ORIGINAL
            first = new int[(int)NUM_BUCKETS];
            for (int i = 0; i < NUM_BUCKETS; i++) first[i] = STOP;
#else
            first = new Dictionary<uint, int>((int)NUM_BUCKETS);
#endif
        }

        /// <summary>
        /// can optimize the NUM_BUCKETS : CELL_SIZE parameters for current homogeneous points list
        /// </summary>
        public abstract void GetOptimalBucketSize(T minCorner, T maxCorner, int numofpoints);
        /// <summary>
        /// This code show the brute force algorithm. only for debug
        /// </summary>
        public void RunBruteForce()
        {
            TimeCounter = Environment.TickCount;
            CollisionCounter = 0;
            UsedBucketCounter = 0;
            WELD_EPSILON_SQ = WELD_EPSILON * WELD_EPSILON;

            if (CELL_SIZE <= 2 * WELD_EPSILON)
                throw new ArgumentException("must be this condition : CELL_SIZE > 2 * WELD_EPSILON");

            // the predicted worst case
            int counter = (int)Maths.Mathelp.fattoriale(vertices.Count);

            for (int i = 0; i < vertices.Count; i++)
            {
                //Console.WriteLine(string.Format("Welding v{0} :", i));
                for (int j = i + 1; j < vertices.Count; j++)
                {
                    if (marked[j]) continue;

                    if (TestCollision(i, j))
                    {
                        //Console.WriteLine(string.Format("  distance v{0} -> v{1} COLLISION!", i, j));
                        Weld(i, j);
                    }
                    else
                    {
                        //Console.WriteLine(string.Format("  distance v{0} -> v{1}", i, j));
                    }
                }
            }
            TimeCounter = Environment.TickCount - TimeCounter;

            next = null;
#if ORIGINAL
            first = null;
#else
            first.Clear();
            first = null;
#endif
            vertices = null;
        }

        /// <summary>
        /// Do the algorithm
        /// </summary>
        public void Run()
        {
            TimeCounter = Environment.TickCount;
            CollisionCounter = 0;
            UsedBucketCounter = 0;

            if (WELD_EPSILON < float.Epsilon)
                throw new ArgumentException("WELD_EPSILON is the radius of collision and must be > float minimum value");

            WELD_EPSILON_SQ = WELD_EPSILON * WELD_EPSILON;

            if (CELL_SIZE <= 2 * WELD_EPSILON)
                throw new ArgumentException("must be this condition : CELL_SIZE > 2 * WELD_EPSILON");

            // A "incremental" algorithm pseudo-code
            for (int i = 0; i < vertices.Count; i++)
            {
                if (!marked[i])
                {
                    //Console.WriteLine(string.Format("Welding v{0} :", i));
                    int iw = AddVertex(i);
                    if (iw > -1)
                    {
                        Weld(iw, i);
                    }
                }
            }

            TimeCounter = Environment.TickCount - TimeCounter;

            next = null;
#if ORIGINAL
            first = null;
#else
            first.Clear();
            first = null;
#endif
            vertices = null;
            //grouplist = new Group[welded.Count];
            //welded.Values.CopyTo(grouplist, 0);
            //foreach (Group group in grouplist)
            //    group.StopCollection();
        }

        /// <summary>
        /// Return the vertex that collide with iv
        /// </summary>
        protected abstract int AddVertex(int iv);

        // TODO : improve a little
        protected bool BucketVisited(uint bucket)
        {
            return Array.BinarySearch<uint>(visitedbucket, bucket) < 0;
        }

        /// <summary>
        /// scan all vertex in this bucket and test if distance is &lt; EPSILON
        /// </summary>
        protected int ScanVertexInBucket(int iv, uint bucket)
        {
            // Scan through linked list of vertices at this bucket
#if ORIGINAL
            for (int index = first[bucket]; index != STOP; index = next[index])
#else
            for (int index = first[bucket]; index != STOP; index = next[index])
#endif
            {
                if (CollisionFunction(iv, index))
                {
                    //Console.WriteLine(string.Format("  distance v{0} -> v{1} COLLISION!", iv, index));
                    return index;
                }
                else
                {
                    //Console.WriteLine(string.Format("  distance v{0} -> v{1}", iv, index));
                }

            }
            return -1;
        }

        /// <summary>
        /// Add vertex to bucket : 
        /// 1 : set the "next" vertex of iv as last vertex added to this bucket ( first[bucket] )
        /// 2 : update last vertex added to this bucket with iv
        /// </summary>
        protected void AddVertexToBucket(int iv, uint bucket)
        {
#if ORIGINAL
            int idx = first[bucket];
            next[iv] = idx;
            first[bucket] = iv;
            if (idx == STOP) UsedBucketCounter++;         
#else
            next[iv] = first[bucket];
            first[bucket] = iv;
#endif
        }


        const uint magic1 = 0x8da6b343; // Large multiplicative constants;
        const uint magic2 = 0xd8163841; // here arbitrarily chosen primes
        const uint magic3 = 7199369;
        /// <summary>
        /// Hash function , get the bucket key using grid coordinates. A uniform hash function is exential for a
        /// sparse index but only with empirical test can implement it, using example bucket = (uint)(x ^ y ^ z) % NUM_BUCKETS decrease a lot
        /// the efficency
        /// </summary>
        /// <remarks>
        /// Exemple of 100x slow hash function:
        /// <code>
        /// return unchecked ((uint)(x ^ y ^ z)) % NUM_BUCKETS;
        /// </code>
        /// </remarks>
        protected uint GetGridCellBucket(int x, int y , int z)
        {
            //overflow is correct
            return unchecked((uint)((x * magic1) ^ (y * magic2) ^ (z * magic3))) % NUM_BUCKETS;
        }

        /// <summary>
        /// Collision test between two vertices, skip when test itself
        /// </summary>
        bool TestCollision(int iv, int iw)
        {
            // in scan line you will find the same vertex, avoid itself collision
            if (iv != iw)
            {
                CollisionCounter++;
                return Distance(iv, iw) <= WELD_EPSILON_SQ;
            }
            return false;
        }
        /// <summary>
        /// same of <seealso cref="TestCollision"/>, but for debug only. The Distance equation
        /// are calculate only to test time consuming, but function return always false to test the
        /// worst case
        /// </summary>
        bool NullCollision(int iv, int iw)
        {
            if (iv != iw)
            {
                CollisionCounter++;
                bool result = Distance(iv, iw) <= WELD_EPSILON_SQ;
            }
            return false;
        }

        /// <summary>
        /// Distance for each T implementations
        /// </summary>
        protected abstract float Distance(int iv, int iw);

        /// <summary>
        /// Do the weld of vertex iw -> to iv
        /// </summary>
        /// <param name="iv">the targhet of welding</param>
        /// <param name="iw">vertex to weld</param>
        protected void Weld(int iv, int iw)
        {
            marked[iw] = true;

            List<int> indices;
            //Group group;
            if (!groups.ContainsKey(iv))
            {
                indices = new List<int>();
                indices.Add(iv);
                groups.Add(iv, indices);
                //group = new Group();
                //groups.Add(iv, group);
                //group.Add(iv);
            }
            else
            {
                indices = groups[iv];
                //group = groups[iv];
            }
            indices.Add(iw);
            //group.Add(iw);
        }

        [DebuggerDisplay("Count = {m_count}")]
        public class Group
        {
            int count = 0;
            public List<int> vertices = new List<int>();

            internal void Add(int vertex)
            {
                vertices.Add(vertex);
                count++;
            }
        }
        // Table of prime numbers to use as hash table sizes.
        // The entry used for capacity is the smallest prime number in this aaray
        // that is larger than twice the previous capacity.

        protected static readonly int[] primes = new int[]{
            3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
            1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
            17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
            187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
            1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369};
    }

    /// <summary>
    /// 3D version
    /// </summary>
    public class WeldVertex3D : WeldAlgorithm<Vector3f>
    {
        public WeldVertex3D()
        {
            visitedbucket = new uint[8];
        }
        
        protected override int AddVertex(int iv)
        {
            Vector3f v = vertices[iv];

            // Make sure epsilon is not too small for the coordinates used!
            Debug.Assert(v.x - WELD_EPSILON != v.x && v.x + WELD_EPSILON != v.x, "x float.epsilon don't change value");
            Debug.Assert(v.y - WELD_EPSILON != v.y && v.y + WELD_EPSILON != v.y, "y float.epsilon don't change value");
            Debug.Assert(v.z - WELD_EPSILON != v.z && v.z + WELD_EPSILON != v.z, "z float.epsilon don't change value");

            // Compute cell coordinates of bounding box of vertex epsilon neighborhood
            int left = (int)((v.x - WELD_EPSILON) / CELL_SIZE);
            int right = (int)((v.x + WELD_EPSILON) / CELL_SIZE);
            int top = (int)((v.y + WELD_EPSILON) / CELL_SIZE);
            int down = (int)((v.y - WELD_EPSILON) / CELL_SIZE);
            int front = (int)((v.z + WELD_EPSILON) / CELL_SIZE);
            int back = (int)((v.z - WELD_EPSILON) / CELL_SIZE);
            // with CellSize > 2*Epsilon we ensure we are testing only 8 bounding box in the octree

            int nextbucket = 0;

            for (int i = 0; i < 8; visitedbucket[i++] = 0) ;


            // Loop over all overlapped cells and test against their buckets
            for (int z = back; z <= front; z++)
                for (int x = left; x <= right; x++)
                    for (int y = down; y <= top; y++)
                    {
                        uint b = GetGridCellBucket(x, y, z);
#if ORIGINAL
#else
                        // with dictionary instead hashtable, i need to initialize zero index for all possible buckets
                        // to avoid a continuos ContainsKey check
                        if (!first.ContainsKey(b))
                        {
                            first.Add(b, STOP);
                            UsedBucketCounter++;
                        }
#endif
                        // If this bucket already tested, don’t test it again
                        if (BucketVisited(b))
                        {
                            //Console.WriteLine(string.Format("  do bucket h{0}", b));
                            int iw = ScanVertexInBucket(iv, b);
                            if (iw > -1) return iw;
                        }
                        visitedbucket[nextbucket++] = b;
                    }

            // if vertex not found add to bucket;
            // Tiling function :
            uint bucket = GetGridCellBucket(
                (int)(v.x / CELL_SIZE),
                (int)(v.y / CELL_SIZE),
                (int)(v.z / CELL_SIZE));

            AddVertexToBucket(iv, bucket);

            return -1;
        }

        protected override float Distance(int iv, int iw)
        {
            return (vertices[iv] - vertices[iw]).LengthSq;
        }

        public override void GetOptimalBucketSize(Vector3f minCorner, Vector3f maxCorner, int numofpoints)
        {
            float deltaX = maxCorner.x - minCorner.x;
            float deltaY = maxCorner.y - minCorner.y;
            float deltaZ = maxCorner.z - minCorner.z;

            NUM_BUCKETS = (uint)(numofpoints / System.Math.Sqrt(numofpoints) * 4.0 - 1);
            CELL_SIZE = (float)(((deltaX + deltaY + deltaZ) * 0.333333) / System.Math.Sqrt(NUM_BUCKETS));

            if (CELL_SIZE < WELD_EPSILON * 2) CELL_SIZE = WELD_EPSILON * 2.1f;
        }

    }

    /// <summary>
    /// 2D version
    /// </summary>
    public class WeldVertex2D : WeldAlgorithm<Vector2f>
    {
        public WeldVertex2D()
        {
            visitedbucket = new uint[4];
        }
        protected override int AddVertex(int iv)
        {
            Vector2f v = vertices[iv];

            // Make sure epsilon is not too small for the coordinates used!
            Debug.Assert(v.x - WELD_EPSILON != v.x && v.x + WELD_EPSILON != v.x, "x float.epsilon don't change value");
            Debug.Assert(v.y - WELD_EPSILON != v.y && v.y + WELD_EPSILON != v.y, "y float.epsilon don't change value");

            // Compute cell coordinates of bounding box of vertex epsilon neighborhood
            int left = (int)((v.x - WELD_EPSILON) / CELL_SIZE);
            int right = (int)((v.x + WELD_EPSILON) / CELL_SIZE);
            int top = (int)((v.y + WELD_EPSILON) / CELL_SIZE);
            int down = (int)((v.y - WELD_EPSILON) / CELL_SIZE);
            // with CellSize > 2*Epsilon we ensure we are testing only 8 bounding box in the octree

            int nextbucket = 0;

            for (int i = 0; i < 4; visitedbucket[i++] = 0) ;


            // Loop over all overlapped cells and test against their buckets
            for (int x = left; x <= right; x++)
                for (int y = down; y <= top; y++)
                {
                    uint b = GetGridCellBucket(x, y, 0);
#if ORIGINAL
#else
                    // with dictionary instead hashtable, i need to initialize zero index for all possible buckets
                    // to avoid a continuos ContainsKey check
                    if (!first.ContainsKey(b))
                    {
                        first.Add(b, STOP);
                        UsedBucketCounter++;
                    }
#endif
                    // If this bucket already tested, don’t test it again
                    if (BucketVisited(b))
                    {
                        //Console.WriteLine(string.Format("  do bucket h{0}", b));
                        int iw = ScanVertexInBucket(iv, b);
                        if (iw > -1) return iw;
                    }
                    visitedbucket[nextbucket++] = b;
                }

            // if vertex not found add to bucket;
            uint bucket = GetGridCellBucket((int)(v.x / CELL_SIZE), (int)(v.y / CELL_SIZE), 0);

            AddVertexToBucket(iv, bucket);

            return -1;
        }

        protected override float Distance(int iv, int iw)
        {
            return Vector2f.GetLengthSquared(vertices[iv] - vertices[iw]);
        }

        public override void GetOptimalBucketSize(Vector2f minCorner, Vector2f maxCorner, int numofpoints)
        {
            float deltaX = maxCorner.x - minCorner.x;
            float deltaY = maxCorner.y - minCorner.y;

            NUM_BUCKETS = (uint)(numofpoints / System.Math.Sqrt(numofpoints) * 4.0 - 1);
            CELL_SIZE = (float)(((deltaX + deltaY) * 0.5) / System.Math.Sqrt(NUM_BUCKETS));
            
            if (CELL_SIZE < WELD_EPSILON * 2) CELL_SIZE = WELD_EPSILON * 2.1f;
        }
    }

}
