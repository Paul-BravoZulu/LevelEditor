﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CATHODE.Misc
{
    /* Handles CATHODE COLLISION.MAP files */
    public class CollisionMAP
    {
        private string filepath;
        public alien_collision_map_header header;
        public alien_collision_map_entry[] entries;

        /* Load the file */
        public CollisionMAP(string path)
        {
            filepath = path;

            BinaryReader stream = new BinaryReader(File.OpenRead(path));
            header = Utilities.Consume<alien_collision_map_header>(stream);
            entries = Utilities.ConsumeArray<alien_collision_map_entry>(stream, header.EntryCount);
            stream.Close();
        }

        /* Save the file */
        public void Save()
        {
            BinaryWriter stream = new BinaryWriter(File.OpenWrite(filepath));
            stream.BaseStream.SetLength(0);
            Utilities.Write<alien_collision_map_header>(stream, header);
            Utilities.Write<alien_collision_map_entry>(stream, entries);
            stream.Close();
        }

        /* Data accessors */
        public int EntryCount { get { return entries.Length; } }
        public alien_collision_map_entry[] Entries { get { return entries; } }
        public alien_collision_map_entry GetEntry(int i)
        {
            return entries[i];
        }

        /* Data setters */
        public void SetEntry(int i, alien_collision_map_entry content)
        {
            entries[i] = content;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct alien_collision_map_header
    {
        public int DataSize;
        public int EntryCount;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct alien_collision_map_entry
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public int[] Unknowns1; //12
        public int ID;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public int[] Unknowns2; //12
    };
}