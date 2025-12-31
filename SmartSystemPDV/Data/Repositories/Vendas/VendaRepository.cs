using Microsoft.EntityFrameworkCore;
using SmartSystemPDV.Data.Context;
using SmartSystemPDV.Models;

namespace SmartSystemPDV.Data.Repositories.Vendas
{
    public class VendaRepository : Repository<Venda>, IVendaRepository
    {
        public VendaRepository(AppDbContext context) : base(context) { }

        public override async Task<IEnumerable<Venda>> GetAllAsync()
        {
            return await _dbSet
                .Include(v => v.Cliente)
                .Include(v => v.Itens)
                    .ThenInclude(i => i.Produto)
                .OrderByDescending(v => v.DataVenda)
                .ToListAsync();
        }

        public async Task<Venda> GetVendaCompletaAsync(int id)
        {
            return await _dbSet
                .Include(v => v.Cliente)
                .Include(v => v.Itens)
                    .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<string> GerarNumeroVendaAsync()
        {
            var hoje = DateTime.Now;
            var prefixo = hoje.ToString("yyyyMMdd");

            var ultimaVenda = await _dbSet
                .Where(v => v.NumeroVenda.StartsWith(prefixo))
                .OrderByDescending(v => v.NumeroVenda)
                .FirstOrDefaultAsync();

            if (ultimaVenda == null)
                return $"{prefixo}0001";

            var ultimoNumero = int.Parse(ultimaVenda.NumeroVenda.Substring(8));
            var novoNumero = ultimoNumero + 1;

            return $"{prefixo}{novoNumero:D4}";
        }

        public async Task<IEnumerable<Venda>> GetVendasDoPeriodoAsync(DateTime dataInicio, DateTime dataFim)
        {
            return await _dbSet
                .Include(v => v.Cliente)
                .Include(v => v.Itens)
                .Where(v => v.DataVenda >= dataInicio && v.DataVenda <= dataFim)
                .OrderByDescending(v => v.DataVenda)
                .ToListAsync();
        }

        public async Task<IEnumerable<Venda>> GetVendasPorClienteAsync(int clienteId)
        {
            return await _dbSet
                .Include(v => v.Itens)
                .Where(v => v.ClienteId == clienteId)
                .OrderByDescending(v => v.DataVenda)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalVendasDiaAsync(DateTime data)
        {
            var inicio = data.Date;
            var fim = inicio.AddDays(1);

            return await _dbSet
                .Where(v => v.DataVenda >= inicio && v.DataVenda < fim && v.Status == "FINALIZADA")
                .SumAsync(v => v.ValorFinal);
        }
    }
}
