import React, { useState } from 'react'; // Asegúrate de que useState también esté aquí
import './App.css';
import InvoiceList from './components/InvoiceList';
import AddCreditNoteForm from './components/AddCreditNoteForm'; // Importa el componente
import ReportViewer from './components/ReportViewer'; // Importa el componente

function App() {
  // Usaremos un estado para "forzar" la actualización de InvoiceList
  // cuando se agrega una NC, para que la tabla refleje el cambio de estado/saldo
  const [refreshKey, setRefreshKey] = useState(0); // Asegúrate de importar useState

  const handleCreditNoteAdded = () => {
    setRefreshKey(prevKey => prevKey + 1); // Incrementa la clave para forzar la actualización
  };

  return (
    <div className="App">
      <header className="App-header">
        <h1>Sistema de Gestión de Facturas</h1>
      </header>
      {/* Pasa refreshKey como prop y maneja el callback */}
      <InvoiceList key={refreshKey} />
      {/* Renderiza el formulario de NC */}
      <AddCreditNoteForm onCreditNoteAdded={handleCreditNoteAdded} />
      {/* Renderiza el visor de reportes */}
      <ReportViewer />
    </div>
  );
}

export default App;