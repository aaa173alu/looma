# Generated Clean Architecture scaffold (demo)

Archivos generados a partir de `diagrama2.wsd` y `solution.plan.md`:

- `domain.model.json` — representación JSON del modelo de dominio.
- `ApplicationCore/` — proyecto con entidades de dominio (`EN`) y interfaces de repositorio.
- `Infrastructure/` — adaptadores en memoria (implementaciones de repositorios para demo).
- `InitializeDb/` — proyecto consola que demuestra creación/listado de entidades usando los repositorios in-memory.

Cómo compilar y ejecutar (PowerShell):

```powershell
Push-Location .\InitializeDb
dotnet run --project .\InitializeDb.csproj
Pop-Location
```

Modo NHibernate (crear esquema y seed):

```powershell
Push-Location .\InitializeDb
dotnet run --project .\InitializeDb.csproj nhibernate
Pop-Location
```

Si no quieres que `InitializeDb` ejecute `SchemaExport` (por ejemplo en entornos donde no deseas modificar la BD), pasa el flag `noschema`:

```powershell
Push-Location .\InitializeDb
dotnet run --project .\InitializeDb.csproj nhibernate noschema
Pop-Location
```

También puedes definir la cadena de conexión por variable de entorno `NH_CONNECTION` para no tocar `NHibernate.cfg.xml`:

```powershell
$env:NH_CONNECTION = 'Data Source=localhost\\SQLEXPRESS;Initial Catalog=MyDb;Integrated Security=True;'
dotnet run --project .\InitializeDb.csproj nhibernate
```

Instalación del SDK (.NET 8):

1. Descarga e instala el .NET SDK desde https://dotnet.microsoft.com/download
2. Verifica con `dotnet --version` (debe mostrar 8.x)

Alternativa sin privilegios (script local):

Si no quieres o no puedes instalar el SDK globalmente, puedes usar el script incluido que descargará una copia local del SDK en `./.dotnet` y la usará para compilar:

```powershell
# descarga e instala .NET 8 en ./.dotnet y compila la solución
.\scripts\install-and-build.ps1
```

Después de ejecutar el script, puedes añadir la ruta local a la sesión PowerShell con:

```powershell
. .\scripts\use-local-dotnet.ps1
dotnet --version
```


Notas:
- Implementaciones en memoria están disponibles para pruebas rápidas. El modo `nhibernate` usa los mapeos `.hbm.xml` en `Infrastructure/NHibernate/Mappings` y ejecuta `SchemaExport` para crear el esquema en la base configurada en `Infrastructure/NHibernate/NHibernate.cfg.xml`.
- Ajusta la cadena de conexión en `NHibernate.cfg.xml` si necesitas usar `localhost\\SQLEXPRESS` u otra instancia/credenciales.
- Si prefieres que `InitializeDb` no ejecute `SchemaExport` automáticamente, puedo añadir una opción adicional (`--no-schema`) para evitar modificaciones a la base de datos.

CI (GitHub Actions):

- He añadido un workflow de ejemplo en `.github/workflows/ci.yml` que compila los proyectos y ejecuta los tests en .NET 8.

