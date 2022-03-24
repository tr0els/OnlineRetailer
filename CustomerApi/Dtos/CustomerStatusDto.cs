using CustomerApi.Models;

namespace CustomerApi.Dtos
{
    public class CustomerStatusDto
    {
        public int Id { get; set; }
        public CreditStanding CreditStanding { get; set; }
    }
}
