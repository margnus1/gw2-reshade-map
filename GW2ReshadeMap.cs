/*
 * GW2ReshadeMap - Use Guild Wars 2's map ID in your ReShade presets
 * Copyright (C) 2015  Magnus Lång
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public License
 *  as published by the Free Software Foundation; either version 2
 *  of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA
 *  02110-1301, USA.
 */

using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;

namespace GW2ReshadeMap {
    class Program {
        const string MumbleLinkFile = "MumbleLink";
        static readonly int LinkedMemSize = Marshal.SizeOf(typeof(LinkedMem));

        static string oldContents = "";

        static void Main(string[] args) {
            string fileName = "gw2map.h";
            if (args.Length>0) fileName = args[0];
            Console.WriteLine("Maintaining {0} with map data from Guild Wars 2 "
                              + "using the Mumble Link API", fileName);

            using (var f = MemoryMappedFile.CreateOrOpen("MumbleLink", LinkedMemSize)) {
                MemoryMappedViewAccessor view = f.CreateViewAccessor();

                while (true) {
                    LinkedMem  state   = UnmarshalRead<LinkedMem>(view);
                    GW2Context context = UnmarshalRead<GW2Context>(state.context);

                    string newContents = genContents(state, context);
                    if (newContents != oldContents) {
                        DayNightCycle.TimeOfDay tod = DayNightCycle.Classify();

                        Console.WriteLine("Updated file: GW2MapId = {0}, GW2TOD = {1} ({2}).",
                            context.mapId, (int)tod, tod);
                        File.WriteAllText(fileName, newContents);
                        oldContents = newContents;
                    }

                    Thread.Sleep(100);
                }
            }
        }

        private static string genContents(LinkedMem state, GW2Context context) {
            return String.Format("#define GW2MapId {0}\n#define GW2TOD {1}\n",
                context.mapId,
                (int)DayNightCycle.Classify());
        }

        static T UnmarshalRead<T>(MemoryMappedViewAccessor view) where T : struct {
            byte[] data = new byte[Marshal.SizeOf(typeof(LinkedMem))];
            view.ReadArray(0, data, 0, data.Length);
            return UnmarshalRead<T>(data);
        }

        static T UnmarshalRead<T>(byte[] data) where T : struct {
            GCHandle pin = GCHandle.Alloc(data, GCHandleType.Pinned);
            try {
                return (T)Marshal.PtrToStructure(pin.AddrOfPinnedObject(), typeof(T));
            } finally {
                pin.Free();
            }
        }

        /* From https://github.com/arenanet/api-cdi/blob/master/mumble.md */
        [StructLayout(LayoutKind.Sequential)]
        struct GW2Context {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=28, ArraySubType=UnmanagedType.I1)]
            public byte[] serverAddress; // contains sockaddr_in or sockaddr_in6
            public uint mapId;
            public uint mapType;
            public uint shardId;
            public uint instance;
            public uint buildId;
        }

        /* From http://wiki.mumble.info/wiki/Link */
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        struct LinkedMem {
            UInt32 uiVersion;
            UInt32 uiTick;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=3, ArraySubType=UnmanagedType.R4)]
            public float[] fAvatarPosition;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=3, ArraySubType=UnmanagedType.R4)]
            public float[] fAvatarFront;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=3, ArraySubType=UnmanagedType.R4)]
            public float[] fAvatarTop;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=256)]
            public string name;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=3, ArraySubType=UnmanagedType.R4)]
            public float[] fCameraPosition;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=3, ArraySubType=UnmanagedType.R4)]
            public float[] fCameraFront;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=3, ArraySubType=UnmanagedType.R4)]
            public float[] fCameraTop;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=256)]
            public string identity;
            public UInt32 context_len;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=256, ArraySubType=UnmanagedType.I1)]
            public byte[] context;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=2048)]
            public string description;
        };
    }

    class DayNightCycle {
        /* From https://wiki.guildwars2.com/wiki/Day_and_night */
        const double origin   = 25 * 60;  /* Cycle starts at 00:25 UTC */
        const double duration = 120 * 60; /* and lasts for two hours */

        public static double Time() {
            return ((DateTime.UtcNow.TimeOfDay.TotalSeconds - origin + duration) / duration) % 1;
        }

        public static TimeOfDay Classify() {
            double time = DayNightCycle.Time();
            if (time < 5  / 120.0) return TimeOfDay.Dawn;
            if (time < 75 / 120.0) return TimeOfDay.Day;
            if (time < 80 / 120.0) return TimeOfDay.Dusk;
            return TimeOfDay.Night;
        }

        public enum TimeOfDay {
            Dawn,
            Day,
            Dusk,
            Night,
        }
    }
}
