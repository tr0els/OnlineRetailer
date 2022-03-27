using System.Collections.Generic;
using SharedModels;

namespace OrderApi.Infrastructure
{
    public interface IMessagePublisher
    {
        void PublishOrderStatusChangedMessage(int? customerId,
            IList<OrderLineDto> orderLines, string topic);

        void PublishCreditStandingChangedMessage(int customerId, string topic);
    }
}
