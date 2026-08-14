using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.Common.Settings
{
    public class DtdcSettings :ICourierSettings
    {
        public string? BaseUrl { get; set; }

        public string? ApiKey { get; set; }

        public string? ApiSecret { get; set; }

        public bool Enabled { get; set; } = true;
    }
}
