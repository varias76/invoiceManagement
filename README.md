Sistema de Gestión de Facturas

Sistema Full-Stack para la gestión de facturas, implementado en .NET 8 (Backend API) y React (Frontend). Permite la integración de datos desde un archivo JSON, su almacenamiento en una base de datos SQLite y la administración de facturas mediante una interfaz web intuitiva.

Requisitos del Sistema

Para ejecutar este proyecto, necesitas tener instalados los siguientes componentes:
SDK de .NET 8
Node.js y npm

Estructura del Proyecto
El repositorio está organizado de la siguiente manera:

InvoiceManagement.Api/: Contiene el proyecto del Backend (ASP.NET Core Web API).
invoice-management-frontend/: Contiene el proyecto del Frontend (React).

Instalación y Ejecución del Proyecto
Sigue los pasos a continuación para configurar y ejecutar la aplicación.

Clonar el Repositorio
https://github.com/varias76/invoiceManagement.git

Configuración e Inicio del Backend (ASP.NET Core API)
Navega al directorio del proyecto Backend: cd InvoiceManagement.Api

Restaura las dependencias de .NET:
dotnet restore

Asegúrate de que la herramienta dotnet-ef esté instalada globalmente:
dotnet tool install --global dotnet-ef
	# Si ya está instalada, puedes actualizarla:
	# dotnet tool update --global dotnet-ef

Crea la base de datos SQLite y aplica las migraciones.
 Esto creará el archivo invoices.db.  
  dotnet ef database update

Inicia la aplicación Backend:
dotnet run

La API se ejecutará en http://localhost:PUERTO_BACKEND (ej. http://localhost:5296).

Configuración e Inicio del Frontend (React)
navega al directorio del proyecto Frontend:
cd ../invoice-management-frontend
Instala las dependencias de Node.js:
	npm install

Inicia la aplicación Frontend:
npm run dev

Carga Inicial de Datos (Importante)
Una vez que tanto el Backend como el Frontend estén ejecutándose, necesitas importar los datos iniciales desde el archivo JSON:

Abre tu navegador y ve a la URL de Swagger UI del Backend: http://localhost:5296/swagger/index.html  ( u otro puerto )

Busca el endpoint POST /api/Import/import-json.
Expándelo, haz clic en "Try it out",  luego  "Execute".

Verifica que el Response code sea 200 OK y el mensaje indique que se importaron 50 facturas. 
Este paso solo se necesita ejecutar una vez por cada inicio de la base de datos limpia.

Acceder a la Aplicación
Abre tu navegador y ve a la URL del Frontend:   http://localhost:5173/ ( u otro puerto )


Deberías ver la lista de 50 facturas cargada.
	
