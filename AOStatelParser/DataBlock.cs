using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

public class DataBlock
{
    public long Offset { get; set; }
    public long CompressedSize { get; set; }
    public long DecompressedSize { get; set; }
    public string Format { get; set; } = "raw";
    public int Depth { get; set; }
    public bool Success { get; set; }
    public List<DataBlock> Children { get; } = new();
}

public static class CompressionAnalyzer
{
    public static List<DataBlock> Analyze(byte[] data, int maxDepth = 3)
    {
        var blocks = new List<DataBlock>();
        AnalyzeLayer(data, 0, 0, maxDepth, blocks);
        return blocks;
    }

    private static void AnalyzeLayer(
        byte[] data,
        int depth,
        long baseOffset,
        int maxDepth,
        List<DataBlock> output)
    {
        if (depth >= maxDepth || data.Length < 4)
            return;

        int i = 0;
        while (i < data.Length)
        {
            bool found = false;
            foreach (var mode in new[] { "gzip", "zlib", "deflate" })
            {
                try
                {
                    using var input = new MemoryStream(data, i, data.Length - i);
                    Stream? ds = mode switch
                    {
                        "gzip" => new GZipStream(input, CompressionMode.Decompress, leaveOpen: true),
                        "zlib" => new ZLibStream(input, CompressionMode.Decompress, leaveOpen: true),
                        "deflate" => new DeflateStream(input, CompressionMode.Decompress, leaveOpen: true),
                        _ => null
                    };

                    using var temp = new MemoryStream();
                    ds!.CopyTo(temp);
                    byte[] decompressed = temp.ToArray();
                    int used = (int)input.Position;
                    if (used <= 0) used = 1;

                    var block = new DataBlock
                    {
                        Offset = baseOffset + i,
                        CompressedSize = used,
                        DecompressedSize = decompressed.Length,
                        Format = mode,
                        Depth = depth,
                        Success = decompressed.Length > 0
                    };
                    output.Add(block);

                    if (decompressed.Length > 0 && depth + 1 < maxDepth)
                    {
                        AnalyzeLayer(decompressed, depth + 1, block.Offset, maxDepth, block.Children);
                    }

                    i += used;
                    found = true;
                    break;
                }
                catch { }
            }

            if (!found)
            {
                output.Add(new DataBlock
                {
                    Offset = baseOffset + i,
                    CompressedSize = 1,
                    DecompressedSize = 1,
                    Format = "raw",
                    Depth = depth,
                    Success = true
                });
                i++;
            }
        }
    }

    public static void PrintReport(List<DataBlock> blocks, int indent = 0)
    {
        string pad = new string(' ', indent * 2);
        foreach (var b in blocks)
        {
            Console.WriteLine($"{pad}[{b.Offset:D6}] {b.Format,-8} | C={b.CompressedSize,6} | D={b.DecompressedSize,6} | {(b.Success ? "OK" : "FAIL")}");
            if (b.Children.Count > 0)
                PrintReport(b.Children, indent + 1);
        }
    }
}