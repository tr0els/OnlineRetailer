﻿using System.Collections.Generic;
using SharedModels;

namespace OrderApi.Infrastructure
{
    public interface IMessagePublisher
    {
        void PublishOrderStatusChangedMessage(int? customerId,
            IList<OrderLine> orderLines, string topic);

        void PublishCreditStandingChangedMessage(int customerId,
            decimal payment, string topic);
    }
}
