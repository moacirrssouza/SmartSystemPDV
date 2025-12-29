using SmartSystemPDV.Models;

namespace SmartSystemPDV.Data.Repositories.CadastroProduto;

/// <summary>
/// Interface para repositório de Produtos
/// </summary>
public interface IProdutoRepository : IRepository<Produto>
{
    Produto GetByCodigo(string codigo);
    Task<Produto> GetByCodigoAsync(string codigo);
    IEnumerable<Produto> GetProdutosAtivos();
    Task<IEnumerable<Produto>> GetProdutosAtivosAsync();
    IEnumerable<Produto> GetProdutosEstoqueBaixo();
    Task<IEnumerable<Produto>> GetProdutosEstoqueBaixoAsync();
    IEnumerable<Produto> GetPorCategoria(string categoria);
    Task<IEnumerable<Produto>> GetPorCategoriaAsync(string categoria);
    decimal GetValorTotalEstoque();
    Task<decimal> GetValorTotalEstoqueAsync();
}