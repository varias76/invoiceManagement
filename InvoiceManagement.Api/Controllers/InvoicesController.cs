using Microsoft.AspNetCore.Mvc;
using InvoiceManagement.Api.Data;
using InvoiceManagement.Api.Models;
using Microsoft.EntityFrameworkCore; // Necesario para .ToListAsync(), .Where()
using System.Linq;
using InvoiceManagement.Api.DTOs; // Necesario para .Linq queries

namespace InvoiceManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Ruta base: /api/Invoices
    public class InvoicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InvoicesController> _logger; // Para logging

        // Constructor con Inyección de Dependencias
        public InvoicesController(ApplicationDbContext context, ILogger<InvoicesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las facturas del sistema.
        /// </summary>
        [HttpGet] // GET /api/Invoices
        public async Task<ActionResult<IEnumerable<Invoice>>> GetAllInvoices()
        {
            _logger.LogInformation("Obteniendo todas las facturas.");
            // Incluimos productos y notas de crédito para que vengan con la factura
            return await _context.Invoices
                                 .Include(i => i.Products)
                                 .Include(i => i.CreditNotes)
                                 .ToListAsync();
        }

        /// <summary>
        /// Busca una factura por su número.
        /// </summary>
        /// <param name="invoiceNumber">El número de la factura a buscar.</param>
        /// <returns>La factura encontrada o NotFound si no existe.</returns>
        [HttpGet("{invoiceNumber}")] // GET /api/Invoices/{invoiceNumber}
        public async Task<ActionResult<Invoice>> GetInvoiceByNumber(string invoiceNumber)
        {
            _logger.LogInformation($"Buscando factura por número: {invoiceNumber}");
            var invoice = await _context.Invoices
                                        .Include(i => i.Products)
                                        .Include(i => i.CreditNotes)
                                        .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);

            if (invoice == null)
            {
                _logger.LogWarning($"Factura con número {invoiceNumber} no encontrada.");
                return NotFound(); // Retorna 404 Not Found
            }

            return invoice; // Retorna 200 OK con la factura
        }

        /// <summary>
        /// Busca facturas por estado de factura y/o estado de pago.
        /// </summary>
        /// <param name="status">Estado de la factura (Issued, Cancelled, Partial).</param>
        /// <param name="paymentStatus">Estado de pago (Pending, Overdue, Paid).</param>
        /// <returns>Lista de facturas que coinciden con los criterios.</returns>
        [HttpGet("search")] // GET /api/Invoices/search?status=...&paymentStatus=...
        public async Task<ActionResult<IEnumerable<Invoice>>> SearchInvoices(
            [FromQuery] string? status,
            [FromQuery] string? paymentStatus)
        {
            _logger.LogInformation($"Buscando facturas por estado: {status ?? "N/A"}, estado de pago: {paymentStatus ?? "N/A"}");

            IQueryable<Invoice> query = _context.Invoices
                                              .Include(i => i.Products)
                                              .Include(i => i.CreditNotes);

            // Filtrar por estado de factura si se proporciona
            if (!string.IsNullOrEmpty(status))
            {
                // Convertimos a minúsculas para una búsqueda insensible a mayúsculas/minúsculas
                query = query.Where(i => i.Status.ToLower() == status.ToLower());
            }

            // Filtrar por estado de pago si se proporciona
            if (!string.IsNullOrEmpty(paymentStatus))
            {
                query = query.Where(i => i.PaymentStatus.ToLower() == paymentStatus.ToLower());
            }

            var invoices = await query.ToListAsync();

            if (!invoices.Any())
            {
                _logger.LogInformation("No se encontraron facturas con los criterios de búsqueda especificados.");
                return NotFound("No se encontraron facturas que coincidan con los criterios de búsqueda."); // Retorna 404 si no hay resultados
            }

            return Ok(invoices); // Retorna 200 OK con la lista de facturas
        }

        // ... código existente de InvoicesController ...

        /// <summary>
        /// Agrega una nota de crédito a una factura existente.
        /// Valida que el monto de la NC no supere el saldo pendiente.
        /// Actualiza el estado de la factura (Cancelled/Partial) y el estado de pago.
        /// </summary>
        /// <param name="dto">Datos de la nota de crédito a agregar.</param>
        /// <returns>La factura actualizada o BadRequest si hay un error.</returns>
        [HttpPost("credit-note")] // POST /api/Invoices/credit-note
        public async Task<IActionResult> AddCreditNote([FromBody] AddCreditNoteDto dto)
        {
            _logger.LogInformation($"Intentando agregar nota de crédito de {dto.Amount} a factura {dto.InvoiceNumber}");

            // Validación automática del modelo (requeridos, rangos, etc.)
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modelo de AddCreditNoteDto inválido.");
                return BadRequest(ModelState); // Retorna 400 Bad Request con detalles de validación
            }

            // 1. Encontrar la factura
            var invoice = await _context.Invoices
                                        .Include(i => i.CreditNotes) // Es crucial cargar las NCs para calcular el saldo pendiente
                                        .FirstOrDefaultAsync(i => i.InvoiceNumber == dto.InvoiceNumber);

            if (invoice == null)
            {
                _logger.LogWarning($"Factura con número '{dto.InvoiceNumber}' no encontrada para agregar nota de crédito.");
                return NotFound(new { Message = $"Factura con número '{dto.InvoiceNumber}' no encontrada." });
            }

            // Si la factura ya está cancelada, no se pueden agregar más NCs
            if (invoice.Status == "Cancelled")
            {
                _logger.LogWarning($"No se puede agregar NC a la factura {dto.InvoiceNumber} porque ya está Cancelled.");
                return BadRequest(new { Message = "No se puede agregar una nota de crédito a una factura ya cancelada." });
            }

            // 2. Validar que el monto de NC no supere el saldo pendiente
            // La propiedad OutstandingAmount se calcula automáticamente en el modelo Invoice
            if (dto.Amount > invoice.OutstandingAmount)
            {
                _logger.LogWarning($"Monto de NC ({dto.Amount}) supera el saldo pendiente ({invoice.OutstandingAmount}) para factura {dto.InvoiceNumber}.");
                return BadRequest(new { Message = $"El monto de la nota de crédito (${dto.Amount}) excede el saldo pendiente (${invoice.OutstandingAmount}) de la factura." });
            }

            // 3. Crear y agregar la nueva nota de crédito
            var newCreditNote = new CreditNote
            {
                InvoiceNumber = invoice.InvoiceNumber,
                Amount = dto.Amount,
                IssueDate = DateTime.UtcNow // Fecha de creación automática
                // CreditNoteNumber se genera por defecto en el modelo
            };

            invoice.CreditNotes.Add(newCreditNote);

            // 4. Actualizar el estado de la factura (Status y PaymentStatus)
            // Recalcular el saldo pendiente después de agregar la nueva NC
            // La propiedad OutstandingAmount se recalcula automáticamente al accederla
            if (invoice.OutstandingAmount <= 0) // Si el saldo pendiente es 0 o negativo después de la NC
            {
                invoice.Status = "Cancelled";
                invoice.PaymentStatus = "Paid"; // Si está cancelada por NC, se considera "pagada"
            }
            else
            {
                invoice.Status = "Partial"; // Si aún queda saldo, es "Partial"
            }

            // 5. Guardar los cambios en la base de datos
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Nota de crédito agregada exitosamente a la factura {dto.InvoiceNumber}. Nuevo estado: {invoice.Status}, Saldo pendiente: {invoice.OutstandingAmount}.");

            // Retornar la factura actualizada para reflejar los cambios
            // Podríamos retornar un DTO más ligero si la respuesta es solo de éxito
                  var response = new InvoiceResponseDto
            {
                InvoiceNumber = invoice.InvoiceNumber,
                IssueDate = invoice.IssueDate,
                PaymentDueDate = invoice.PaymentDueDate,
                TotalAmount = invoice.TotalAmount,
                Status = invoice.Status,
                PaymentStatus = invoice.PaymentStatus,
                IsConsistent = invoice.IsConsistent,
                OutstandingAmount = invoice.OutstandingAmount,
                Products = invoice.Products.Select(p => new InvoiceProductResponseDto
                {
                    Id = p.Id,
                    Description = p.Description ?? string.Empty,
                    Quantity = p.Quantity,
                    UnitPrice = p.UnitPrice,
                    Subtotal = p.Subtotal
                }).ToList(),
                CreditNotes = invoice.CreditNotes.Select(cn => new CreditNoteResponseDto
                {
                    Id = cn.Id,
                    CreditNoteNumber = cn.CreditNoteNumber,
                    IssueDate = cn.IssueDate,
                    Amount = cn.Amount
                }).ToList()
            };

            return Ok(invoice); // Retorna 200 OK con la factura actualizada
        }
        /// <summary>
        /// Obtiene facturas consistentes, vencidas por más de 30 días y sin pagos registrados o notas de crédito.
        /// </summary>
        /// <returns>Lista de facturas que cumplen los criterios del reporte.</returns>

        // ... (código existente de InvoicesController) ...
        // ... (código existente de InvoicesController) ...

        [HttpGet("report/overdue-unpaid")] // GET /api/Invoices/report/overdue-unpaid
        public async Task<ActionResult<IEnumerable<InvoiceResponseDto>>> GetOverdueUnpaidConsistentInvoices()
        {
            _logger.LogInformation("Generando reporte de facturas consistentes, vencidas y sin pago/NC.");

            var currentDate = DateTime.UtcNow.Date; // Obtener la fecha actual una sola vez

            // PASO 1: Traer los datos relevantes de la base de datos con filtros que SÍ se pueden traducir a SQL
            // Esta vez, solo filtramos por 'IsConsistent' en la consulta a la DB
            var allEligibleInvoices = await _context.Invoices
                                        .Include(i => i.CreditNotes)
                                        .Include(i => i.Products)
                                        .Where(i => i.IsConsistent) // Solo filtrar por IsConsistent en la consulta de DB
                                        .ToListAsync(); // <-- Ejecutar la consulta de DB aquí

            // PASO 2: Filtrar los resultados en memoria (en C#)
            var filteredInvoices = allEligibleInvoices
                                        .Where(i => i.PaymentStatus == "Overdue" && // Filtrar PaymentStatus en C#
                                                    (currentDate - i.PaymentDueDate.Date).TotalDays > 30 && // Comparación de fechas en C#
                                                    i.CreditNotes.Count == 0 && // Conteo de notas de crédito en C#
                                                    i.OutstandingAmount == i.TotalAmount) // Monto pendiente en C#
                                        .ToList();


            // Mapear a DTOs de respuesta
            var response = filteredInvoices.Select(i => new InvoiceResponseDto
            {
                InvoiceNumber = i.InvoiceNumber,
                IssueDate = i.IssueDate,
                PaymentDueDate = i.PaymentDueDate,
                TotalAmount = i.TotalAmount,
                Status = i.Status,
                PaymentStatus = i.PaymentStatus,
                IsConsistent = i.IsConsistent,
                OutstandingAmount = i.OutstandingAmount,
                Products = i.Products.Select(p => new InvoiceProductResponseDto { Id = p.Id, Description = p.Description ?? string.Empty, Quantity = p.Quantity, UnitPrice = p.UnitPrice, Subtotal = p.Subtotal }).ToList(),
                CreditNotes = i.CreditNotes.Select(cn => new CreditNoteResponseDto
                {
                    Id = cn.Id,
                    CreditNoteNumber = cn.CreditNoteNumber,
                    IssueDate = cn.IssueDate,
                    Amount = cn.Amount
                }).ToList()
            }).ToList();

            if (!response.Any())
            {
                _logger.LogInformation("No se encontraron facturas para el reporte de vencidas sin pago/NC.");
                return NotFound("No se encontraron facturas que cumplan los criterios del reporte.");
            }

            return Ok(response);
        }
        // ...
        // ... (código existente de InvoicesController) ...

        /// <summary>
        /// Obtiene un resumen total y porcentaje de facturas por estado de pago (Paid, Pending, Overdue).
        /// </summary>
        /// <returns>Un objeto que resume los estados de pago.</returns>
        [HttpGet("report/payment-summary")] // GET /api/Invoices/report/payment-summary
        public async Task<ActionResult<OverallPaymentSummaryDto>> GetPaymentStatusSummary()
        {
            _logger.LogInformation("Generando reporte de resumen por estado de pago.");

            var allInvoices = await _context.Invoices.ToListAsync();
            var totalInvoices = allInvoices.Count;

            if (totalInvoices == 0)
            {
                return Ok(new OverallPaymentSummaryDto { TotalInvoices = 0, Summaries = new List<PaymentStatusSummaryDto>() });
            }

            // Agrupar facturas por PaymentStatus y contar
            var groupedByStatus = allInvoices
                .GroupBy(i => i.PaymentStatus)
                .Select(g => new PaymentStatusSummaryDto
                {
                    Status = g.Key,
                    Count = g.Count(),
                    Percentage = Math.Round((decimal)g.Count() / totalInvoices * 100, 2) // Calcular porcentaje, redondear a 2 decimales
                })
                .ToList();

            var summary = new OverallPaymentSummaryDto
            {
                TotalInvoices = totalInvoices,
                Summaries = groupedByStatus
            };

            return Ok(summary);
        }
    
      /// <summary>
        /// Obtiene una lista de facturas marcadas como inconsistentes.
        /// (donde el total declarado no coincide con la suma de los productos).
        /// </summary>
        /// <returns>Lista de facturas inconsistentes.</returns>
        [HttpGet("report/inconsistent")] // GET /api/Invoices/report/inconsistent
        public async Task<ActionResult<IEnumerable<InvoiceResponseDto>>> GetInconsistentInvoices()
        {
            _logger.LogInformation("Generando reporte de facturas inconsistentes.");

            var invoices = await _context.Invoices
                                        .Include(i => i.Products)
                                        .Include(i => i.CreditNotes)
                                        .Where(i => !i.IsConsistent) // Filtrar por IsConsistent = false
                                        .ToListAsync();

            // Mapear a DTOs de respuesta
            var response = invoices.Select(i => new InvoiceResponseDto
            {
                InvoiceNumber = i.InvoiceNumber,
                IssueDate = i.IssueDate,
                PaymentDueDate = i.PaymentDueDate,
                TotalAmount = i.TotalAmount,
                Status = i.Status,
                PaymentStatus = i.PaymentStatus,
                IsConsistent = i.IsConsistent,
                OutstandingAmount = i.OutstandingAmount,
                Products = i.Products.Select(p => new InvoiceProductResponseDto
                {
                    Id = p.Id,
                    Description = p.Description ?? string.Empty,
                    Quantity = p.Quantity,
                    UnitPrice = p.UnitPrice,
                    Subtotal = p.Subtotal
                }).ToList(),
                CreditNotes = i.CreditNotes.Select(cn => new CreditNoteResponseDto
                {
                    Id = cn.Id,
                    CreditNoteNumber = cn.CreditNoteNumber,
                    IssueDate = cn.IssueDate,
                    Amount = cn.Amount
                }).ToList()
            }).ToList();

            if (!response.Any())
            {
                _logger.LogInformation("No se encontraron facturas inconsistentes.");
                return NotFound("No se encontraron facturas inconsistentes en el sistema.");
            }

            return Ok(response);
        }
    }
}


   







