using Microsoft.EntityFrameworkCore;
using SmartSystemPDV.Data.Context;
using SmartSystemPDV.Models;

namespace SmartSystemPDV.Data.Repositories.MovimentacaoEstoque;

public class MovimentacaoEstoqueRepository : Repository<Models.MovimentacaoEstoque>, IMovimentacaoEstoqueRepository
{
    public MovimentacaoEstoqueRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Models.MovimentacaoEstoque>> GetByProdutoIdAsync(int produtoId)
    {
        return await _dbSet
            .Include(m => m.Produto)
            .Include(m => m.Usuario)
            .Where(m => m.ProdutoId == produtoId)
            .OrderByDescending(m => m.DataHora)
            .ToListAsync();
    }

    public async Task<IEnumerable<Models.MovimentacaoEstoque>> GetByPeriodoAsync(DateTime inicio, DateTime fim)
    {
        return await _dbSet
            .Include(m => m.Produto)
            .Include(m => m.Usuario)
            .Where(m => m.DataHora >= inicio && m.DataHora <= fim)
            .OrderByDescending(m => m.DataHora)
            .ToListAsync();
    }
    
    public override async Task<IEnumerable<Models.MovimentacaoEstoque>> GetAllAsync()
    {
        return await _dbSet
            .Include(m => m.Produto)
            .Include(m => m.Usuario)
            .OrderByDescending(m => m.DataHora)
            .ToListAsync();
    }
}
