using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceManagement.Api.Models
{
    public class InvoiceProduct
    {
        public int Id { get; set; } // Clave primaria
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; } // Quantity * UnitPrice

        // Clave foránea a Invoice
        public string? InvoiceNumber { get; set; }
        public Invoice? Invoice { get; set; } // Propiedad de navegación
    }
}