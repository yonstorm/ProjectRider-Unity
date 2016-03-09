using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ChaosModel.ProjectRider
{
	internal class ProjectValidator{
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
			return false;
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

		}

		private bool ValidateDebugSettings(){
			return false;
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

		void CreateDotSettingsFile (string dotSettingsFile)
		{
			throw new NotImplementedException ();
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
	}
}