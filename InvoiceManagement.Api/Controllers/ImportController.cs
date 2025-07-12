using Microsoft.AspNetCore.Mvc;
using InvoiceManagement.Api.Services;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.AspNetCore.Hosting; // Necesario para IWebHostEnvironment

namespace InvoiceManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImportController : ControllerBase
    {
        private readonly InvoiceImportService _importService;
        private readonly ILogger<ImportController> _logger;
        private readonly IWebHostEnvironment _env;

        public ImportController(InvoiceImportService importService, ILogger<ImportController> logger, IWebHostEnvironment env)
        {
            _importService = importService;
            _logger = logger;
            _env = env;
        }

        /// <summary>
        /// Importa facturas desde el archivo JSON predefinido (bd_exam_invoices.json).
        /// </summary>
        /// <returns>Un mensaje de resultado de la importación.</returns>
        [HttpPost("import-json")]
        public async Task<IActionResult> ImportJsonData()
        {
            string jsonFilePath = Path.Combine(_env.ContentRootPath, "bd_exam_invoices.json");

            _logger.LogInformation($"Intentando importar datos desde: {jsonFilePath}");

            var result = await _importService.ImportInvoicesFromJsonAsync(jsonFilePath);

            if (result.StartsWith("Error:"))
            {
                return BadRequest(new { Message = result });
            }

            return Ok(new { Message = result });
        }
    }
}