using SmartSystemPDV.Data.Context;
using SmartSystemPDV.Models;
using Microsoft.EntityFrameworkCore;

namespace SmartSystemPDV.Data.Repositories.FormaPagamentos;

public class FormaPagamentoRepository : Repository<FormaPagamento>, IFormaPagamentoRepository
{
    public FormaPagamentoRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<FormaPagamento>> GetFormasAtivasAsync()
    {
        return await _dbSet
            .Where(f => f.Ativo)
            .OrderBy(f => f.Nome)
            .ToListAsync();
    }
}