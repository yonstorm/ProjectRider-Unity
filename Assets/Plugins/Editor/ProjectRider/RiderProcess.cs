using System;
using System.Diagnostics;

namespace ChaosModel.ProjectRider{
	internal class RiderInstance {
		private static RiderInstance _riderInstance;

		private readonly string _appName;
		private readonly string _baseArgs;

		private RiderInstance(string app, string baseArgs){
			_appName = app;
			_baseArgs = "\"" + baseArgs + "\"";
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


		internal static RiderInstance Instance(string solutionFile)
		{
		    #if UNITY_EDITOR_OSX
			const string app = @"/Applications/Rider EAP.app/Contents/MacOS/rider";
			#elif
			const string app = @"";
			throw new PlatformNotSupportedException("[ProjectRider integration] Only OSX integration supported at the moment.");
			#endif

		    return _riderInstance ?? (_riderInstance = new RiderInstance(app, solutionFile));
		}
	}
}
