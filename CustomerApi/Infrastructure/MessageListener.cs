﻿using System;
using System.Threading;
using CustomerApi.Data;
using CustomerApi.Models;
using EasyNetQ;
using Microsoft.Extensions.DependencyInjection;
using SharedModels;

namespace CustomerApi.Infrastructure
{
    public class MessageListener
    {
        IServiceProvider provider;
        string connectionString;

        // The service provider is passed as a parameter, because the class needs
        // access to the product repository. With the service provider, we can create
        // a service scope that can provide an instance of the product repository.
        public MessageListener(IServiceProvider provider, string connectionString)
        {
            this.provider = provider;
            this.connectionString = connectionString;
        }

        public void Start()
        {
            using (var bus = RabbitHutch.CreateBus(connectionString))
            {
                bus.PubSub.Subscribe<CreditStandingChangedMessage>("customerApiHkCompleted", 
                    HandleOrderPaid, x => x.WithTopic("creditchanged"));

                // Block the thread so that it will not exit and stop subscribing.
                lock (this)
                {
                    Monitor.Wait(this);
                }
            }

        }

        private void HandleOrderPaid(CreditStandingChangedMessage message)
        {
            // A service scope is created to get an instance of the customer repository.
            // When the service scope is disposed, the customer repository instance will
            // also be disposed.
            using (var scope = provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var repository = services.GetService<IRepository<Customer>>();

                var customer = repository.Get(message.CustomerId);
                customer.CreditStanding = customer.CreditStanding == CreditStanding.Good 
                    ? CreditStanding.Bad : CreditStanding.Good;
                repository.Edit(customer);
            }
        }

    }
}
