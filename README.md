# NovaShop MVP

Proyecto de arquitectura de software para el TP de **Construcción de Aplicaciones Informáticas (CAI)** — UBA FCE, cátedra Méndez.

Migración del monolito "El Núcleo" de NovaShop hacia una arquitectura de **microservicios** independientes, comunicados por HTTP/JSON.

## Estructura del repositorio
novashop-mvp/
├── novashop-mvp.slnx       # Solución que agrupa todos los proyectos
├── src/
│   ├── Products.API/       # Catálogo y stock (puerto 5280)
│   ├── Users.API/          # Registro y autenticación (puerto 5012)
│   ├── Orders.API/         # Pedidos y estados (puerto 5080)
│   ├── Cart.API/           # Carrito de compras (puerto 5006)
│   └── Notifications.API/  # Alertas y notificaciones (puerto 5038)
├── docs/                   # Diagrama de arquitectura
└── README.md

## Requisitos

- .NET 10 SDK

## Cómo ejecutar cada servicio

Cada microservicio es independiente. Abrí una terminal por cada uno:

```bash
# Products
cd src/Products.API && dotnet run
# http://localhost:5280/swagger

# Users
cd src/Users.API && dotnet run
# http://localhost:5012/swagger

# Orders (requiere Products y Users corriendo)
cd src/Orders.API && dotnet run
# http://localhost:5080/swagger

# Cart (requiere Products corriendo)
cd src/Cart.API && dotnet run
# http://localhost:5006/swagger

# Notifications (requiere Users corriendo)
cd src/Notifications.API && dotnet run
# http://localhost:5038/swagger
```

## Endpoints disponibles por servicio

| Endpoint | Descripción |
|---|---|
| `/swagger` | Documentación interactiva de la API |
| `/health` | Estado general del servicio |
| `/health/ready` | Estado de la base de datos |
| `/health/live` | Liveness check |

## Restricciones de diseño cumplidas

1. **Independencia de datos**: cada servicio tiene su propia base SQLite. Orders y Notifications consultan otros servicios via HTTP, nunca acceden a sus bases de datos directamente.
2. **Tecnologías mixtas**: cada servicio es un proyecto .NET independiente, reemplazable sin afectar a los demás.
3. **Comunicación estándar**: todos los servicios exponen APIs REST sobre HTTP/JSON.

## Formato de errores

Todos los endpoints devuelven errores con el formato ProblemDetails + errorCode:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "El recurso solicitado no fue encontrado.",
  "instance": "/api/products/99",
  "errorCode": "PRD-001",
  "errorMessage": "Producto no encontrado.",
  "correlationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

## Catálogo de errores

| Código | HTTP | Descripción |
|---|---|---|
| PRD-001 | 404 | Producto no encontrado |
| PRD-002 | 400 | Datos del producto inválidos |
| PRD-003 | 409 | Producto duplicado en la categoría |
| PRD-005 | 500 | Error interno en Products |
| USR-001 | 409 | Email ya registrado |
| USR-002 | 400 | Datos del usuario inválidos |
| USR-003 | 401 | Credenciales incorrectas |
| USR-004 | 403 | Usuario bloqueado por intentos fallidos |
| USR-005 | 403 | Usuario bloqueado por fraude |
| ORD-001 | 404 | Orden no encontrada |
| ORD-002 | 400 | Datos de la orden inválidos |
| ORD-003 | 404 | Usuario no encontrado al crear orden |
| ORD-004 | 404 | Producto no encontrado al crear orden |
| ORD-005 | 422 | Stock insuficiente |
| ORD-006 | 409 | Transición de estado inválida |
| CRT-001 | 404 | Carrito no encontrado |
| CRT-002 | 404 | Producto no encontrado en carrito |
| CRT-003 | 422 | Stock insuficiente en carrito |
| CRT-004 | 400 | Cantidad inválida |
| NTF-001 | 404 | Usuario no encontrado en notificación |
| NTF-002 | 400 | Datos de notificación inválidos |
| NTF-003 | 404 | No hay notificaciones para el usuario |

## Stack tecnológico

- .NET 10 Web API con Controllers
- Serilog (consola + archivo JSON)
- SQLite + Dapper (persistencia)
- Swagger / Swashbuckle (documentación)
- IExceptionHandler + ProblemDetails (manejo de errores)
- Correlation ID propagado entre servicios

## Integrantes

- Martina Giata Ganellis
- Ornella Castronuovo
