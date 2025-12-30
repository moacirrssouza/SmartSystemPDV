using SmartSystemPDV.Data.Repositories.CadastroProduto;
using SmartSystemPDV.Data.Repositories.CadastroUsuarios;
using SmartSystemPDV.Repositories.CadastroProduto;
using Microsoft.EntityFrameworkCore.Storage;
using SmartSystemPDV.Data.Context;

namespace SmartSystemPDV.Data.UnitOfWork;

/// <summary>
/// Implementação do Unit of Work - gerencia transações e repositórios
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction _transaction;

    // Repositórios
    private IProdutoRepository _produtos;
    //private IClienteRepository _clientes;
    //private IVendaRepository _vendas;
    //private IMovimentacaoEstoqueRepository _movimentacoesEstoque;
    //private IUsuarioRepository _usuarios;
    //private IFormaPagamentoRepository _formasPagamento;
    //private ICategoriaRepository _categorias;

    public UnitOfWork(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    #region Propriedades dos Repositórios

    public IProdutoRepository Produtos
    {
        get
        {
            if (_produtos == null)
                _produtos = new ProdutoRepository(_context);
            return _produtos;
        }
    }

    //public IClienteRepository Clientes
    //{
    //    get
    //    {
    //        if (_clientes == null)
    //            _clientes = new ClienteRepository(_context);
    //        return _clientes;
    //    }
    //}

    //public IVendaRepository Vendas
    //{
    //    get
    //    {
    //        if (_vendas == null)
    //            _vendas = new VendaRepository(_context);
    //        return _vendas;
    //    }
    //}

    //public IMovimentacaoEstoqueRepository MovimentacoesEstoque
    //{
    //    get
    //    {
    //        if (_movimentacoesEstoque == null)
    //            _movimentacoesEstoque = new MovimentacaoEstoqueRepository(_context);
    //        return _movimentacoesEstoque;
    //    }
    //}

    //public IUsuarioRepository Usuarios
    //{
    //    get
    //    {
    //        if (_usuarios == null)
    //            _usuarios = new UsuarioRepository(_context);
    //        return _usuarios;
    //    }
    //}

    //public IFormaPagamentoRepository FormasPagamento
    //{
    //    get
    //    {
    //        if (_formasPagamento == null)
    //            _formasPagamento = new FormaPagamentoRepository(_context);
    //        return _formasPagamento;
    //    }
    //}

    //public ICategoriaRepository Categorias
    //{
    //    get
    //    {
    //        if (_categorias == null)
    //            _categorias = new CategoriaRepository(_context);
    //        return _categorias;
    //    }
    //}

    #endregion

    #region Métodos de Salvamento

    public int SaveChanges()
    {
        try
        {
            return _context.SaveChanges();
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao salvar alterações no banco de dados: {ex.Message}", ex);
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        try
        {
            return await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao salvar alterações no banco de dados: {ex.Message}", ex);
        }
    }

    #endregion

    #region Métodos de Transação

    public void BeginTransaction()
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("Uma transação já está em andamento.");
        }

        _transaction = _context.Database.BeginTransaction();
    }

    public void Commit()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("Não há transação em andamento para confirmar.");
        }

        try
        {
            _context.SaveChanges();
            _transaction.Commit();
        }
        catch
        {
            Rollback();
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public void Rollback()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("Não há transação em andamento para reverter.");
        }

        try
        {
            _transaction.Rollback();
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    #endregion

    #region Dispose

    private bool _disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _transaction?.Dispose();
                _context?.Dispose();
            }
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}