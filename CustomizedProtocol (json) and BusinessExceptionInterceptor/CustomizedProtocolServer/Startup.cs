﻿using DotNetty.Buffers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tars.Net.Codecs;
using Tars.Net.Configurations;
using Tars.Net.Hosting;
using CustomizedProtocolCommon;

namespace CustomizedProtocolServer
{
    public class Startup : IStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //todo: add Decoder and Encoder
            services.TryAddSingleton<IDecoder<IByteBuffer>, TestDecoder>();
            services.TryAddSingleton<IEncoder<IByteBuffer>, TestEncoder>();
            services.AddConfiguration();
        }
    }
}