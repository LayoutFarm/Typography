//BSD, 2014-present, WinterDev

using System.IO;
using PixelFarm.Drawing;
namespace PixelFarm.CpuBlit.VertexProcessing
{
#if DEBUG
    public class dbugVertexSourceIO
    {
        public static void Load(PathWriter vertexSource, string pathAndFileName)
        {
            //vertexSource.Clear();
            //var vxs = vertexSource.Vxs;
            //string[] allLines = File.ReadAllLines(pathAndFileName);
            //foreach (string line in allLines)
            //{
            //    string[] elements = line.Split(',');
            //    double x = double.Parse(elements[0]);
            //    double y = double.Parse(elements[1]);
            //    VertexCmd flagsAndCommand = (VertexCmd)System.Enum.Parse(typeof(VertexCmd), elements[2].Trim());
            //    int len = elements.Length;
            //    for (int i = 3; i < len; i++)
            //    {
            //        flagsAndCommand |= (VertexCmd)System.Enum.Parse(typeof(VertexCmd), elements[i].Trim());
            //    }
            //    vxs.AddVertex(x, y, flagsAndCommand);
            //}
        }


        //-------------------------------------------------
        public static void WriteToStream(BinaryWriter writer, PathWriter pathSource)
        {
            int num_vertice;
            int num_alloc_vertice;
            double[] coord_xy;
            byte[] cmds;
            PathWriter.UnsafeDirectGetData(pathSource,
                out num_alloc_vertice,
                out num_vertice,
                out coord_xy,
                out cmds);
            //write to binary format ?
            //1.  
            writer.Write(num_alloc_vertice);//hint
            //2. 
            writer.Write(num_vertice); //actual vertices
            //3. all vertice
            int totalCoord = num_vertice << 1;
            for (int i = 0; i < totalCoord; )
            {
                writer.Write(coord_xy[i]);//x
                i++;
                writer.Write(coord_xy[i]);//y
                i++;
            }
            writer.Write(num_vertice); //actual vertices
            //4. all commands
            for (int i = 0; i < num_vertice; ++i)
            {
                writer.Write((byte)cmds[i]);
            }
            writer.Write((int)0);
            writer.Flush();
            //------------------------------------
        }
        public static void WriteColorsToStream(BinaryWriter writer, Color[] colors)
        {
            int len = colors.Length;
            //1.
            writer.Write(len);
            for (int i = 0; i < len; ++i)
            {
                Color color = colors[i];
                writer.Write(color.red);
                writer.Write(color.green);
                writer.Write(color.blue);
                writer.Write(color.alpha);
            }
            writer.Write((int)0);
            writer.Flush();
        }
        public static void WritePathIndexListToStream(BinaryWriter writer, int[] pathIndice, int len)
        {
            //1.
            writer.Write(len);
            for (int i = 0; i < len; ++i)
            {
                writer.Write(pathIndice[i]);
            }
            writer.Write((int)0);
            writer.Flush();
        }
        public static void ReadPathDataFromStream(BinaryReader reader, out PathWriter newPath)
        {
            newPath = new PathWriter();
            //1.
            int num_alloc_vertice = reader.ReadInt32();//hint
            //2.
            int num_vertice = reader.ReadInt32();//actual vertice num
            int totalCoord = num_vertice << 1;
            //3.
            double[] coord_xy = new double[totalCoord];
            //4.
            byte[] cmds = new byte[num_vertice];
            for (int i = 0; i < totalCoord; )
            {
                coord_xy[i] = reader.ReadDouble();
                i++;
                coord_xy[i] = reader.ReadDouble();
                i++;
            }
            //4.
            int cmds_count = reader.ReadInt32();
            for (int i = 0; i < cmds_count; ++i)
            {
                cmds[i] = reader.ReadByte();
            }

            PathWriter.UnsafeDirectSetData(
                newPath,
                num_alloc_vertice,
                num_vertice,
                coord_xy,
                cmds);
            int end = reader.ReadInt32();
        }

        public static void ReadColorDataFromStream(BinaryReader reader, out Color[] colors)
        {
            int len = reader.ReadInt32();
            colors = new Color[len];
            for (int i = 0; i < len; ++i)
            {
                byte r = reader.ReadByte();
                byte g = reader.ReadByte();
                byte b = reader.ReadByte();
                byte a = reader.ReadByte();
                colors[i] = new Color(a, r, g, b);
            }
            int end = reader.ReadInt32();
        }
        public static void ReadPathIndexListFromStream(BinaryReader reader, out int len, out int[] pathIndexList)
        {
            len = reader.ReadInt32();
            pathIndexList = new int[len];
            for (int i = 0; i < len; ++i)
            {
                pathIndexList[i] = reader.ReadInt32();
            }
            int end = reader.ReadInt32();
        }
    }
#endif
}
