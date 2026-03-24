using System;
using Common.Maths;

namespace Common.Geometry
{
    /// <summary>
    /// Define base informations what a generic geometry must have
    /// </summary>
    public abstract class BaseGeometry2D
    {
        protected Matrix3x3f globalcoord;
        protected Matrix3x3f globalcoord_inv;

        /// <summary>
        /// need a basic volume information example to understand che center of mesh, the value are in LOCAL space.
        /// The only remark if for instance nodes, the bounding sphere is stored in the main node.
        /// Need to be updated when you want.
        /// </summary>
        public Sphere boundSphere;


        public BaseGeometry2D()
            : this("BaseGeometry")
        {
        }
        /// <summary>
        /// </summary>
        public BaseGeometry2D(string name)
        {
            globalcoord = globalcoord_inv = Matrix4x4f.Identity;
            boundSphere = Sphere.NaN;
            this.name = name;
        }

        /// <summary>
        /// Copy primitivetype,bsphere,world and name
        /// </summary>
        public BaseGeometry2D(BaseGeometry2D src)
        {
            boundSphere = src.boundSphere;
            globalcoord = src.globalcoord;
            globalcoord_inv = src.globalcoord_inv;
            name = src.name;
        }

        /// <summary>
        /// Need to know if i will use indexbuffer
        /// </summary>
        public abstract bool IsIndexed { get; }
        /// <summary>
        /// The number of vertices
        /// </summary>
        public abstract int numVertices { get; }
        /// <summary>
        /// The number of primitives, depend by primitive you are using
        /// </summary>
        public abstract int numPrimitives { get; }
        /// <summary>
        /// The number of indices, Face and Edged are primitive and contain respectively 3 and 2 indices
        /// </summary>
        public abstract int numIndices { get; }
        /// <summary>
        /// changing the transfrom matrix without affect vertices position in the world space. The vertices
        /// and normals are trasformed in world space and re-trasformed in the new local space
        /// </summary>
        public virtual void changeTransform(Matrix3x3f newtransform) { }
        /// <summary>
        /// you can set a string to debug
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Is the transformation of this node from 3d world root
        /// </summary>
        public Matrix3x3f transform
        {
            get { return globalcoord; }
            set { globalcoord = value; globalcoord_inv = Matrix3x3f.Inverse(value); }
        }
        /// <summary>
        /// When necessary is usefull to have a inverse matrix calculated only when necessary
        /// </summary>
        public Matrix3x3f transform_inv
        {
            get { return globalcoord_inv; }
        }
    }



}
