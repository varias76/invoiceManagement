using InvoiceManagement.Api.Data;
using InvoiceManagement.Api.Models;
using InvoiceManagement.Api.DTOs;
using Microsoft.EntityFrameworkCore; // Necesario para .Any() y .Add()
using System.Text.Json; // Necesario para JsonSerializer
using System.IO; // Necesario para File.ReadAllTextAsync
using System.Linq; // Necesario para .Sum()

namespace InvoiceManagement.Api.Services
{
    public class InvoiceImportService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InvoiceImportService> _logger; // Para logs

        public InvoiceImportService(ApplicationDbContext context, ILogger<InvoiceImportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> ImportInvoicesFromJsonAsync(string jsonFilePath)
        {
            _logger.LogInformation($"Iniciando importación de facturas desde: {jsonFilePath}");
            var importedCount = 0;
            var inconsistentCount = 0;
            var duplicateCount = 0;
            var totalInvoicesInFile = 0;

            if (!File.Exists(jsonFilePath))
            {
                _logger.LogError($"El archivo JSON no se encontró en la ruta: {jsonFilePath}");
                return "Error: Archivo JSON no encontrado.";
            }

            try
            {
                var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                var rootDto = JsonSerializer.Deserialize<RootJsonDto>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // Esto permite que el JSON tenga snake_case y las propiedades C# PascalCase sin [JsonPropertyName]
                                                       // Aunque ya lo usamos con [JsonPropertyName], esto es una buena práctica.
                });

                if (rootDto == null || !rootDto.Invoices.Any())
                {
                    _logger.LogWarning("El archivo JSON está vacío o no contiene facturas.");
                    return "No se encontraron facturas en el archivo JSON.";
                }

                totalInvoicesInFile = rootDto.Invoices.Count;

                // Usamos un HashSet para verificar unicidad eficientemente
                var existingInvoiceNumbers = await _context.Invoices
                                                          .Select(i => i.InvoiceNumber)
                                                          .ToListAsync();
                var processedInvoiceNumbers = new HashSet<string>(existingInvoiceNumbers);

                foreach (var jsonInvoice in rootDto.Invoices)
                {
                    var invoiceNumber = jsonInvoice.InvoiceNumber.ToString();

                    // 1. Validar invoice_number único
                    if (processedInvoiceNumbers.Contains(invoiceNumber))
                    {
                        _logger.LogWarning($"Factura con número '{invoiceNumber}' ya existe. Se omitirá.");
                        duplicateCount++;
                        continue; // Saltar esta factura si es duplicada
                    }

                    // 2. Coherencia entre suma de subtotales de productos y total_amount
                    decimal calculatedTotal = jsonInvoice.InvoiceDetail?.Sum(p => p.Subtotal) ?? 0;
                    bool isConsistent = (calculatedTotal == jsonInvoice.TotalAmount);

                    if (!isConsistent)
                    {
                        inconsistentCount++;
                        _logger.LogWarning($"Factura '{invoiceNumber}' es inconsistente. Total calculado: {calculatedTotal}, Total declarado: {jsonInvoice.TotalAmount}. Se marcará como inconsistente.");
                        // No la excluimos del sistema activo AHORA, solo la marcamos como inconsistente.
                        // El requisito dice "excluir del sistema activo (pero mantener en base para reporte)".
                        // Esto se manejará en las consultas (vistas) más adelante.
                    }

                    // Mapeo de DTO a Modelo de BD
                    var invoice = new Invoice
                    {
                        InvoiceNumber = invoiceNumber,
                        IssueDate = DateTime.Parse(jsonInvoice.InvoiceDate??"" ),
                        PaymentDueDate = DateTime.Parse(jsonInvoice.PaymentDueDate??""),
                        TotalAmount = jsonInvoice.TotalAmount,
                        IsConsistent = isConsistent, // Establece la consistencia basada en la validación
                        Status = "Issued" // Estado inicial por defecto, se recalculará si tiene NCs
                    };

                    // Mapear productos
                    foreach (var jsonProduct in jsonInvoice.InvoiceDetail ?? new List<InvoiceDetailJsonDto>()) //
                    {
                        invoice.Products.Add(new InvoiceProduct
                        {
                            Description = jsonProduct.ProductName,
                            Quantity = jsonProduct.Quantity,
                            UnitPrice = jsonProduct.UnitPrice,
                            Subtotal = jsonProduct.Subtotal
                            // InvoiceNumber y Invoice se setearán automáticamente por EF Core al guardar la Invoice
                        });
                    }

                    // Mapear notas de crédito (si existen)
                    decimal totalCreditNotesAmount = 0;
                    if (jsonInvoice.InvoiceCreditNote != null && jsonInvoice.InvoiceCreditNote.Any())
                    {
                        foreach (var jsonCreditNote in jsonInvoice.InvoiceCreditNote)
                        {
                            invoice.CreditNotes.Add(new CreditNote
                            {
                                CreditNoteNumber = jsonCreditNote.CreditNoteNumber.ToString(),
                                IssueDate = DateTime.Parse(jsonCreditNote.CreditNoteDate),
                                Amount = jsonCreditNote.CreditNoteAmount
                            });
                            totalCreditNotesAmount += jsonCreditNote.CreditNoteAmount;
                        }
                    }

                    // 3. Calcular de forma automática el estado de facturas:
                    // "Issued" sin notas de crédito.
                    // "Cancelled" suma montos NC igual al monto total de la factura.
                    // "Partial" suma de montos NC es menor al monto total.
                    if (totalCreditNotesAmount > 0)
                    {
                        if (totalCreditNotesAmount >= invoice.TotalAmount) // Usamos >= por si hay un pequeño desfase decimal o si cancela exactamente el total
                        {
                            invoice.Status = "Cancelled";
                        }
                        else
                        {
                            invoice.Status = "Partial";
                        }
                    }
                    // Si totalCreditNotesAmount es 0, Status sigue siendo "Issued" por defecto (línea 86)

                    // 4. Calcular de forma automática el estado de pago de la factura:
                    // "Pending" pago pendiente dentro del plazo.
                    // "Overdue" si a la fecha se encuentra vencida (payment_due_date).
                    // "Paid" pago registrado.

                    // Primero, si hay un pago registrado en el JSON, es "Paid"
                    if (jsonInvoice.InvoicePayment != null && !string.IsNullOrEmpty(jsonInvoice.InvoicePayment.PaymentMethod) && !string.IsNullOrEmpty(jsonInvoice.InvoicePayment.PaymentDate))
                    {
                        invoice.PaymentStatus = "Paid";
                    }
                    else // Si no hay pago registrado, calcular Pending/Overdue
                    {
                        // Se asume la fecha actual para determinar si está vencida
                        // Usaremos DateTime.UtcNow para consistencia. Puedes usar DateTime.Today si prefieres la fecha local sin hora.
                        var currentDate = DateTime.UtcNow.Date;
                        var paymentDueDate = invoice.PaymentDueDate.Date;

                        if (currentDate > paymentDueDate)
                        {
                            invoice.PaymentStatus = "Overdue";
                        }
                        else
                        {
                            invoice.PaymentStatus = "Pending";
                        }
                    }

                    _context.Invoices.Add(invoice);
                    processedInvoiceNumbers.Add(invoiceNumber); // Añadir al set para futuras comprobaciones de duplicados dentro del mismo archivo
                    importedCount++;
                }

                await _context.SaveChangesAsync(); // Guarda todos los cambios en la base de datos

                _logger.LogInformation($"Importación finalizada. Total de facturas en archivo: {totalInvoicesInFile}. Importadas: {importedCount}. Duplicadas omitidas: {duplicateCount}. Inconsistentes: {inconsistentCount}.");

                return $"Importación finalizada. Total de facturas en archivo: {totalInvoicesInFile}. Importadas: {importedCount}. Duplicadas omitidas: {duplicateCount}. Inconsistentes: {inconsistentCount}.";
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error de formato JSON al importar facturas.");
                return $"Error: Formato JSON inválido. Detalles: {ex.Message}";
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Error al parsear fechas o números del JSON.");
                return $"Error: Formato de datos incorrecto en JSON (fechas/números). Detalles: {ex.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado durante la importación de facturas.");
                return $"Error inesperado: {ex.Message}";
            }
        }
    }
}