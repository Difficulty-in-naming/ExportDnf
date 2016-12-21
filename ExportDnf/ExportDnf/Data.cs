namespace ExportDnf
{
    public class NpkHeader
    {
        public int Count; //包内文件的数目
        public string Flag; //文件标识
    }

    public class NpkIndex
    {
        public string Name;
        public uint Offset;
        public uint Size;
    }

    public class NImgFHeader
    {
        public string Flag; // 文件标石"Neople Img File"
        public int IndexCount; // 索引表数目
        public int IndexSize; // 索引表大小，以字节为单位
        public int Unknown1;
        public int Unknown2;
    }

    public class NImgFIndex
    {
        public uint DwCompress; // 目前已知的类型有 0x06(zlib压缩) 0x05(未压缩)
        public uint DwType; //目前已知的类型有 0x0E(1555格式) 0x0F(4444格式) 0x10(8888格式) 0x11(不包含任何数据，可能是指内容同上一帧)
        public int Height; // 高度
        public int KeyX; // X关键点，当前图片在整图中的X坐标
        public int KeyY; // Y关键点，当前图片在整图中的Y坐标
        public int MaxHeight; // 整图的高度，有此数据是为了对齐精灵
        public int MaxWidth; // 整图的宽度
        public int Size; // 压缩时size为压缩后大小，未压缩时size为转换成8888格式时占用的内存大小
        public int Width; // 宽度
    }

}