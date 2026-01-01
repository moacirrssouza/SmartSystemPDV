using Microsoft.EntityFrameworkCore;
using SmartSystemPDV.Data.Context;
using SmartSystemPDV.Data.Repositories;
using SmartSystemPDV.Models;
using SmartSystemPDV.Data.Repositories.Categorias;

namespace SmartSystemPDV.Repositories.Categorias
{
    public class CategoriaRepository : Repository<Categoria>, ICategoriaRepository
    {
        public CategoriaRepository(AppDbContext context) : base(context) { }

        public IEnumerable<Categoria> GetCategoriasAtivas()
        {
            return _dbSet.Where(c => c.Ativo).ToList();
        }

        public async Task<IEnumerable<Categoria>> GetCategoriasAtivasAsync()
        {
            return await _dbSet.Where(c => c.Ativo).ToListAsync();
        }

        public Categoria GetByNome(string nome)
        {
            return _dbSet.FirstOrDefault(c => c.Nome == nome);
        }

        public async Task<Categoria> GetByNomeAsync(string nome)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Nome == nome);
        }

        public int GetQuantidadeProdutosPorCategoria(int categoriaId)
        {
            return _dbSet.Include(c => c.Produtos).Where(c => c.Id == categoriaId).Select(c => c.Produtos.Count).FirstOrDefault();
        }

        public async Task<int> GetQuantidadeProdutosPorCategoriaAsync(int categoriaId)
        {
            var categoria = await _dbSet.Include(c => c.Produtos).FirstOrDefaultAsync(c => c.Id == categoriaId);
            return categoria?.Produtos?.Count ?? 0;
        }
    }
}
