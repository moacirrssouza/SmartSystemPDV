using Microsoft.EntityFrameworkCore;
using SmartSystemPDV.Data.Context;
using SmartSystemPDV.Data.Repositories;
using SmartSystemPDV.Data.Repositories.CadastroProduto;
using SmartSystemPDV.Models;

namespace SmartSystemPDV.Repositories.CadastroProduto;

public class ProdutoRepository : Repository<Produto>, IProdutoRepository
{
    public ProdutoRepository(AppDbContext context) : base(context)
    {
    }

    public Produto GetByCodigo(string codigo)
    {
        return _dbSet.FirstOrDefault(p => p.Codigo.ToString() == codigo);
    }

    public async Task<Produto> GetByCodigoAsync(string codigo)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.Codigo.ToString() == codigo);
    }

    public IEnumerable<Produto> GetProdutosAtivos()
    {
        return _dbSet.Where(p => p.Status == "Ativo").ToList();
    }

    public async Task<IEnumerable<Produto>> GetProdutosAtivosAsync()
    {
        return await _dbSet.Where(p => p.Status == "Ativo").ToListAsync();
    }

    public IEnumerable<Produto> GetProdutosEstoqueBaixo()
    {
        return _dbSet.Where(p => p.Estoque <= p.EstoqueMinimo && p.Status == "Ativo").ToList();
    }

    public async Task<IEnumerable<Produto>> GetProdutosEstoqueBaixoAsync()
    {
        return await _dbSet.Where(p => p.Estoque <= p.EstoqueMinimo && p.Status == "Ativo").ToListAsync();
    }

    public IEnumerable<Produto> GetPorCategoria(string categoria)
    {
        return _dbSet.Where(p => p.Categoria == categoria && p.Status == "Ativo").ToList();
    }

    public async Task<IEnumerable<Produto>> GetPorCategoriaAsync(string categoria)
    {
        return await _dbSet.Where(p => p.Categoria == categoria && p.Status == "Ativo").ToListAsync();
    }

    public decimal GetValorTotalEstoque()
    {
        return _dbSet.Where(p => p.Status == "Ativo")
                    .Sum(p => p.PrecoVenda * p.Estoque);
    }

    public async Task<decimal> GetValorTotalEstoqueAsync()
    {
        return await _dbSet.Where(p => p.Status == "Ativo")
                          .SumAsync(p => p.PrecoVenda * p.Estoque);
    }
}