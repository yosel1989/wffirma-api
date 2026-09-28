# WFF - API Hexagonal

API desarrollada bajo arquitectura Hexagonal (Ports & Adapters).

---

## 📌 Descripción

Esta API actúa como orquestador entre:

- SSO (proveedor de autenticación)
- API Suc (menú y datos externos)
- Base de datos en Oracle

---

## 🏗 Arquitectura

La solución sigue el patrón Hexagonal:

- **Domain**
  - Entidades
  - Value Objects
  - Puertos (Interfaces)

- **Application**
  - Casos de uso
  - DTOs
  - Handlers (MediatR)

- **Infrastructure**
  - Adaptadores externos
  - Repositorios
  - Configuración de seguridad (JWT)

- **API**
  - Controllers
  - Configuración Swagger
  - Middleware

---

## 🔐 Autenticación

La API utiliza autenticación basada en JWT.

El token es emitido por el SSO y debe enviarse en el header:
