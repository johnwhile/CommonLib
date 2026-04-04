using Common;
using Common.Maths;
using Common.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace Test_Code
{
    /// <summary>
    /// Semplified and Filter scene tree, contains only relevant meshes
    /// </summary>
    public class EmpyrionModel
    {
        public readonly static long EmpSignature = BitConverterExt.ToInt64("EMPMODEL");

        public readonly SceneTree Tree;

        public string Name => Tree.Name;

        public EmpyrionModel(BinaryReader reader)
        {
            Tree = new SceneTree();
            Read(reader);
        }

        public bool Read(BinaryReader reader)
        {
            long signature = reader.ReadInt64();
            long bytesize = reader.ReadInt64();

            if (signature != EmpSignature) throw new Exception("Wrong header for Empyrion EmpyrionModel class");

            if (!Tree.Read(reader)) return false;

            for (int i = 0; i < Tree.ElementsCount; i++)
            {
                //read only mesh classes
                if (reader.ReadBoolean())
                {
                    (long meshsignature, long meshsize) = Mesh.ReadHeaderAndBack(reader);
                    if (meshsignature != TriMesh.TriMeshSignature &&
                        meshsignature != Mesh.MeshSignature)
                        throw new Exception("Can't read unknow elements, must be a mesh");

                    var tmesh = new TriMesh();
                    tmesh.Read(reader);
                    Tree.Element[i] = tmesh;
                }
                else
                {
                    Tree.Element[i] = null;
                }
            }

            return true;
        }

        public bool Write(BinaryWriter writer, TransformVersion matversion = TransformVersion.Float16)
        {
            long begin = writer.BaseStream.Position;
            writer.WriteLong(EmpSignature);
            writer.WriteLong();
            //write the tree
            if (!Tree.Write(writer, matversion)) return false;

            //write the meshes
            for (int i = 0; i < Tree.ElementsCount; i++)
            {
                //write only mesh classes
                if (Tree.Element[i] is TriMesh mesh)
                {
                    writer.Write(true);
                    mesh.Version.Major = 2;
                    mesh.Write(writer, 
                        CompressionTransform.MatrixTRS,
                        CompressionIndices.None, 
                        CompressionVertices.None,
                        CompressionNormals.Normals24, 
                        CompressionTexCoord.None,
                        CompressionColor.None,
                        CompressionTangents.Normals16_WeightInt16);
                }
                else
                {
                    writer.Write(false);
                }
            }
            long end = writer.BaseStream.Position;
            writer.BaseStream.Position = begin + 8;
            writer.WriteLong(end - begin);
            writer.BaseStream.Position = end;
            return true;
        }
    }

    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var directory = @"C:\Users\Administrator\Desktop\Shapes2";
            var directory2 = @"C:\Users\Administrator\Desktop\Shapes2_converted";
            var filenames = Directory.GetFiles(directory);

            foreach(var filename in filenames)
            {
                EmpyrionModel model;
                using (var file = File.OpenRead(filename))
                using (var reader = new BinaryReader(file))
                {
                    model = new EmpyrionModel(reader);
                }

                var filename_converted = Path.Combine(directory2, Path.GetFileName(filename));

                using (var file = File.OpenWrite(filename_converted))
                using (var writer = new BinaryWriter(file))
                {
                    model.Write(writer);
                }
            }
        }
    }
}
