import React, { useState } from 'react';
import axios from 'axios';

const AddCreditNoteForm = ({ onCreditNoteAdded }) => {
  const [invoiceNumber, setInvoiceNumber] = useState('');
  const [amount, setAmount] = useState('');
  const [message, setMessage] = useState('');
  const [messageType, setMessageType] = useState(''); // 'success' or 'error'

  const handleSubmit = async (event) => {
    event.preventDefault(); // Previene el comportamiento por defecto del formulario
    setMessage(''); // Limpia mensajes anteriores

    // Validaciones básicas del formulario
    if (!invoiceNumber || !amount) {
      setMessage('Por favor, ingresa el número de factura y el monto.');
      setMessageType('error');
      return;
    }
    if (isNaN(parseFloat(amount)) || parseFloat(amount) <= 0) {
      setMessage('El monto debe ser un número positivo.');
      setMessageType('error');
      return;
    }

    try {
      // Realiza la petición POST a tu API de backend para agregar la nota de crédito
      const response = await axios.post('http://localhost:5296/api/Invoices/credit-note', {
        invoiceNumber: invoiceNumber,
        amount: parseFloat(amount),
      });
      setMessage(`Nota de crédito agregada a factura ${response.data.invoiceNumber}. Nuevo estado: ${response.data.status}.`);
      setMessageType('success');
      setInvoiceNumber(''); // Limpia los campos del formulario
      setAmount('');
      if (onCreditNoteAdded) {
        onCreditNoteAdded(); // Llama al callback para que InvoiceList se actualice si es necesario
      }
    } catch (err) {
      console.error('Error adding credit note:', err.response || err);
      // Muestra un mensaje de error más específico si viene del backend
      if (err.response && err.response.data && err.response.data.Message) {
        setMessage(`Error: ${err.response.data.Message}`);
      } else {
        setMessage('Error al agregar la nota de crédito. Verifica el número de factura y el monto.');
      }
      setMessageType('error');
    }
  };

  return (
    <div style={{ marginTop: '30px', borderTop: '1px solid #eee', paddingTop: '20px' }}>
      <h3>Agregar Nota de Crédito</h3>
      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', maxWidth: '300px', margin: '0 auto' }}>
        <label style={{ marginBottom: '10px' }}>
          Número de Factura:
          <input
            type="text"
            value={invoiceNumber}
            onChange={(e) => setInvoiceNumber(e.target.value)}
            style={{ marginLeft: '10px', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }}
          />
        </label>
        <label style={{ marginBottom: '10px' }}>
          Monto:
          <input
            type="number"
            value={amount}
            onChange={(e) => setAmount(e.target.value)}
            style={{ marginLeft: '10px', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }}
          />
        </label>
        <button type="submit" style={{ padding: '10px 20px', backgroundColor: '#28a745', color: 'white', border: 'none', borderRadius: '5px', cursor: 'pointer' }}>
          Agregar NC
        </button>
      </form>
      {message && (
        <p style={{ color: messageType === 'error' ? 'red' : 'green', marginTop: '15px' }}>
          {message}
        </p>
      )}
    </div>
  );
};

export default AddCreditNoteForm;