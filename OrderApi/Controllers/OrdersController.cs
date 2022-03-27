using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Data;
using OrderApi.Infrastructure;
using OrderApi.Models;
using SharedModels;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository repository;
        private readonly IServiceGateway<ProductDto> productServiceGateway;
        private readonly IServiceGateway<CustomerStatusDto> customerServiceGateway;
        private readonly IMessagePublisher messagePublisher;
        private readonly IConverter<OrderLine, OrderLineDto> orderLineConverter;

        public OrdersController(IRepository<Order> repos,
            IServiceGateway<ProductDto> productGateway,
            IServiceGateway<CustomerStatusDto> customerGateway,
            IMessagePublisher publisher,
            IConverter<OrderLine, OrderLineDto> converter)
        {
            repository = repos as IOrderRepository;
            productServiceGateway = productGateway;
            customerServiceGateway = customerGateway;
            messagePublisher = publisher;
            orderLineConverter = converter;
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
        [HttpGet("bycustomer/{customerId}", Name = "GetByCustomer")]
        public IActionResult GetByCustomer(int customerId)
        {
            if (customerId <= 0)
            {
                return BadRequest("Customer ID must be greater than 0.");
            }

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
                        order.CustomerId, orderLineConverter.ConvertMany(order.OrderLines), "completed");

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
            return customerServiceGateway.Get(order.CustomerId).GoodCreditStanding;
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
                            order.CustomerId, orderLineConverter.ConvertMany(order.OrderLines), "cancelled");

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
                messagePublisher.PublishOrderStatusChangedMessage(
                    order.CustomerId, orderLineConverter.ConvertMany(order.OrderLines), "shipped");                      

                // Update order status to shipped.
                order.Status = Order.OrderStatus.Shipped;
                repository.Edit(order);

                // If customer credit standing is good, change it to bad
                if (CustomerStatusGood(order))
                {
                    messagePublisher.PublishCreditStandingChangedMessage(
                            order.CustomerId, "creditchanged");
                }

                return Ok();
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
                // Update order status to paid
                order.Status = Order.OrderStatus.Paid;
                repository.Edit(order);
                
                // If this payment means customer no longer has unpaid shipped orders,
                // change their credit standing
                if (!CustomerStatusGood(order) && CustomerHasNoUnpaidShippedOrders(order.CustomerId))
                {
                    messagePublisher.PublishCreditStandingChangedMessage(
                            order.CustomerId, "creditchanged");
                }

                return Ok();
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

        private bool CustomerHasNoUnpaidShippedOrders(int customerId)
        {
            List<Order> customerOrders = repository.GetByCustomer(customerId) as List<Order>;

            foreach (var order in customerOrders)
            {
                if (order.Status == Order.OrderStatus.Shipped)
                {
                    return false;
                }
            }

            return true;
        }
    }

}

