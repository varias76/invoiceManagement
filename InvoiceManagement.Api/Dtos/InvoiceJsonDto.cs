using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace InvoiceManagement.Api.DTOs
{
    public class InvoiceJsonDto
    {
        [JsonPropertyName("invoice_number")]
        public int InvoiceNumber { get; set; }

        [JsonPropertyName("invoice_date")]
        public string? InvoiceDate { get; set; } // Lo leeremos como string y luego lo parsearemos a DateTime

        [JsonPropertyName("invoice_status")]
        public string? InvoiceStatus { get; set; }

        [JsonPropertyName("total_amount")]
        public decimal TotalAmount { get; set; }

        [JsonPropertyName("days_to_due")]
        public int DaysToDue { get; set; }

        [JsonPropertyName("payment_due_date")]
        public string? PaymentDueDate { get; set; } // Lo leeremos como string y luego lo parsearemos a DateTime

        [JsonPropertyName("payment_status")]
        public string? PaymentStatus { get; set; }

        [JsonPropertyName("invoice_detail")]
        public List<InvoiceDetailJsonDto> InvoiceDetail { get; set; } = new List<InvoiceDetailJsonDto>();

        [JsonPropertyName("invoice_payment")]
        public InvoicePaymentJsonDto? InvoicePayment { get; set; }

        [JsonPropertyName("invoice_credit_note")]
        public List<CreditNoteJsonDto> InvoiceCreditNote { get; set; } = new List<CreditNoteJsonDto>();

        [JsonPropertyName("customer")]
        public CustomerJsonDto? Customer { get; set; }
    }
}