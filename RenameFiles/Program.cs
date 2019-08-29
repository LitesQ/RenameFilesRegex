using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace RenameFiles
{
    class Program
    {
        /// <summary>
        /// params:
        ///     --rg=
        ///     --rg2=
        ///     --src=
        ///     --dest=
        ///     --mask=
        ///     --help
        /// options:
        ///     -s - rewrite
        ///     -r - перенос с сохранением иерархии в dest
        ///     -R - игнор иерархии и перенос все кучей в dest
        /// </summary>

        static void Main(string[] args)
        {
            string[] Config = File.ReadAllLines(Application.StartupPath + @"\config.ini",Encoding.GetEncoding(1251));
            string[] Params = new string [Config.Length];
            string[] Splitter;

            for (int i = 0; i < Config.Length; i++)
            {
                if (Config[i].Contains("=:~"))
                {
                    Splitter = Config[i].Split(new[] { "=:~" }, StringSplitOptions.RemoveEmptyEntries);
                    Params[i] = Splitter[1];
                }
            }
            string Rg = Params[0];
            string Rg2 = Params[1];
            string Src = Params[2];
            string Dest = Params[3];
            string Mask = Params[4];

            bool Rewrite = false;
            bool Recursive = false;
            bool RecursiveAllInOne = false;

            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].Contains('='))
                    {
                        Splitter = args[i].Split('=');
                        switch (Splitter[0])
                        {
                            case "--rg":
                                Rg = Splitter[1];
                                break;
                            case "--rg2":
                                Rg2 = Splitter[1];
                                break;
                            case "--src":
                                Src = Splitter[1];
                                break;
                            case "--dest":
                                Dest = Splitter[1];
                                break;
                            case "--mask":
                                Mask = Splitter[1];
                                break;
                            default:
                                Console.WriteLine("Incorrect params. Type --help for help");
                                return;
                        }
                    }
                    else if (!(args[i].Contains('=')))
                    {
                        switch (args[i])
                        {
                            case "-s":
                                Rewrite = true;
                                break;
                            case "-r":
                                Recursive = true;
                                break;
                            case "-R":
                                RecursiveAllInOne = true;
                                break;
                            case "--help":
                                Console.WriteLine("\nRenameFilesRegexp.exe [params] [options]\nParams:\n\t--rg=regex_formula\n\t--rg2=regex_formula\n\t--src=source_path\n\t--dest=destination_path\nOptions:\n\t-s - rewrite files\n\t-r - recursive, save structure\n\t-R - recursive, put all files in one folder\nIf you dont enter the params they will be taken from config.ini");
                                return;
                            default:
                                Console.WriteLine("Incorrect options. Type --help for help");
                                return;
                        }
                    }
                }
            }

            if (!Dest.Contains(":"))
                Dest = Src + @"\" + Dest;

            if (Recursive == true && RecursiveAllInOne == true) { Console.WriteLine("Error. -r and -R cant be used together"); return; }

            else if (Recursive == false && RecursiveAllInOne == false)
            {
                RenameFiles(Src, Mask, Rg, Rg2, Dest, Rewrite);
            }
            else if (Recursive == true && RecursiveAllInOne == false)
            {
                string[] list_dirs;
                try
                {
                    list_dirs = Directory.GetDirectories(Src, "*", SearchOption.AllDirectories);
                }
                catch (System.IO.DirectoryNotFoundException ex)
                {
                    Console.WriteLine("Incorrect src\n" + ex.Message);
                    return;
                }
                string[] new_dirs = new string[list_dirs.Length];

                for (int i =0; i < list_dirs.Length; i++)
                {
                    new_dirs[i] = list_dirs[i].Remove(0, Src.Length);
                    Directory.CreateDirectory(Dest + new_dirs[i]);
                    RenameFiles(list_dirs[i], Mask, Rg, Rg2, Dest + new_dirs[i], Rewrite);
                }
                RenameFiles(Src, Mask, Rg, Rg2, Dest, Rewrite);
            }
            else if (Recursive == false && RecursiveAllInOne == true)
            {
                List <string> list_dirs = new List<string>();
                string[] all_dirs;
                try
                {
                    all_dirs = Directory.GetDirectories(Src, "*", SearchOption.AllDirectories);
                }
                catch (System.IO.DirectoryNotFoundException ex)
                {
                    Console.WriteLine("Incorrect src\n" + ex.Message);
                    return;
                }
                for (int i = 0; i < all_dirs.Length; i++)
                {
                    list_dirs.Add(all_dirs[i]);
                }
                list_dirs.Add(Src);
                RenameFiles(list_dirs, Mask, Rg, Rg2, Dest, Rewrite);
            }


        }

        static void RenameFiles(string Src, string Mask, string Rg, string Rg2, string Dest, bool Rewrite)
        {
            FileInfo Finfo;
            List<string> SrcPaths;
            try
            {
                if (Mask != "all")
                    SrcPaths = Directory.GetFiles(Src, Mask).ToList();
                else
                    SrcPaths = Directory.GetFiles(Src).ToList();
            }
            catch(System.IO.DirectoryNotFoundException ex)
            {
                Console.WriteLine("Incorrect src\n" + ex.Message);
                return;
            }

            for (int i = 0; i < SrcPaths.Count; i++)
            {
                if (SrcPaths[i].Contains("Thumbs.db"))
                {
                    SrcPaths.RemoveAt(i);
                }
            }

            string[] FileNames = new string[SrcPaths.Count];

            Regex regex;

            try
            {
                regex = new Regex(Rg);
            }
            catch (System.ArgumentException ex)
            {
                Console.WriteLine("Incorrect rg2 \n" + ex.Message);
                return;
            }

            for (int i = 0; i < FileNames.Length; i++)
            {
                Finfo = new FileInfo(SrcPaths[i]);
                FileNames[i] = Finfo.Name;
                if (regex.IsMatch(FileNames[i]))
                {
                    FileNames[i] = regex.Replace(FileNames[i], String.Empty);
                }
            }

            try
            {
                regex = new Regex(Rg2);
            }
            catch(System.ArgumentException ex)
            {
                Console.WriteLine("Incorrect rg2 \n" + ex.Message);
                return;
            }

            int counter = 0;
            string RegFile = String.Empty;
            List<string> VendorCodes = new List<string>();
            List<int> CodesCounters = new List<int>();
            int buff_counter = 0;
            bool Exists = false;

            for (int i = 0; i < FileNames.Length; i++)
            {
                if (regex.IsMatch(FileNames[i]))
                {
                    RegFile = regex.Replace(FileNames[i], String.Empty);
                }

                if (i == 0)
                {
                    VendorCodes.Add(RegFile);

                    for (int k = 0; k < FileNames.Length; k++)
                    {
                        if (FileNames[k].Contains(RegFile))
                        {
                            counter++;
                        }
                    }

                    CodesCounters.Add(counter);
                }

                if (i != 0)
                {
                    for (int k = 0; k < CodesCounters.Count; k++)
                    {
                        if (VendorCodes[k] == RegFile)
                        {
                            Exists = true;
                            buff_counter = k;
                            break;
                        }
                    }
                    if (Exists)
                    {
                        counter = CodesCounters[buff_counter];
                    }
                    else
                    {
                        VendorCodes.Add(RegFile);

                        for (int k = 0; k < FileNames.Length; k++)
                        {
                            if (FileNames[k].Contains(RegFile))
                            {
                                counter++;
                            }
                        }
                        CodesCounters.Add(counter);

                        for (int k = 0; k < CodesCounters.Count; k++)
                        {
                            if (VendorCodes[k] == RegFile)
                            {
                                counter = CodesCounters[k];
                                break;
                            }
                        }
                    }
                }

                for (int j = 0; j < FileNames.Length; j++)
                {
                    if (FileNames[i] == FileNames[j] && i != j)
                    {
                        counter++;
                        if (counter <= 9)
                        {
                            FileNames[j] = RegFile + "_0" + (counter);
                        }

                        else if (counter >= 10)
                        {
                            FileNames[j] = RegFile + "_" + (counter);
                        }
                    }
                }

                if (Exists)
                {
                    CodesCounters[buff_counter] = counter;
                }
                else
                {
                    for (int k = 0; k < CodesCounters.Count; k++)
                    {
                        if (VendorCodes[k] == RegFile)
                        {
                            CodesCounters[k] = counter;
                            break;
                        }
                    }
                }

                counter = 0;
                Exists = false;
                buff_counter = 0;
            }

            if (!Directory.Exists(Dest))
                Directory.CreateDirectory(Dest);

            for (int i = 0; i < FileNames.Length; i++)
            {
                Console.WriteLine(SrcPaths[i]);
                Finfo = new FileInfo(SrcPaths[i]);
                try
                {
                    File.Copy(SrcPaths[i], Dest + @"\" + FileNames[i] + Finfo.Extension, Rewrite);
                }
                catch (System.IO.IOException) { Console.WriteLine("File(-s) exists. Use -s for overwrite"); Console.Write("Error! Press any key to exit..."); Console.ReadKey(); return; }
            }
        }
        static void RenameFiles(List <string> Src, string Mask, string Rg, string Rg2, string Dest, bool Rewrite)
        {
            FileInfo Finfo;
            List<string> SrcPaths = new List<string>();
            string[] buffer;

            try
            {
                for (int i = 0; i < Src.Count; i++)
                {
                    if (Mask != "all")
                        buffer = Directory.GetFiles(Src[i], Mask);
                    else
                        buffer = Directory.GetFiles(Src[i]);

                    for (int k = 0; k < buffer.Length; k++)
                    {
                        SrcPaths.Add(buffer[k]);
                    }
                }
            }
            catch (System.IO.DirectoryNotFoundException ex)
            {
                Console.WriteLine("Incorrect src\n" + ex.Message);
                return;
            }

            for (int i = 0; i < SrcPaths.Count; i++)
            {
                if (SrcPaths[i].Contains("Thumbs.db"))
                {
                    SrcPaths.RemoveAt(i);
                }
            }

            string[] FileNames = new string[SrcPaths.Count];

            Regex regex;

            try
            {
                regex = new Regex(Rg);
            }
            catch (System.ArgumentException ex)
            {
                Console.WriteLine("Incorrect rg2 \n" + ex.Message);
                return;
            }

            for (int i = 0; i < FileNames.Length; i++)
            {
                Finfo = new FileInfo(SrcPaths[i]);
                FileNames[i] = Finfo.Name;
                if (regex.IsMatch(FileNames[i]))
                {
                    FileNames[i] = regex.Replace(FileNames[i], String.Empty);
                }
            }

            try
            {
                regex = new Regex(Rg2);
            }
            catch (System.ArgumentException ex)
            {
                Console.WriteLine("Incorrect rg2 \n" + ex.Message);
                return;
            }

            int counter = 0;
            string RegFile = String.Empty;
            List<string> VendorCodes = new List<string>();
            List<int> CodesCounters = new List<int>();
            int buff_counter = 0;
            bool Exists = false;

            for (int i = 0; i < FileNames.Length; i++)
            {
                if (regex.IsMatch(FileNames[i]))
                {
                    RegFile = regex.Replace(FileNames[i], String.Empty);
                }

                if (i == 0)
                {
                    VendorCodes.Add(RegFile);

                    for (int k = 0; k < FileNames.Length; k++)
                    {
                        if (FileNames[k].Contains(RegFile))
                        {
                            counter++;
                        }
                    }

                    CodesCounters.Add(counter);
                }

                if (i != 0)
                {
                    for (int k = 0; k < CodesCounters.Count; k++)
                    {
                        if (VendorCodes[k] == RegFile)
                        {
                            Exists = true;
                            buff_counter = k;
                            break;
                        }
                    }
                    if (Exists)
                    {
                        counter = CodesCounters[buff_counter];
                    }
                    else
                    {
                        VendorCodes.Add(RegFile);

                        for (int k = 0; k < FileNames.Length; k++)
                        {
                            if (FileNames[k].Contains(RegFile))
                            {
                                counter++;
                            }
                        }
                        CodesCounters.Add(counter);

                        for (int k = 0; k < CodesCounters.Count; k++)
                        {
                            if (VendorCodes[k] == RegFile)
                            {
                                counter = CodesCounters[k];
                                break;
                            }
                        }
                    }
                }

                for (int j = 0; j < FileNames.Length; j++)
                {
                    if (FileNames[i] == FileNames[j] && i != j)
                    {
                        counter++;
                        if (counter <= 9)
                        {
                            FileNames[j] = RegFile + "_0" + (counter);
                        }

                        else if (counter >= 10)
                        {
                            FileNames[j] = RegFile + "_" + (counter);
                        }
                    }
                }

                if (Exists)
                {
                    CodesCounters[buff_counter] = counter;
                }
                else
                {
                    for (int k = 0; k < CodesCounters.Count; k++)
                    {
                        if (VendorCodes[k] == RegFile)
                        {
                            CodesCounters[k] = counter;
                            break;
                        }
                    }
                }

                counter = 0;
                Exists = false;
                buff_counter = 0;
            }

            if (!Directory.Exists(Dest))
                Directory.CreateDirectory(Dest);

            for (int i = 0; i < FileNames.Length; i++)
            {
                Console.WriteLine(SrcPaths[i]);
                Finfo = new FileInfo(SrcPaths[i]);
                try
                {
                   File.Copy(SrcPaths[i], Dest + @"\" + FileNames[i] + Finfo.Extension, Rewrite);
                }
                catch (System.IO.IOException) { Console.WriteLine("File(-s) exists. Use -s for overwrite"); Console.Write("Error! Press any key to exit..."); Console.ReadKey(); return; }
            }
        }
    }
}
