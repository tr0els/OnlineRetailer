using System.Collections.Generic;

namespace SharedModels
{
    public class OrderStatusChangedMessage
    {
        public int? CustomerId { get; set; }
        public IList<OrderLineDto> OrderLines { get; set; }
    }
}
