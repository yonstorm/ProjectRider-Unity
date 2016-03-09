using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ChaosModel.ProjectRider{

	[InitializeOnLoad]
	public static class ProjectRider{

		public static string ProjectPath
		{
			get { return System.IO.Path.GetDirectoryName(Application.dataPath); }
		}

		private static ProjectValidator _validator;
		private static RiderInstance _riderInstance;

		static ProjectRider(){
			Debug.Log("Rider integration initializing");
			_validator = new ProjectValidator(ProjectPath,PlayerSettings.productName,"v4.0");
			if(!_validator.Validate()){
				Debug.LogError("[ProjectRider] Failed to validate project settings");
				return;
			}

			_riderInstance = RiderInstance.CreateRiderInstance(_validator.SolutionFile);
		}

		[UnityEditor.Callbacks.DidReloadScripts]
		private static void Revalidate(){
			_validator.Validate();
		}

		[UnityEditor.Callbacks.OnOpenAsset]
		private static bool OnAssetOpened(int instanceId, int line){
			var selected = EditorUtility.InstanceIDToObject(instanceId);
			if (!(selected is MonoScript))
			{
				return false;
			}
			
			var completeAssetPath = ProjectPath + System.IO.Path.DirectorySeparatorChar + AssetDatabase.GetAssetPath(selected);
			var args = string.Format(@"{0} --line {1} {2}", _validator.SolutionFile, line, completeAssetPath);
			
			_riderInstance.OpenRider(args);
			
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
