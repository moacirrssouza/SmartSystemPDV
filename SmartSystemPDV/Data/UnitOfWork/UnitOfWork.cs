using SmartSystemPDV.Data.Context;
using SmartSystemPDV.Data.Repositories.CadastroProduto;
using SmartSystemPDV.Data.Repositories.CadastroUsuarios;
using SmartSystemPDV.Data.Repositories.Vendas;
using SmartSystemPDV.Data.Repositories.FormaPagamentos;
using SmartSystemPDV.Data.Repositories.MovimentacaoEstoque;
using SmartSystemPDV.Data.Repositories.Categorias;
using SmartSystemPDV.Repositories.CadastroProduto;
using Microsoft.EntityFrameworkCore.Storage;

namespace SmartSystemPDV.Data.UnitOfWork;

/// <summary>
/// Implementação do Unit of Work - gerencia transações e repositórios
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    #region Campos Privados

    private readonly AppDbContext _context;
    private IDbContextTransaction _transaction;
    private bool _disposed;

    private IProdutoRepository _produtos;
    
    private IVendaRepository _vendas;
    private IMovimentacaoEstoqueRepository _movimentacoesEstoque;
    private IUsuarioRepository _usuarios;
    private IFormaPagamentoRepository _formasPagamento;
    private ICategoriaRepository _categorias;

    #endregion

    #region Construtor

    public UnitOfWork(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    #endregion

    #region Propriedades dos Repositórios

    public IProdutoRepository Produtos =>
        _produtos ??= new ProdutoRepository(_context);

    public IVendaRepository Vendas =>
        _vendas ??= new VendaRepository(_context);

    public IMovimentacaoEstoqueRepository MovimentacoesEstoque =>
        _movimentacoesEstoque ??= new MovimentacaoEstoqueRepository(_context);

    public IUsuarioRepository Usuarios =>
        _usuarios ??= new UsuarioRepository(_context);

    public IFormaPagamentoRepository FormasPagamento =>
        _formasPagamento ??= new FormaPagamentoRepository(_context);

    public ICategoriaRepository Categorias =>
        _categorias ??= new SmartSystemPDV.Repositories.Categorias.CategoriaRepository(_context);

    #endregion

    #region Persistência

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

    #region Transações

    public void BeginTransaction()
    {
        if (_transaction != null)
            throw new InvalidOperationException("Uma transação já está em andamento.");

        _transaction = _context.Database.BeginTransaction();
    }

    public void Commit()
    {
        if (_transaction == null)
            throw new InvalidOperationException("Não há transação em andamento para confirmar.");

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
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public void Rollback()
    {
        if (_transaction == null)
            throw new InvalidOperationException("Não há transação em andamento para reverter.");

        _transaction.Rollback();
        _transaction.Dispose();
        _transaction = null;
    }

    #endregion

    #region Dispose

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _transaction?.Dispose();
            _context.Dispose();
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