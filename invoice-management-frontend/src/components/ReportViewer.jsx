import React, { useState } from 'react';
import axios from 'axios';

const ReportViewer = () => {
  const [reportType, setReportType] = useState('');
  const [reportData, setReportData] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const fetchReport = async (type) => {
    setReportType(type);
    setReportData(null); // Limpia datos de reporte anteriores
    setLoading(true);    // Muestra estado de carga
    setError(null);      // Limpia errores anteriores
    try {
      let url = '';
      // Construye la URL del endpoint del reporte en tu backend
      switch (type) {
        case 'overdue-unpaid':
          url = 'http://localhost:5296/api/Invoices/report/overdue-unpaid';
          break;
        case 'payment-summary':
          url = 'http://localhost:5296/api/Invoices/report/payment-summary';
          break;
        case 'inconsistent':
          url = 'http://localhost:5296/api/Invoices/report/inconsistent';
          break;
        default:
          setLoading(false);
          return;
      }
      const response = await axios.get(url);
      setReportData(response.data); // Actualiza el estado con los datos del reporte
      setLoading(false);
    } catch (err) {
      console.error(`Error fetching ${type} report:`, err.response || err);
      // Manejo de errores para cuando no hay datos en el reporte (404) o otros errores
      if (err.response && err.response.data) {
        setError(`Error al cargar el reporte: ${err.response.data}`);
      } else {
        setError(`Error al cargar el reporte de ${type}.`);
      }
      setLoading(false);
    }
  };

  // Función para renderizar los datos del reporte según su tipo
  const renderReportData = () => {
    if (!reportData) return null; // No hay datos para mostrar

    switch (reportType) {
      case 'overdue-unpaid':
        if (Array.isArray(reportData) && reportData.length === 0) {
          return <p>No hay facturas consistentes, vencidas por más de 30 días sin pago o NC.</p>;
        }
        return (
          <div>
            <h4>Facturas Consistentes, Vencidas sin Pago/NC (30+ días)</h4>
            <table>
              <thead>
                <tr>
                  <th>Número Factura</th>
                  <th>Fecha Vencimiento</th>
                  <th>Monto Total</th>
                  <th>Estado Pago</th>
                </tr>
              </thead>
              <tbody>
                {reportData.map((invoice) => (
                  <tr key={invoice.invoiceNumber}>
                    <td>{invoice.invoiceNumber}</td>
                    <td>{new Date(invoice.paymentDueDate).toLocaleDateString()}</td>
                    <td>{invoice.totalAmount.toLocaleString('es-CL', { style: 'currency', currency: 'CLP' })}</td>
                    <td>{invoice.paymentStatus}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        );
      case 'payment-summary':
        return (
          <div>
            <h4>Resumen por Estado de Pago</h4>
            <p>Total de Facturas: {reportData.totalInvoices}</p>
            <table>
              <thead>
                <tr>
                  <th>Estado</th>
                  <th>Cantidad</th>
                  <th>Porcentaje (%)</th>
                </tr>
              </thead>
              <tbody>
                {reportData.summaries.map((summary) => (
                  <tr key={summary.status}>
                    <td>{summary.status}</td>
                    <td>{summary.count}</td>
                    <td>{summary.percentage}%</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        );
      case 'inconsistent':
        if (Array.isArray(reportData) && reportData.length === 0) {
          return <p>No hay facturas inconsistentes.</p>;
        }
        return (
          <div>
            <h4>Facturas Inconsistentes</h4>
            <table>
              <thead>
                <tr>
                  <th>Número Factura</th>
                  <th>Monto Total</th>
                  <th>Productos Suma</th>
                  <th>Consistente</th>
                </tr>
              </thead>
              <tbody>
                {reportData.map((invoice) => (
                  <tr key={invoice.invoiceNumber}>
                    <td>{invoice.invoiceNumber}</td>
                    <td>{invoice.totalAmount.toLocaleString('es-CL', { style: 'currency', currency: 'CLP' })}</td>
                    {/* Suma los subtotales de los productos para mostrar la inconsistencia */}
                    <td>{invoice.products.reduce((sum, p) => sum + p.subtotal, 0).toLocaleString('es-CL', { style: 'currency', currency: 'CLP' })}</td>
                    <td>{invoice.isConsistent ? 'Sí' : 'No'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        );
      default:
        return null;
    }
  };

  return (
    <div style={{ marginTop: '30px', borderTop: '1px solid #eee', paddingTop: '20px' }}>
      <h3>Reportes</h3>
      <div style={{ marginBottom: '20px', display: 'flex', gap: '10px', justifyContent: 'center' }}>
        <button onClick={() => fetchReport('overdue-unpaid')}>Facturas Vencidas sin Pago</button>
        <button onClick={() => fetchReport('payment-summary')}>Resumen Estados de Pago</button>
        <button onClick={() => fetchReport('inconsistent')}>Facturas Inconsistentes</button>
      </div>

      {loading && <p>Cargando reporte...</p>}
      {error && <p style={{ color: 'red' }}>{error}</p>}
      {renderReportData()}
    </div>
  );
};

export default ReportViewer;