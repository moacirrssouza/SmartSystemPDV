using SmartSystemPDV.Data.UnitOfWork;
using SmartSystemPDV.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using SmartSystemPDV.Commands;
using System.Windows;

namespace SmartSystemPDV.ViewModels
{
    public class CategoriasViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;

        private ObservableCollection<Categoria> _categorias;
        public ObservableCollection<Categoria> Categorias
        {
            get => _categorias;
            set => SetProperty(ref _categorias, value);
        }

        private Categoria? _categoriaSelecionada;
        public Categoria? CategoriaSelecionada
        {
            get => _categoriaSelecionada;
            set
            {
                if (SetProperty(ref _categoriaSelecionada, value))
                {
                    if (value != null)
                    {
                        Id = value.Id;
                        Nome = value.Nome ?? string.Empty;
                        Descricao = value.Descricao ?? string.Empty;
                        Ativo = value.Ativo;
                    }
                    else
                    {
                        Id = 0;
                        Nome = string.Empty;
                        Descricao = string.Empty;
                        Ativo = true;
                    }

                    (ExcluirCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
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

        private string _descricao;
        public string Descricao
        {
            get => _descricao;
            set => SetProperty(ref _descricao, value);
        }

        private bool _ativo = true;
        public bool Ativo
        {
            get => _ativo;
            set => SetProperty(ref _ativo, value);
        }

        public ICommand NovoCommand { get; }
        public ICommand SalvarCommand { get; }
        public ICommand ExcluirCommand { get; }
        public ICommand CarregarCommand { get; }

        public CategoriasViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _categorias = new ObservableCollection<Categoria>();
            NovoCommand = new RelayCommand(_ => Novo());
            SalvarCommand = new RelayCommand(async _ => await Salvar());
            ExcluirCommand = new RelayCommand(async _ => await Excluir(), _ => CategoriaSelecionada != null);
            CarregarCommand = new RelayCommand(async _ => await Carregar());
            _nome = string.Empty;
            _descricao = string.Empty;
        }

        private async Task Carregar()
        {
            var lista = await _unitOfWork.Categorias.GetAllAsync();
            Categorias = new ObservableCollection<Categoria>(lista);
        }

        private void Novo()
        {
            Id = 0;
            Nome = string.Empty;
            Descricao = string.Empty;
            Ativo = true;
            CategoriaSelecionada = null;
        }

        private async Task Salvar()
        {
            if (string.IsNullOrWhiteSpace(Nome))
            {
                MessageBox.Show("Informe o nome da categoria.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Id == 0)
            {
                var cat = new Categoria { Nome = Nome, Descricao = Descricao, Ativo = Ativo };
                await _unitOfWork.Categorias.AddAsync(cat);
            }
            else
            {
                var cat = await _unitOfWork.Categorias.GetByIdAsync(Id);
                if (cat != null)
                {
                    cat.Nome = Nome;
                    cat.Descricao = Descricao;
                    cat.Ativo = Ativo;
                }
            }

            await _unitOfWork.SaveChangesAsync();
            await Carregar();
            Novo();
            MessageBox.Show("Categoria salva.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task Excluir()
        {
            if (CategoriaSelecionada == null) return;
            _unitOfWork.Categorias.Remove(CategoriaSelecionada);
            await _unitOfWork.SaveChangesAsync();
            await Carregar();
            MessageBox.Show("Categoria excluída.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
