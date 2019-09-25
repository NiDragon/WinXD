using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace winxxd
{
    class App
    {
        static string legacy = "-l";
        static string multipleDocuments = "-m";
        static string nullTerminated = "-n";
        static string silent = "-s";

        // Args are <Filename.txt> flags output
        // input files are first
        // Flags are always second
        // Output file is always last
        static void Main(string[] args)
        {
            bool multiDoc = false;

            if (args.Count() == 0)
            {
                Console.WriteLine("No arguements detected use ? or help for useage");

                if (!args.Contains(silent))
                {
                    Console.WriteLine("Press Any Key To Exit...");
                    Console.ReadKey(true);
                    return;
                }
            }

            if (args[0].ToLower() == "help" || args[0] == "?" || args[0] == "/?" || args[0] == "-?") {
                printHelp();

                Console.WriteLine("Press Any Key To Exit...");
                Console.ReadKey(true);
                return;
            }

            if (File.Exists(args[0]))
            {
                // Do we have the flag for multiple documents?
                if (args.Contains(multipleDocuments))
                    multiDoc = true;

                // Get the output file to string
                string output = args[args.Count()-1];

                if (output.IndexOf("\\") != -1)
                {
                    int str_target = output.LastIndexOf("\\");
                    output = output.Remove(0, str_target+1);
                }

                // List of files to be read could be one could be many
                List<String> inFiles = new List<String>();

                // Add files to list for parsing
                if (multiDoc)
                {
                    for (int i = 0; i < args.Count() - 1; i++)
                    {
                        if (args[i].Contains('.'))
                            inFiles.Add(args[i]);
                    }
                }
                else
                {
                    inFiles.Add(args[0]);
                }

                // list of finished outputs
                List<String> pointers = new List<String>();

                TextReader tr;

                for (int i = 0; i < inFiles.Count; i++)
                {
                    if (!File.Exists(inFiles[i]))
                        continue;

                    tr = new StreamReader(File.Open(inFiles[i], FileMode.Open), Encoding.UTF8);

                    string input = tr.ReadToEnd();

                    byte[] inbytes = ASCIIEncoding.ASCII.GetBytes(input);

                    string excess = inFiles[i].Remove(inFiles[i].IndexOf('.'));

                    if (excess.IndexOf("\\") != -1)
                    {
                        int str_target = excess.LastIndexOf("\\");
                        excess = excess.Remove(0, str_target + 1);
                    }

                    string outline = String.Format("const unsigned char {0}[] = {{\n", excess);

                    // start a new line every 8 characters
                    int counter = 0;

                    outline += "\t";

                    for (int j = 0; j < inbytes.Count(); j++)
                    {

                        outline += String.Format("0x{0:x2}", inbytes[j]);

                        if(j != inbytes.Count()-1)
                            outline += ", ";

                        counter++;

                        if (counter == 8 && j != (inbytes.Count()-1))
                        {
                            outline += "\n\t";
                            counter = 0;
                        }
                    }

                    if (args.Contains(nullTerminated))
                    {
                        outline += ", 0x00\n};\n";
                    }
                    else
                    {
                        outline += "\n};";
                    }

                    if ((args.Contains(legacy) && !args.Contains(nullTerminated)) ||
                        (!args.Contains(legacy) && !args.Contains(nullTerminated)))
                        outline += String.Format("\nunsigned int {0}_len = {1};\n", excess, inbytes.Length);

                    pointers.Add(outline);

                    tr.Close();
                    tr.Dispose();
                }

                output = output.Remove(output.IndexOf('.'));
                output += ".h";

                // Create writer to write our output file
                TextWriter tw = new StreamWriter(File.Open("./"+output, FileMode.Create), Encoding.UTF8);

                string tag = output.Remove(output.IndexOf('.'));

                tw.WriteLine("#ifndef __{0}_H__", tag);
                tw.WriteLine("#define __{0}_H__ \n", tag);

                foreach (String pointer in pointers)
                {
                    tw.Write(pointer);
                    tw.Write("\n");
                }

                tw.Write("#endif // __{0}_H__", tag);

                tw.Close();
                tw.Dispose();

                Console.WriteLine("File save complete!");

                if (!args.Contains(silent))
                {
                    Console.WriteLine("Press Any Key To Exit...");
                    Console.ReadKey(true);
                }
            }
            else
            {
                Console.WriteLine("Invalid input file: {0}", args[0]);
                if (!args.Contains(silent))
                {
                    Console.WriteLine("Press Any Key To Exit...");
                    Console.ReadKey(true);
                }
            }
        }

        private static void printHelp()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;
            
            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine("   winxxd Copyright 2015 Joshua Perry Version: {0}", version);
            Console.WriteLine("");
            Console.WriteLine("   Useage: Compile text files into hex character arrays.");
            Console.WriteLine("   Output C++ headers");
            Console.WriteLine("   Arguments: <filenames> -flags <outfile>");
            Console.WriteLine("");
            Console.WriteLine("   -l - Generate byte counts for arrays. (default)");
            Console.WriteLine("   -n - Terminate all arrays with a NULL 0x00 byte.");
            Console.WriteLine("   -m - Compile multiple documents into single header file.");
            Console.WriteLine("   -s - Silent mode program exits after compile.");
            Console.WriteLine("------------------------------------------------------------");
        }
    }
}
