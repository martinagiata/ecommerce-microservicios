# E-Commerce - Arquitectura de Microservicios

Trabajo práctico de **Arquitectura y Diseño de Software** — implementación de un sistema de E-Commerce basado en microservicios REST independientes con C# / .NET Core 10.

## Estructura del repositorio

```
ecommerce-microservicios/
├── ECommerce.slnx          # Solución que agrupa todos los proyectos
├── src/
│   ├── Products.API/       # Catálogo y stock
│   ├── Users.API/          # Registro y autenticación
│   ├── Orders.API/         # Pedidos y estados
│   ├── Cart.API/           # Carrito de compras
│   └── Notifications.API/  # Alertas y notificaciones
├── docs/                   # Diagrama de arquitectura y capturas
└── README.md
```

## Diagrama de arquitectura

```
   ┌─────────────────┐   ┌─────────────────┐   ┌─────────────────┐
   │  Products.API   │   │    Users.API    │   │    Cart.API     │
   │      :5280      │   │      :5012      │   │      :5006      │
   │   products.db   │   │    users.db     │   │     cart.db     │
   └────────▲────────┘   └────────▲────────┘   └────────┬────────┘
            │                     │                     │
            │  GET /api/products/{id}                   │
            ├─────────────────────┼─────────────────────┘
            │                     │  GET /api/users/{id}
   ┌────────┴────────┐   ┌────────┴────────┐
   │   Orders.API    │   │Notifications.API│
   │      :5080      │   │    orders.db    │
   └─────────────────┘   └─────────────────┘

   Cada servicio tiene su propia base SQLite.
   Las flechas indican consumo via HTTP/JSON (nunca acceso directo a otra DB).
```

Versión en imagen: [`docs/arquitectura.svg`](docs/arquitectura.svg)

## Requisitos

- .NET 10 SDK

## Cómo ejecutar cada servicio

Cada microservicio es independiente. Abrí una terminal por cada uno:

```bash
cd src/Products.API && dotnet run
cd src/Users.API && dotnet run
cd src/Orders.API && dotnet run        # requiere Products y Users corriendo
cd src/Cart.API && dotnet run          # requiere Products corriendo
cd src/Notifications.API && dotnet run # requiere Users corriendo
```

## Tabla de puertos

| Servicio | Puerto | Swagger | Descripción |
|---|---|---|---|
| Products.API | 5280 | http://localhost:5280/swagger | Catálogo de productos y stock |
| Users.API | 5012 | http://localhost:5012/swagger | Registro, login y bloqueo de usuarios |
| Orders.API | 5080 | http://localhost:5080/swagger | Órdenes con items, total y estados |
| Cart.API | 5006 | http://localhost:5006/swagger | Carrito de compras por usuario |
| Notifications.API | 5038 | http://localhost:5038/swagger | Envío y consulta de notificaciones |

Cada servicio expone además: `/health`, `/health/ready` y `/health/live`.

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

## Tabla de códigos de error

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
| USR-006 | 500 | Error interno en Users |
| ORD-001 | 404 | Orden no encontrada |
| ORD-002 | 400 | Datos de la orden inválidos |
| ORD-003 | 404 | Usuario no encontrado al crear orden |
| ORD-004 | 404 | Producto no encontrado al crear orden |
| ORD-005 | 422 | Stock insuficiente |
| ORD-006 | 409 | Transición de estado inválida |
| ORD-007 | 500 | Error interno en Orders |
| CRT-001 | 404 | Carrito no encontrado |
| CRT-002 | 404 | Producto no encontrado en carrito |
| CRT-003 | 422 | Stock insuficiente en carrito |
| CRT-004 | 400 | Cantidad inválida |
| CRT-005 | 500 | Error interno en Cart |
| NTF-001 | 404 | Usuario no encontrado en notificación |
| NTF-002 | 400 | Datos de notificación inválidos |
| NTF-003 | 404 | No hay notificaciones para el usuario |
| NTF-004 | 500 | Error interno en Notifications |

## Stack tecnológico

- .NET 10 Web API con Controllers
- Serilog (consola + archivo JSON en `logs/`)
- SQLite + Dapper (persistencia)
- Swagger / Swashbuckle (documentación con XML comments)
- IExceptionHandler + ProblemDetails (manejo global de errores)
- Correlation ID (`X-Correlation-Id`) propagado entre servicios
- Health Checks (`/health`, `/health/ready`, `/health/live`)

## Integrante

- Martina Giata Ganellis
