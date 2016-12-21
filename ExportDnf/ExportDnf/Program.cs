using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using Ionic.Zlib;

namespace ExportDnf
{
    class Program
    {
        public const uint Argb1555 = 0x0e;
        public const uint Argb4444 = 0x0f;
        public const uint Argb8888 = 0x10;
        public const uint ArgbNone = 0x11;
        public const uint CompressZlib = 0x06;
        public const uint CompressNone = 0x05;
        public static StringBuilder DecordFlag = new StringBuilder(256);
        public static string Path = @"E:\sss\SuperDnf\Plugins\Assets\ExportDnf\ExportDnf\res";
        public static string SavePath = @"E:\sss\SuperDnf\Npk";
        static void Main(string[] args)
        {
            DecordFlag.Append("puchikon@neople dungeon and fighter DNF");
            int len = DecordFlag.Length;
            for (int i = len; i < 256; i++)
            {
                if ((i - len) % 3 == 0)
                    DecordFlag.Append('D');
                else if ((i - len) % 3 == 1)
                    DecordFlag.Append('N');
                else if ((i - len) % 3 == 2)
                    DecordFlag.Append('F');
            }
            DecordFlag[DecordFlag.Length - 1] = ' ';
            string[] files = Directory.GetFiles(Path, "*.NPK", SearchOption.AllDirectories);
            foreach (string node in files)
            {
                ExtractNpk(node, false);
            }
            Console.Write("按任意键退出...");
            Console.ReadKey(true);
        }


        public static void ExtractNpk(string file, bool onlyImg)
        {
            Debug.Log("当前破解NPK文件" + file + "...");
            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                NpkHeader header = new NpkHeader();
                header.Flag = fs.ReadBytes<string>(16);
                header.Count = fs.ReadBytes<int>(4);
                List<NpkIndex> allFileIndex = new List<NpkIndex>();
                for (int i = 0; i < header.Count; i++)
                {
                    NpkIndex index = new NpkIndex
                    {
                        Offset = fs.ReadBytes<uint>(4),
                        Size = fs.ReadBytes<uint>(4)
                    };
                    char[] temp = fs.ReadBytes<string>(256).ToCharArray();
                    char[] decodeByte = DecordFlag.ToString().ToCharArray();
                    for (int t = 0; t < temp.Length; t++)
                    {
                        index.Name += (char) (temp[t] ^ decodeByte[t]);
                    }
                    allFileIndex.Add(index);
                }
                foreach (NpkIndex node in allFileIndex)
                {
                    fs.Seek(node.Offset, SeekOrigin.Begin);
                    if (onlyImg)
                    {
/*                        char[] temp = new char[node.Size];
                        fs.Read(bytes = new byte[node.Size], 0, (int) node.Size);
                        if (Directory.Exists(node.Name))
                            Directory.Delete(node.Name);
                        Directory.CreateDirectory(node.Name);*/
                    }
                    else
                    {
                        extract_img_npk(fs, node.Offset, node.Name, file);
                    }
                }
                fs.Close();
            }
        }

        static void extract_img_npk(FileStream fs, uint offset, string file, string npkFile)
        {
            Debug.Log("破解中" + file.Replace("\0","") + "...");
            NImgFHeader header = new NImgFHeader();
            header.Flag = fs.ReadBytes<string>(16);
            header.IndexSize = fs.ReadBytes<int>(4);
            header.Unknown1 = fs.ReadBytes<int>(4);
            header.Unknown2 = fs.ReadBytes<int>(4);
            header.IndexCount = fs.ReadBytes<int>(4);
            if (header.Flag != "Neople Img File\0")
            {
                Debug.Log("错误的标头 " + header.Flag + "  in file" + file);
                return;
            }
            List<NImgFIndex> allFileIndex = new List<NImgFIndex>();
            for (int i = 0; i < header.IndexCount; i++)
            {
                NImgFIndex index = new NImgFIndex
                {
                    DwType = fs.ReadBytes<uint>(4),
                    DwCompress = fs.ReadBytes<uint>(4)
                };
                if (i > 100 && index.DwCompress == i)
                {
                    index.DwCompress = 6; //部分皮肤文件无法导出开启这个
                }
                if (index.DwType == ArgbNone)
                {
                    allFileIndex.Add(index);
                    continue;
                }
                index.Width = fs.ReadBytes<int>(4);
                index.Height = fs.ReadBytes<int>(4);
                index.Size = fs.ReadBytes<int>(4);
                index.KeyX = fs.ReadBytes<int>(4);
                index.KeyY = fs.ReadBytes<int>(4);
                index.MaxHeight = fs.ReadBytes<int>(4);
                index.MaxWidth = fs.ReadBytes<int>(4);
                allFileIndex.Add(index);
            }
            byte[] tempFileData = {};
            byte[] tempZlibData = {};
            fs.Seek(offset + header.IndexSize + 32, SeekOrigin.Begin);
            string filePath;
            try
            {
                 filePath = file.Substring(0, file.LastIndexOf(".")).Replace(" ","");
            }
            catch
            {
                return;
            }
            Directory.CreateDirectory(filePath);
            int index2 = -1;
            string lastName = filePath.Substring(filePath.LastIndexOf("/")).Replace(" ", "");
            //检查数据文件是否存在
            if(File.Exists(filePath + "/" + lastName + ".txt"))
                File.Delete(filePath + "/" + lastName + ".txt");
            for (int index = 0; index < allFileIndex.Count; index++)
            {
                NImgFIndex node = allFileIndex[index];
                index2++;
                string path = filePath + "/" + lastName + "_";
                if (node.DwType == ArgbNone)
                {
                    if (File.Exists(path + node.DwCompress.ToString().PadLeft(3, '0') + ".png"))
                        File.Copy(path + node.DwCompress.ToString().PadLeft(3, '0') + ".png", path + index2.ToString().PadLeft(3,'0') + ".png", true);
                    WriteData(allFileIndex[(int) node.DwCompress], filePath, lastName);
                    continue;
                }
                tempFileData = fs.ReadBytes<byte[]>(node.Size);
                if (node.DwCompress == CompressZlib)
                {
                    try
                    {
                        tempZlibData = ZlibStream.UncompressBuffer(tempFileData);
                    }
                    catch
                    {
                        fs.Position -= node.Size;
                        tempZlibData = fs.ReadBytes<byte[]>(node.Width * node.Height * (int)Argb1555);
                    }
                }
                else if (node.DwCompress == CompressNone)
                {
                    tempZlibData = tempFileData;
                }
                ConvertToPng(path + index2.ToString().PadLeft(3, '0') + ".png", node, tempZlibData);
                WriteData(node, filePath, lastName);
            }
        }

        private static void WriteData(NImgFIndex node, string filePath, string lastName)
        {
            string data = node.KeyX + "," + node.KeyY + "," + node.Width + "," + node.Height + "," +
                          node.MaxWidth + "," + node.MaxHeight + ",\n";
            File.AppendAllText(filePath + "/" + lastName + ".txt", data);
        }

        static void ConvertToPng(string fileName, NImgFIndex index, byte[] data)
        {
            var width = index.Width;
            var height = index.Height;
            var type = index.DwType;
            Bitmap sPicture = new Bitmap(index.MaxHeight, index.MaxWidth, PixelFormat.Format32bppArgb);
            using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                Graphics g = Graphics.FromImage(sPicture);
                g.Clear(Color.FromArgb(0));
                for (int i = 0; i < height; ++i)
                {
                    for (int j = 0; j < width; ++j)
                    {
                        switch (type)
                        {
                            case Argb1555:
                                g.DrawRectangle(new Pen(Color.FromArgb(data[i*width*2 + j*2 + 1] >> 7 == 0 ? 0 : 255,
                                    ((data[i*width*2 + j*2 + 1] & 127) >> 2) << 3,
                                    (((data[i*width*2 + j*2 + 1] & 0x0003) << 3) |
                                     ((data[i*width*2 + j*2] >> 5) & 0x0007)) << 3,
                                    ((data[i*width*2 + j*2] & 0x003f) << 3).LoopNumber(0, 255))), index.KeyX + j, index.KeyY + i, 0.5f, 0.5f);
                                break;
                            case Argb4444:
                                g.DrawRectangle(new Pen(Color.FromArgb(((data[i*width*2 + j*2 + 1] & 0xf0) >> 4) << 4,
                                    (data[i*width*2 + j*2 + 1] & 0x0f) << 4,
                                    ((data[i*width*2 + j*2 + 0] & 0xf0) >> 4) << 4,
                                    (data[i*width*2 + j*2 + 0] & 0x0f) << 4)),
                                    index.KeyX + j, index.KeyY + i, 0.5f, 0.5f);
                                break;
                            case Argb8888:
                                g.DrawRectangle(new Pen(Color.FromArgb(data[i*width*4 + j*4 + 3],
                                    data[i*width*4 + j*4 + 2],
                                    data[i*width*4 + j*4 + 1],
                                    data[i*width*4 + j*4 + 0])),
                                    index.KeyX + j, index.KeyY + i, 0.5f, 0.5f);
                                break;
                        }
                    }
                }
                g.Save();
                g.Dispose();
            }
            if(!File.Exists(fileName))
                Console.WriteLine("hahahahah");
            sPicture.Save(fileName.Replace(" ",""), ImageFormat.Png);

        }
    }
}