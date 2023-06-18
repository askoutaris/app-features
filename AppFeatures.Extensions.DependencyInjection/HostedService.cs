using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace AppFeatures.Extensions.DependencyInjection
{
	class AppFeaturesHostedService : IHostedService
	{
		private readonly IFeaturesManager _manager;

		public AppFeaturesHostedService(IFeaturesManager manager)
		{
			_manager = manager;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			return _manager.Init();
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
