using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.Common.Settings
{
    public class SmsSettings
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string SenderId { get; set; }
        public string TemplateId { get; set; }
        public string BaseUrl { get; set; }
    }
}
