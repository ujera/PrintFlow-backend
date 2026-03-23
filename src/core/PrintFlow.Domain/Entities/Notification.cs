using PrintFlow.Domain.Common;
using PrintFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintFlow.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid? OrderId { get; set; }
        public NotificationType Type { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime? SentAt { get; set; }
        public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
        public bool IsRead { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public Order? Order { get; set; }
    }
}
