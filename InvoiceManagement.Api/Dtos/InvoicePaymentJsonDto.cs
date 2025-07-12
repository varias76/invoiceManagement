using System.Text.Json.Serialization;

namespace InvoiceManagement.Api.DTOs
{
    public class InvoicePaymentJsonDto
    {
        [JsonPropertyName("payment_method")]
        public string? PaymentMethod { get; set; } // Puede ser null

        [JsonPropertyName("payment_date")]
        public string? PaymentDate { get; set; } // Puede ser null, lo leeremos como string
    }
}