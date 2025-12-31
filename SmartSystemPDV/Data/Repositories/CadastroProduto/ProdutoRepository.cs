using SmartSystemPDV.Data.Context;
using SmartSystemPDV.Data.Repositories;
using SmartSystemPDV.Data.Repositories.CadastroProduto;
using SmartSystemPDV.Models;
using Microsoft.EntityFrameworkCore;

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

    public async Task<IEnumerable<Produto>> SearchAsync(string termo)
    {
        if (string.IsNullOrWhiteSpace(termo))
            return await GetProdutosAtivosAsync();

        termo = termo.ToLower().Trim();

        return await _dbSet
            .Include(p => p.Categoria)
            .Where(p => p.Ativo && (
                p.Codigo.ToString().Contains(termo) ||
                p.CodigoBarras.ToLower().Contains(termo) ||
                p.Nome.ToLower().Contains(termo)
            ))
            .OrderBy(p => p.Nome)
            .Take(50)
            .ToListAsync();
    }

    public async Task<IEnumerable<Produto>> GetProdutosBaixoEstoqueAsync()
    {
        return await _dbSet
            .Include(p => p.Categoria)
            .Where(p => p.Ativo && p.EstoqueAtual <= p.EstoqueMinimo)
            .OrderBy(p => p.EstoqueAtual)
            .ToListAsync();
    }

    public async Task<bool> AtualizarEstoqueAsync(int produtoId, int quantidade)
    {
        var produto = await GetByIdAsync(produtoId);
        if (produto == null)
            return false;

        produto.EstoqueAtual += quantidade;
        produto.DataAtualizacao = DateTime.Now;

        _dbSet.Update(produto);
        await _context.SaveChangesAsync();

        return true;
    }
}