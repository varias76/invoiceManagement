using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceManagement.Api.Models
{
    public class CreditNote
    {
        public int Id { get; set; } // Clave primaria
        public string CreditNoteNumber { get; set; } = Guid.NewGuid().ToString(); // Generar un GUID por defecto
        public DateTime IssueDate { get; set; } = DateTime.UtcNow; // Fecha de creación automática
        public decimal Amount { get; set; }

        // Clave foránea a Invoice
        public string? InvoiceNumber { get; set; }
        public Invoice? Invoice { get; set; } // Propiedad de navegación
    }
}