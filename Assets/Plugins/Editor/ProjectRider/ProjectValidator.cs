using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ChaosModel.ProjectRider
{
    internal sealed class ProjectValidator
    {
        private readonly string _projectDirectory;
        private readonly string _productName;
        private readonly string _targetVersion;

        internal string SolutionFile
        {
            get { return _projectDirectory + Path.DirectorySeparatorChar + _productName + ".sln"; }
        }

        public ProjectValidator(string projectDirectory, string productName, string targetVersion)
        {
            _projectDirectory = projectDirectory;
            _productName = productName;
            _targetVersion = targetVersion;
        }

        public bool Validate()
        {
            return ValidateProjectFiles() && ValidateDotSettings() && ValidateDebugSettings();
        }

        private bool ValidateProjectFiles()
        {
            if (!File.Exists(SolutionFile) && !SyncSolution())
            {
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

            const string pattern = @"\nUnity(.*)TCP \*:(?<port>\d+)";
            var match = Regex.Match(output, pattern);

            var port = -1;
            if (match.Success)
            {
                int.TryParse(match.Groups["port"].Value,out port);
            }

            return port;
        }

        private const string DotSettingsContent = @"<wpf:ResourceDictionary xml:space=""preserve"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns:s=""clr-namespace:System;assembly=mscorlib"" xmlns:ss=""urn:shemas-jetbrains-com:settings-storage-xaml"" xmlns:wpf=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
                                                                    		<s:String x:Key=""/Default/CodeInspection/CSharpLanguageProject/LanguageLevel/@EntryValue"">CSharp50</s:String></wpf:ResourceDictionary>";
    }
}