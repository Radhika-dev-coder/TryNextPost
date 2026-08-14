using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.Common.Settings
{
    public class IndiaPostSettings : ICourierSettings
    {
        public string? BaseUrl { get; set; }

        public string? ApiKey { get; set; }

        public string? ApiSecret { get; set; }

        public string? AccountCode { get; set; }

        public bool Enabled { get; set; } = true;
    }
}
