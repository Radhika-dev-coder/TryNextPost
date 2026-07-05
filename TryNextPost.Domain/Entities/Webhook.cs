using System;
using System.ComponentModel.DataAnnotations;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.Entities
{
    public class Webhook
    {
        [Key]
        public long WebhookId { get; set; } 
        public string Source { get; set; } = string.Empty;     // e.g. Delhivery, XpressBees
        public string EventType { get; set; } = string.Empty;  // e.g. Delivered, InTransit
        public string Payload { get; set; } = string.Empty;    // JSON data
        public WebhookStatus Status { get; set; } = WebhookStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}