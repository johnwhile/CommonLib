using System.IO;
using System.Xml;

namespace Common.IO
{
    public interface IBinarySerializable
    {
        bool Read(BinaryReader reader);
        bool Write(BinaryWriter writer);
    }
    public interface IXmlSerializable
    {
        bool Read(XmlReader reader);
        bool Write(XmlWriter writer);
    }
}
