using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq; // Necesario para .Sum() en OutstandingAmount

namespace InvoiceManagement.Api.Models
{
    public class Invoice
    {
        [Key] // Marca InvoiceNumber como clave primaria
        public string InvoiceNumber { get; set; } = Guid.NewGuid().ToString(); // Generar un GUID por defecto
        public DateTime IssueDate { get; set; }
        public DateTime PaymentDueDate { get; set; }
        public decimal TotalAmount { get; set; }

        // Relación con los productos de la factura
        public ICollection<InvoiceProduct> Products { get; set; } = new List<InvoiceProduct>();

        // Propiedades calculadas o de estado
        public string Status { get; set; } = "Issued"; // "Issued", "Cancelled", "Partial"
        public string PaymentStatus { get; set; } = "Pending"; // "Pending", "Overdue", "Paid"
        public bool IsConsistent { get; set; } = true; // true si TotalAmount coincide con la suma de subtotales de productos

        // Relación con las notas de crédito
        public ICollection<CreditNote> CreditNotes { get; set; } = new List<CreditNote>();

        // Propiedad calculada no mapeada a la base de datos, para el saldo pendiente
        [NotMapped] // Indica a EF Core que esta propiedad no debe ser una columna en la tabla
        public decimal OutstandingAmount
        {
            get
            {
                // Asegúrate de que CreditNotes no sea null antes de usar Sum
                decimal totalCreditNotes = CreditNotes?.Sum(cn => cn.Amount) ?? 0;
                return TotalAmount - totalCreditNotes;
            }
        }
    }
}