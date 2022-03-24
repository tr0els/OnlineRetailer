namespace CustomerApi.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string BillingAddress { get; set; }
        public string ShippingAddress { get; set; }
        // CreditStanding could presently be implemented as a bool property instead,
        // however using an enum gives us the opportunity to add more credit levels later
        public CreditStanding CreditStanding { get; set; }
    }

    public enum CreditStanding
    {
        Good,
        Bad
    }
}
