﻿using System.Diagnostics;

namespace ChaosModel.ProjectRider{
	internal class RiderInstance {
		private static RiderInstance riderInstance;

		private readonly string _appName;
		private readonly string _baseArgs;

		private RiderInstance(string app, string baseArgs){
			_appName = app;
			_baseArgs = baseArgs;
		}

		internal void OpenRider(string args){
			var process = new Process
			{
				StartInfo =
				{
					FileName = _appName,
					Arguments = _baseArgs + " " + args,
					UseShellExecute = false
				}
			};
			process.Start();
		}


		internal static RiderInstance CreateRiderInstance(string solutionFile)
		{
		    #if UNITY_EDITOR_OSX
			const string app = @"/Applications/Rider EAP.app/Contents/MacOS/rider";
			#elif
			const string app = @"";
			UnityEngine.Debug.LogError("[ProjectRider integration] Only OSX integration supported at the moment.");
			#endif

			var baseArgs = solutionFile;

		    return riderInstance ?? (riderInstance = new RiderInstance(app, baseArgs));
		}
	}
}
