namespace LexiConnect.Models.VnPay
{
    public class PaymentInformationModel
    {
        public string OrderType { get; set; }
        public decimal Amount { get; set; }
        public string OrderDescription { get; set; }
        public string Name { get; set; }
        public string Currency {  get; set; }
        public int? SubscriptionId { get; set; }
        public int? DurationMonths { get; set; }
    }
}
