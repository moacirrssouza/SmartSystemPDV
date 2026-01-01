using SmartSystemPDV.Models;

namespace SmartSystemPDV.Data.Repositories.CadastroUsuarios;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario> ObterPorLoginAsync(string login);
    Task<bool> ValidarLoginAsync(string login, string senha);
    Task<bool> LoginExisteAsync(string login);
    Task<bool> EmailExisteAsync(string email);
}