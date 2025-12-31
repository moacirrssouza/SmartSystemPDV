using SmartSystemPDV.Data.Context;
using SmartSystemPDV.Data.UnitOfWork;
using SmartSystemPDV.Models;
using SmartSystemPDV.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SmartSystemPDV.View.Caixa
{
    public partial class BuscaProdutoWindow : Window, INotifyPropertyChanged
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly VendaViewModel _vendaViewModel;

        private ObservableCollection<Produto> _resultados;
        public ObservableCollection<Produto> Resultados
        {
            get => _resultados;
            set { _resultados = value; OnPropertyChanged(); }
        }

        private Produto _produtoSelecionado;
        public Produto ProdutoSelecionado
        {
            get => _produtoSelecionado;
            set { _produtoSelecionado = value; OnPropertyChanged(); OnPropertyChanged(nameof(PodeAdicionar)); }
        }

        private string _termoBusca;
        public string TermoBusca
        {
            get => _termoBusca;
            set { _termoBusca = value; OnPropertyChanged(); }
        }

        private string _quantidade;
        public string Quantidade
        {
            get => _quantidade;
            set { _quantidade = value; OnPropertyChanged(); OnPropertyChanged(nameof(PodeAdicionar)); }
        }

        public bool PodeAdicionar => ProdutoSelecionado != null && !string.IsNullOrWhiteSpace(Quantidade);

        public BuscaProdutoWindow(VendaViewModel vendaViewModel)
        {
            InitializeComponent();
            _vendaViewModel = vendaViewModel;
            _unitOfWork = new UnitOfWork(new AppDbContext());
            Resultados = new ObservableCollection<Produto>();
            TermoBusca = string.Empty;
            Quantidade = "1";
            DataContext = this;

            dgProdutos.MouseDoubleClick += (s, e) =>
            {
                if (ProdutoSelecionado != null)
                    BtnAdicionar_Click(s, e);
            };

            txtTermo.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter) BtnBuscar_Click(s, e);
            };
            txtQuantidade.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter) BtnAdicionar_Click(s, e);
            };

            KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape) BtnCancelar_Click(s, e);
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var lista = await _unitOfWork.Produtos.SearchAsync(TermoBusca ?? "");
                Resultados.Clear();
                foreach (var p in lista)
                    Resultados.Add(p);
                if (Resultados.Any())
                    dgProdutos.SelectedIndex = 0;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Erro ao buscar: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAdicionar_Click(object sender, RoutedEventArgs e)
        {
            if (ProdutoSelecionado == null)
            {
                MessageBox.Show("Selecione um produto.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(Quantidade))
            {
                MessageBox.Show("Informe a quantidade.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _vendaViewModel.ProdutoSelecionado = ProdutoSelecionado;
            _vendaViewModel.QuantidadeProduto = Quantidade;
            _vendaViewModel.AdicionarProdutoCommand.Execute(null);
            Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
