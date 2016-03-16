using UnityEngine;
using UnityEditor;

/*
ProjectRider - Unity integration
Version 0.1.1
*/

namespace ChaosModel.ProjectRider{

	[InitializeOnLoad]
	public static class ProjectRider{

		public static string ProjectPath
		{
			get { return System.IO.Path.GetDirectoryName(Application.dataPath); }
		}

		private static readonly ProjectValidator Validator;
		private static readonly RiderInstance RiderInstance;

		static ProjectRider(){
			Validator = new ProjectValidator(ProjectPath,PlayerSettings.productName,"v4.0");
			if(!Validator.Validate()){
				Debug.LogError("[ProjectRider] Failed to validate project settings");
				return;
			}

			RiderInstance = RiderInstance.CreateRiderInstance(Validator.SolutionFile);
		}

		private static void Revalidate(){
			Validator.Validate();
		}

		[UnityEditor.Callbacks.OnOpenAsset]
		private static bool OnAssetOpened(int instanceId, int line){
			var selected = EditorUtility.InstanceIDToObject(instanceId);
			if (!(selected is MonoScript))
			{
				return false;
			}
			
			var completeAssetPath = "\"" + ProjectPath + System.IO.Path.DirectorySeparatorChar + AssetDatabase.GetAssetPath(selected) + "\"";
			var args = string.Format("{0} --line {1} {2}", Validator.SolutionFile, line, completeAssetPath);
			
			RiderInstance.OpenRider(args);
			
			return true;
		}

		public class RiderAssetPostProcessor : AssetPostprocessor
		{
			public static void OnGeneratedCSProjectFiles()
			{
				Revalidate();
			}
		}

	}
}
