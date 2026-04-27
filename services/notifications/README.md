# Notifications Service

## Responsabilidad
Gestiona el envío de alertas y comunicaciones a los usuarios de NovaShop.

## Funcionalidades
- Enviar email de bienvenida al registrarse
- Notificar confirmación de una orden
- Alertar cambios en el estado de un pedido

## Comunicación
- Expone una API REST (HTTP/JSON)
- Es disparado por: Orders, Users

## Nota de diseño
Este servicio está aislado del resto del sistema. Un error aquí
no puede afectar el flujo de pagos ni otros servicios críticos.
