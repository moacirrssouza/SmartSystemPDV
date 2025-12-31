using SmartSystemPDV.Models;

namespace SmartSystemPDV.Data.Repositories.FormaPagamentos;

public interface IFormaPagamentoRepository : IRepository<FormaPagamento>
{
    Task<IEnumerable<FormaPagamento>> GetFormasAtivasAsync();
}