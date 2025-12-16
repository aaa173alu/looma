using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.UnitOfWork;

public class NHibernateUnitOfWork : IUnitOfWork, IDisposable
{
    private readonly ISession _session;
    private ITransaction? _transaction;
    private bool _disposed;

    public NHibernateUnitOfWork(ISession session)
    {
        _session = session;
    }

    public ISession Session => _session;

    public void BeginTransaction()
    {
        if (_transaction is { IsActive: true }) return;
        _transaction = _session.BeginTransaction();
    }

    public void Commit()
    {
        if (_transaction is { IsActive: true })
        {
            _transaction.Commit();
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public void Rollback()
    {
        if (_transaction is { IsActive: true })
        {
            _transaction.Rollback();
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public void SaveChanges()
    {
        if (!_session.IsOpen) throw new InvalidOperationException("Sesión NHibernate cerrada.");
        var hasTx = _transaction is { IsActive: true };
        if (!hasTx) _transaction = _session.BeginTransaction();
        _session.Flush();
        if (!hasTx)
        {
            _transaction!.Commit();
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        try
        {
            if (_transaction is { IsActive: true })
                _transaction.Rollback();
            _transaction?.Dispose();
            if (_session.IsOpen) _session.Close();
            _session.Dispose();
        }
        finally { _disposed = true; }
    }
}
