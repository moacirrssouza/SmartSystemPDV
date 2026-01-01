using SmartSystemPDV.Commands;
using SmartSystemPDV.Data.UnitOfWork;
using SmartSystemPDV.Models;
using SmartSystemPDV.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SmartSystemPDV.ViewModels
{
    public class CadastroUsuariosViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;

        #region Collections

        private ObservableCollection<Usuario> _listaUsuarios;
        public ObservableCollection<Usuario> ListaUsuarios
        {
            get => _listaUsuarios;
            set => SetProperty(ref _listaUsuarios, value);
        }

        public ObservableCollection<string> Perfis { get; } = new ObservableCollection<string>
        {
            "Administrador", "Gerente", "Vendedor", "Estoquista", "Operador"
        };

        public ObservableCollection<string> StatusOptions { get; } = new ObservableCollection<string>
        {
            "Ativo", "Inativo"
        };

        public ObservableCollection<string> Departamentos { get; } = new ObservableCollection<string>
        {
            "Administrativo", "Vendas", "Estoque", "Financeiro", "TI", "RH", "Gerência"
        };

        #endregion

        #region Properties - Form

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

        private string _login;
        public string Login
        {
            get => _login;
            set => SetProperty(ref _login, value);
        }

        private string _cargo;
        public string Cargo
        {
            get => _cargo;
            set => SetProperty(ref _cargo, value);
        }

        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _senha;
        public string Senha
        {
            get => _senha;
            set => SetProperty(ref _senha, value);
        }

        private string _confirmacaoSenha;
        public string ConfirmacaoSenha
        {
            get => _confirmacaoSenha;
            set => SetProperty(ref _confirmacaoSenha, value);
        }

        private string _telefone;
        public string Telefone
        {
            get => _telefone;
            set => SetProperty(ref _telefone, value);
        }

        private string _departamentoSelecionado;
        public string DepartamentoSelecionado
        {
            get => _departamentoSelecionado;
            set => SetProperty(ref _departamentoSelecionado, value);
        }

        private string _perfilSelecionado;
        public string PerfilSelecionado
        {
            get => _perfilSelecionado;
            set
            {
                if (SetProperty(ref _perfilSelecionado, value))
                {
                    AtualizarDescricaoPerfil();
                }
            }
        }

        private string _descricaoPerfil;
        public string DescricaoPerfil
        {
            get => _descricaoPerfil;
            set => SetProperty(ref _descricaoPerfil, value);
        }

        private string _statusSelecionado;
        public string StatusSelecionado
        {
            get => _statusSelecionado;
            set => SetProperty(ref _statusSelecionado, value);
        }

        private bool _camposHabilitados;
        public bool CamposHabilitados
        {
            get => _camposHabilitados;
            set => SetProperty(ref _camposHabilitados, value);
        }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public string TotalUsuariosTexto => $"Total: {ListaUsuarios?.Count ?? 0} usuário(s)";

        #endregion

        #region Properties - Filter

        private string _textoPesquisa;
        public string TextoPesquisa
        {
            get => _textoPesquisa;
            set
            {
                if (SetProperty(ref _textoPesquisa, value))
                {
                    FiltrarUsuarios();
                }
            }
        }

        private string _filtroPerfil;
        public string FiltroPerfil
        {
            get => _filtroPerfil;
            set
            {
                if (SetProperty(ref _filtroPerfil, value))
                {
                    FiltrarUsuarios();
                }
            }
        }

        private string _filtroStatus;
        public string FiltroStatus
        {
            get => _filtroStatus;
            set
            {
                if (SetProperty(ref _filtroStatus, value))
                {
                    FiltrarUsuarios();
                }
            }
        }

        #endregion

        #region Properties - Selection

        private Usuario _usuarioSelecionadoGrid;
        public Usuario UsuarioSelecionadoGrid
        {
            get => _usuarioSelecionadoGrid;
            set
            {
                if (SetProperty(ref _usuarioSelecionadoGrid, value))
                {
                    // Update commands availability
                    (EditarCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (ExcluirCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (ResetarSenhaCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (GerenciarPermissoesCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Commands

        public ICommand NovoCommand { get; }
        public ICommand SalvarCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand ExcluirCommand { get; }
        public ICommand CancelarCommand { get; }
        public ICommand VoltarCommand { get; }
        public ICommand ResetarSenhaCommand { get; }
        public ICommand GerenciarPermissoesCommand { get; }

        public string UsuarioLogado => SessionManager.CurrentUser?.Nome ?? "Usuário Desconhecido";

        #endregion

        public CadastroUsuariosViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            
            // Initialize collections
            _listaUsuarios = new ObservableCollection<Usuario>(); // Initialize backing field
            
            // Initialize non-nullable fields
            _nome = string.Empty;
            _login = string.Empty;
            _cargo = string.Empty;
            _email = string.Empty;
            _senha = string.Empty;
            _confirmacaoSenha = string.Empty;
            _telefone = string.Empty;
            _departamentoSelecionado = string.Empty;
            _perfilSelecionado = string.Empty;
            _descricaoPerfil = string.Empty;
            _statusSelecionado = "Ativo";
            _usuarioSelecionadoGrid = null!;
            _textoPesquisa = string.Empty;
            _filtroPerfil = "Todos";
            _filtroStatus = "Todos";

            NovoCommand = new RelayCommand(_ => NovoUsuario());
            SalvarCommand = new RelayCommand(async _ => await SalvarUsuario());
            EditarCommand = new RelayCommand(_ => EditarUsuario(), _ => UsuarioSelecionadoGrid != null);
            ExcluirCommand = new RelayCommand(async _ => await ExcluirUsuario(), _ => UsuarioSelecionadoGrid != null);
            CancelarCommand = new RelayCommand(_ => CancelarEdicao());
            VoltarCommand = new RelayCommand(Voltar);
            ResetarSenhaCommand = new RelayCommand(_ => ResetarSenha(), _ => UsuarioSelecionadoGrid != null);
            GerenciarPermissoesCommand = new RelayCommand(_ => GerenciarPermissoes(), _ => UsuarioSelecionadoGrid != null);

            CamposHabilitados = false;
            
            CarregarUsuarios();
        }

        private async void CarregarUsuarios()
        {
            try
            {
                var usuarios = await _unitOfWork.Usuarios.GetAllAsync();
                _allUsuarios = usuarios.OrderBy(u => u.Nome).ToList();
                FiltrarUsuarios();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar usuários: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<Usuario> _allUsuarios = new List<Usuario>();

        private void FiltrarUsuarios()
        {
            var query = _allUsuarios.AsQueryable();

            if (!string.IsNullOrWhiteSpace(TextoPesquisa))
            {
                string filtro = TextoPesquisa.ToLower().Trim();
                query = query.Where(u => 
                    u.Nome.ToLower().Contains(filtro) || 
                    u.Login.ToLower().Contains(filtro) ||
                    (u.Email != null && u.Email.ToLower().Contains(filtro)));
            }

            if (!string.IsNullOrWhiteSpace(FiltroPerfil) && FiltroPerfil != "Todos")
            {
                query = query.Where(u => u.Perfil == FiltroPerfil);
            }

            if (!string.IsNullOrWhiteSpace(FiltroStatus) && FiltroStatus != "Todos")
            {
                bool ativo = FiltroStatus == "Ativo";
                query = query.Where(u => u.Ativo == ativo);
            }

            ListaUsuarios = new ObservableCollection<Usuario>(query.ToList());
            OnPropertyChanged(nameof(TotalUsuariosTexto));
        }

        private void AtualizarDescricaoPerfil()
        {
            switch (PerfilSelecionado)
            {
                case "Administrador":
                    DescricaoPerfil = "Acesso total ao sistema, incluindo configurações e gerenciamento de usuários.";
                    break;
                case "Gerente":
                    DescricaoPerfil = "Acesso a relatórios, cancelamentos e gestão de estoque.";
                    break;
                case "Vendedor":
                    DescricaoPerfil = "Acesso a vendas e consulta de produtos.";
                    break;
                case "Estoquista":
                    DescricaoPerfil = "Acesso ao controle de estoque e cadastro de produtos.";
                    break;
                case "Operador":
                    DescricaoPerfil = "Acesso básico para operações de caixa.";
                    break;
                default:
                    DescricaoPerfil = string.Empty;
                    break;
            }
        }

        private void NovoUsuario()
        {
            LimparCampos();
            CamposHabilitados = true;
            IsEditing = false;
            StatusSelecionado = "Ativo";
            PerfilSelecionado = "Vendedor"; // Default
            UsuarioSelecionadoGrid = null;
        }

        private void EditarUsuario()
        {
            if (UsuarioSelecionadoGrid != null)
            {
                Id = UsuarioSelecionadoGrid.Id;
                Nome = UsuarioSelecionadoGrid.Nome;
                Login = UsuarioSelecionadoGrid.Login;
                Cargo = UsuarioSelecionadoGrid.Cargo;
                Email = UsuarioSelecionadoGrid.Email;
                Telefone = UsuarioSelecionadoGrid.Telefone;
                DepartamentoSelecionado = UsuarioSelecionadoGrid.Departamento;
                // Senha is not loaded for security, handled separately or kept as is if empty
                Senha = ""; 
                ConfirmacaoSenha = "";
                PerfilSelecionado = UsuarioSelecionadoGrid.Perfil;
                StatusSelecionado = UsuarioSelecionadoGrid.Ativo ? "Ativo" : "Inativo";

                CamposHabilitados = true;
                IsEditing = true;
            }
        }

        private async Task SalvarUsuario()
        {
            if (!ValidarCampos()) return;

            try
            {
                bool isNew = Id == 0;
                Usuario usuario;

                if (isNew)
                {
                    if (await _unitOfWork.Usuarios.LoginExisteAsync(Login))
                    {
                        MessageBox.Show("Login já existe no sistema.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    usuario = new Usuario
                    {
                        Nome = Nome,
                        Login = Login,
                        Cargo = Cargo,
                        Email = Email,
                        Telefone = Telefone,
                        Departamento = DepartamentoSelecionado,
                        Senha = Senha, // Should be hashed in production
                        Perfil = PerfilSelecionado,
                        Ativo = StatusSelecionado == "Ativo",
                        DataCadastro = DateTime.Now
                    };
                    await _unitOfWork.Usuarios.AddAsync(usuario);
                }
                else
                {
                    usuario = await _unitOfWork.Usuarios.GetByIdAsync(Id);
                    if (usuario != null)
                    {
                        usuario.Nome = Nome;
                        usuario.Login = Login;
                        usuario.Cargo = Cargo;
                        usuario.Email = Email;
                        usuario.Telefone = Telefone;
                        usuario.Departamento = DepartamentoSelecionado;
                        usuario.Perfil = PerfilSelecionado;
                        usuario.Ativo = StatusSelecionado == "Ativo";
                        
                        if (!string.IsNullOrWhiteSpace(Senha))
                        {
                            usuario.Senha = Senha; // Should be hashed
                        }
                        
                        // EF Core tracks changes, but we might need to call Update explicitly if using generic repo attached differently
                        // But UnitOfWork shares context, so it should be fine.
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                
                MessageBox.Show("Usuário salvo com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                
                LimparCampos();
                CamposHabilitados = false;
                IsEditing = false;
                CarregarUsuarios();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar usuário: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExcluirUsuario()
        {
            if (UsuarioSelecionadoGrid == null) return;

            var result = MessageBox.Show($"Deseja realmente excluir o usuário '{UsuarioSelecionadoGrid.Nome}'?", 
                "Confirmar Exclusão", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _unitOfWork.Usuarios.Remove(UsuarioSelecionadoGrid);
                    await _unitOfWork.SaveChangesAsync();
                    
                    MessageBox.Show("Usuário excluído com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                    CarregarUsuarios();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao excluir usuário: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CancelarEdicao()
        {
            LimparCampos();
            CamposHabilitados = false;
            IsEditing = false;
            UsuarioSelecionadoGrid = null;
        }

        private void LimparCampos()
        {
            Id = 0;
            Nome = string.Empty;
            Login = string.Empty;
            Cargo = string.Empty;
            Email = string.Empty;
            Telefone = string.Empty;
            DepartamentoSelecionado = null;
            Senha = string.Empty;
            ConfirmacaoSenha = string.Empty;
            PerfilSelecionado = null;
            StatusSelecionado = null;
            DescricaoPerfil = string.Empty;
        }

        private bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(Nome))
            {
                MessageBox.Show("O nome é obrigatório.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(Login))
            {
                MessageBox.Show("O login é obrigatório.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(Email))
            {
                MessageBox.Show("O email é obrigatório.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (Id == 0 && string.IsNullOrWhiteSpace(Senha))
            {
                MessageBox.Show("A senha é obrigatória para novos usuários.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!string.IsNullOrEmpty(Senha) && Senha != ConfirmacaoSenha)
            {
                MessageBox.Show("As senhas não conferem.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(PerfilSelecionado))
            {
                MessageBox.Show("Selecione um perfil.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void ResetarSenha()
        {
             if (UsuarioSelecionadoGrid == null) return;
             // Logic to reset password or show dialog
             MessageBox.Show("Funcionalidade de resetar senha será implementada em breve.", "Informação", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void GerenciarPermissoes()
        {
             if (UsuarioSelecionadoGrid == null) return;
             // Logic to open permissions window
             // For now, just show message or open existing window if compatible
             MessageBox.Show("Funcionalidade de gerenciar permissões será integrada em breve.", "Informação", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Voltar(object param)
        {
             Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
        }
    }
}
