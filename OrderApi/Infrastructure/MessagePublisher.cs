using System;
using System.Collections.Generic;
using EasyNetQ;
using SharedModels;

namespace OrderApi.Infrastructure
{
    public class MessagePublisher : IMessagePublisher, IDisposable
    {
        IBus bus;

        public MessagePublisher(string connectionString)
        {
            bus = RabbitHutch.CreateBus(connectionString);
        }

        public void Dispose()
        {
            bus.Dispose();
        }

        public void PublishCreditStandingChangedMessage(int customerId, decimal payment, string topic)
        {
            var message = new CreditStandingChangedMessage
            {
                CustomerId = customerId,
                Payment = payment
            };

            bus.PubSub.Publish(message, topic);
        }

        public void PublishOrderStatusChangedMessage(int? customerId, IList<OrderLine> orderLines, string topic)
        {
            var message = new OrderStatusChangedMessage
            { 
                CustomerId = customerId,
                OrderLines = orderLines 
            };

            bus.PubSub.Publish(message, topic);
        }

    }
}
