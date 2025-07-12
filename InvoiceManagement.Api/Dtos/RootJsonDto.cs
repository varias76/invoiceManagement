using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace InvoiceManagement.Api.DTOs
{
    public class RootJsonDto
    {
        [JsonPropertyName("invoices")]
        public List<InvoiceJsonDto> Invoices { get; set; } = new List<InvoiceJsonDto>();
    }
}