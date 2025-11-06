using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;

public static class RecursiveDecompressor
{
    /// <summary>
    /// (GZIP, ZLIB, DEFLATE)
    /// </summary>
    public static (byte[] allData, byte[] decompressedParts)
        ExtractAndDecompressRecursive(byte[] data, int maxDepth = 1)
    {
        using var allDataStream = new MemoryStream();
        using var decompressedStream = new MemoryStream();

        ProcessLayer(data, allDataStream, decompressedStream, 0, maxDepth);

        return (allDataStream.ToArray(), decompressedStream.ToArray());
    }

    private static void ProcessLayer(
        byte[] data,
        MemoryStream allOut,
        MemoryStream decOut,
        int depth,
        int maxDepth)
    {
        if (depth >= maxDepth || data.Length < 4)
        {
            allOut.Write(data, 0, data.Length);
            return;
        }

        int i = 0;
        while (i < data.Length)
        {
            bool found = false;

            foreach (var mode in new[] { "gzip", "zlib", "deflate" })
            {
                try
                {
                    using var inputStream = new MemoryStream(data, i, data.Length - i);
                    Stream? ds = mode switch
                    {
                        "gzip" => new GZipStream(inputStream, CompressionMode.Decompress, leaveOpen: true),
                        "zlib" => new ZLibStream(inputStream, CompressionMode.Decompress, leaveOpen: true),
                        "deflate" => new DeflateStream(inputStream, CompressionMode.Decompress, leaveOpen: true),
                        _ => null
                    };

                    using var tempStream = new MemoryStream();
                    ds!.CopyTo(tempStream);
                    ds.Dispose();

                    byte[] decompressed = tempStream.ToArray();
                    if (decompressed.Length > 0)
                    {
                        decOut.Write(decompressed, 0, decompressed.Length);
                        using var nestedAll = new MemoryStream();
                        using var nestedDec = new MemoryStream();
                        ProcessLayer(decompressed, nestedAll, nestedDec, depth + 1, maxDepth);

                        byte[] nestedAllBytes = nestedAll.ToArray();
                        allOut.Write(nestedAllBytes, 0, nestedAllBytes.Length);

                        found = true;
                    }

                    int usedBytes = (int)inputStream.Position;
                    if (usedBytes <= 0) usedBytes = 1;
                    i += usedBytes;
                    break;
                }
                catch (InvalidDataException)
                {
                    // Invalid exception
                }
                catch
                {
                    // Other ignored
                }
            }

            if (!found)
            {
                allOut.WriteByte(data[i]);
                i++;
            }
        }
    }
}