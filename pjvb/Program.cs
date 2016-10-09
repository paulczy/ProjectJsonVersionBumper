using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using Mono.Options;

namespace pjvb
{
     public class Program
    {
        public static void Main(string[] args)
        {
            var showHelp = false;
            var inputFolder = Environment.CurrentDirectory;
            var version = "0.0.0.0";

            var optionSet = new OptionSet() {
                { "i|input=", "path to project.json", v => inputFolder = v },
                { "v|version=", "version to update in project.json", v => version = v },
                { "h|help",  "show this message and exit", v => showHelp = v != null },
            };

            try
            {
                optionSet.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("greet: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `greet --help' for more information.");
                return;
            }

            //Console.WriteLine($"help:{showHelp}");
            //Console.WriteLine($"input:{inputFolder}");
            //Console.WriteLine($"version:{version}");

            if (showHelp)
            {
                ShowHelp(optionSet);
                return;
            }

            var projectJsonFile = $"{inputFolder}/project.json";

            if (!File.Exists(projectJsonFile))
            {
                Console.WriteLine($"File \"{projectJsonFile}\" not found.");
                return;
            }

            Update(projectJsonFile, version);
        }

        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: pjvb [OPTIONS]+ --version \"x.x.x.x\"");
            Console.WriteLine("Greet a list of individuals with an optional message.");
            Console.WriteLine("If no input is specified, the current working folder is used.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

        private static void Update(string inputFile, string version)
        {
            try
            {
                var json = File.ReadAllText(inputFile);
                var serializer = new JavaScriptSerializer();

                var jsonObject = serializer.DeserializeObject(StripComments(json)) as Dictionary<string, object>;
                if (jsonObject != null)
                {
                    var caseInsensitiveDictionary = new Dictionary<string, object>(jsonObject,
                        StringComparer.OrdinalIgnoreCase)
                    {
                        ["version"] = version
                    };


                    var jsonFile = serializer.Serialize(caseInsensitiveDictionary);


                    File.WriteAllText(inputFile, jsonFile);

                    Console.WriteLine($"project.json version attribute updated to {version}");
                }
                else
                {
                    Console.WriteLine("Unable to parse project.json file");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error - {ex.Message}");
            }
        }

        private static string StripComments(string input)
        {
            input = Regex.Replace(input, @"^\s*//.*$", "", RegexOptions.Multiline);  // removes comments like this
            input = Regex.Replace(input, @"^\s*/\*(\s|\S)*?\*/\s*$", "", RegexOptions.Multiline); /* comments like this */
            return input;
        }
    }
}