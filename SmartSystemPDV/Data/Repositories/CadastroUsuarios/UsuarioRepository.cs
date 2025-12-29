using SmartSystemPDV.Models;
using System.Data;
using Dapper;

namespace SmartSystemPDV.Data.Repositories.CadastroUsuarios;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly DatabaseConnection _databaseConnection;

    public UsuarioRepository(DatabaseConnection databaseConnection)
    {
        _databaseConnection = databaseConnection;
    }

    public async Task<IEnumerable<Usuario>> ObterTodosAsync()
    {
        using (IDbConnection db = _databaseConnection.CreateConnection())
        {
            string sql = @"SELECT Id, Nome, Email, Login, Ativo, DataCadastro, 
                              DataAlteracao, Perfil FROM Usuarios ORDER BY Nome";
            return await db.QueryAsync<Usuario>(sql);
        }
    }

    public async Task<Usuario> ObterPorIdAsync(int id)
    {
        using (IDbConnection db = _databaseConnection.CreateConnection())
        {
            string sql = @"SELECT Id, Nome, Email, Login, Senha, Ativo, 
                              DataCadastro, DataAlteracao, Perfil 
                              FROM Usuarios WHERE Id = @Id";
            return await db.QueryFirstOrDefaultAsync<Usuario>(sql, new { Id = id });
        }
    }

    public async Task<Usuario> ObterPorLoginAsync(string login)
    {
        using (IDbConnection db = _databaseConnection.CreateConnection())
        {
            string sql = @"SELECT Id, Nome, Email, Login, Senha, Ativo, 
                              DataCadastro, DataAlteracao, Perfil 
                              FROM Usuarios WHERE Login = @Login";
            return await db.QueryFirstOrDefaultAsync<Usuario>(sql, new { Login = login });
        }
    }

    public async Task<int> InserirAsync(Usuario usuario)
    {
        using (IDbConnection db = _databaseConnection.CreateConnection())
        {
            string sql = @"INSERT INTO Usuarios (Nome, Email, Login, Senha, Ativo, DataCadastro, Perfil) 
                              VALUES (@Nome, @Email, @Login, @Senha, @Ativo, @DataCadastro, @Perfil);
                              SELECT CAST(SCOPE_IDENTITY() as int)";

            usuario.DataCadastro = DateTime.Now;
            usuario.Ativo = true;

            return await db.ExecuteScalarAsync<int>(sql, usuario);
        }
    }

    public async Task<bool> AtualizarAsync(Usuario usuario)
    {
        using (IDbConnection db = _databaseConnection.CreateConnection())
        {
            string sql = @"UPDATE Usuarios 
                              SET Nome = @Nome, 
                                  Email = @Email, 
                                  Login = @Login, 
                                  Senha = @Senha, 
                                  Ativo = @Ativo, 
                                  DataAlteracao = @DataAlteracao, 
                                  Perfil = @Perfil 
                              WHERE Id = @Id";

           // usuario.DataAlteracao = DateTime.Now;

            int linhasAfetadas = await db.ExecuteAsync(sql, usuario);
            return linhasAfetadas > 0;
        }
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        using (IDbConnection db = _databaseConnection.CreateConnection())
        {
            string sql = "DELETE FROM Usuarios WHERE Id = @Id";
            int linhasAfetadas = await db.ExecuteAsync(sql, new { Id = id });
            return linhasAfetadas > 0;
        }
    }

    public async Task<bool> ValidarLoginAsync(string login, string senha)
    {
        using (IDbConnection db = _databaseConnection.CreateConnection())
        {
            string sql = @"SELECT COUNT(1) FROM Usuarios 
                              WHERE Login = @Login AND Senha = @Senha AND Ativo = 1";

            int count = await db.ExecuteScalarAsync<int>(sql, new { Login = login, Senha = senha });
            return count > 0;
        }
    }

    public async Task<bool> LoginExisteAsync(string login)
    {
        using (IDbConnection db = _databaseConnection.CreateConnection())
        {
            string sql = "SELECT COUNT(1) FROM Usuarios WHERE Login = @Login";
            int count = await db.ExecuteScalarAsync<int>(sql, new { Login = login });
            return count > 0;
        }
    }

    public async Task<bool> EmailExisteAsync(string email)
    {
        using (IDbConnection db = _databaseConnection.CreateConnection())
        {
            string sql = "SELECT COUNT(1) FROM Usuarios WHERE Email = @Email";
            int count = await db.ExecuteScalarAsync<int>(sql, new { Email = email });
            return count > 0;
        }
    }
}