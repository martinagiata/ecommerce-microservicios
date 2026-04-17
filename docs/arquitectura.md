# Arquitectura NovaShop MVP

## Contexto
NovaShop migra de un sistema monolítico ("El Núcleo") a una arquitectura
basada en servicios con responsabilidades separadas.

## Servicios

| Servicio      | Responsabilidad principal              |
|---------------|----------------------------------------|
| Products      | Catálogo y stock                       |
| Users         | Registro, perfiles y seguridad         |
| Cart          | Carrito de compras                     |
| Orders        | Procesamiento y estado de pedidos      |
| Notifications | Alertas y comunicaciones               |

## Restricciones de diseño
1. **Independencia de datos**: Orders no accede directamente a la DB de Users.
2. **Tecnologías mixtas**: Products puede cambiar de lenguaje sin afectar a Cart.
3. **Comunicación estándar**: Todos los servicios se comunican por HTTP/JSON.

## Diagrama de dependencias

```
Users ←──── Orders ────→ Cart ────→ Products
  ↑              ↓
  └──── Notifications
```
