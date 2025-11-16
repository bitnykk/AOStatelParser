using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Xml.Linq;
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
            int ICount = new int();
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
                            ICount = ICount + 1;
                        }
                    }
                }
                catch
                {
                    SColor = "Red";
                }
                if (SColor=="Green") {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                if (SColor == "Red")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                Console.Write(FName+" ");
                Console.ForegroundColor = ConsoleColor.White;
            }
            Console.WriteLine("");
            Console.WriteLine("Total:"+ICount);
        }

        private static void RunParsing(ParsingOptions opts)
        {
            byte[] fileBytes = File.ReadAllBytes($"{opts.AOPath}\\cd_image\\data\\statels\\{opts.Pf}.pf");
            var report = CompressionAnalyzer.Analyze(fileBytes, maxDepth: 3);
            Console.WriteLine("=== Structure analysis ===");
            CompressionAnalyzer.PrintReport(report);

            Console.WriteLine();
            Console.WriteLine("*** "+PfName(opts.Pf)+" ***");

            var InParser = new StatelParser(opts.AOPath,opts.Pf);
            SortedDictionary<uint, string> IdDict = new SortedDictionary<uint, string>();
            foreach (Statel s in InParser.Statels)
            {
                // not using Rot, Scale nor TextureOverrides
                string[] SCoords = s.Pos.ToString().Split(' ');
                string SValue = "/waypoint " + SCoords[0].Remove(0, 1)[..^1] + " " + SCoords[2][..^1] + " " + opts.Pf;
                if (!IdDict.ContainsKey(s.Id))
                {                    
                    IdDict.Add(s.Id,SValue); // 1st statel stored, but could stand outbound ...
                } else
                {
                    System.Random Rnd = new System.Random();
                    int IRoll = Rnd.Next(1, 5);
                    if(IRoll==4)
                    {   
                        IdDict[s.Id] = SValue; // ... so get other statels @ 25% chances each
                    }
                }
            }
            Console.WriteLine(IdDict.Count + " statel(s) found");
            foreach (var s in IdDict)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(" "+s.Key+":");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(s.Value+" ");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        static void HandleParseError(IEnumerable<Error> errs)
        {
            Console.Read();
        }
        static string PfName(int Id)
        {
            var Pfnames = new Dictionary<int, string>();
            Pfnames[120] = "camelot castle";
            Pfnames[124] = "tir tower";
            Pfnames[125] = "smuggler's den mantis";
            Pfnames[127] = "subway";
            Pfnames[128] = "big cross";
            Pfnames[1840] = "OT club";
            Pfnames[1862] = "smugglers den humans";
            Pfnames[1886] = "versailles tower";
            Pfnames[1891] = "happy rebel";
            Pfnames[1893] = "the cup";
            Pfnames[152] = "grid";
            Pfnames[1902] = "newland disco";
            Pfnames[1913] = "reet retreat";
            Pfnames[1931] = "temple of three winds";
            Pfnames[1933] = "step of madness";
            Pfnames[1941] = "foreman alpha";
            Pfnames[1943] = "inner sanctuum";
            Pfnames[4102] = "hope bar";
            Pfnames[4107] = "fixer grid";
            Pfnames[4121] = "will to fight";
            Pfnames[4805] = "crypt of home";
            Pfnames[362] = "cata / SL statics";
            Pfnames[382] = "alien ship";
            Pfnames[386] = "LE mish";
            Pfnames[4376] = "icc council";
            Pfnames[4336] = "alappa";
            Pfnames[4337] = "albtraum";
            Pfnames[4335] = "dark ruins";
            Pfnames[6003] = "old db";
            Pfnames[6055] = "new db";
            Pfnames[6057] = "boss room";
            Pfnames[4315] = "reactor";
            Pfnames[4327] = "flat";
            Pfnames[4363] = "club";
            Pfnames[4894] = "cost of harmony";
            Pfnames[500] = "ark parnassos 1";
            Pfnames[501] = "ark parnassos 2";
            Pfnames[896] = "ark laboratory";
            Pfnames[4364] = "apf outpost";
            Pfnames[4365] = "s13";
            Pfnames[4366] = "s28";
            Pfnames[4367] = "s35";
            Pfnames[4368] = "low";
            Pfnames[4370] = "s42";
            Pfnames[4374] = "s10";
            Pfnames[4468] = "s7 out";
            Pfnames[6123] = "s7 in";
            Pfnames[6011] = "arid rift";
            Pfnames[6012] = "neretva";
            Pfnames[6013] = "lox hub";
            Pfnames[6022] = "area X";
            Pfnames[6015] = "12man";
            Pfnames[6017] = "mitaar";
            Pfnames[6060] = "vortexx";
            Pfnames[6024] = "quest xan";
            Pfnames[6041] = "arid ruins";
            Pfnames[4530] = "platform";
            Pfnames[4531] = "harbour";
            Pfnames[4532] = "market";
            Pfnames[4533] = "plazza";
            Pfnames[4526] = "split";
            Pfnames[4534] = "appart";
            Pfnames[4001] = "nasc center";
            Pfnames[4001] = "nasc south";
            Pfnames[4310] = "nasc north";
            Pfnames[4311] = "nasc east";
            Pfnames[4312] = "nas west";
            Pfnames[4540] = "ely south";
            Pfnames[4541] = "ely west";
            Pfnames[4542] = "ely center";
            Pfnames[4543] = "ely east";
            Pfnames[4544] = "ely north";
            Pfnames[4881] = "scheo north";
            Pfnames[4880] = "scheo south";
            Pfnames[4877] = "ado entrance";
            Pfnames[4872] = "ado externe";
            Pfnames[4873] = "ado abyss";
            Pfnames[6035] = "dark ruin";
            Pfnames[4322] = "pen hollows";
            Pfnames[4321] = "pen east";
            Pfnames[4320] = "pen ouest";
            Pfnames[4006] = "pen ouest 2";
            Pfnames[4605] = "inf east";
            Pfnames[4005] = "inf west";
            Pfnames[4328] = "caina";
            Pfnames[4329] = "antenora";
            Pfnames[4330] = "ptolema";
            Pfnames[4331] = "judecca";
            Pfnames[4211] = "catacombs/statics";
            Pfnames[4212] = "catacombs/statics";
            Pfnames[4213] = "catacombs/statics";
            Pfnames[4214] = "catacombs/statics";
            Pfnames[4215] = "catacombs/statics";
            Pfnames[4220] = "catacombs/statics";
            Pfnames[4221] = "catacombs/statics";
            Pfnames[4222] = "catacombs/statics";
            Pfnames[4223] = "catacombs/statics";
            Pfnames[4224] = "catacombs/statics";
            Pfnames[4341] = "city buildings";
            Pfnames[4342] = "city buildings";
            Pfnames[4343] = "city buildings";
            Pfnames[4344] = "city buildings";
            Pfnames[4345] = "city buildings";
            Pfnames[4346] = "city buildings";
            Pfnames[4347] = "city buildings";
            Pfnames[4348] = "city buildings";
            Pfnames[4349] = "city buildings";
            Pfnames[4350] = "city buildings";
            Pfnames[4351] = "city buildings";
            Pfnames[4352] = "city buildings";
            Pfnames[4354] = "city buildings";
            Pfnames[4355] = "city buildings";
            Pfnames[4356] = "city buildings";
            Pfnames[4357] = "city buildings";
            Pfnames[4360] = "city buildings";
            Pfnames[4582] = "noob isl";
            Pfnames[4604] = "noob isl2";
            Pfnames[4833] = "noob isl3";
            Pfnames[505] = "avalon";
            Pfnames[540] = "old athen";
            Pfnames[545] = "west athen";
            Pfnames[550] = "athen shire";
            Pfnames[551] = "wailing wastes";
            Pfnames[556] = "cost of peace";
            Pfnames[560] = "mort";
            Pfnames[565] = "newland desert";
            Pfnames[566] = "newland city";
            Pfnames[567] = "newland";
            Pfnames[570] = "perpetual";
            Pfnames[585] = "aegean";
            Pfnames[586] = "wattorn";
            Pfnames[590] = "central AV";
            Pfnames[595] = "deep AV";
            Pfnames[600] = "varmint woods";
            Pfnames[605] = "belial forest";
            Pfnames[610] = "southern AV";
            Pfnames[615] = "southern FH";
            Pfnames[620] = "eastern FP";
            Pfnames[625] = "milky way";
            Pfnames[630] = "pleasant meadows";
            Pfnames[635] = "stret east B";
            Pfnames[640] = "tir city";
            Pfnames[641] = "tir arena";
            Pfnames[646] = "tir county";
            Pfnames[647] = "greater TC";
            Pfnames[650] = "upper SEB";
            Pfnames[655] = "andromeda";
            Pfnames[656] = "coast of tranquility";
            Pfnames[665] = "broken shores";
            Pfnames[670] = "clondyke";
            Pfnames[685] = "galway county";
            Pfnames[687] = "galway shire";
            Pfnames[695] = "lush fields";
            Pfnames[696] = "mutant domain";
            Pfnames[700] = "omni HQ";
            Pfnames[705] = "omni HQ south";
            Pfnames[706] = "omni ent";
            Pfnames[710] = "omni trade";
            Pfnames[716] = "omni forest";
            Pfnames[717] = "greater OF";
            Pfnames[730] = "rome red";
            Pfnames[735] = "rome blue";
            Pfnames[740] = "rome green";
            Pfnames[760] = "4holes";
            Pfnames[790] = "stret west bank";
            Pfnames[791] = "holes in the wall";
            Pfnames[795] = "longest road";
            Pfnames[800] = "borealis";
            Pfnames[4380] = "bs";
            Pfnames[4381] = "bs";
            Pfnames[4382] = "bs";
            Pfnames[4383] = "bs";
            Pfnames[4384] = "bs";
            Pfnames[4385] = "bs";
            Pfnames[4386] = "bs";
            Pfnames[4387] = "bs";
            Pfnames[4388] = "bs";
            Pfnames[4010] = "garden/sanct/temple";
            Pfnames[4011] = "garden/sanct/temple";
            Pfnames[4621] = "garden/sanct/temple";
            Pfnames[4622] = "garden/sanct/temple";
            Pfnames[4623] = "garden/sanct/temple";
            Pfnames[4624] = "garden/sanct/temple";
            Pfnames[4625] = "garden/sanct/temple";
            Pfnames[4626] = "garden/sanct/temple";
            Pfnames[4627] = "garden/sanct/temple";
            Pfnames[4628] = "garden/sanct/temple";
            Pfnames[4629] = "garden/sanct/temple";
            Pfnames[4630] = "garden/sanct/temple";
            Pfnames[4676] = "garden/sanct/temple";
            Pfnames[4677] = "garden/sanct/temple";
            Pfnames[4678] = "garden/sanct/temple";
            Pfnames[4679] = "garden/sanct/temple";
            Pfnames[4680] = "garden/sanct/temple";
            Pfnames[4681] = "garden/sanct/temple";
            Pfnames[4682] = "garden/sanct/temple";
            Pfnames[4683] = "garden/sanct/temple";
            Pfnames[4684] = "garden/sanct/temple";
            Pfnames[4685] = "garden/sanct/temple";
            Pfnames[4686] = "garden/sanct/temple";
            Pfnames[4687] = "garden/sanct/temple";
            Pfnames[4688] = "garden/sanct/temple";
            Pfnames[4689] = "garden/sanct/temple";
            Pfnames[4690] = "garden/sanct/temple";
            Pfnames[4691] = "garden/sanct/temple";
            Pfnames[4692] = "garden/sanct/temple";
            Pfnames[4693] = "garden/sanct/temple";
            Pfnames[4694] = "garden/sanct/temple";
            Pfnames[4695] = "garden/sanct/temple";
            Pfnames[4696] = "garden/sanct/temple";
            Pfnames[4697] = "garden/sanct/temple";
            Pfnames[4698] = "garden/sanct/temple";
            Pfnames[4699] = "garden/sanct/temple";
            Pfnames[3000] = "backyard";
            Pfnames[3001] = "backyard";
            Pfnames[3002] = "backyard";
            Pfnames[3003] = "backyard";
            Pfnames[3004] = "backyard";
            Pfnames[3005] = "backyard";
            Pfnames[3006] = "backyard";
            Pfnames[3007] = "backyard";
            Pfnames[3008] = "backyard";
            Pfnames[3009] = "backyard";
            Pfnames[3010] = "backyard";
            Pfnames[3011] = "backyard";
            Pfnames[3012] = "backyard";
            Pfnames[3013] = "backyard";
            Pfnames[3014] = "backyard";
            Pfnames[3015] = "backyard";
            Pfnames[3016] = "backyard";
            Pfnames[3017] = "backyard";
            Pfnames[3018] = "backyard";
            Pfnames[3019] = "backyard";
            Pfnames[3020] = "backyard";
            Pfnames[3021] = "backyard";
            Pfnames[3022] = "backyard";
            Pfnames[3023] = "backyard";
            Pfnames[3024] = "backyard";
            Pfnames[3025] = "backyard";
            Pfnames[3026] = "backyard";
            Pfnames[3027] = "backyard";
            Pfnames[3028] = "backyard";
            Pfnames[3029] = "backyard";
            Pfnames[3030] = "backyard";
            Pfnames[3031] = "backyard";
            Pfnames[3032] = "backyard";
            Pfnames[3033] = "backyard";
            Pfnames[3034] = "backyard";
            Pfnames[3035] = "backyard";
            Pfnames[3036] = "backyard";
            Pfnames[3037] = "backyard";
            Pfnames[3038] = "backyard";
            Pfnames[3039] = "backyard";
            Pfnames[3040] = "backyard";
            Pfnames[3041] = "backyard";
            Pfnames[3042] = "backyard";
            Pfnames[3043] = "backyard";
            Pfnames[3044] = "backyard";
            Pfnames[3045] = "backyard";
            Pfnames[3046] = "backyard";
            Pfnames[3047] = "backyard";
            Pfnames[3048] = "backyard";
            Pfnames[3049] = "backyard";
            Pfnames[3050] = "backyard";
            Pfnames[3051] = "backyard";
            Pfnames[3052] = "backyard";
            Pfnames[3053] = "backyard";
            Pfnames[3054] = "backyard";
            Pfnames[3055] = "backyard";
            Pfnames[3056] = "backyard";
            Pfnames[3057] = "backyard";
            Pfnames[3058] = "backyard";
            Pfnames[3059] = "backyard";
            Pfnames[3060] = "backyard";
            Pfnames[3061] = "backyard";
            Pfnames[3062] = "backyard";
            Pfnames[3063] = "backyard";
            Pfnames[3064] = "backyard";
            Pfnames[3065] = "backyard";
            Pfnames[3066] = "backyard";
            Pfnames[3067] = "backyard";
            Pfnames[3068] = "backyard";
            Pfnames[3069] = "backyard";
            Pfnames[3070] = "backyard";
            Pfnames[3071] = "backyard";
            Pfnames[3080] = "backyard";
            Pfnames[3081] = "backyard";
            Pfnames[3082] = "backyard";
            Pfnames[3083] = "backyard";
            Pfnames[3084] = "backyard";
            Pfnames[3085] = "backyard";
            Pfnames[3086] = "backyard";
            Pfnames[3087] = "backyard";
            Pfnames[3088] = "backyard";
            Pfnames[3089] = "backyard";
            Pfnames[3100] = "backyard";
            Pfnames[3101] = "backyard";
            Pfnames[3102] = "backyard";
            Pfnames[3103] = "backyard";
            Pfnames[3104] = "backyard";
            Pfnames[3105] = "backyard";
            Pfnames[3106] = "backyard";
            Pfnames[3107] = "backyard";
            Pfnames[3108] = "backyard";
            Pfnames[3109] = "backyard";
            Pfnames[3110] = "backyard";
            Pfnames[3111] = "backyard";
            Pfnames[3112] = "backyard";
            Pfnames[3113] = "backyard";
            Pfnames[3114] = "backyard";
            Pfnames[3115] = "backyard";
            Pfnames[3116] = "backyard";
            Pfnames[3117] = "backyard";
            Pfnames[3120] = "backyard";
            Pfnames[3121] = "backyard";
            Pfnames[3122] = "backyard";
            Pfnames[3123] = "backyard";
            Pfnames[3124] = "backyard";
            Pfnames[3125] = "backyard";
            Pfnames[3126] = "backyard";
            Pfnames[3127] = "backyard";
            Pfnames[3128] = "backyard";
            Pfnames[3129] = "backyard";
            Pfnames[3130] = "backyard";
            Pfnames[3131] = "backyard";
            Pfnames[3132] = "backyard";
            Pfnames[3133] = "backyard";
            Pfnames[3134] = "backyard";
            Pfnames[3135] = "backyard";
            Pfnames[3136] = "backyard";
            Pfnames[3137] = "backyard";
            Pfnames[3138] = "backyard";
            Pfnames[3139] = "backyard";
            Pfnames[3140] = "backyard";
            Pfnames[3141] = "backyard";
            Pfnames[3142] = "backyard";
            Pfnames[3143] = "backyard";
            Pfnames[3144] = "backyard";
            Pfnames[3145] = "backyard";
            Pfnames[3146] = "backyard";
            Pfnames[3147] = "backyard";
            Pfnames[3148] = "backyard";
            Pfnames[3149] = "backyard";
            Pfnames[1001] = "house/mission";
            Pfnames[1002] = "house/mission";
            Pfnames[1003] = "house/mission";
            Pfnames[1004] = "house/mission";
            Pfnames[1005] = "house/mission";
            Pfnames[1011] = "house/mission";
            Pfnames[1012] = "house/mission";
            Pfnames[1021] = "house/mission";
            Pfnames[1031] = "house/mission";
            Pfnames[1211] = "house/mission";
            Pfnames[1231] = "house/mission";
            Pfnames[1232] = "house/mission";
            Pfnames[1233] = "house/mission";
            Pfnames[1241] = "house/mission";
            Pfnames[1242] = "house/mission";
            Pfnames[1243] = "house/mission";
            Pfnames[1251] = "house/mission";
            Pfnames[1321] = "house/mission";
            Pfnames[1322] = "house/mission";
            Pfnames[1323] = "house/mission";
            Pfnames[1324] = "house/mission";
            Pfnames[1325] = "house/mission";
            Pfnames[1326] = "house/mission";
            Pfnames[1327] = "house/mission";
            Pfnames[1328] = "house/mission";
            Pfnames[1329] = "house/mission";
            Pfnames[1330] = "house/mission";
            Pfnames[1404] = "house/mission";
            Pfnames[1405] = "house/mission";
            Pfnames[1406] = "house/mission";
            Pfnames[1407] = "house/mission";
            Pfnames[1410] = "house/mission";
            Pfnames[1421] = "house/mission";
            Pfnames[1422] = "house/mission";
            Pfnames[1423] = "house/mission";
            Pfnames[1424] = "house/mission";
            Pfnames[1426] = "house/mission";
            Pfnames[1427] = "house/mission";
            Pfnames[1428] = "house/mission";
            Pfnames[1501] = "house/mission";
            Pfnames[1502] = "house/mission";
            Pfnames[1503] = "house/mission";
            Pfnames[1504] = "house/mission";
            Pfnames[1505] = "house/mission";
            Pfnames[1506] = "house/mission";
            Pfnames[1507] = "house/mission";
            Pfnames[1510] = "house/mission";
            Pfnames[1511] = "house/mission";
            Pfnames[1601] = "house/mission";
            Pfnames[1602] = "house/mission";
            Pfnames[1603] = "house/mission";
            Pfnames[1604] = "house/mission";
            Pfnames[1611] = "house/mission";
            Pfnames[1612] = "house/mission";
            Pfnames[1613] = "house/mission";
            Pfnames[1614] = "house/mission";
            Pfnames[1621] = "house/mission";
            Pfnames[1622] = "house/mission";
            Pfnames[1623] = "house/mission";
            Pfnames[1624] = "house/mission";
            Pfnames[1625] = "house/mission";
            Pfnames[1626] = "house/mission";
            Pfnames[1627] = "house/mission";
            Pfnames[1641] = "house/mission";
            Pfnames[1642] = "house/mission";
            Pfnames[1643] = "house/mission";
            Pfnames[1644] = "house/mission";
            Pfnames[1645] = "house/mission";
            Pfnames[1646] = "house/mission";
            Pfnames[1647] = "house/mission";
            Pfnames[1648] = "house/mission";
            Pfnames[1651] = "house/mission";
            Pfnames[1652] = "house/mission";
            Pfnames[1653] = "house/mission";
            Pfnames[1661] = "house/mission";
            Pfnames[1662] = "house/mission";
            Pfnames[1663] = "house/mission";
            Pfnames[1671] = "house/mission";
            Pfnames[1672] = "house/mission";
            Pfnames[1673] = "house/mission";
            Pfnames[1674] = "house/mission";
            Pfnames[1675] = "house/mission";
            Pfnames[1701] = "house/mission";
            Pfnames[1702] = "house/mission";
            Pfnames[1703] = "house/mission";
            Pfnames[1711] = "house/mission";
            Pfnames[1712] = "house/mission";
            Pfnames[1721] = "house/mission";
            Pfnames[1722] = "house/mission";
            Pfnames[1741] = "house/mission";
            Pfnames[1742] = "house/mission";
            Pfnames[1743] = "house/mission";
            Pfnames[1826] = "house/mission";
            Pfnames[1827] = "house/mission";
            Pfnames[1833] = "house/mission";
            Pfnames[1836] = "house/mission";
            Pfnames[1846] = "house/mission";
            Pfnames[1866] = "house/mission";
            Pfnames[1887] = "house/mission";
            Pfnames[1892] = "house/mission";
            Pfnames[1894] = "house/mission";
            Pfnames[1901] = "house/mission";
            Pfnames[320] = "house/mission";
            Pfnames[321] = "house/mission";
            Pfnames[322] = "house/mission";
            Pfnames[324] = "house/mission";
            Pfnames[331] = "house/mission";
            Pfnames[341] = "house/mission";
            Pfnames[346] = "house/mission";
            Pfnames[351] = "house/mission";
            Pfnames[1136] = "shops";
            Pfnames[1137] = "shops";
            Pfnames[1180] = "shops";
            Pfnames[1181] = "shops";
            Pfnames[1182] = "shops";
            Pfnames[1183] = "shops";
            Pfnames[1184] = "shops";
            Pfnames[1185] = "shops";
            Pfnames[1186] = "shops";
            Pfnames[1187] = "shops";
            Pfnames[1189] = "shops";
            Pfnames[1190] = "shops";
            Pfnames[1191] = "shops";
            Pfnames[1192] = "shops";
            Pfnames[1193] = "shops";
            Pfnames[2001] = "shops";
            Pfnames[2002] = "shops";
            Pfnames[2003] = "shops";
            Pfnames[2004] = "shops";
            Pfnames[2005] = "shops";
            Pfnames[2006] = "shops";
            Pfnames[2010] = "shops";
            Pfnames[2011] = "shops";
            Pfnames[2012] = "shops";
            Pfnames[2013] = "shops";
            Pfnames[2014] = "shops";
            Pfnames[2020] = "shops";
            Pfnames[2021] = "shops";
            Pfnames[2022] = "shops";
            Pfnames[2023] = "shops";
            Pfnames[2024] = "shops";
            Pfnames[2030] = "shops";
            Pfnames[2031] = "shops";
            Pfnames[2032] = "shops";
            Pfnames[2033] = "shops";
            Pfnames[2034] = "shops";
            Pfnames[2040] = "shops";
            Pfnames[2041] = "shops";
            Pfnames[2042] = "shops";
            Pfnames[2043] = "shops";
            Pfnames[2050] = "shops";
            Pfnames[2051] = "shops";
            Pfnames[2052] = "shops";
            Pfnames[2053] = "shops";
            Pfnames[2060] = "shops";
            Pfnames[2061] = "shops";
            Pfnames[2062] = "shops";
            Pfnames[2063] = "shops";
            Pfnames[2064] = "shops";
            Pfnames[2070] = "shops";
            Pfnames[2071] = "shops";
            Pfnames[2072] = "shops";
            Pfnames[2073] = "shops";
            Pfnames[2096] = "shops";
            Pfnames[4563] = "shops";
            Pfnames[4564] = "shops";
            Pfnames[4565] = "shops";
            Pfnames[4567] = "shops";
            Pfnames[4568] = "shops";
            Pfnames[4569] = "shops";
            Pfnames[4571] = "shops";
            Pfnames[4572] = "shops";
            Pfnames[4573] = "shops";
            Pfnames[4575] = "shops";
            Pfnames[4576] = "shops";
            Pfnames[4577] = "shops";
            Pfnames[4704] = "shops";
            Pfnames[950] = "training grounds";
            Pfnames[952] = "training grounds";
            Pfnames[953] = "training grounds";
            Pfnames[954] = "training grounds";
            Pfnames[955] = "training grounds";
            Pfnames[4313] = "old sl start";
            Pfnames[100] = "testing zone";
            Pfnames[101] = "testing zone";
            Pfnames[102] = "testing zone";
            Pfnames[103] = "testing zone";
            Pfnames[104] = "testing zone";
            Pfnames[105] = "testing zone";
            Pfnames[107] = "testing zone";
            Pfnames[109] = "testing zone";
            Pfnames[110] = "testing zone";
            Pfnames[111] = "testing zone";
            Pfnames[4525] = "testing zone";
            Pfnames[4561] = "testing zone";
            Pfnames[6133] = "sauna";
            Pfnames[6007] = "hub BS";
            Pfnames[6104] = "notum miner";
            Pfnames[6300] = "notum miner";
            Pfnames[6301] = "notum miner";
            Pfnames[6302] = "notum miner";
            Pfnames[6303] = "notum miner";
            Pfnames[6304] = "notum miner";
            Pfnames[6305] = "notum miner";
            Pfnames[4468] = "sector 7";
            Pfnames[6051] = "serenity";
            Pfnames[6115] = "ailaby1";
            Pfnames[6121] = "ailaby2";
            Pfnames[6123] = "ailaby3";
            Pfnames[6125] = "error";
            Pfnames[6127] = "aibox";
            Pfnames[6129] = "ailaby4";
            Pfnames[6131] = "aiHQ";
            Pfnames[7018] = "bureau";
            Pfnames[7019] = "bureau2";
            Pfnames[7020] = "bureau3";
            Pfnames[7101] = "error2";
            Pfnames[6553] = "arete landing";
            Pfnames[6550] = "uturn";
            Pfnames[750] = "the reck";
            Pfnames[4021] = "db3";
            Pfnames[4900] = "gauntlet";
            Pfnames[6052] = "serenity";
            Pfnames[596] = "enigma entvines";
            Pfnames[4474] = "s10 boss";
            Pfnames[4475] = "s10 boss2";
            Pfnames[4885] = "shadow Crypt";
            Pfnames[5001] = "city playa";
            Pfnames[5002] = "city montroyal";
            Pfnames[6001] = "space flat";
            Pfnames[6026] = "omni lab tp";
            Pfnames[6028] = "snow test";
            Pfnames[7018] = "bedroom";
            Pfnames[7019] = "bedroom2";
            Pfnames[7020] = "bedroom3";
            Pfnames[8004] = "clanmish";
            Pfnames[8009] = "crash";
            Pfnames[8020] = "pyramid";
            Pfnames[8030] = "crash";
            Pfnames[8040] = "crash";
            Pfnames[8045] = "crash";
            Pfnames[8046] = "crash";
            Pfnames[8050] = "foundry of nightmares";
            Pfnames[9041] = "crash";
            Pfnames[9042] = "xan reliq Crash";
            Pfnames[7015] = "collector start";
            Pfnames[7015] = "collector boss";
            Pfnames[7015] = "collector aerial";
            return Pfnames[Id];
            if (Pfnames.ContainsKey(Id))
            {
                return Pfnames[Id];
            } else
            {
                return "unknown";
            }
        }

    }
}