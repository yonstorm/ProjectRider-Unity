using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UnityEngine;

namespace ChaosModel.ProjectRider
{
    internal sealed class ProjectValidator
    {
        private readonly string _projectDirectory;
        private readonly string _targetVersion;

        internal string SolutionFile { get; private set; }

        public ProjectValidator(string projectDirectory, string targetVersion)
        {
            _projectDirectory = projectDirectory;
            _targetVersion = targetVersion;
        }

        private bool ResolveSolutionFile()
        {
            var solutionFiles = Directory.GetFiles(_projectDirectory, "*.sln");
            if (solutionFiles.Length > 1)
            {
                foreach (var file in solutionFiles)
                {
                    File.Delete(file);
                }
            }
            else if (solutionFiles.Length == 1)
            {
                SolutionFile = solutionFiles[0];
            }
            else
            {
                if (SyncSolution())
                    SolutionFile = Directory.GetFiles(_projectDirectory, "*.sln")[0];
            }

            return !string.IsNullOrEmpty(SolutionFile) && File.Exists(SolutionFile);
        }

        public bool Validate()
        {
            UnityEngine.Debug.Log("[Rider] Validating...");
            return ValidateProjectFiles() && ValidateDotSettings() && ValidateDebugSettings();
        }

        private bool ValidateProjectFiles()
        {
            UnityEngine.Debug.Log("[Rider] Validating... project files");
            if (!ResolveSolutionFile())
            {
                UnityEngine.Debug.LogError("Could not resolve SolutionFile.");
                return false;
            }

            var projectFiles = Directory.GetFiles(_projectDirectory, "*.csproj");
            foreach (var file in projectFiles)
            {
                ChangeFrameworkVersion(file, _targetVersion);
            }

            return true;
        }

        private bool ValidateDotSettings()
        {
            UnityEngine.Debug.Log("[Rider] Validating... dot settings");
            var projectFiles = Directory.GetFiles(_projectDirectory, "*.csproj");

            foreach (var file in projectFiles)
            {
                var dotSettingsFile = file + ".DotSettings";

                if (File.Exists(dotSettingsFile))
                {
                    continue;
                }

                CreateDotSettingsFile(dotSettingsFile, DotSettingsContent);
            }

            return true;
        }

        private bool ValidateDebugSettings()
        {
            UnityEngine.Debug.Log("[Rider] Validating... debug settings");
            var workspaceFile = _projectDirectory + Path.DirectorySeparatorChar + ".idea" + Path.DirectorySeparatorChar +
                                "workspace.xml";
            if (!File.Exists(workspaceFile))
            {
                // TODO: write workspace settings from a template to be able to write debug settings before Rider is started for the first time.
                return true;
            }

            var document = XDocument.Load(workspaceFile);
            var runManagerElement = (from elem in document.Descendants()
                where elem.Attribute("name") != null && elem.Attribute("name").Value.Equals("RunManager")
                select elem).FirstOrDefault();

            if (runManagerElement == null)
            {
                var projectElement = document.Element("project");
                if (projectElement == null)
                    return false;

                runManagerElement = new XElement("component", new XAttribute("name", "RunManager"));
                projectElement.Add(runManagerElement);
            }

            var editorConfigElem = (from elem in runManagerElement.Descendants()
                where elem.Attribute("name") != null && elem.Attribute("name").Value.Equals("UnityEditor-generated")
                select elem).FirstOrDefault();

            var currentDebugPort = GetDebugPort();
            if (editorConfigElem == null)
            {
                editorConfigElem = new XElement("configuration");
                var defaultAttr = new XAttribute("default", false);
                var nameAttr = new XAttribute("name", "UnityEditor-generated");
                var typeAttr = new XAttribute("type", "ConnectRemote");
                var factoryNameAttr = new XAttribute("factoryName", "Mono remote");
                var showStdErrAttr = new XAttribute("show_console_on_std_err", false);
                var showStdOutAttr = new XAttribute("show_console_on_std_out", true);
                var portAttr = new XAttribute("port", currentDebugPort);
                var addressAttr = new XAttribute("address", "localhost");

                editorConfigElem.Add(defaultAttr, nameAttr, typeAttr, factoryNameAttr, showStdErrAttr, showStdOutAttr,
                    portAttr, addressAttr);

                runManagerElement.Add(new XAttribute("selected", "Mono remote.UnityEditor-generated"));
                runManagerElement.Add(editorConfigElem);
            }
            else
            {
                editorConfigElem.Attribute("port").Value = currentDebugPort.ToString();
            }

            document.Save(workspaceFile);

            // Rider doesn't like it small... :/
            var lines = File.ReadAllLines(workspaceFile);
            lines[0] = lines[0].Replace("utf-8", "UTF-8");
            File.WriteAllLines(workspaceFile, lines);

            return true;
        }

        private static void ChangeFrameworkVersion(string projectFile, string targetVersion)
        {
            var document = XDocument.Load(projectFile);
            var frameworkElement = (from el in document.Descendants()
                where el.Name.LocalName == "TargetFrameworkVersion"
                select el).FirstOrDefault();

            if (frameworkElement == null)
            {
                UnityEngine.Debug.LogWarning("[Rider] Could not find TargetFrameworkVersion in: \n" + projectFile);
                return;
            }

            frameworkElement.Value = targetVersion;

            document.Save(projectFile);
        }

        private static void CreateDotSettingsFile(string dotSettingsFile, string content)
        {
            using (var writer = File.CreateText(dotSettingsFile))
            {
                writer.Write(content);
            }
        }

        private static bool SyncSolution()
        {
            var T = Type.GetType("UnityEditor.SyncVS,UnityEditor");
            if (T == null)
            {
                return false;
            }

            var syncSolution = T.GetMethod("SyncSolution",
                BindingFlags.Public | BindingFlags.Static);
            syncSolution.Invoke(null, null);

            return true;
        }


        private static int GetDebugPort()
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = "lsof",
                    Arguments = "-c /^Unity$/ -i 4tcp -a",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };
            process.Start();

            var output = process.StandardOutput.ReadToEnd();

            process.WaitForExit();

            const string pattern = @"\nUnity(.*)TCP \*:(?<port>\d+)";
            var match = Regex.Match(output, pattern);

            var port = 0;
            if (match.Success)
            {
                int.TryParse(match.Groups["port"].Value, out port);
            }

            return port;
        }

        private const string DotSettingsContent =
            @"<wpf:ResourceDictionary xml:space=""preserve"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns:s=""clr-namespace:System;assembly=mscorlib"" xmlns:ss=""urn:shemas-jetbrains-com:settings-storage-xaml"" xmlns:wpf=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
                                                                    		<s:String x:Key=""/Default/CodeInspection/CSharpLanguageProject/LanguageLevel/@EntryValue"">CSharp50</s:String></wpf:ResourceDictionary>";
    }
}