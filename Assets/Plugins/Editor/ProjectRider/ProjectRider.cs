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

		static ProjectRider(){
			Validator = new ProjectValidator(ProjectPath,"v4.0");

		    if (Validator.Validate()) return;

		    Debug.LogError("[ProjectRider] Failed to validate project settings.");
		}

		[UnityEditor.Callbacks.OnOpenAsset]
		private static bool OnAssetOpened(int instanceId, int line){
			var selected = EditorUtility.InstanceIDToObject(instanceId);
			if (!(selected is MonoScript))
			{
				return false;
			}

		    if (!System.IO.File.Exists(Validator.SolutionFile))
		    {
		        Validator.Validate();
		    }
			
			var completeAssetPath = "\"" + ProjectPath + System.IO.Path.DirectorySeparatorChar + AssetDatabase.GetAssetPath(selected) + "\"";
			var args = string.Format(" --line {0} {1}", line, completeAssetPath);

			RiderInstance.Instance(Validator.SolutionFile).OpenRider(args);
			
			return true;
		}

	}
}
