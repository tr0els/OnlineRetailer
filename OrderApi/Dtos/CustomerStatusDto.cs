namespace OrderApi.Dtos
{
    public class CustomerStatusDto
    {
        public int Id { get; set; }
        public CreditStanding CreditStanding { get; set; }

    }

    public enum CreditStanding
    {
        Good,
        Bad
    }
}
