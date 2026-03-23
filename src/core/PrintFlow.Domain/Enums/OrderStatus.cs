using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintFlow.Domain.Enums
{
    public enum OrderStatus
    {
        Created = 0,
        AwaitingPayment = 1,
        Paid = 2,
        PaymentFailed = 3,
        InProduction = 4,
        QualityCheck = 5,
        ReadyForPickup = 6,
        Completed = 7,
        Cancelled = 8
    }
}
