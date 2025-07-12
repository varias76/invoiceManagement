using System;
using System.Collections.Generic;

namespace InvoiceManagement.Api.DTOs
{
    // DTO para la respuesta de una factura, sin referencias circulares de vuelta
    public class InvoiceResponseDto
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime PaymentDueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public bool IsConsistent { get; set; }
        public decimal OutstandingAmount { get; set; } // Propiedad calculada

        public List<InvoiceProductResponseDto> Products { get; set; } = new List<InvoiceProductResponseDto>();
        public List<CreditNoteResponseDto> CreditNotes { get; set; } = new List<CreditNoteResponseDto>();

        // No incluimos Customer aquí para simplificar por ahora, pero podrías agregarlo si es necesario.
        // public CustomerResponseDto? Customer { get; set; } // Si tuvieras un CustomerResponseDto
    }

    // DTO para los productos de la factura en la respuesta
    public class InvoiceProductResponseDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }

        // NOTA CLAVE: NO incluimos la propiedad 'Invoice Invoice { get; set; }' aquí.
        // Esto rompe el ciclo de referencia.
    }

    // DTO para las notas de crédito en la respuesta
    public class CreditNoteResponseDto
    {
        public int Id { get; set; }
        public string CreditNoteNumber { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public decimal Amount { get; set; }

        // NOTA CLAVE: NO incluimos la propiedad 'Invoice Invoice { get; set; }' aquí.
        // Esto rompe el ciclo de referencia.
    }
    
   // ... (otras clases DTO existentes en InvoiceResponseDto.cs) ...

    public class PaymentStatusSummaryDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class OverallPaymentSummaryDto
    {
        public int TotalInvoices { get; set; }
        public List<PaymentStatusSummaryDto> Summaries { get; set; } = new List<PaymentStatusSummaryDto>();
    }

}   