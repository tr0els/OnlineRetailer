using SharedModels;
using System.Collections.Generic;

namespace OrderApi.Models
{
    public class OrderLineConverter : IConverter<OrderLine, OrderLineDto>
    {
        public OrderLine Convert(OrderLineDto sharedOrderLine)
        {
            return new OrderLine
            {
                Id = sharedOrderLine.Id,
                OrderId = sharedOrderLine.OrderId,
                Price = sharedOrderLine.Price,
                ProductId = sharedOrderLine.ProductId,
                Quantity = sharedOrderLine.Quantity
            };
        }

        public OrderLineDto Convert(OrderLine hiddenOrderLine)
        {
            return new OrderLineDto
            {
                Id = hiddenOrderLine.Id,
                OrderId = hiddenOrderLine.OrderId,
                Price = hiddenOrderLine.Price,
                ProductId = hiddenOrderLine.ProductId,
                Quantity = hiddenOrderLine.Quantity
            };
        }

        public IList<OrderLine> ConvertMany(IList<OrderLineDto> models)
        {
            IList<OrderLine> orderLines = new List<OrderLine>();
            foreach (var line in models)
            {
                orderLines.Add(Convert(line));
            }
            return orderLines;
        }

        public IList<OrderLineDto> ConvertMany(IList<OrderLine> models)
        {
            IList<OrderLineDto> orderLines = new List<OrderLineDto>();
            foreach (var line in models)
            {
                orderLines.Add(Convert(line));
            }
            return orderLines;
        }
    }
}
