# NovaShop MVP

Proyecto de arquitectura de software para el TP de **Construcción de Aplicaciones Informáticas (CAI)** — UBA FCE, cátedra Méndez.

Migración del monolito "El Núcleo" de NovaShop hacia una arquitectura de **microservicios** independientes, comunicados por HTTP/JSON.

## Estructura del repositorio
novashop-mvp/
├── novashop-mvp.slnx       # Solución que agrupa todos los proyectos
├── MiniApi/                 # Proyecto base ToDoList (cátedra)
├── docs/                     # Documentación de arquitectura
└── services/
├── products/             # Catálogo y stock
├── Users/                # Registro y perfiles
├── Cart/                 # Carrito de compras
├── Orders/               # Pedidos y estados
└── Notifications/        # Alertas y notificaciones

## Requisitos

- .NET 10 SDK
- macOS, Windows o Linux

## Cómo ejecutar cada servicio

Cada microservicio es independiente y corre en su propio puerto. Para levantarlos, abrí una terminal por cada uno:

```bash
# Products
cd services/products
dotnet run
# http://localhost:5051/swagger

# Users
cd services/Users
dotnet run
# http://localhost:5161/swagger

# Cart
cd services/Cart
dotnet run
# http://localhost:5225/swagger

# Orders
cd services/Orders
dotnet run
# http://localhost:5030/swagger

# Notifications
cd services/Notifications
dotnet run
# http://localhost:5049/swagger

# MiniApi (ToDoList base)
cd MiniApi
dotnet run
# http://localhost:5112/swagger
```

## Endpoints disponibles por servicio

| Endpoint | Descripción |
|---|---|
| `/swagger` | Documentación interactiva de la API |
| `/health` | Estado de la base de datos (JSON) |
| `/health-ui` | Dashboard visual de Health Checks |
| `/` | Mensaje de estado del servicio |

## Restricciones de diseño cumplidas

1. **Independencia de datos**: Orders nunca consulta directamente la base de datos de Users; obtiene la información vía HTTP (`GET /users/{id}`).
2. **Tecnologías mixtas**: cada servicio es un proyecto .NET independiente con su propia base SQLite, por lo que cualquiera podría reescribirse en otro lenguaje sin afectar a los demás.
3. **Comunicación estándar**: todos los servicios exponen APIs REST sobre HTTP/JSON.

## Formato de errores

Todos los endpoints devuelven errores con el siguiente formato:

```json
{
  "errorCode": "PRODUCT_NOT_FOUND",
  "errorMessage": "Product con id 99 no encontrado"
}
```

## Stack tecnológico

- .NET 10 Minimal API
- Serilog (logging)
- SQLite + Dapper (persistencia)
- Swagger / Swashbuckle (documentación)
- Health Checks UI (monitoreo)

## Integrantes

- Martina Giata Ganellis
- Ornella

## Documentación adicional

Ver carpeta `docs/` para el detalle de la arquitectura y diagrama de dependencias entre servicios.
