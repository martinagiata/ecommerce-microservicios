# Orders Service

## Responsabilidad
Gestiona el procesamiento de compras y el seguimiento del estado de cada pedido.

## Funcionalidades
- Crear una nueva orden a partir del carrito
- Actualizar el estado del pedido (pendiente, confirmado, enviado, entregado)
- Consultar historial de órdenes de un usuario

## Comunicación
- Expone una API REST (HTTP/JSON)
- Consulta a: Users (vía API, nunca directo a su base de datos), Cart, Products
- Notifica a: Notifications al confirmar una orden

## Nota de diseño
Cumple la restricción de independencia de datos: obtiene información de usuarios
exclusivamente a través de la API del servicio Users.
