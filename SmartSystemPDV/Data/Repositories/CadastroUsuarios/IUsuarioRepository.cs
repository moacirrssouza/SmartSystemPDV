using System.Collections.Generic;
using System.Threading.Tasks;
using SmartSystemPDV.Models;

namespace SmartSystemPDV.Data.Repositories.CadastroUsuarios;

public interface IUsuarioRepository
{
    Task<IEnumerable<Usuario>> ObterTodosAsync();
    Task<Usuario> ObterPorIdAsync(int id);
    Task<Usuario> ObterPorLoginAsync(string login);
    Task<int> InserirAsync(Usuario usuario);
    Task<bool> AtualizarAsync(Usuario usuario);
    Task<bool> ExcluirAsync(int id);
    Task<bool> ValidarLoginAsync(string login, string senha);
    Task<bool> LoginExisteAsync(string login);
    Task<bool> EmailExisteAsync(string email);
}