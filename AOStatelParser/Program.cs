using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Runtime;
using UnityEngine;

namespace AoStatelParser
{
    internal class Program
    {
        [Verb("parse")]
        private class ParsingOptions : BaseOptions
        {
            [Option("pf", Required = true, HelpText = "Pf to be parsed.")]
            public int Pf { get; set; }
        }

        [Verb("search")]
        private class SearchingOptions : BaseOptions
        {
            [Option("id", Required = true, HelpText = "Id to be searched.")]
            public int Id { get; set; }
        }

        private class BaseOptions
        {
            [Option("aopath", Required = true, HelpText = "The path to the Anarchy Online folder.")]
            public string AOPath { get; set; }
        }

        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<ParsingOptions,SearchingOptions>(args)
                .WithParsed<ParsingOptions>(options => RunParsing(options))
                .WithParsed<SearchingOptions>(options => RunSearching(options))
                .WithNotParsed(HandleParseError);
        }

        private static void RunSearching(SearchingOptions opts)
        {
            Console.Write(opts.AOPath + " " + opts.Id + ": ");
            DirectoryInfo SDir = new DirectoryInfo(Path.Combine(opts.AOPath, "cd_image/data/statels/"));
            FileInfo[] SFiles = SDir.GetFiles("*.pf");
            foreach (FileInfo SFile in SFiles)
            {
                string FName = Path.GetFileNameWithoutExtension(SFile.ToString());
                string SColor = "White";
                try
                {                    
                    var InParser = new StatelParser(opts.AOPath, Int32.Parse(FName));
                    foreach (Statel s in InParser.Statels)
                    {
                        if (s.Id == opts.Id)
                        {
                            SColor = "Green";
                        }
                    }
                }
                catch
                {
                    SColor = "Red";
                }
                Console.ForegroundColor = ConsoleColor.White;
                if (SColor=="Green") {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                if (SColor == "Red")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                Console.Write(FName+" ");
            }
        }

        private static void RunParsing(ParsingOptions opts)
        {
            Console.Write(opts.AOPath+" "+opts.Pf+": ");
            var InParser = new StatelParser(opts.AOPath,opts.Pf);
            Dictionary<uint, string> IdDict = new Dictionary<uint, string>();
            foreach (Statel s in InParser.Statels)
            {
                // not showing TextureOverrides ; Pos/Rot axis order => X Z Y
                if (!IdDict.ContainsKey(s.Id)) {
                    IdDict.Add(s.Id, "@"+s.Pos+"°"+s.Rot+"%[" + s.Scale+"]");
                }
            }
            Console.WriteLine(IdDict.Count + " statel(s) found");
            foreach (var s in IdDict)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(" "+s.Key+":");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(s.Value+" ");
            }
        }

        static void HandleParseError(IEnumerable<Error> errs)
        {
            Console.Read();
        }

    }
}