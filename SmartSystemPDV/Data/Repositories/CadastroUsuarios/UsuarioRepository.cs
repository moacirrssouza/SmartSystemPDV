using Microsoft.EntityFrameworkCore;
using SmartSystemPDV.Data.Context;
using SmartSystemPDV.Models;

namespace SmartSystemPDV.Data.Repositories.CadastroUsuarios;

public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Usuario> ObterPorLoginAsync(string login)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Login == login);
    }

    public async Task<bool> ValidarLoginAsync(string login, string senha)
    {
        return await _dbSet.AnyAsync(u => u.Login == login && u.Senha == senha && u.Ativo);
    }

    public async Task<bool> LoginExisteAsync(string login)
    {
        return await _dbSet.AnyAsync(u => u.Login == login);
    }

    public async Task<bool> EmailExisteAsync(string email)
    {
        return await _dbSet.AnyAsync(u => u.Email == email);
    }
}
