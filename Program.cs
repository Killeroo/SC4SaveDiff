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

namespace SC4Extractor // Note: actual namespace depends on the project name.
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
            if (File.Exists(args[0]))
            {
                Console.WriteLine("Could not file file: " + args[0]);
                return;
            }
            if (File.Exists(args[1]))
            {
                Console.WriteLine("Could not file file: " + args[1]);
                return;
            }
            
            // Check extensions
            try
            {
                using (SC4SaveFile saveGame1 = new(args[0]))
                using (SC4SaveFile saveGame2 = new(args[1]))
                {
                    List<IndexEntry> entries1 = saveGame1.IndexEntries;
                    List<IndexEntry> entries2 = saveGame2.IndexEntries;

                    entries1.OrderBy(x => x.TGI);
                    entries2.OrderBy(x => x.TGI);


                }
            }
            catch (Exception ex)
            {
                // Log out
            }

        }

    }
}