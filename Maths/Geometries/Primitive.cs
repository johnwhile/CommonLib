using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Maths
{
    /// <summary>
    /// Primitive Topology. Not match with Directx11, example TriangleFan are no longer supported by Dx10
    /// </summary>
    public enum Primitive : byte
    {
        Undefined = 0,
        /// <summary>
        /// <code>v0 v1 v1 v2 ...</code>
        /// </summary>
        Point = 1,
        /// <summary>
        /// <code>v0──v1 v1──v2 ...</code>
        /// </summary>
        LineList = 2,
        /// <summary>
        /// <code>v0──v1──v2 ...</code>
        /// </summary>
        LineStrip = 3,
        /// <summary>
        /// <code>
        /// v0───v1  v2───v3 ...
        ///   \ /      \ /
        ///    v2       v4
        /// </code>
        /// </summary>
        TriangleList = 4,
        /// <summary>
        /// <code>
        /// v1───v2───v5...
        /// │  / │  / │
        /// | /  | /  |
        /// │/___│/___│ ...
        /// v3    v4   v6
        /// </code>
        /// </summary>
        TriangleStrip = 5,
        /// <summary>
        /// <code>
        /// v1─────v2
        /// │ \'.  │ 
        /// |  \ ',v3
        /// │___\/
        /// v5  v4
        /// </code>
        /// </summary>
        TriangleFan = 6,
    }
}
