using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SmartSystemPDV.Commands;
using SmartSystemPDV.Data.Repositories.CadastroUsuarios;
using SmartSystemPDV.Models;

namespace SmartSystemPDV.ViewModels
{
    public class UsuarioViewModel : ViewModelBase
    {
        private readonly IUsuarioRepository _usuarioRepository;

        #region Propriedades

        private ObservableCollection<Usuario> _usuarios;
        public ObservableCollection<Usuario> Usuarios
        {
            get => _usuarios;
            set => SetProperty(ref _usuarios, value);
        }

        private Usuario _usuarioSelecionado;
        public Usuario UsuarioSelecionado
        {
            get => _usuarioSelecionado;
            set
            {
                SetProperty(ref _usuarioSelecionado, value);
                if (value != null)
                    PreencherCampos(value);
            }
        }

        private int _id;
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _nome;
        public string Nome
        {
            get => _nome;
            set => SetProperty(ref _nome, value);
        }

        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _login;
        public string Login
        {
            get => _login;
            set => SetProperty(ref _login, value);
        }

        private string _senha;
        public string Senha
        {
            get => _senha;
            set => SetProperty(ref _senha, value);
        }

        private string _confirmarSenha;
        public string ConfirmarSenha
        {
            get => _confirmarSenha;
            set => SetProperty(ref _confirmarSenha, value);
        }

        private bool _ativo;
        public bool Ativo
        {
            get => _ativo;
            set => SetProperty(ref _ativo, value);
        }

        private string _perfil;
        public string Perfil
        {
            get => _perfil;
            set => SetProperty(ref _perfil, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ObservableCollection<string> Perfis { get; set; }

        #endregion

        #region Comandos

        public ICommand SalvarCommand { get; }
        public ICommand NovoCommand { get; }
        public ICommand ExcluirCommand { get; }
        public ICommand CarregarUsuariosCommand { get; }

        #endregion

        public UsuarioViewModel(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;

            Usuarios = new ObservableCollection<Usuario>();
            Perfis = new ObservableCollection<string> { "Admin", "Gerente", "Usuario", "Vendedor" };

            SalvarCommand = new RelayCommand(async _ => await SalvarAsync(), _ => PodeSalvar());
            NovoCommand = new RelayCommand(_ => Novo());
            ExcluirCommand = new RelayCommand(async _ => await ExcluirAsync(), _ => PodeExcluir());
            CarregarUsuariosCommand = new RelayCommand(async _ => await CarregarUsuariosAsync());

            Ativo = true;
            Perfil = "Usuario";
        }

        #region Métodos

        private async Task CarregarUsuariosAsync()
        {
            try
            {
                IsLoading = true;
                var usuarios = await _usuarioRepository.ObterTodosAsync();
                Usuarios.Clear();
                foreach (var usuario in usuarios)
                {
                    Usuarios.Add(usuario);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar usuários: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool PodeSalvar()
        {
            return !string.IsNullOrWhiteSpace(Nome) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Login) &&
                   !string.IsNullOrWhiteSpace(Senha);
        }

        private async Task SalvarAsync()
        {
            try
            {
                // Validações
                if (!ValidarCampos())
                    return;

                IsLoading = true;

                var usuario = new Usuario
                {
                    Id = Id,
                    Nome = Nome,
                    Email = Email,
                    Login = Login,
                    Senha = Senha, // Em produção, você deve criptografar a senha
                    Ativo = Ativo,
                    Perfil = Perfil
                };

                if (Id == 0)
                {
                    // Novo usuário
                    if (await _usuarioRepository.LoginExisteAsync(Login))
                    {
                        MessageBox.Show("Login já existe no sistema.", "Aviso",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (await _usuarioRepository.EmailExisteAsync(Email))
                    {
                        MessageBox.Show("Email já existe no sistema.", "Aviso",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    int novoId = await _usuarioRepository.InserirAsync(usuario);
                    usuario.Id = novoId;
                    Usuarios.Add(usuario);
                    MessageBox.Show("Usuário cadastrado com sucesso!", "Sucesso",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Atualizar usuário
                    bool sucesso = await _usuarioRepository.AtualizarAsync(usuario);
                    if (sucesso)
                    {
                        var usuarioExistente = Usuarios.FirstOrDefault(u => u.Id == Id);
                        if (usuarioExistente != null)
                        {
                            Usuarios.Remove(usuarioExistente);
                            Usuarios.Add(usuario);
                        }
                        MessageBox.Show("Usuário atualizado com sucesso!", "Sucesso",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

                Novo();
                await CarregarUsuariosAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar usuário: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(Nome))
            {
                MessageBox.Show("Nome é obrigatório.", "Validação",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                MessageBox.Show("Email é obrigatório.", "Validação",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(Login))
            {
                MessageBox.Show("Login é obrigatório.", "Validação",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(Senha))
            {
                MessageBox.Show("Senha é obrigatória.", "Validação",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (Senha.Length < 6)
            {
                MessageBox.Show("Senha deve ter no mínimo 6 caracteres.", "Validação",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (Senha != ConfirmarSenha)
            {
                MessageBox.Show("Senha e Confirmar Senha não conferem.", "Validação",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void Novo()
        {
            Id = 0;
            Nome = string.Empty;
            Email = string.Empty;
            Login = string.Empty;
            Senha = string.Empty;
            ConfirmarSenha = string.Empty;
            Ativo = true;
            Perfil = "Usuario";
            UsuarioSelecionado = null;
        }

        private bool PodeExcluir()
        {
            return UsuarioSelecionado != null;
        }

        private async Task ExcluirAsync()
        {
            try
            {
                if (UsuarioSelecionado == null)
                    return;

                var resultado = MessageBox.Show(
                    $"Deseja realmente excluir o usuário '{UsuarioSelecionado.Nome}'?",
                    "Confirmação",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (resultado != MessageBoxResult.Yes)
                    return;

                IsLoading = true;

                bool sucesso = await _usuarioRepository.ExcluirAsync(UsuarioSelecionado.Id);
                if (sucesso)
                {
                    Usuarios.Remove(UsuarioSelecionado);
                    MessageBox.Show("Usuário excluído com sucesso!", "Sucesso",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    Novo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao excluir usuário: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void PreencherCampos(Usuario usuario)
        {
            Id = usuario.Id;
            Nome = usuario.Nome;
            Email = usuario.Email;
            Login = usuario.Login;
            Senha = usuario.Senha;
            ConfirmarSenha = usuario.Senha;
            Ativo = usuario.Ativo;
            Perfil = usuario.Perfil;
        }

        #endregion
    }
}