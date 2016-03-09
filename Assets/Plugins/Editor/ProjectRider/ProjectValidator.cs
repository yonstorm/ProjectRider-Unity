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

		private string SolutionFile{
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

			var dotSettingsContent = Base64Decode(encodedDotSettingsContent);
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

		private static string Base64Decode(string base64EncodedData) {
			var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
			return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
		}

		private readonly string encodedDotSettingsContent = "PHdwZjpSZXNvdXJjZURpY3Rpb25hcnkgeG1sOnNwYWNlPSIicHJlc2VydmUiIiB4bWxuczp4PSIi" +
				"aHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93aW5meC8yMDA2L3hhbWwiIiB4bWxuczpzPSIi" +
				"Y2xyLW5hbWVzcGFjZTpTeXN0ZW07YXNzZW1ibHk9bXNjb3JsaWIiIiB4bWxuczpzcz0iInVybjpz" +
				"aGVtYXMtamV0YnJhaW5zLWNvbTpzZXR0aW5ncy1zdG9yYWdlLXhhbWwiIiB4bWxuczp3cGY9IiJo" +
				"dHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dpbmZ4LzIwMDYveGFtbC9wcmVzZW50YXRpb24i" +
				"Ij4gICAgICAgICAgICAgICAgCQk8czpTdHJpbmcgeDpLZXk9IiIvRGVmYXVsdC9Db2RlSW5zcGVj" +
				"dGlvbi9DU2hhcnBMYW5ndWFnZVByb2plY3QvTGFuZ3VhZ2VMZXZlbC9ARW50cnlWYWx1ZSIiPkNT" +
				"aGFycDUwPC9zOlN0cmluZz48L3dwZjpSZXNvdXJjZURpY3Rpb25hcnk+";
		
	}
}