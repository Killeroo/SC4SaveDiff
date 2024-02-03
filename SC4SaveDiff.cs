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
    internal class SC4SaveDiff
    {
        public const string kVersion = "1.0";

        public enum EntryStatus : byte
        {
            Missing,
            Added,
            SizeDifference,
            LocationDifferent
            // DataDifference
        }

        private static void Main(string[] args)
        {
            // Check arguments
            if (args.Length < 2)
            {
                Console.WriteLine("SC4SaveDiff {0}", kVersion);
                Console.WriteLine("Not enough arguments, please specify the Simcity 4 games you would like to compare:" +
                    Environment.NewLine +
                    "For example: SC4SaveDiff C:\\Path\\To\\Target\\Simcity\\Save.sc4 C:\\Path\\To\\Other\\Simcity\\Save.sc4");
                return;
            }

            // Check files exist
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Could not find file: " + args[0]);
                return;
            }
            if (!File.Exists(args[1]))
            {
                Console.WriteLine("Could not find file: " + args[1]);
                return;
            }

            // Try and load the save games
            SC4SaveFile target, source;
            try
            {
                target = new(args[0]);
            }
            catch (DBPFParsingException)
            {
                Console.WriteLine("Could not parse {0} save game, invalid format", Path.GetFileName(args[0]));
                return;
            }
            try
            {
                source = new(args[1]);
            }
            catch (DBPFParsingException)
            {
                Console.WriteLine("Could not parse {0} save game, invalid format", Path.GetFileName(args[1]));
                return;
            }

            // Get differences
            var sourceSaveDifferences = CompareSavegames(target, source);

            // Print results
            if (sourceSaveDifferences.Count == 0)
            {
                Console.WriteLine("No differences found.");
                return;
            }

            Console.WriteLine("Differences from \"{0}\" to \"{1}\"...", GetPrettySaveName(Path.GetFileName(args[1])), GetPrettySaveName(Path.GetFileName(args[0])));
            sourceSaveDifferences.Sort((x, y) => x.State.CompareTo(y.State));
            foreach ((IndexEntry Entry, EntryStatus State, IndexEntry? OldEntry) diff in sourceSaveDifferences)
            {
                switch (diff.State)
                {
                    case EntryStatus.Added:
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("[Added] [{0}] Size={1} Location=0x{2}", diff.Entry.TGI.ToString(), diff.Entry.FileSize, diff.Entry.FileLocation.ToString("X6"));
                            Console.ResetColor();
                            break;
                        }

                    case EntryStatus.Missing:
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("[Removed] [{0}] Size={1} bytes Location=0x{2}", diff.Entry.TGI.ToString(), diff.Entry.FileSize, diff.Entry.FileLocation.ToString("X6"));
                            Console.ResetColor();
                            break;
                        }

                    case EntryStatus.LocationDifferent:
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("[Changed] [{0}] Location: 0x{1} -> 0x{2}", diff.Entry.TGI.ToString(), diff.OldEntry?.FileLocation.ToString("X6"), diff.Entry.FileLocation.ToString("X6"));
                            Console.ResetColor();
                            break;
                        }

                    case EntryStatus.SizeDifference:
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("[Changed] [{0}] Size: {1} -> {2} bytes", diff.Entry.TGI.ToString(), diff.OldEntry?.FileSize, diff.Entry.FileSize);
                            Console.ResetColor();
                            break;
                        }
                }
            }
        }

        // Compares target and source save games, generating a list of differences from the target save to source
        // Not performant but that is fine.
        private static List<(IndexEntry Entry, EntryStatus State, IndexEntry? OldEntry)> CompareSavegames(SC4SaveFile target, SC4SaveFile source)
        {
            List<(IndexEntry Entry, EntryStatus State, IndexEntry? OldEntry)> differencesFromSource = new();

            List<IndexEntry> sourceEntries = source.IndexEntries;
            List<IndexEntry> targetEntries = target.IndexEntries;

            sourceEntries.OrderBy(x => x.TGI);
            targetEntries.OrderBy(x => x.TGI);

            // Check and see if entries from the target exist in the source
            // if they do then check their size and location
            foreach (IndexEntry targetEntry in targetEntries)
            {
                IndexEntry foundEntry = null;
                for (int j = 0; j < sourceEntries.Count; j++)
                {
                    if (targetEntry.TGI == sourceEntries[j].TGI)
                    {
                        foundEntry = sourceEntries[j];
                        break;
                    }
                }

                if (foundEntry == null)
                {
                    differencesFromSource.Add((targetEntry, EntryStatus.Added, null));
                    continue;
                }

                // Compare size of two entries
                if (targetEntry.FileSize != foundEntry.FileSize)
                {
                    differencesFromSource.Add((targetEntry, EntryStatus.SizeDifference, foundEntry));
                }
                if (targetEntry.FileLocation != foundEntry.FileLocation)
                {
                    differencesFromSource.Add((targetEntry, EntryStatus.LocationDifferent, foundEntry));
                }
            }

            // Check and see if anything is missing from the target that is present in the source
            foreach (IndexEntry sourceEntry in sourceEntries)
            {
                IndexEntry foundEntry = null;
                for (int j = 0; j < targetEntries.Count; j++)
                {
                    if (sourceEntry.TGI == targetEntries[j].TGI)
                    {
                        foundEntry = targetEntries[j];
                        break;
                    }
                }

                if (foundEntry == null)
                {
                    differencesFromSource.Add((sourceEntry, EntryStatus.Missing, null));
                }
            }

            return differencesFromSource;
        }

        // Get city name with 'City -' stripped
        private static string GetPrettySaveName(string saveName)
        {
            return saveName.Split(" - ").Last();
        }
    }
}