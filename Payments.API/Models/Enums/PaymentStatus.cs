using System.Text.Json.Serialization;

namespace Payments.API.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentStatus
{
    Pending,
    Approved,
    Rejected
}