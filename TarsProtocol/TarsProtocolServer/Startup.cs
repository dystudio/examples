using DotNetty.Buffers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tars.Net.Clients;
using Tars.Net.Codecs;
using Tars.Net.Configurations;
using Tars.Net.Hosting;
using TarsProtocolCommon;

namespace TarsProtocolServer
{
    public class Startup : IStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.ReigsterRpcClients();
            services.AddTarsCodecs();
            services.AddConfiguration();
        }
    }
}