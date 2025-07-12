using System.ComponentModel.DataAnnotations;

namespace InvoiceManagement.Api.DTOs
{
    public class AddCreditNoteDto
    {
        [Required(ErrorMessage = "El número de factura es requerido.")] // Indica que InvoiceNumber es obligatorio
        public string InvoiceNumber { get; set; } = string.Empty; // Número de la factura a la que se aplica la NC

        [Required(ErrorMessage = "El monto de la nota de crédito es requerido.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero.")] // Valida que el monto sea positivo
        public decimal Amount { get; set; } // Monto de la nota de crédito
    }
}