﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CATHODE.Misc
{
    public class ResourcesBIN
    {
        public static alien_resources_bin Load(string FullFilePath)
        {
            alien_resources_bin Result = new alien_resources_bin();
            BinaryReader Stream = new BinaryReader(File.OpenRead(FullFilePath));

            Result.Header = Utilities.Consume<alien_resources_bin_header>(ref Stream);
            // TODO: Seems to be varying length or something weirder.
            Result.Entries = Utilities.ConsumeArray<alien_resources_bin_entry>(ref Stream, Result.Header.EntryCount);

            return Result;
        }
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct alien_resources_bin_header
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public int[] Unknown0_; // 2
    public int EntryCount;
    public int Unknown1_;
};

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct alien_resources_bin_entry
{
    public int Unknown0_;
    public int ResourceID;
    public int UnknownResourceIndex;
};

public struct alien_resources_bin
{
    public alien_resources_bin_header Header;
    public List<alien_resources_bin_entry> Entries;
};