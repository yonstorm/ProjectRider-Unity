using System.Diagnostics;

namespace ChaosModel.ProjectRider{
	internal class RiderProcess {
		private static RiderInstance riderInstance;

		internal class RiderInstance{
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
		}

		internal static RiderInstance CreateRiderIntance(string solutionFile){
			var app = "";
			var baseArgs = solutionFile;

			if(riderInstance == null){
				riderInstance = new RiderInstance(app,baseArgs);
			}

			return riderInstance;
		}
	}
}
