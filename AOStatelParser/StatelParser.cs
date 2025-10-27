using System;
using System.Collections.Generic;
using System.IO;
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

        using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(path)))
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