# Cart Service

## Responsabilidad
Gestiona la persistencia de los productos seleccionados por el usuario
antes de confirmar una compra.

## Funcionalidades
- Agregar y quitar productos del carrito
- Consultar el carrito actual de un usuario
- Vaciar el carrito al confirmar una orden

## Comunicación
- Expone una API REST (HTTP/JSON)
- Consulta a: Products (para validar existencia y precio)
- Es consultado por: Orders
