using Payments.API.Models.Enums;

namespace Payments.API.Models;

public class Payment
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public Guid OrderId { get; set; }
    public PaymentStatus Status { get; set; }
}