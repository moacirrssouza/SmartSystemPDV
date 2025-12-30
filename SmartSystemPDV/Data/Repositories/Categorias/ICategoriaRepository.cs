using SmartSystemPDV.Models;

namespace SmartSystemPDV.Data.Repositories.Categorias;

/// <summary>
/// Interface para repositório de Categorias
/// </summary>
public interface ICategoriaRepository : IRepository<Categoria>
{
    IEnumerable<Categoria> GetCategoriasAtivas();
    Task<IEnumerable<Categoria>> GetCategoriasAtivasAsync();
    Categoria GetByNome(string nome);
    Task<Categoria> GetByNomeAsync(string nome);
    int GetQuantidadeProdutosPorCategoria(int categoriaId);
    Task<int> GetQuantidadeProdutosPorCategoriaAsync(int categoriaId);
}