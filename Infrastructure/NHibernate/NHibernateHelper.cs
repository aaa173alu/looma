using NHibernate;
using NHibernate.Cfg;
using System;
using System.IO;

namespace Infrastructure.NHibernate;

public static class NHibernateHelper
{
    private static ISessionFactory? _sessionFactory;

    public static ISessionFactory SessionFactory
    {
        get
        {
            if (_sessionFactory == null) _sessionFactory = BuildSessionFactory();
            return _sessionFactory;
        }
    }

    private static ISessionFactory BuildSessionFactory()
    {
        Configuration cfg = BuildConfiguration();
        cfg.SetProperty("use_proxy_validator", "false");
        return cfg.BuildSessionFactory();
    }

    public static Configuration BuildConfiguration()
    {
        Configuration cfg = new Configuration();
        string baseDir = AppContext.BaseDirectory;
        string cfgPath = Path.Combine(baseDir, "NHibernate", "NHibernate.cfg.xml");
        if (!File.Exists(cfgPath))
        {
            // fallback to project path
            cfgPath = Path.Combine(Directory.GetCurrentDirectory(), "Infrastructure", "NHibernate", "NHibernate.cfg.xml");
        }

        cfg.Configure(cfgPath);

        // Manually add only the mappings we need
        string[] mappingsToLoad = new[] {
            "Producto.hbm.xml",
            "Pedido.hbm.xml",
            "ItemPedido.hbm.xml",
            "Usuario.hbm.xml",
            "Favoritos.hbm.xml",
            "Valoracion.hbm.xml",
            "Carrito.hbm.xml",
            "Categoria.hbm.xml",
            "Tarjeta.hbm.xml"
        };

        string mappingsDir = Path.Combine(baseDir, "NHibernate", "Mappings");
        if (Directory.Exists(mappingsDir))
        {
            foreach (string? mappingFile in mappingsToLoad)
            {
                string filePath = Path.Combine(mappingsDir, mappingFile);
                if (File.Exists(filePath))
                {
                    cfg.AddFile(filePath);
                }
            }
        }

        // Allow overriding the connection string via environment variable NH_CONNECTION
        string? envConn = System.Environment.GetEnvironmentVariable("NH_CONNECTION");
        if (!string.IsNullOrWhiteSpace(envConn))
        {
            cfg.SetProperty("connection.connection_string", envConn);
        }

        // Ensure DataDirectory exists for AttachDBFilename tokens
        try
        {
            string dataDir = Path.Combine(Directory.GetCurrentDirectory(), "InitializeDb", "Data");
            if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);
            AppDomain.CurrentDomain.SetData("DataDirectory", dataDir);
        }
        catch { /* best-effort */ }
        return cfg;
    }

    public static ISession OpenSession() => SessionFactory.OpenSession();
}
