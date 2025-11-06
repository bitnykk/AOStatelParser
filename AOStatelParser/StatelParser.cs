using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class StatelParser
{
    public List<Statel> Statels = new List<Statel>();

    public StatelParser(string aoBasePath, int pf)
    {
        Parse(aoBasePath, pf);
    }    

        private void Parse(string aoBasePath, int pf)
    {
        string path = $"{aoBasePath}\\cd_image\\data\\statels\\{pf}.pf";

        if (!File.Exists(path))
            return;

        var rawData = File.ReadAllBytes(path);
        var readData = rawData;
        int[] rawArray = { 101, 102, 103, 104, 105, 106, 107, 109, 110, 111, 1410, 3000, 3001, 3002, 3003, 3004, 3005, 3006, 3007, 3008, 3009, 3010, 3011, 3012, 3013, 3014, 3015, 3016, 3017, 3018, 3019, 3020, 3021, 3022, 3023, 3024, 3025, 3026, 3027, 3028, 3029, 3030, 3031, 3032, 3033, 3034, 3035, 3036, 3037, 3038, 3039, 3040, 3041, 3042, 3043, 3044, 3045, 3046, 3047, 3048, 3049, 3050, 3051, 3052, 3053, 3054, 3055, 3056, 3057, 3058, 3059, 3060, 3061, 3062, 3063, 3064, 3065, 3066, 3067, 3068, 3069, 3070, 3071, 3080, 3081, 3082, 3083, 3084, 3085, 3086, 3087, 3088, 3089, 3100, 3101, 3102, 3103, 3104, 3105, 3106, 3107, 3108, 3109, 3110, 3111, 3112, 3113, 3114, 3115, 3116, 3117, 3120, 3121, 3122, 3123, 3124, 3125, 3126, 3127, 3128, 3129, 3130, 3131, 3132, 3133, 3134, 3135, 3136, 3137, 3138, 3139, 3140, 3141, 3142, 3143, 3144, 3145, 3146, 3147, 3148, 3149, 4001, 4005, 4006, 4010, 4011, 4310, 4312, 4313, 4320, 4322, 4328, 4329, 4330, 4335, 4336, 4337, 4365, 4366, 4367, 4368, 4370, 4374, 4380, 4381, 4382, 4383, 4384, 4385, 4386, 4387, 4388, 4389, 4390, 4468, 4474, 4475, 4530, 4531, 4532, 4533, 4534, 4542, 4543, 4544, 4582, 4605, 4677, 4680, 4681, 4683, 4686, 4687, 4690, 4691, 4694, 4695, 4697, 4699, 4872, 4873, 4894, 4898, 4899, 4900, 500, 5001, 5002, 501, 540, 545, 550, 551, 556, 565, 566, 567, 570, 585, 586, 590, 596, 6001, 6002, 6003, 6010, 6011, 6012, 6013, 6014, 6017, 6020, 6021, 6022, 6035, 6036, 6041, 6050, 6051, 6052, 6055, 6061, 6071, 6101, 6102, 6104, 615, 6300, 6301, 6302, 6303, 6304, 6305, 6306, 635, 640, 641, 646, 650, 6550, 6551, 656, 685, 687, 696, 700, 7011, 7012, 7013, 7015, 705, 706, 710, 730, 740, 790, 791, 795, 800, 8002, 8009, 8040, 8045, 8046, 8050, 9042, 950, 952, 953, 954, 955 };
        int[] oneArray = { 1002, 1005, 1137, 1183, 1186, 1190, 1211, 128, 1322, 1329, 1404, 1421, 1504, 1505, 152, 1646, 1673, 1703, 1712, 1741, 1742, 1743, 1893, 2010, 2011, 2020, 2021, 2032, 2033, 2063, 2096, 4318, 4327, 4331, 4334, 4350, 4376, 4391, 4561, 4704, 6024, 6056, 6057, 6060, 6125, 6127, 7101, 8070, 9080 };
        if (rawArray.Contains(pf) == false) {
            if (oneArray.Contains(pf) == true)
            {
                var (allData, decompressedParts) = RecursiveDecompressor.ExtractAndDecompressRecursive(rawData);
                readData = decompressedParts;
            } else
            {
                var (allData, decompressedParts) = RecursiveDecompressor.ExtractAndDecompressRecursive(rawData, 2);
                readData = decompressedParts;
            }            
        }
        using (MemoryStream ms = new MemoryStream(readData))
        {
            using (BinaryReader reader = new BinaryReader(ms))
            {
                List<int> statelOffsets = new List<int>();

                int statelOffset = reader.ReadInt32();
                while (statelOffsets.Count == 0 || statelOffsets.Last() < statelOffset)
                {
                    statelOffsets.Add(statelOffset);
                    statelOffset = reader.ReadInt32();
                }

                reader.BaseStream.Position -= 4;

                ParseFirstBlock(reader);

                for (int i = 1; i < statelOffsets.Count; i++)
                {
                    //uint len = 0; //Is this even used?
                    uint len = (uint)((i == statelOffsets.Count - 1) ? -1 : (statelOffsets[i + 1] - statelOffsets[i]));
                    if (len < 0)
                        continue;

                    ParseSet(reader, statelOffsets[i], len);
                }
            }
        }
    }

    private void ParseFirstBlock(BinaryReader reader)
    {
        int firstBlockLength = reader.ReadInt32();
        ParseBuilding(reader);
        ParseBuilding(reader);
        ParseShortBuilding(reader);
    }

    private void ParseSet(BinaryReader reader, int pos, uint len)
    {
        reader.BaseStream.Position = pos;

        int extraShorts = reader.ReadUInt16();
        for (int i = 0; i < extraShorts; i++)
        {
            int extra = reader.ReadUInt16();
        }

        // read building-sub-blocks (5x times)
        for (int i = 0; i < 5; i++)
        {
            ParseBuilding(reader);
        }

        // check if block is read fully
        //if (Reader.Index != pos + len)
        //throw new Exception("Reader.Index(" + Reader.Index + ")  != pos + len(" + (pos+len) + ")" );
    }

    private void ParseShortBuilding(BinaryReader reader)
    {
        int nrOfBuildings = reader.ReadUInt16();
        for (int i = 0; i < nrOfBuildings; i++)
        {
            Vector3 p = new Vector3(reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle());
            reader.ReadByte();
            reader.ReadByte();
            reader.ReadByte();
            reader.ReadByte();
            reader.ReadByte();
            int byte6 = reader.ReadByte();
            if (byte6 > 0)
                reader.BaseStream.Position += (byte6 + 1) * 4;
        }
    }

    private void ParseBuilding(BinaryReader reader)
    {
        int nrOfBuildings1 = reader.ReadUInt16();
        for (int i = 0; i < nrOfBuildings1; i++)
        {
            List<uint> unks = new List<uint>();
            Vector3 pos = new Vector3(reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle());
            reader.ReadByte();
            Vector3 rot = PopRot(reader);

            uint buldingId = reader.ReadUInt32();

            if (buldingId > 300000)
            {
                byte scale = reader.ReadByte();
                byte byte6 = reader.ReadByte();
                reader.BaseStream.Position += 6;
            }
            else
            {
                byte scale = reader.ReadByte();
                byte byte6 = reader.ReadByte();
                if (byte6 > 0)
                {
                    for (int o = 0; o < byte6 + 1; o++)
                        unks.Add(reader.ReadUInt32());
                }

                Statel Statel = new Statel();
                Statel.Pos = pos;
                Statel.Rot = rot;
                Statel.Scale = scale;
                Statel.Id = buldingId;
                Statel.TextureOverrides = unks;

                Statels.Add(Statel);
            }
        }
    }

    private Vector3 PopRot(BinaryReader reader)
    {
        var data = new byte[] { reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), 0 };
        uint Raw = BitConverter.ToUInt32(data, 0);
        Vector3 Rot;
        Rot.z = Raw / 32400;
        Rot.y = (Raw % 32400) / 90;
        Rot.x = ((Raw % 32400) % 90) * 2;
        return Rot;
    }
}

public class Statel
{
    public Vector3 Pos;
    public Vector3 Rot;
    public byte Scale;
    public uint Id;
    public List<uint> TextureOverrides;

    public Statel()
    {
        TextureOverrides = new List<uint>();
    }
}