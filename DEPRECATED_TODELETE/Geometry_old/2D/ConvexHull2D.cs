using System;
using System.Collections.Generic;
using System.Text;

using Common.Maths;

namespace Common.Geometry
{

    /// <summary>
    /// Generate a Convex Hull in 2D 
    /// </summary> 
    public static class ConvexHull2D
    {
        private static sbyte Compare(Vector3f a, Vector3f b)
        {
            if (a.x > b.x) return 1;
            else if (a.x < b.x) return -1;
            else if (a.z > b.z) return 1;
            else if (a.z < b.z) return -1;
            return 0;
        }

        private static void quicksort(Vector3f[] array, ushort[] index, int left, int right)
        {
            int l = left;
            int r = right;
            int p = (left + right) / 2;

            // 1. Pick a pivot value somewhere in the middle.
            Vector3f pivot = array[index[p]];

            // 2. Loop until pointers meet on the pivot.
            while (l <= r)
            {
                // 3. Find a larger value to the right of the pivot.
                //    If there is non we end up at the pivot.
                while (Compare(array[index[l]], pivot) < 0) l++;

                // 4. Find a smaller value to the left of the pivot.
                //    If there is non we end up at the pivot.
                while (Compare(array[index[r]], pivot) > 0) r--;

                // 5. Check if both pointers are not on the pivot.
                if (l <= r)
                {
                    // 6. Swap both values to the right side.
                    ushort swap = index[l];
                    index[l] = index[r];
                    index[r] = swap;

                    l++;
                    r--;
                }
            }
            // Here's where the pivot value is in the right spot

            // 7. Recursively call the algorithm on the unsorted array 
            //    to the left of the pivot (if exists).
            if (left < r) quicksort(array, index, left, r);

            // 8. Recursively call the algorithm on the unsorted array 
            //    to the right of the pivot (if exists).
            if (l < right) quicksort(array, index, l, right);

            // 9. The algorithm returns when all sub arrays are sorted.
        }


        // Copyright 2001, softSurfer (www.softsurfer.com)
        // This code may be freely used and modified for any purpose
        // providing that this copyright notice is included with it.
        // SoftSurfer makes no warranty for this code, and cannot be held
        // liable for any real or imagined damage resulting from its use.
        // Users of this code must verify correctness for their application.

        // Assume that a class is already given for the object:
        //    Point with coordinates {float x, y;}
        //===================================================================

        // isLeft(): tests if a point is Left|On|Right of an infinite line.
        //    Input:  three points P0, P1, and P2
        //    Return: >0 for P2 left of the line through P0 and P1
        //            =0 for P2 on the line
        //            <0 for P2 right of the line
        //    See: the January 2001 Algorithm on Area of Triangles

        private static float isLeft(Vector3f P0, Vector3f P1, Vector3f P2)
        {
            return (P1.x - P0.x) * (P2.z - P0.z) - (P2.x - P0.x) * (P1.z - P0.z);
        }
        //===================================================================
        // chainHull_2D(): Andrew's monotone chain 2D convex hull algorithm
        //     Input:  P[] = an array of 2D points 
        //                   presorted by increasing x- and y-coordinates
        //             n = the number of points in P[]
        //     Output: H[] = an array of the convex hull vertices (max is n)
        //     Return: the number of points in H[]
        public static int chainHull_2D(Vector3f[] P, ref Vector3f[] H, bool needresort)
        {
            // the output array H[] will be used as the stack
            int bot = 0, top = (-1);  // indices for bottom and top of the stack
            int i;                // array scan index
            // Get the indices of points with min x-coord and min|max y-coord
            int minmin = 0, minmax;

            int N = P.Length;
            ushort[] index = new ushort[N];
            for (i = 0; i < N; i++) index[i] = (ushort)i;


            if (needresort)
            {
                quicksort(P, index, 0, N - 1);
            }


            float xmin = P[0].x;
            for (i = 1; i < N; i++)
                if (P[i].x != xmin) break;
            minmax = i - 1;
            if (minmax == N - 1)
            {       // degenerate case: all x-coords == xmin
                H[++top] = P[minmin];
                if (P[minmax].z != P[minmin].z) // a nontrivial segment
                    H[++top] = P[minmax];
                H[++top] = P[minmin];           // add polygon endpoint
                return top + 1;
            }

            // Get the indices of points with max x-coord and min|max y-coord
            int maxmin, maxmax = N - 1;
            float xmax = P[N - 1].x;
            for (i = N - 2; i >= 0; i--)
                if (P[i].x != xmax) break;
            maxmin = i + 1;

            // Compute the lower hull on the stack H
            H[++top] = P[minmin];      // push minmin point onto stack
            i = minmax;
            while (++i <= maxmin)
            {
                // the lower line joins P[minmin] with P[maxmin]
                if (isLeft(P[minmin], P[maxmin], P[i]) >= 0 && i < maxmin)
                    continue;          // ignore P[i] above or on the lower line

                while (top > 0)        // there are at least 2 points on the stack
                {
                    // test if P[i] is left of the line at the stack top
                    if (isLeft(H[top - 1], H[top], P[i]) > 0)
                        break;         // P[i] is a new hull vertex
                    else
                        top--;         // pop top point off stack
                }
                H[++top] = P[i];       // push P[i] onto stack
            }

            // Next, compute the upper hull on the stack H above the bottom hull
            if (maxmax != maxmin)      // if distinct xmax points
                H[++top] = P[maxmax];  // push maxmax point onto stack
            bot = top;                 // the bottom point of the upper hull stack
            i = maxmin;
            while (--i >= minmax)
            {
                // the upper line joins P[maxmax] with P[minmax]
                if (isLeft(P[maxmax], P[minmax], P[i]) >= 0 && i > minmax)
                    continue;          // ignore P[i] below or on the upper line

                while (top > bot)    // at least 2 points on the upper stack
                {
                    // test if P[i] is left of the line at the stack top
                    if (isLeft(H[top - 1], H[top], P[i]) > 0)
                        break;         // P[i] is a new hull vertex
                    else
                        top--;         // pop top point off stack
                }
                H[++top] = P[i];       // push P[i] onto stack
            }
            if (minmax != minmin)
                H[++top] = P[minmin];  // push joining endpoint onto stack

            return top + 1;
        }
    }

    /// <summary>
    /// Find minimum distance rotation with "rotation caliper algoritm"
    /// </summary> 
    public static class RotationCaliper
    {
        public static Matrix4x4f GetMinRotation(Vector3f[] hull)
        {
            throw new NotImplementedException();
        }
    }

}
