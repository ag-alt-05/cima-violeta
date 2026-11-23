# CimaVioleta

> Plataforma de rutas de autobuses con comentarios, votos y detección de toxicidad.

## Instrucciones para Ejecutar

### 1. Clonar el repositorio

    git clone https://github.com/ag-alt-05/cima-violeta.git

### 2. Base de datos

Ejecuta `RutaVioleta.sql` en **SQL Server Management Studio 22**.

### 3. Backend

Abre `CimaVioletaBackend.slnx` en **Visual Studio 2026**.

### 4. Frontend

Abre `CimaVioletaFrontend.slnx` en **Visual Studio 2026**.

### 5. API de Toxicidad (Python)

1. Abre `api_toxicidad.py` en **Visual Studio Code**
2. En la terminal, instala las dependencias:

    pip install fastapi
    pip install transformers
    pip install torch
    pip install uvicorn

| Paquete | Función |
|---------|---------|
| fastapi | Framework de la API |
| transformers | Carga el modelo de Hugging Face |
| torch | Motor de inferencia |
| uvicorn | Servidor de la API |

3. Ejecuta el archivo:

    python api_toxicidad.py

> ⚠️ La primera vez descarga el modelo (~2 GB). Queda en cache y luego arranca rápido.

### 6. Ejecutar todo

| Proyecto | Dónde |
|----------|-------|
| CimaVioletaBackend | Visual Studio 2026 → F5 |
| CimaVioletaFrontend | Visual Studio 2026 → F5 |
| api_toxicidad.py | Terminal → python api_toxicidad.py |

Los tres deben estar corriendo a la vez para que la base de datos y el backend se sincronicen.
