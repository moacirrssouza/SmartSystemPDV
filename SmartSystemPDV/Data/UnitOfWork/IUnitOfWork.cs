using SmartSystemPDV.Data.Repositories.CadastroProduto;
using SmartSystemPDV.Data.Repositories.CadastroUsuarios;
using System;
using System.Threading.Tasks;

namespace SmartSystemPDV.Data.UnitOfWork;

/// <summary>
/// Interface para Unit of Work - gerencia transações e repositórios
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // Repositórios
    IProdutoRepository Produtos { get; }
    //IClienteRepository Clientes { get; }
    //IVendaRepository Vendas { get; }
    //IMovimentacaoEstoqueRepository MovimentacoesEstoque { get; }
    //IUsuarioRepository Usuarios { get; }
    //IFormaPagamentoRepository FormasPagamento { get; }
    //ICategoriaRepository Categorias { get; }

    // Métodos para salvar alterações
    int SaveChanges();
    Task<int> SaveChangesAsync();

    // Métodos para transações
    void BeginTransaction();
    void Commit();
    void Rollback();
}