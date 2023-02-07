using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using SC4Parser;
using SC4Parser.DataStructures;
using SC4Parser.Files;
using SC4Parser.Subfiles;
using SC4Parser.Types;
using SC4Parser.Logging;

namespace SC4Diff
{
    internal class Program
    {

        private static void Main(string[] args)
        {
            // Check arguments
            if (args.Length < 2)
            {
                Console.WriteLine("Not enougha arguments, please specify the Simcty 4 games you would like to compare:" +
                    Environment.NewLine +
                    "For example: SC4Diff C:\\Path\\To\\First\\Simcity\\Save.sc4 C:\\Path\\To\\Second\\Simcity\\Save.sc4");
                return;
            }

            
            // Check files exist
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Could not file file: " + args[0]);
                return;
            }
            if (!File.Exists(args[1]))
            {
                Console.WriteLine("Could not file file: " + args[1]);
                return;
            }
            
            // TODO: Check extensions
            try
            {
                using (SC4SaveFile saveGame1 = new(args[0]))
                using (SC4SaveFile saveGame2 = new(args[1]))
                {
                    List<IndexEntry> entries1 = saveGame1.IndexEntries;
                    List<IndexEntry> entries2 = saveGame2.IndexEntries;

                    entries1.OrderBy(x => x.TGI);
                    entries2.OrderBy(x => x.TGI);

                    List<TypeGroupInstance> missingEntries = new List<TypeGroupInstance>();
                    List<TypeGroupInstance> matchingEntries = new List<TypeGroupInstance>();

                    for (int i = 0; i < entries1.Count; i++)
                    {
                        IndexEntry sourceEntry = entries1[i];
                        
                        IndexEntry foundEntry = null;
                        for (int j = 0; j < entries2.Count; j++)
                        {
                            if (sourceEntry.TGI == entries2[j].TGI)
                            {
                                foundEntry = entries2[j];
                                break;
                            }
                        }

                        if (foundEntry == null)
                        {
                            missingEntries.Add(sourceEntry.TGI);

                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("{0}: Entry missing",
                                sourceEntry.TGI);
                        }
                        else
                        {
                            matchingEntries.Add(sourceEntry.TGI);

                            // Compare size of two entries
                            if (sourceEntry.FileSize != foundEntry.FileSize)
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("[{0}] Entry has a different filesize [{1} -> {2}]",
                                    sourceEntry.TGI.ToString(),
                                    sourceEntry.FileSize,
                                    foundEntry.FileSize);
                                Console.ResetColor();
                            }
                            if (sourceEntry.FileLocation != foundEntry.FileLocation)
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("[{0}] Entry is in a different location [0x{1} -> 0x{2}]",
                                    sourceEntry.TGI.ToString(),
                                    sourceEntry.FileSize.ToString("X8"),
                                    foundEntry.FileSize.ToString("X8"));
                                Console.ResetColor();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log out
            }

        }

    }
}