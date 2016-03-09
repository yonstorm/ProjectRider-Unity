using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ChaosModel.ProjectRider{
	[InitializeOnLoad]
	public static class ProjectRider{
		static ProjectRider(){
			Debug.Log("Rider integration initializing");
		}

		[UnityEditor.Callbacks.OnOpenAsset]
		private static bool OnAssetOpened(int instanceId, int line){
			return false;
		}



	}
}
