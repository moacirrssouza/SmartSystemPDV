using SmartSystemPDV.Data.Repositories.CadastroProduto;
using SmartSystemPDV.Data.Repositories.CadastroUsuarios;
using SmartSystemPDV.Data.Repositories.FormaPagamentos;
using SmartSystemPDV.Data.Repositories.MovimentacaoEstoque;
using SmartSystemPDV.Data.Repositories.Vendas;
using SmartSystemPDV.Data.Repositories.Categorias;

namespace SmartSystemPDV.Data.UnitOfWork;

/// <summary>
/// Interface para Unit of Work - gerencia transações e repositórios
/// </summary>
public interface IUnitOfWork : IDisposable
{
    #region Repositórios

    IProdutoRepository Produtos { get; }
    // IClienteRepository Clientes { get; }
    IVendaRepository Vendas { get; }
    IMovimentacaoEstoqueRepository MovimentacoesEstoque { get; }
    IUsuarioRepository Usuarios { get; }
    IFormaPagamentoRepository FormasPagamento { get; }
    ICategoriaRepository Categorias { get; }

    #endregion

    #region Persistência

    int SaveChanges();
    Task<int> SaveChangesAsync();

    #endregion

    #region Transações

    void BeginTransaction();
    void Commit();
    void Rollback();

    #endregion
}