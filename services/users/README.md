# Users Service

## Responsabilidad
Gestiona el registro de usuarios, perfiles y seguridad de NovaShop.

## Funcionalidades
- Registro e inicio de sesión de usuarios
- Consulta de perfil y datos personales
- Autenticación y autorización

## Comunicación
- Expone una API REST (HTTP/JSON)
- Es consultado por: Orders, Notifications

## Nota de diseño
Ningún otro servicio accede directamente a la base de datos de Users.
Toda consulta de datos de usuario se hace a través de esta API.
