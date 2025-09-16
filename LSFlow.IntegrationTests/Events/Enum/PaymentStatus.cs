using System.ComponentModel;
using System.Text.Json.Serialization;

namespace LSFlow.IntegrationTests.Events.Enum;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentStatus
{
    [Description("payment.processing")] Processing,
    [Description("payment.confirmed")] Confirmed,
    [Description("payment.rejected")] Rejected
}