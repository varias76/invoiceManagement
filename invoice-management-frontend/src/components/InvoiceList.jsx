import React, { useEffect, useState } from 'react';
import axios from 'axios';

const InvoiceList = () => {
  const [invoices, setInvoices] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // NUEVOS ESTADOS PARA LOS FILTROS
  const [searchTerm, setSearchTerm] = useState(''); // Para buscar por número de factura
  const [filterStatus, setFilterStatus] = useState(''); // Para filtrar por estado de factura (Issued, Partial, Cancelled)
  const [filterPaymentStatus, setFilterPaymentStatus] = useState(''); // Para filtrar por estado de pago (Pending, Overdue, Paid)

  // useEffect ahora se activará cuando los filtros cambien
  useEffect(() => {
    const fetchInvoices = async () => {
      setLoading(true); // Siempre que se haga una nueva búsqueda, mostrar cargando
      setError(null); // Limpiar errores anteriores
      try {
        let url = 'http://localhost:5296/api/Invoices'; // URL base para obtener todas

        // Si hay filtros, cambiaremos la URL
        if (searchTerm || filterStatus || filterPaymentStatus) {
            // Si hay searchTerm, usamos el endpoint específico de búsqueda por número
            if (searchTerm) {
                url = `http://localhost:5296/api/Invoices/${searchTerm}`;
            } else {
                // Si no hay searchTerm, usamos el endpoint de búsqueda general con query params
                url = 'http://localhost:5296/api/Invoices/search';
                const params = new URLSearchParams();
                if (filterStatus) params.append('status', filterStatus);
                if (filterPaymentStatus) params.append('paymentStatus', filterPaymentStatus);
                url = `${url}?${params.toString()}`;
            }
        }

        const response = await axios.get(url);
        // Si la respuesta es un solo objeto (por invoiceNumber), convertirlo a array
        // La API devuelve una lista para Get All/Search, pero un objeto para Get By Number.
        setInvoices(Array.isArray(response.data) ? response.data : [response.data]); 
        setLoading(false);
      } catch (err) {
        // Manejo de errores más específico para 404 de búsqueda
        if (err.response && err.response.status === 404) {
          setError('No se encontraron facturas con los criterios de búsqueda.');
          setInvoices([]); // Vaciar la lista si no hay resultados
        } else {
          setError('Error al cargar las facturas. Asegúrate de que el backend esté funcionando y los datos importados.');
          console.error('Error fetching invoices:', err);
        }
        setLoading(false);
      }
    };

    fetchInvoices();
  }, [searchTerm, filterStatus, filterPaymentStatus]); // Dependencias: se ejecuta cuando estos estados cambian

  const handleSearchChange = (event) => {
    setSearchTerm(event.target.value);
  };

  const handleStatusChange = (event) => {
    setFilterStatus(event.target.value);
  };

  const handlePaymentStatusChange = (event) => {
    setFilterPaymentStatus(event.target.value);
  };

  if (loading) {
    return <p>Cargando facturas...</p>;
  }

  if (error) {
    return <p style={{ color: 'red' }}>{error}</p>;
  }

  return (
    <div>
      <h2>Lista de Facturas</h2>

      {/* Sección de Filtros */}
      <div style={{ marginBottom: '20px', display: 'flex', gap: '10px', justifyContent: 'center' }}>
        <input
          type="text"
          placeholder="Buscar por # Factura"
          value={searchTerm}
          onChange={handleSearchChange}
          style={{ padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }}
        />
        <select value={filterStatus} onChange={handleStatusChange} style={{ padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }}>
          <option value="">Filtrar por Estado...</option>
          <option value="Issued">Issued</option>
          <option value="Partial">Partial</option>
          <option value="Cancelled">Cancelled</option>
        </select>
        <select value={filterPaymentStatus} onChange={handlePaymentStatusChange} style={{ padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }}>
          <option value="">Filtrar por Estado de Pago...</option>
          <option value="Pending">Pending</option>
          <option value="Overdue">Overdue</option>
          <option value="Paid">Paid</option>
        </select>
        <button onClick={() => {setSearchTerm(''); setFilterStatus(''); setFilterPaymentStatus('');}}>Limpiar Filtros</button>
      </div>

      {invoices.length === 0 && !loading && !error ? ( // Si no hay facturas y no estamos cargando ni hay error
        <p>No se encontraron facturas que coincidan con los criterios de búsqueda.</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>Número Factura</th>
              <th>Fecha Emisión</th>
              <th>Monto Total</th>
              <th>Estado</th>
              <th>Estado Pago</th>
              <th>Pendiente ($)</th>
              <th>Consistente</th>
            </tr>
          </thead>
          <tbody>
            {invoices.map((invoice) => (
              <tr key={invoice.invoiceNumber}>
                <td>{invoice.invoiceNumber}</td>
                <td>{new Date(invoice.issueDate).toLocaleDateString()}</td>
                <td>{invoice.totalAmount.toLocaleString('es-CL', { style: 'currency', currency: 'CLP' })}</td>
                <td>{invoice.status}</td>
                <td>{invoice.paymentStatus}</td>
                <td>{invoice.outstandingAmount.toLocaleString('es-CL', { style: 'currency', currency: 'CLP' })}</td>
                <td>{invoice.isConsistent ? 'Sí' : 'No'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
};

export default InvoiceList;