using SmartSystemPDV.Data.UnitOfWork;
using SmartSystemPDV.Services;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SmartSystemPDV.Commands;
using SmartSystemPDV.Models;

namespace SmartSystemPDV.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;

        private string _usuario;
        public string Usuario
        {
            get => _usuario;
            set => SetProperty(ref _usuario, value);
        }

        private string _senha;
        public string Senha
        {
            get => _senha;
            set => SetProperty(ref _senha, value);
        }

        private bool _lembrarMe;
        public bool LembrarMe
        {
            get => _lembrarMe;
            set => SetProperty(ref _lembrarMe, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand EntrarCommand { get; }
        public ICommand FecharCommand { get; }

        public event System.Action LoginSucesso;

        public LoginViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            EntrarCommand = new RelayCommand(async _ => await Entrar(), _ => PodeEntrar());
            FecharCommand = new RelayCommand(_ => Application.Current.Shutdown());
            _usuario = string.Empty;
            _senha = string.Empty;
        }

        private bool PodeEntrar()
        {
            return !string.IsNullOrWhiteSpace(Usuario) && !string.IsNullOrWhiteSpace(Senha);
        }

        private async Task Entrar()
        {
            try
            {
                IsLoading = true;
                bool valido = await _unitOfWork.Usuarios.ValidarLoginAsync(Usuario, Senha);
                if (!valido)
                {
                    MessageBox.Show("Usuário ou senha inválidos.", "Login", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Usuario usuario = await _unitOfWork.Usuarios.ObterPorLoginAsync(Usuario);
                SessionManager.CurrentUser = usuario;
                LoginSucesso?.Invoke();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Erro ao efetuar login: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
