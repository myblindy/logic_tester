using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using tester_rpi;
using static tester_rpi.RPiProgram;

namespace tester_rpi_web.Hub
{
    public class RTUpdatesHubService : BackgroundService
    {
        private readonly IHubContext<RTUpdatesHub> _hubContext;
        public RTUpdatesHubService(IHubContext<RTUpdatesHub> hubContext)
        {
            _hubContext = hubContext;
        }

        Thread Runner;

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Runner = new Thread(() => RpiMain(null)) { IsBackground = true, Name = "RPi Runner Thread" };
            Runner?.Start();
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(1000);

            var sb = new StringBuilder();

            while (!stoppingToken.IsCancellationRequested)
            {
                string LightsToString(Light[] lights)
                {
                    sb.Clear();
                    lights.ForEach(l => sb.Append(l.Active ? '1' : '0'));
                    return sb.ToString();
                }

                await _hubContext.Clients.All.SendAsync("Update", LightsToString(Inputs), LightsToString(Outputs),
                    RPiProgram.Logic.Starea, RPiProgram.Logic.Region);

                await Task.Delay(30);
            }
        }
    }
}
