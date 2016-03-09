using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ChaosModel.ProjectRider{
	[InitializeOnLoad]
	public static class ProjectRider{
		private static ProjectValidator _validator;

		static ProjectRider(){
			Debug.Log("Rider integration initializing");
			_validator = new ProjectValidator(ProjectPath,PlayerSettings.productName,"v4.0");
			if(!_validator.Validate()){
				Debug.LogError("[ProjectRider] Failed to validate project settings");
			}
		}

		[UnityEditor.Callbacks.DidReloadScripts]
		private static void Revalidate(){
			_validator.Validate();
		}

		[UnityEditor.Callbacks.OnOpenAsset]
		private static bool OnAssetOpened(int instanceId, int line){
			return false;
		}

		public static string ProjectPath
		{
			get { return System.IO.Path.GetDirectoryName(Application.dataPath); }
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
