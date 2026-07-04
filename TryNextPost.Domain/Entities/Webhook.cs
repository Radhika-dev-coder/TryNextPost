using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Domain.Entities
{
    public class Webhook
    {
        public string WebhookId { get; set; } = Guid.NewGuid().ToString();

        public string Source { get; set; } = string.Empty; // e.g. Delhivery, XpressBees

        public string EventType { get; set; } = string.Empty; // e.g. Delivered, InTransit

        public string Payload { get; set; } = string.Empty; // JSON data

        public string Status { get; set; } = "Pending"; // Processed / Failed

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
