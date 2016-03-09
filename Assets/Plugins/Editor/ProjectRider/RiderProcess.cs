using System.Diagnostics;

namespace ChaosModel.ProjectRider{
	internal class RiderInstance {
		private static RiderInstance riderInstance;

		private string _appName;
		private string _baseArgs;

		private RiderInstance(string app, string baseArgs){
			this._appName = app;
			this._baseArgs = baseArgs;
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


		internal static RiderInstance CreateRiderInstance(string solutionFile){
			var app = @"/Applications/Rider EAP.app/Contents/MacOS/rider";
			var baseArgs = solutionFile;

			if(riderInstance == null){
				riderInstance = new RiderInstance(app,baseArgs);
			}

			return riderInstance;
		}
	}
}
