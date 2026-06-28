# Sistema de Citas Médicas

Sistema web para la gestión y administración de citas médicas, desarrollado con **ASP.NET Core MVC**.  
El proyecto permite gestionar pacientes, médicos, solicitudes de citas, horarios disponibles y asignación de consultas mediante un sistema basado en roles.

---

## Descripción del proyecto

El Sistema de Citas Médicas tiene como objetivo facilitar la organización de consultas médicas dentro de una institución de salud.

La aplicación permite que los pacientes puedan solicitar citas indicando una especialidad, fecha, hora y motivo de consulta. Posteriormente, los médicos o administradores pueden gestionar estas solicitudes, validando disponibilidad de horarios y evitando conflictos de citas.

El sistema cuenta con control de acceso mediante roles, permitiendo diferentes funcionalidades dependiendo del tipo de usuario.

---

# Tecnologías utilizadas

## Backend
- ASP.NET Core MVC (.NET 9)
- Entity Framework Core
- C#
- LINQ
- SQL Server

## Frontend
- Razor Views
- HTML5
- CSS3
- Bootstrap
- JavaScript
- jQuery DataTables
- Bootstrap Icons

## Seguridad
- ASP.NET Core Identity / Authentication
- Control de acceso basado en roles
- Manejo de sesiones
- Encriptación de contraseñas con BCrypt

---

# Roles del sistema

## Paciente
Funciones principales:

- Registro de usuario
- Inicio de sesión
- Solicitar citas médicas
- Consultar estado de solicitudes
- Ver información de sus citas

---

## Médico
Funciones principales:

- Visualizar solicitudes pendientes
- Aceptar solicitudes de citas
- Cancelar solicitudes
- Gestionar citas asignadas
- Validación automática de disponibilidad:
  - Horario laboral
  - Conflictos de citas
  - Duración de consulta

---

## Administrador
Funciones principales:

- Gestión de usuarios
- Gestión de médicos
- Gestión de pacientes
- Gestión de especialidades
- Asignación manual de médicos
- Administración general del sistema

---

# Gestión de citas

El sistema implementa validaciones para evitar errores comunes:

✅ Un médico no puede aceptar dos citas en el mismo horario.

✅ Las citas solamente pueden ser creadas dentro del horario establecido del médico.

✅ Se calcula automáticamente la duración de la cita según la configuración del médico.

✅ Se mantiene un historial de acciones realizadas sobre las solicitudes.

---

# Estructura del proyecto

```
SistemaCitasMedicas
│
├── Controllers
│   ├── AccountController
│   ├── PacientesController
│   ├── MedicosController
│   ├── SolicitudesMedicoController
│   └── ...
│
├── Models
│   ├── Usuario
│   ├── Paciente
│   ├── Medico
│   ├── Cita
│   ├── SolicitudCita
│   └── ...
│
├── Data
│   └── ApplicationDbContext
│
├── Views
│   ├── Account
│   ├── Pacientes
│   ├── Medicos
│   └── SolicitudesMedico
│
├── wwwroot
│   ├── css
│   ├── js
│   └── assets
│
└── Program.cs
```

---

# Instalación y configuración

## 1. Clonar repositorio

```bash
git clone URL_DEL_REPOSITORIO
```

---

## 2. Configurar la base de datos

Modificar la cadena de conexión en:

```
appsettings.json
```

Ejemplo:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=SistemaCitasMedicas;Trusted_Connection=True;TrustServerCertificate=True"
}
```

---

## 3. Aplicar migraciones

Ejecutar:

```bash
dotnet ef database update
```

---

## 4. Ejecutar proyecto

Desde Visual Studio:

```
F5
```

o mediante terminal:

```bash
dotnet run
```

---

# Credenciales de prueba

*(Modificar según tus usuarios creados)*

Administrador:

```
Correo:
admin@email.com

Contraseña:
********
```

Médico:

```
Correo:
medico@email.com

Contraseña:
********
```

Paciente:

```
Correo:
paciente@email.com

Contraseña:
********
```

---

# Funcionalidades implementadas

- [x] Registro e inicio de sesión
- [x] Sistema de roles
- [x] CRUD de pacientes
- [x] CRUD de médicos
- [x] Gestión de especialidades
- [x] Solicitud de citas
- [x] Aceptación de solicitudes
- [x] Cancelación con motivo
- [x] Control de horarios médicos
- [x] Validación de conflictos de citas
- [x] Historial de solicitudes
- [x] Interfaz responsive

---

# Autor

**Marlon José Suarez Baltodano**

Proyecto académico desarrollado como sistema web de gestión médica.

---

# 📄 Licencia

Este proyecto está desarrollado con fines educativos.
