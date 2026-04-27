# Products Service

## Responsabilidad
Gestiona el catálogo de productos y el control de existencias (stock) de NovaShop.

## Funcionalidades
- Listar productos disponibles
- Consultar detalle de un producto
- Actualizar stock al confirmar una compra

## Comunicación
- Expone una API REST (HTTP/JSON)
- Es consultado por: Cart, Orders

## Nota de diseño
Este servicio puede cambiar de lenguaje o tecnología interna sin afectar a los demás,
ya que la comunicación se hace exclusivamente a través de su API.
