using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Data;
using OrderApi.Dtos;
using OrderApi.Infrastructure;
using OrderApi.Models;
using SharedModels;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        IOrderRepository repository;
        IServiceGateway<ProductDto> productServiceGateway;
        IServiceGateway<CustomerStatusDto> customerServiceGateway;
        IMessagePublisher messagePublisher;

        public OrdersController(IRepository<Order> repos,
            IServiceGateway<ProductDto> productGateway,
            IServiceGateway<CustomerStatusDto> customerGateway,
            IMessagePublisher publisher)
        {
            repository = repos as IOrderRepository;
            productServiceGateway = productGateway;
            customerServiceGateway = customerGateway;
            messagePublisher = publisher;
        }

        // GET orders
        [HttpGet]
        public IEnumerable<Order> Get()
        {
            return repository.GetAll();
        }

        // GET orders/5
        [HttpGet("{id}", Name = "GetOrder")]
        public IActionResult Get(int id)
        {
            var item = repository.Get(id);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
        }

        // GET orders by customer
        [HttpGet("bycustomer/{id}", Name = "GetByCustomer")]
        public IActionResult GetByCustomer(int customerId)
        {
            try
            {
                // will throw exception if customer does not exist
                customerServiceGateway.Get(customerId);
                return new ObjectResult(repository.GetByCustomer(customerId));
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(e.Message);
            }          
            
        }

        // POST orders
        [HttpPost]
        public IActionResult Post([FromBody]Order order)
        {
            if (order == null)
            {
                return BadRequest();
            }

            try
            {
                if (!CustomerStatusGood(order))
                {
                    return StatusCode(500, "Customer not in good credit standing.");
                }
            }
            catch (KeyNotFoundException e)
            {
                return StatusCode(404, e.Message);
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }            

            try
            {
                if (ProductItemsAvailable(order))
                {                    
                    // Publish OrderStatusChangedMessage. If this operation
                    // fails, the order will not be created
                    messagePublisher.PublishOrderStatusChangedMessage(
                        order.CustomerId, order.OrderLines, "completed");

                    // Create order.
                    order.Status = Order.OrderStatus.Completed;
                    var newOrder = repository.Add(order);
                    return CreatedAtRoute("GetOrder", new { id = newOrder.Id }, newOrder);                   
                }
                else
                {
                    // If there are not enough product items available.
                    return StatusCode(500, "Not enough items in stock.");
                }
            }
            catch (KeyNotFoundException e)
            {
                return StatusCode(404, e.Message);
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error happened. Try again.");
            }

        }

        private bool ProductItemsAvailable(Order order)
        {
            foreach (var orderLine in order.OrderLines)
            {
                if (orderLine.ProductId <= 0)
                {
                    throw new ArgumentException("Product ID must be greater than 0.");
                }

                if (orderLine.Quantity <= 0)
                {
                    throw new ArgumentException("Quantity ordered must be greater than 0.");
                }
                
                // Call product service to get the product ordered.
                var orderedProduct = productServiceGateway.Get(orderLine.ProductId);
                if (orderLine.Quantity > orderedProduct.ItemsInStock - orderedProduct.ItemsReserved)
                {
                    return false;
                }
                orderLine.Price = orderedProduct.Price;
            }
            return true;
        }

        private bool CustomerStatusGood(Order order)
        {
            if (order.CustomerId <= 0)
            {
                throw new ArgumentException("Customer ID must be greater than 0.");
            }
            return customerServiceGateway.Get(order.CustomerId).CreditStanding >= 0;
        }

        // PUT orders/5/cancel
        // This action method cancels an order and publishes an OrderStatusChangedMessage
        // with topic set to "cancelled".
        [HttpPut("{id}/cancel")]
        public IActionResult Cancel(int id)
        {
            var order = repository.Get(id);
            if (order == null)
            {
                return NotFound();
            }
            if (order.Status != Order.OrderStatus.Completed)
            {
                return BadRequest("Can only cancel completed orders");
            }

            try
            {
                messagePublisher.PublishOrderStatusChangedMessage(
                            order.CustomerId, order.OrderLines, "cancelled");

                // Update order status to cancelled
                order.Status = Order.OrderStatus.Cancelled;
                repository.Edit(order);
                return Ok();
            }
            catch
            {
                return StatusCode(500, "An error happened. Try again.");
            }
        }

        // PUT orders/5/ship
        // This action method ships an order and publishes an OrderStatusChangedMessage.
        // with topic set to "shipped".
        [HttpPut("{id}/ship")]
        public IActionResult Ship(int id)
        {
            var order = repository.Get(id);
            if (order == null)
            {
                return NotFound();
            }
            if (order.Status != Order.OrderStatus.Completed)
            {
                return BadRequest("Can only ship completed orders");
            }

            try
            {              
                // Publish OrderStatusChangedMessage. If this operation
                // fails, the order will not be created
                messagePublisher.PublishOrderStatusChangedMessage(order.CustomerId, order.OrderLines, "shipped");

                messagePublisher.PublishCreditStandingChangedMessage(
                            order.CustomerId, -CalculatePayment(order), "creditchanged");

                // Update order status to shipped.
                order.Status = Order.OrderStatus.Shipped;
                repository.Edit(order);
                return Ok();
            }
            catch
            {
                return StatusCode(500, "An error happened. Try again.");
            }
        }

        // PUT orders/5/pay
        // This action method marks an order as paid and publishes a CreditStandingChangedMessage
        // (which have not yet been implemented), if the credit standing changes.
        [HttpPut("{id}/pay")]
        public IActionResult Pay(int id)
        {
            var order = repository.Get(id);
            if (order == null)
            {
                return NotFound();
            }
            if (order.Status != Order.OrderStatus.Shipped)
            {
                return BadRequest("Can only pay for shipped orders");
            }

            try
            {
                messagePublisher.PublishCreditStandingChangedMessage(
                            order.CustomerId, CalculatePayment(order), "creditchanged");

                // Update order status to paid
                order.Status = Order.OrderStatus.Paid;
                repository.Edit(order);
                return Ok();
            }
            catch
            {
                return StatusCode(500, "An error happened. Try again.");
            }
        }

        private decimal CalculatePayment(Order order)
        {
            decimal totalPayment = 0;
            foreach (var line in order.OrderLines)
            {
                totalPayment += line.Quantity * line.Price;
            }
            return totalPayment;
        }
    }

}

