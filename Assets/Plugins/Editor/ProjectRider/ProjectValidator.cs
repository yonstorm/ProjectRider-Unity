using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ChaosModel.ProjectRider
{
	internal sealed class ProjectValidator{
		private string projectDirectory;
		private string _productName;
		private string _targetVersion;

		internal string SolutionFile{
			get { return projectDirectory + Path.DirectorySeparatorChar + _productName + ".sln"; }
		}

		public ProjectValidator(string projectDirectory, string productName, string targetVersion){
			this.projectDirectory = projectDirectory;
			this._productName = productName;
			this._targetVersion = targetVersion;
		}

		public bool Validate(){
			return ValidateProjectFiles() && ValidateDotSettings() && ValidateDebugSettings();
		}

		private bool ValidateProjectFiles(){
			if(!File.Exists(SolutionFile) && !SyncSolution()){
				return false;
			}

			var projectFiles = Directory.GetFiles(projectDirectory, "*.csproj");
			foreach(var file in projectFiles){
				ChangeFrameworkVersion(file, _targetVersion);
			}
			
			return true;
		}

		private bool ValidateDotSettings(){
			var projectFiles = Directory.GetFiles(projectDirectory, "*.csproj");

			foreach(var file in projectFiles){
				var dotSettingsFile = file + ".DotSettings";
				
				if(File.Exists(dotSettingsFile)){
					continue;
				}
				
				CreateDotSettingsFile(dotSettingsFile,dotSettingsContent);
			}
			
			return true;
		}

		private bool ValidateDebugSettings(){
			return true;
		}

		private static void ChangeFrameworkVersion (string projectFile, string targetVersion)
		{
			var document = XDocument.Load(projectFile);
			var frameworkElement = (from el in document.Descendants() 
			                        where el.Name.LocalName == "TargetFrameworkVersion" 
			                        select el).FirstOrDefault();

			if(frameworkElement == null){
				return;
			}

			frameworkElement.Value = targetVersion;

			document.Save(projectFile);
		}

		private void CreateDotSettingsFile (string dotSettingsFile, string content)
		{
			using (var writer = File.CreateText(dotSettingsFile))
			{
				writer.Write(content);
			}		

		}

		private static bool SyncSolution(){
			var T = System.Type.GetType("UnityEditor.SyncVS,UnityEditor");
			if(T == null){
				return false;
			}

			var syncSolution = T.GetMethod("SyncSolution", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
			syncSolution.Invoke(null, null);

			return true;
		}

		private readonly string dotSettingsContent = @"<wpf:ResourceDictionary xml:space=""preserve"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns:s=""clr-namespace:System;assembly=mscorlib"" xmlns:ss=""urn:shemas-jetbrains-com:settings-storage-xaml"" xmlns:wpf=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
                                                                    		<s:String x:Key=""/Default/CodeInspection/CSharpLanguageProject/LanguageLevel/@EntryValue"">CSharp50</s:String></wpf:ResourceDictionary>";
		
	}
}