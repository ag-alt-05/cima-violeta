# CimaVioleta

Plataforma de rutas de autobuses con geolocalización en tiempo real, sistema de comentarios, votos y detección de toxicidad mediante Inteligencia Artificial

## Requisitos Previos

Antes de comenzar, asegúrate de tener instalado lo siguiente:

- Git
- SQL Server y SQL Server Management Studio (SSMS)
- Visual Studio 2026 (con la carga de trabajo de "Desarrollo web y ASP.NET")
- Visual Studio Code 
- Python 3.10+ (asegúrate de marcar "Add Python to PATH" durante la instalación)

## Instalación y Configuración

### Paso 1: Clonar el repositorio

Abre una terminal o Símbolo del sistema y ejecuta:

```bash
git clone https://github.com/ag-alt-05/cima-violeta.git
cd cima-violeta
```

### Paso 2: Configurar la Base de Datos

1. Abre **SQL Server Management Studio (SSMS)** y conéctate a tu servidor local (usualmente `.\SQLEXPRESS` o `localhost`).
2. Abre el archivo `RutaVioleta.sql` que está en la raíz del proyecto.
3. Ejecútalo (**F5**) para crear la base de datos, tablas, vistas, procedimientos almacenados y los datos de prueba.

### Paso 3: Configurar el Backend (C#)

1. Abre el archivo `CimaVioletaBackend.slnx` en **Visual Studio 2026**.
2. En el Explorador de soluciones, busca y abre el archivo `appsettings.json`.
3. Modifica la cadena de conexión con el nombre de tu servidor SQL local:

```json
"ConnectionStrings": {
  "ConexionRutaVioleta": "Server=TU_SERVIDOR_SQL;Database=RutaVioleta;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

> **Nota:** Si usas SQLEXPRESS, usualmente es `Server=.\SQLEXPRESS`. Si usas autenticación de SQL, agrega `User Id=sa;Password=tu_contraseña;`.

4. Presiona **F5** para ejecutar. Anota el puerto en el que se abre (ej. `https://localhost:7123`).

### Paso 4: Configurar el Frontend (ASP.NET Razor Pages)

1. Abre el archivo `CimaVioletaFrontend.slnx` en **Visual Studio 2026** (puedes tener ambas soluciones abiertas a la vez).
2. Verifica que la URL de la API en el frontend apunte a tu backend local.

> **Nota:** Si estás probando en local, asegúrate de cambiar las URLs en los archivos `.cshtml` a `https://localhost:TU_PUERTO/api/...`.

3. Presiona **F5** para ejecutar el frontend.

### Paso 5: Configurar la API de Toxicidad (Python)

1. Abre **Visual Studio Code**.
2. Ve a **Archivo > Abrir Carpeta** y selecciona la carpeta raíz del proyecto (donde está `api_toxicidad.py`).
3. Abre una terminal en VS Code (**Ctrl + ñ** o **Terminal > Nueva Terminal**).
4. Crea un entorno virtual (opcional pero recomendado):

```bash
# Windows
python -m venv venv
.\venv\Scripts\activate

# Mac/Linux
python3 -m venv venv
source venv/bin/activate
```

5. Instala las dependencias necesarias:

```bash
pip install fastapi uvicorn transformers torch
```

| Paquete | Función |
|---------|---------|
| `fastapi` | Framework de la API |
| `uvicorn` | Servidor que corre la API |
| `transformers` | Carga el modelo de Hugging Face |
| `torch` | Motor de inferencia |

6. Ejecuta el servidor de la IA:

```bash
python api_toxicidad.py
```

> ⚠️ **Advertencia:** La primera vez que lo ejecutes, descargará el modelo de Hugging Face de ~2 GB. Esto puede tardar varios minutos dependiendo de tu internet. Después quedará en caché y arrancará en segundos.   