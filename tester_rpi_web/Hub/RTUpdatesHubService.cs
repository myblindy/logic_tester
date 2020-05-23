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

        Thread LogicRunner, UIRunner;

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            LogicInitialized = new AutoResetEvent(false);
            LogicRunner = new Thread(() => RpiMain(null)) { IsBackground = true, Name = "RPi Runner Thread" };
            LogicRunner?.Start();

            LogicInitialized.WaitOne();
            tester_rpi_LCD.Program.Logic = Logic;
            UIRunner = new Thread(() => tester_rpi_LCD.Program.Main(Array.Empty<string>())) { IsBackground = true, Name = "LCD Runner Thread" };
            UIRunner?.Start();

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

                await _hubContext.Clients.All.SendAsync("Update", LightsToString(Logic.Inputs), LightsToString(Logic.Outputs),
                    Logic.Starea, Logic.Region, Logic.Counter);

                await Task.Delay(30);
            }
        }
    }
}
