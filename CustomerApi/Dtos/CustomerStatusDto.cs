using CustomerApi.Models;

namespace CustomerApi.Dtos
{
    public class CustomerStatusDto
    {
        public int Id { get; set; }
        public decimal CreditStanding { get; set; }
    }
}
