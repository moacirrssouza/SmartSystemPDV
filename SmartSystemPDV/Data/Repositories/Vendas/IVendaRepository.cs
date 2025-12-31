using SmartSystemPDV.Models;

namespace SmartSystemPDV.Data.Repositories.Vendas;

public interface IVendaRepository : IRepository<Venda>
{
    Task<Venda> GetVendaCompletaAsync(int id);
    Task<string> GerarNumeroVendaAsync();
    Task<IEnumerable<Venda>> GetVendasDoPeriodoAsync(DateTime dataInicio, DateTime dataFim);
    Task<IEnumerable<Venda>> GetVendasPorClienteAsync(int clienteId);
    Task<decimal> GetTotalVendasDiaAsync(DateTime data);
}