# Proyecto WebMVC - Tienda de Zapatos

## 📋 Requisitos de Entrega Cumplidos

✅ **ApplicationCore** (ProyectoGenApplicationCore) - CENs, CPs, Domain  
✅ **Infrastructure** (ProyectoGenInfraestructure) - NHibernate, Repositorios HQL  
✅ **InitializeDb** - Seed data y demostración  
✅ **WebMVC** - Interfaz de usuario ASP.NET Core MVC

## 🎯 Funcionalidades Implementadas

### Interfaz Web MVC
- ✅ **CRUD Completo de Productos** (Create, Read, Update, Delete)
- ✅ **Login Funcional** (`juan@example.com` / `password123`)
- ✅ **Filtros con HQL** (Color, Precio, Destacado) con elemento SELECT
- ✅ **Operaciones Custom** invocadas desde controladores
- ✅ **CPs Transaccionales** disponibles via Dependency Injection

### Backend
- ✅ 7 Entidades CRUD completas
- ✅ Login implementado
- ✅ 7 ReadFilters con HQL
- ✅ 24+ Custom Operations
- ✅ 4 CustomTransactions (CPs)
- ✅ NHibernate + SQL Server LocalDB

## 🚀 Cómo Ejecutar el Proyecto

### Opción 1: Desde Visual Studio 2022

1. Abrir `prac.sln` en Visual Studio 2022
2. **Paso 1:** Ejecutar InitializeDb primero (para crear la BD y datos)
   - Click derecho en `InitializeDb` → **Set as Startup Project**
   - Presionar `F5` o `Ctrl+F5`
   - Esperar a que termine (presionar cualquier tecla al final)

3. **Paso 2:** Ejecutar WebMVC
   - Click derecho en `WebMVC` → **Set as Startup Project**
   - Presionar `F5` (con debugging) o `Ctrl+F5` (sin debugging)
   - Se abrirá el navegador automáticamente

### Opción 2: Desde Terminal PowerShell

```powershell
# 1. Crear la base de datos y datos iniciales
cd "InitializeDb"
dotnet run

# 2. Ejecutar la aplicación web
cd "..\WebMVC"
dotnet run
```

Luego abrir el navegador en: `https://localhost:5001` o `http://localhost:5000`

## 🔐 Credenciales de Prueba

**Email:** juan@example.com  
**Contraseña:** password123

## 📱 Navegación de la Aplicación

1. **Login** (`/Account/Login`) - Página inicial
2. **Catálogo** (`/Productos/Index`) - Lista de zapatos con filtros
3. **Crear** (`/Productos/Create`) - Formulario para nuevo producto
4. **Editar** (`/Productos/Edit/{id}`) - Modificar producto existente
5. **Detalles** (`/Productos/Details/{id}`) - Ver información completa
6. **Eliminar** (`/Productos/Delete/{id}`) - Borrar producto

## 🔍 Filtros con HQL

En la página principal `/Productos/Index` puedes filtrar por:

- **Color** (texto libre): Busca coincidencias en el color (ej: "Negro", "Blanco", "Azul")
- **Precio Máximo** (decimal): Muestra productos hasta ese precio
- **Destacados** (SELECT): Dropdown para filtrar productos destacados ⭐

Estos filtros usan **HQL** (Hibernate Query Language) en el backend mediante `ProductoCEN.ReadFilter()`.

## 🗂️ Estructura del Proyecto

```
prac/
├── ApplicationCore/          # Domain, CEN, CP, Repositories
├── Infrastructure/           # NHibernate, Implementaciones
├── InitializeDb/            # Programa para seed data
├── WebMVC/                  # ⭐ Aplicación Web MVC
│   ├── Controllers/
│   │   ├── AccountController.cs    # Login/Logout
│   │   └── ProductosController.cs  # CRUD + Filtros
│   ├── Views/
│   │   ├── Account/
│   │   │   └── Login.cshtml
│   │   └── Productos/
│   │       ├── Index.cshtml        # Listado + Filtros
│   │       ├── Create.cshtml       # Formulario crear
│   │       ├── Edit.cshtml         # Formulario editar
│   │       ├── Details.cshtml      # Vista detalle
│   │       └── Delete.cshtml       # Confirmación borrado
│   └── Program.cs                  # Configuración DI
└── Tests/                   # Tests unitarios
```

## 📊 Datos de Ejemplo

La base de datos incluye:

- **5 Zapatos:**
  - Nike Air Max 2024 ($129.99) - Negro/Blanco ⭐
  - Adidas Ultraboost ($159.99) - Azul ⭐
  - Vans Old Skool ($65.00) - Negro
  - Clarks Desert Boot ($95.50) - Marrón ⭐
  - Converse Chuck Taylor ($55.00) - Rojo

- **3 Usuarios** (juan@, maria@, carlos@)
- **3 Categorías** (Deportivo, Casual, Formal)

## 🛠️ Tecnologías Utilizadas

- **.NET 8.0**
- **ASP.NET Core MVC**
- **NHibernate 5.6.0** (ORM con HQL)
- **SQL Server LocalDB**
- **Bootstrap 5** (estilos)
- **Razor Views** (templates)

## ⚠️ Notas Importantes

1. **Base de Datos:** Se crea automáticamente en LocalDB al ejecutar InitializeDb
2. **Sesión:** El login usa sesiones ASP.NET Core para mantener el usuario autenticado
3. **HQL:** Todos los filtros usan HQL en lugar de LINQ para cumplir con los requisitos
4. **Dependency Injection:** Todos los CENs y CPs están registrados en `Program.cs`

## 📝 Elemento SELECT Implementado

En el formulario de filtros (`Index.cshtml`), hay un **elemento SELECT** para filtrar productos destacados:

```html
<select name="destacado" class="form-control">
    <option value="">Todos</option>
    <option value="true">Sí ⭐</option>
    <option value="false">No</option>
</select>
```

Este SELECT invoca el método `ProductoCEN.ReadFilter(destacado: bool?)` que ejecuta HQL.

## ✅ Checklist de Entrega

- [x] Solución con proyectos: ApplicationCore, Infrastructure, InitializeDb
- [x] Operaciones custom implementadas
- [x] CPs (transaccionales) implementados
- [x] CRUD customizadas (si las hay)
- [x] HQLs en todos los filtros
- [x] Interfaz ASP.NET Core MVC
- [x] Vistas de cliente generadas
- [x] Invoca métodos CRUD desde controladores
- [x] Login funcional
- [x] Al menos un elemento SELECT (filtro destacados)

---

**Desarrollado como parte del proyecto de DSM - Ingeniería Multimedia**
