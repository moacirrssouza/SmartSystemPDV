using SmartSystemPDV.Models;

namespace SmartSystemPDV.Data.Repositories.MovimentacaoEstoque;

public interface IMovimentacaoEstoqueRepository : IRepository<Models.MovimentacaoEstoque>
{
    Task<IEnumerable<Models.MovimentacaoEstoque>> GetByProdutoIdAsync(int produtoId);
    Task<IEnumerable<Models.MovimentacaoEstoque>> GetByPeriodoAsync(DateTime inicio, DateTime fim);
}
