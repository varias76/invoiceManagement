using System.Text.Json.Serialization;

namespace InvoiceManagement.Api.DTOs
{
    public class CreditNoteJsonDto
    {
        [JsonPropertyName("credit_note_number")]
        public int CreditNoteNumber { get; set; }

        [JsonPropertyName("credit_note_date")]
        public string CreditNoteDate { get; set; } = string.Empty; // Lo leeremos como string

        [JsonPropertyName("credit_note_amount")]
        public decimal CreditNoteAmount { get; set; }
    }
}