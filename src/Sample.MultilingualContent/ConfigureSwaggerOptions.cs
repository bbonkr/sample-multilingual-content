using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using kr.bbon.AspNetCore;
using kr.bbon.AspNetCore.Options;

using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;

namespace Sample.MultilingualContent
{
    public class ConfigureSwaggerOptions : ConfigureSwaggerOptionsBase
    {
        public ConfigureSwaggerOptions(IOptionsMonitor<AppOptions> appOptionsAccessor, IApiVersionDescriptionProvider provider)
            : base(provider)
        {
            this.options = appOptionsAccessor.CurrentValue;
        }

        public override string AppTitle { get => options.Title; }

        private readonly AppOptions options;
    }
}
