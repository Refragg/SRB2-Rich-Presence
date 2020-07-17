using System;
using System.Threading;
using System.Diagnostics;
using DiscordRPC;
using System.Runtime.InteropServices;

namespace SRB2RPC
{
    public static class Program
    {
        const int PROCESS_WM_READ = 0x0010;
        public static string level = "";
        public static string previousLevel = "";
        public static string character = "";
        public static string characterImage = "";
        public static string image = "";
        public static string mode = "";
        public static int RA;
        public static int IsPlaying;
        public static int IsWatching;
        public static int multi;
        public static int gametype;
        public static int exe = 0;
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess,
            int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        //This one is used if we have to deal with larger values than a byte can contain 
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess,
            int lpBaseAddress, int[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);
        public static DiscordRpcClient Client { get; private set; }

        static void Setup()
        {
            Client = new DiscordRpcClient("690687836726231193");
            Client.Initialize();
            Console.WriteLine("Rich Presence initialized");
        }
        static void Cleanup()
        {
            Client.Dispose();
            Console.WriteLine("Rich Presence stopped, exiting");
            Thread.Sleep(3000);
        }
        private static RichPresence presence = new RichPresence();
        public static void Update(string level, string character, string image, string characterImage)
        {
            if (IsPlaying == 1 && IsWatching == 0)
            {
                if (multi == 0)
                {
                    if (RA == 0)
                    {
                        presence.Details = "Playing in singleplayer";
                    }
                    if (RA == 1)
                    {
                        presence.Details = "Playing in record attack";
                    }
                    if (RA == 2)
                    {
                        presence.Details = "Playing in Nights mode";
                    }
                }
                if (multi == 1)
                {
                    presence.Details = "Playing in " + mode;
                }
            }
            else
            {
                if (IsWatching == 1)
                {
                    presence.Details = "Watching a replay";
                    character = null;
                    characterImage = null;
                }
                else
                {
                    presence.Details = "Navigating the menus";
                    image = "menu";
                    level = "Menus";
                    character = null;
                    characterImage = null;
                }
            }

            presence.Assets = new Assets()
            {
                LargeImageText = level,
                SmallImageText = character,
                LargeImageKey = image,
                SmallImageKey = characterImage
            };
            Client.SetPresence(presence);
        }
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting SRB2");
            Process process = new Process();
            process.StartInfo.FileName = System.IO.Directory.GetCurrentDirectory() + "\\srb2win.exe";
            process.StartInfo.WorkingDirectory = System.IO.Directory.GetCurrentDirectory();

            string arg = "";
            string cargs = "";
            if (args.Length > 0)
            {
                foreach (string cArg in args)
                {
                    arg += cArg + " ";
                }
                cargs = arg.Remove(arg.Length - 1, 1);
            }
            if (cargs != "")
            {
                Console.WriteLine("With arguments : \"" + cargs + "\"");
            }
            process.StartInfo.Arguments = cargs;

            try
            {
                process.Start();
            }
            catch
            {
                Console.WriteLine("Could not start SRB2, check if the executable name is \"srb2win.exe\"");
                Thread.Sleep(5000);
                Environment.Exit(0);
            }
            IntPtr processHandle = OpenProcess(PROCESS_WM_READ, false, process.Id);

            //putting a delay to avoid a random error
            Thread.Sleep(5000);

            Console.WriteLine("Getting executable type");
            //Determines whether the game is the 32 bit or the 64 bit version
            if (process.MainModule.ModuleMemorySize == 99889152)
            {
                exe = 32;
            }
            /*if (process.MainModule.ModuleMemorySize == 99946496)
            {
                exe = 64;
            }*/
            if (exe == 0)
            {
                Console.WriteLine("\nWrong executable\nExecutable must be original 2.2.6\nThe program and the game will now exit");
                Thread.Sleep(10000);
                process.Kill();
                Environment.Exit(0);
            }

            int bytesRead = 0;

            //Level
            int[] buffer = new int[200];//200 is an arbitrary value because idk what to put lol (but it works so that's good i guess)

            //Character
            byte[] buffer2 = new byte[1];

            //Record Attack or not
            byte[] buffer4 = new byte[1];

            //Is playing or not
            byte[] buffer5 = new byte[1];

            //Solo or multi
            byte[] buffer6 = new byte[1];

            //Multiplayer gametype
            byte[] buffer7 = new byte[1];

            //Is watching a demo or not
            byte[] buffer8 = new byte[1];

            //Marathon run timer (to detect whether or not you're playing in this mode)
            //int[] buffer9 = new int[200];

            Console.WriteLine("\nInitializing Rich Presence");

            Setup();
            while (!process.HasExited)
            {
                if (exe == 32)
                {
                    ReadProcessMemory((int)processHandle, 0x0064CCD4, buffer, buffer.Length, ref bytesRead);
                    ReadProcessMemory((int)processHandle, 0x00636C34, buffer2, buffer2.Length, ref bytesRead);
                    ReadProcessMemory((int)processHandle, 0x00831BCC, buffer4, buffer4.Length, ref bytesRead);
                    ReadProcessMemory((int)processHandle, 0x059C5AE0, buffer5, buffer5.Length, ref bytesRead);
                    ReadProcessMemory((int)processHandle, 0x04853160, buffer6, buffer6.Length, ref bytesRead);
                    ReadProcessMemory((int)processHandle, 0x008245F4, buffer7, buffer7.Length, ref bytesRead);
                    ReadProcessMemory((int)processHandle, 0x059CCEA4, buffer8, buffer8.Length, ref bytesRead);
                    //ReadProcessMemory((int)processHandle, 0x00831BD8, buffer9, buffer9.Length, ref bytesRead);
                }
                /*if (exe == 64)
                {
                    ReadProcessMemory((int)processHandle, 0x0084BA28, buffer, buffer.Length, ref bytesRead);
                    ReadProcessMemory((int)processHandle, 0x00642948, buffer2, buffer2.Length, ref bytesRead);
                    ReadProcessMemory((int)processHandle, 0x0084BA18, buffer4, buffer4.Length, ref bytesRead);
                    ReadProcessMemory((int)processHandle, 0x0080D414, buffer5, buffer5.Length, ref bytesRead);
                    ReadProcessMemory((int)processHandle, 0x04908DE0, buffer6, buffer6.Length, ref bytesRead);
                    ReadProcessMemory((int)processHandle, 0x00833F34, buffer7, buffer7.Length, ref bytesRead);
                    ReadProcessMemory((int)processHandle, 0x059FBD84, buffer8, buffer8.Length, ref bytesRead);
                }*/
                switch (buffer[0])
                {
                    //Solo campaign Stages

                    case 1:
                        level = "Greenflower Zone Act 1";
                        image = "greenflower_zone_act_1";
                        break;
                    case 2:
                        level = "Greenflower Zone Act 2";
                        image = "greenflower_zone_act_2";
                        break;
                    case 3:
                        level = "Greenflower Zone Act 3";
                        image = "greenflower_zone_act_3";
                        break;
                    case 4:
                        level = "Techno Hill Zone Act 1";
                        image = "techno_hill_zone_act_1";
                        break;
                    case 5:
                        level = "Techno Hill Zone Act 2";
                        image = "techno_hill_zone_act_2";
                        break;
                    case 6:
                        level = "Techno Hill Zone Act 3";
                        image = "techno_hill_zone_act_3";
                        break;
                    case 7:
                        level = "Deep Sea Zone Act 1";
                        image = "deep_sea_zone_act_1";
                        break;
                    case 8:
                        level = "Deep Sea Zone Act 2";
                        image = "deep_sea_zone_act_2";
                        break;
                    case 9:
                        level = "Deep Sea Zone Act 3";
                        image = "deep_sea_zone_act_3";
                        break;
                    case 10:
                        level = "Castle Eggman Zone Act 1";
                        image = "castle_eggman_zone_act_1";
                        break;
                    case 11:
                        level = "Castle Eggman Zone Act 2";
                        image = "castle_eggman_zone_act_2";
                        break;
                    case 12:
                        level = "Castle Eggman Zone Act 3";
                        image = "castle_eggman_zone_act_3";
                        break;
                    case 13:
                        level = "Arid Canyon Zone Act 1";
                        image = "arid_canyon_zone_act_1";
                        break;
                    case 14:
                        level = "Arid Canyon Zone Act 2";
                        image = "arid_canyon_zone_act_2";
                        break;
                    case 15:
                        level = "Arid Canyon Zone Act 3";
                        image = "arid_canyon_zone_act_3";
                        break;
                    case 16:
                        level = "Red Volcano Zone Act 1";
                        image = "red_volcano_zone_act_1";
                        break;
                    case 22:
                        level = "Egg Rock Zone Act 1";
                        image = "egg_rock_zone_act_1";
                        break;
                    case 23:
                        level = "Egg Rock Zone Act 2";
                        image = "egg_rock_zone_act_2";
                        break;
                    case 25:
                        level = "Black Core Zone Act 1";
                        image = "black_core_zone_act_1";
                        break;
                    case 26:
                        level = "Black Core Zone Act 2";
                        image = "black_core_zone_act_2";
                        break;
                    case 27:
                        level = "Black Core Zone Act 3";
                        image = "black_core_zone_act_3";
                        break;

                    //Bonus Stages

                    case 30:
                        level = "Frozen Hillside Zone";
                        image = "frozen_hillside_zone";
                        break;
                    case 31:
                        level = "Pipe Towers Zone";
                        image = "pipe_towers_zone";
                        break;
                    case 32:
                        level = "Forest Fortress Zone";
                        image = "forest_fortress_zone";
                        break;
                    case 33:
                        level = "Techno Legacy Zone";
                        image = "techno_legacy_zone";
                        break;

                    //Challenge Stages

                    case 40:
                        level = "Haunted Heights Zone";
                        image = "haunted_heights_zone";
                        break;
                    case 41:
                        level = "Aerial Garden Zone";
                        image = "aerial_garden_zone";
                        break;
                    case 42:
                        level = "Azure Temple Zone";
                        image = "azure_temple_zone";
                        break;

                    //Special Stages

                    case 50:
                        level = "Floral Field Zone";
                        image = "floral_field_zone";
                        break;
                    case 51:
                        level = "Toxic Plateau Zone";
                        image = "toxic_plateau_zone";
                        break;
                    case 52:
                        level = "Flooded Cove Zone";
                        image = "flooded_cove_zone";
                        break;
                    case 53:
                        level = "Cavern Fortress Zone";
                        image = "cavern_fortress_zone";
                        break;
                    case 54:
                        level = "Dusty Wasteland Zone";
                        image = "dusty_wasteland_zone";
                        break;
                    case 55:
                        level = "Magma Caves Zone";
                        image = "magma_caves_zone";
                        break;
                    case 56:
                        level = "Egg Satellite Zone";
                        image = "egg_satellite_zone";
                        break;
                    case 57:
                        level = "Black Hole Zone";
                        image = "black_hole_zone";
                        break;

                    //Nights Bonus Stages

                    case 70:
                        level = "Christmas Chime Zone";
                        image = "christmas_chime_zone";
                        break;
                    case 71:
                        level = "Dream Hill Zone";
                        image = "dream_hill_zone";
                        break;
                    case 72:
                        level = "Alpine Paradise Zone 1";
                        image = "alpine_paradise_zone_1";
                        break;
                    case 73:
                        level = "Alpine Paradise Zone 2";
                        image = "alpine_paradise_zone_2";
                        break;

                    //CTF Stages

                    case 280:
                        level = "Lime Forest Zone";
                        image = "lime_forest_zone";
                        break;
                    case 281:
                        level = "Lost Palace Zone";
                        image = "lost_palace_zone";
                        break;
                    case 282:
                        level = "Silver Cascade Zone";
                        image = "silver_cascade_zone";
                        break;
                    case 283:
                        level = "Icicle Falls Zone";
                        image = "icicle_falls_zone";
                        break;
                    case 284:
                        level = "Twisted Terminal Zone";
                        image = "twisted_terminal_zone";
                        break;
                    case 285:
                        level = "Clockwork Towers Zone";
                        image = "clockwork_towers_zone";
                        break;
                    case 286:
                        level = "Iron Turret Zone";
                        image = "iron_turret_zone";
                        break;
                    case 287:
                        level = "Dual Fortress Zone";
                        image = "dual_fortress_zone";
                        break;
                    case 288:
                        level = "Nimbus Ruins Zone";
                        image = "nimbus_ruins_zone";
                        break;

                    //Multiplayer Stages

                    case 532:
                        level = "Jade Valley Zone";
                        image = "jade_valley_zone";
                        break;
                    case 533:
                        level = "Noxious Factory Zone";
                        image = "noxious_factory_zone";
                        break;
                    case 534:
                        level = "Tidal Palace Zone";
                        image = "tidal_palace_zone";
                        break;
                    case 535:
                        level = "Thunder Citadel Zone";
                        image = "thunder_citadel_zone";
                        break;
                    case 536:
                        level = "Desolate Twilight Zone";
                        image = "desolate_twilight_zone";
                        break;
                    case 537:
                        level = "Infernal Cavern Zone";
                        image = "infernal_cavern_zone";
                        break;
                    case 538:
                        level = "Orbital Hangar Zone";
                        image = "orbital_hangar_zone";
                        break;
                    case 539:
                        level = "Sapphire Falls Zone";
                        image = "sapphire_falls_zone";
                        break;
                    case 540:
                        level = "Diamon Blizzard Zone";
                        image = "diamond_blizzard_zone";
                        break;
                    case 541:
                        level = "Celestial Sanctuary Zone";
                        image = "celestial_sanctuary_zone";
                        break;
                    case 542:
                        level = "Frost Columns Zone";
                        image = "frost_columns_zone";
                        break;
                    case 543:
                        level = "Meadow Match Zone";
                        image = "meadow_match_zone";
                        break;

                    default:
                        level = "Unknown map";
                        image = "unknown";
                        break;

                    //This one is used by the game when transitioning into levels and also when you start the game (intros and stuff)

                    case 0:
                        break;
                }

                switch (buffer2[0])
                {
                    case 0:
                        character = "Sonic";
                        characterImage = "sonic";
                        break;
                    case 1:
                        character = "Tails";
                        characterImage = "tails";
                        break;
                    case 2:
                        character = "Knuckles";
                        characterImage = "knuckles";
                        break;
                    case 3:
                        character = "Amy";
                        characterImage = "amy";
                        break;
                    case 4:
                        character = "Fang";
                        characterImage = "fang";
                        break;
                    case 5:
                        character = "Metal Sonic";
                        characterImage = "metal_sonic";
                        break;

                }
                if (multi == 1)
                {
                    switch (gametype)
                    {
                        case 0:
                            mode = "Co-op";
                            break;
                        case 1:
                            mode = "Competition";
                            break;
                        case 2:
                            mode = "Race";
                            break;
                        case 3:
                            mode = "Match";
                            break;
                        case 4:
                            mode = "Team Match";
                            break;
                        case 5:
                            mode = "Tag";
                            break;
                        case 6:
                            mode = "Hide and Seek";
                            break;
                        case 7:
                            mode = "Capture The Flag";
                            break;
                    }
                }

                RA = buffer4[0];
                IsPlaying = buffer5[0];
                multi = buffer6[0];
                gametype = buffer7[0];
                IsWatching = buffer8[0];
                Update(level, character, image, characterImage);
                previousLevel = level;
                Thread.Sleep(2000);
            }
            Cleanup();
        }
    }
}
