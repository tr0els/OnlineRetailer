using HotChocolate;
using System.ComponentModel.DataAnnotations;

namespace CustomerApi.Models
{
    [GraphQLDescription("The customer")]
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        [GraphQLDescription("Address to which invoice is sent")]
        public string BillingAddress { get; set; }
        [GraphQLDescription("Address to which order is shipped")]
        public string ShippingAddress { get; set; }
        public CreditStanding CreditStanding { get; set; }
    }

    public enum CreditStanding
    {
        Good,
        Bad
    }
}
