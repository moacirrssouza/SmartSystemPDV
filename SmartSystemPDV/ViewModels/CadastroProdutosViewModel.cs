using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using SmartSystemPDV.Commands;
using SmartSystemPDV.Data.UnitOfWork;
using SmartSystemPDV.Models;

namespace SmartSystemPDV.ViewModels
{
    public class CadastroProdutosViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;
        
        #region Collections
        
        private ObservableCollection<Produto> _listaProdutos;
        public ObservableCollection<Produto> ListaProdutos
        {
            get => _listaProdutos;
            set => SetProperty(ref _listaProdutos, value);
        }

        public ObservableCollection<string> Categorias { get; } = new ObservableCollection<string>
        {
            "Eletrônicos", "Alimentos", "Bebidas", "Limpeza", "Higiene", "Vestuário", "Outros"
        };

        public ObservableCollection<string> Unidades { get; } = new ObservableCollection<string>
        {
            "UN", "KG", "LT", "MT", "CX", "PC"
        };

        public ObservableCollection<string> StatusOptions { get; } = new ObservableCollection<string>
        {
            "Ativo", "Inativo"
        };
        
        public ObservableCollection<int> ItensPorPaginaOptions { get; } = new ObservableCollection<int>
        {
            10, 20, 50, 100
        };

        #endregion

        #region Properties - Form

        private int _codigo;
        public int Codigo
        {
            get => _codigo;
            set => SetProperty(ref _codigo, value);
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

        private string _codigoBarras;
        public string CodigoBarras
        {
            get => _codigoBarras;
            set => SetProperty(ref _codigoBarras, value);
        }

        private string _lote;
        public string Lote
        {
            get => _lote;
            set => SetProperty(ref _lote, value);
        }

        private DateTime? _dataFabricacao;
        public DateTime? DataFabricacao
        {
            get => _dataFabricacao;
            set => SetProperty(ref _dataFabricacao, value);
        }

        private DateTime? _dataVencimento;
        public DateTime? DataVencimento
        {
            get => _dataVencimento;
            set => SetProperty(ref _dataVencimento, value);
        }

        private string _categoriaSelecionada;
        public string CategoriaSelecionada
        {
            get => _categoriaSelecionada;
            set => SetProperty(ref _categoriaSelecionada, value);
        }

        private string _unidadeSelecionada;
        public string UnidadeSelecionada
        {
            get => _unidadeSelecionada;
            set => SetProperty(ref _unidadeSelecionada, value);
        }

        private string _statusSelecionado;
        public string StatusSelecionado
        {
            get => _statusSelecionado;
            set => SetProperty(ref _statusSelecionado, value);
        }

        private string _precoCusto = "0,00";
        public string PrecoCusto
        {
            get => _precoCusto;
            set => SetProperty(ref _precoCusto, value);
        }

        private string _precoVenda = "0,00";
        public string PrecoVenda
        {
            get => _precoVenda;
            set => SetProperty(ref _precoVenda, value);
        }

        private int _estoque;
        public int Estoque
        {
            get => _estoque;
            set => SetProperty(ref _estoque, value);
        }

        private int _estoqueMinimo;
        public int EstoqueMinimo
        {
            get => _estoqueMinimo;
            set => SetProperty(ref _estoqueMinimo, value);
        }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set 
            {
                if (SetProperty(ref _isEditing, value))
                {
                    BotaoSalvarTexto = value ? "💾 Atualizar" : "💾 Salvar";
                }
            }
        }
        
        private bool _camposHabilitados;
        public bool CamposHabilitados
        {
            get => _camposHabilitados;
            set => SetProperty(ref _camposHabilitados, value);
        }

        private string _botaoSalvarTexto = "💾 Salvar";
        public string BotaoSalvarTexto
        {
            get => _botaoSalvarTexto;
            set => SetProperty(ref _botaoSalvarTexto, value);
        }

        #endregion

        #region Properties - Selection/Filtering

        private Produto _produtoSelecionadoGrid;
        public Produto ProdutoSelecionadoGrid
        {
            get => _produtoSelecionadoGrid;
            set
            {
                SetProperty(ref _produtoSelecionadoGrid, value);
                ((RelayCommand)EditarCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ExcluirCommand).RaiseCanExecuteChanged();
            }
        }

        private string _textoPesquisa;
        public string TextoPesquisa
        {
            get => _textoPesquisa;
            set
            {
                if (SetProperty(ref _textoPesquisa, value))
                {
                    _paginaAtual = 1;
                    FiltrarProdutos();
                }
            }
        }

        #endregion

        #region Properties - Pagination

        private int _paginaAtual = 1;
        public int PaginaAtual
        {
            get => _paginaAtual;
            set 
            {
                if (SetProperty(ref _paginaAtual, value))
                    AtualizarPaginacao();
            }
        }

        private int _itensPorPagina = 20;
        public int ItensPorPagina
        {
            get => _itensPorPagina;
            set
            {
                if (SetProperty(ref _itensPorPagina, value))
                {
                    _paginaAtual = 1;
                    AtualizarPaginacao();
                }
            }
        }

        private int _totalItens;
        private int _totalPaginas;
        
        private string _infoPaginacao;
        public string InfoPaginacao
        {
            get => _infoPaginacao;
            set => SetProperty(ref _infoPaginacao, value);
        }
        
        private string _totalProdutosTexto;
        public string TotalProdutosTexto
        {
            get => _totalProdutosTexto;
            set => SetProperty(ref _totalProdutosTexto, value);
        }

        private List<Produto> _todosOsProdutos = new List<Produto>();
        private List<Produto> _produtosFiltrados = new List<Produto>();
        
        // Propriedades para visibilidade/estado dos botões de paginação
        private bool _podeVoltar;
        public bool PodeVoltar
        {
            get => _podeVoltar;
            set => SetProperty(ref _podeVoltar, value);
        }

        private bool _podeAvancar;
        public bool PodeAvancar
        {
            get => _podeAvancar;
            set => SetProperty(ref _podeAvancar, value);
        }

        public ObservableCollection<int> NumerosPagina { get; }

        #endregion

        #region Commands

        public ICommand NovoCommand { get; }
        public ICommand SalvarCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand ExcluirCommand { get; }
        public ICommand CancelarCommand { get; }
        public ICommand VoltarCommand { get; }
        public ICommand PesquisarCommand { get; }
        
        public ICommand ProximaPaginaCommand { get; }
        public ICommand PaginaAnteriorCommand { get; }
        public ICommand PrimeiraPaginaCommand { get; }
        public ICommand UltimaPaginaCommand { get; }
        public ICommand IrParaPaginaCommand { get; }

        public string UsuarioLogado => Services.SessionManager.CurrentUser?.Nome ?? "Usuário Desconhecido";

        #endregion

        public CadastroProdutosViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            
            // Initialize collections
            _listaProdutos = new ObservableCollection<Produto>(); // Initialize backing field directly
            NumerosPagina = new ObservableCollection<int>();

            // Initialize non-nullable fields to avoid CS8618
            _nome = string.Empty;
            _descricao = string.Empty;
            _codigoBarras = string.Empty;
            _lote = string.Empty;
            _categoriaSelecionada = "Selecione...";
            _unidadeSelecionada = "Selecione...";
            _statusSelecionado = "Ativo";
            _produtoSelecionadoGrid = null!; // Can be null initially
            _textoPesquisa = string.Empty;
            _infoPaginacao = string.Empty;
            _totalProdutosTexto = "Total: 0 produto(s)";

            // Initialize Commands
            NovoCommand = new RelayCommand(_ => NovoProduto());
            SalvarCommand = new RelayCommand(async _ => await SalvarProduto());
            EditarCommand = new RelayCommand(_ => EditarProduto(), _ => ProdutoSelecionadoGrid != null);
            ExcluirCommand = new RelayCommand(async _ => await ExcluirProduto(), _ => ProdutoSelecionadoGrid != null);
            CancelarCommand = new RelayCommand(_ => CancelarEdicao());
            VoltarCommand = new RelayCommand(Voltar);
            PesquisarCommand = new RelayCommand(_ => { _paginaAtual = 1; FiltrarProdutos(); });
            
            ProximaPaginaCommand = new RelayCommand(_ => PaginaAtual++, _ => PodeAvancar);
            PaginaAnteriorCommand = new RelayCommand(_ => PaginaAtual--, _ => PodeVoltar);
            PrimeiraPaginaCommand = new RelayCommand(_ => PaginaAtual = 1, _ => PodeVoltar);
            UltimaPaginaCommand = new RelayCommand(_ => PaginaAtual = _totalPaginas, _ => PodeAvancar);
            IrParaPaginaCommand = new RelayCommand(param => { if (param is int p) PaginaAtual = p; });

            CamposHabilitados = false;
            
            // Initial Load
            CarregarProdutos();
        }

        private void CarregarProdutos()
        {
            try
            {
                // In a real scenario with many products, we should paginate at DB level.
                // For now, replicating existing logic of loading all and filtering in memory
                // or using Repository methods if available.
                // The existing implementation used _context.Produtos.OrderBy...
                
                var produtos = _unitOfWork.Produtos.GetAll().OrderBy(p => p.Nome).ToList();
                _todosOsProdutos = produtos;
                FiltrarProdutos(); // This will also handle pagination
                
                // Set initial code for new product
                GerarProximoCodigo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar produtos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FiltrarProdutos()
        {
            if (string.IsNullOrWhiteSpace(TextoPesquisa))
            {
                _produtosFiltrados = _todosOsProdutos;
            }
            else
            {
                string filtro = TextoPesquisa.ToLower().Trim();
                _produtosFiltrados = _todosOsProdutos.Where(p =>
                    p.Codigo.ToString().Contains(filtro) ||
                    (p.Nome?.ToLower().Contains(filtro) ?? false) ||
                    (p.Categoria?.ToLower().Contains(filtro) ?? false) ||
                    (p.CodigoBarras?.ToLower().Contains(filtro) ?? false) ||
                    (p.Lote?.ToLower().Contains(filtro) ?? false) ||
                    (p.Status?.ToLower().Contains(filtro) ?? false)
                ).ToList();
            }
            
            AtualizarPaginacao();
        }

        private void AtualizarPaginacao()
        {
            _totalItens = _produtosFiltrados.Count;
            _totalPaginas = _totalItens > 0 ? (int)Math.Ceiling((double)_totalItens / ItensPorPagina) : 1;

            if (_paginaAtual > _totalPaginas) _paginaAtual = _totalPaginas;
            if (_paginaAtual < 1) _paginaAtual = 1;

            var itensPagina = _produtosFiltrados
                .Skip((_paginaAtual - 1) * ItensPorPagina)
                .Take(ItensPorPagina)
                .ToList();

            ListaProdutos = new ObservableCollection<Produto>(itensPagina);

            // Update info text
            int inicio = _totalItens > 0 ? ((_paginaAtual - 1) * ItensPorPagina) + 1 : 0;
            int fim = Math.Min(_paginaAtual * ItensPorPagina, _totalItens);
            InfoPaginacao = _totalItens > 0
                ? $"Exibindo {inicio}-{fim} de {_totalItens} produtos | Página {_paginaAtual} de {_totalPaginas}"
                : "Nenhum produto encontrado";
            
            TotalProdutosTexto = $"Total: {_totalItens} produto(s)";

            // Update buttons state
            PodeVoltar = _paginaAtual > 1;
            PodeAvancar = _paginaAtual < _totalPaginas;
            
            // Notify commands
            ((RelayCommand)ProximaPaginaCommand).RaiseCanExecuteChanged();
            ((RelayCommand)PaginaAnteriorCommand).RaiseCanExecuteChanged();
            ((RelayCommand)PrimeiraPaginaCommand).RaiseCanExecuteChanged();
            ((RelayCommand)UltimaPaginaCommand).RaiseCanExecuteChanged();

            GerarNumerosPagina();
        }

        private void GerarNumerosPagina()
        {
            NumerosPagina.Clear();
            if (_totalPaginas <= 1) return;

            int inicioPagina = Math.Max(1, _paginaAtual - 3);
            int fimPagina = Math.Min(_totalPaginas, _paginaAtual + 3);

            if (_paginaAtual <= 4)
            {
                inicioPagina = 1;
                fimPagina = Math.Min(7, _totalPaginas);
            }
            else if (_paginaAtual >= _totalPaginas - 3)
            {
                inicioPagina = Math.Max(1, _totalPaginas - 6);
                fimPagina = _totalPaginas;
            }

            for (int i = inicioPagina; i <= fimPagina; i++)
            {
                NumerosPagina.Add(i);
            }
        }

        private void NovoProduto()
        {
            LimparCampos();
            CamposHabilitados = true;
            IsEditing = false;
            GerarProximoCodigo();
            StatusSelecionado = "Ativo"; // Default
            CategoriaSelecionada = "Selecione...";
            UnidadeSelecionada = "Selecione...";
        }

        private void EditarProduto()
        {
            if (ProdutoSelecionadoGrid == null) return;

            var p = ProdutoSelecionadoGrid;
            Codigo = p.Codigo;
            Nome = p.Nome;
            Descricao = p.Descricao;
            CodigoBarras = p.CodigoBarras;
            Lote = p.Lote;
            DataFabricacao = p.DataFabricacao;
            DataVencimento = p.DataVencimento;
            PrecoCusto = p.PrecoCusto.ToString("F2");
            PrecoVenda = p.PrecoVenda.ToString("F2");
            Estoque = p.Estoque;
            EstoqueMinimo = p.EstoqueMinimo;
            CategoriaSelecionada = p.Categoria;
            UnidadeSelecionada = p.Unidade;
            StatusSelecionado = p.Status;

            CamposHabilitados = true;
            IsEditing = true;
        }

        private void CancelarEdicao()
        {
            LimparCampos();
            CamposHabilitados = false;
            IsEditing = false;
            ProdutoSelecionadoGrid = null;
            GerarProximoCodigo();
        }

        private async Task SalvarProduto()
        {
            if (!ValidarCampos()) return;

            try
            {
                if (!decimal.TryParse(PrecoCusto, NumberStyles.Currency, CultureInfo.CurrentCulture, out decimal precoCustoDec))
                    precoCustoDec = 0;
                
                if (!decimal.TryParse(PrecoVenda, NumberStyles.Currency, CultureInfo.CurrentCulture, out decimal precoVendaDec))
                    precoVendaDec = 0;

                if (IsEditing)
                {
                    // Update existing
                    // Need to fetch original or use the selected one from grid but need to be careful with tracking
                    // Since we are using UnitOfWork, we should probably fetch it again to attach or use Update
                    var produto = await _unitOfWork.Produtos.GetByIdAsync(ProdutoSelecionadoGrid.Id);
                    if (produto != null)
                    {
                        produto.Nome = Nome;
                        produto.Descricao = Descricao;
                        produto.Categoria = CategoriaSelecionada;
                        produto.Unidade = UnidadeSelecionada;
                        produto.CodigoBarras = CodigoBarras;
                        produto.Lote = Lote;
                        produto.DataFabricacao = DataFabricacao;
                        produto.DataVencimento = DataVencimento;
                        produto.PrecoCusto = precoCustoDec;
                        produto.PrecoVenda = precoVendaDec;
                        produto.Estoque = Estoque;
                        produto.EstoqueMinimo = EstoqueMinimo;
                        produto.Status = StatusSelecionado;

                        _unitOfWork.Produtos.Update(produto);
                        await _unitOfWork.SaveChangesAsync();
                        MessageBox.Show("Produto atualizado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    // Create new
                    var produto = new Produto
                    {
                        Codigo = Codigo,
                        Nome = Nome,
                        Descricao = Descricao,
                        Categoria = CategoriaSelecionada,
                        Unidade = UnidadeSelecionada,
                        CodigoBarras = CodigoBarras,
                        Lote = Lote,
                        DataFabricacao = DataFabricacao,
                        DataVencimento = DataVencimento,
                        PrecoCusto = precoCustoDec,
                        PrecoVenda = precoVendaDec,
                        Estoque = Estoque,
                        EstoqueMinimo = EstoqueMinimo,
                        Status = StatusSelecionado
                    };

                    await _unitOfWork.Produtos.AddAsync(produto);
                    await _unitOfWork.SaveChangesAsync();
                    MessageBox.Show("Produto salvo com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                CancelarEdicao();
                CarregarProdutos(); // Refresh list
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar produto: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExcluirProduto()
        {
            if (ProdutoSelecionadoGrid == null) return;

            var result = MessageBox.Show(
                    $"Deseja realmente excluir o produto '{ProdutoSelecionadoGrid.Nome}'?",
                    "Confirmar Exclusão",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var produto = await _unitOfWork.Produtos.GetByIdAsync(ProdutoSelecionadoGrid.Id);
                    if (produto != null)
                    {
                        _unitOfWork.Produtos.Remove(produto);
                        await _unitOfWork.SaveChangesAsync();
                        MessageBox.Show("Produto excluído com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                        CarregarProdutos();
                        CancelarEdicao();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao excluir produto: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(Nome))
            {
                MessageBox.Show("Nome do produto é obrigatório!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (CategoriaSelecionada == "Selecione..." || string.IsNullOrEmpty(CategoriaSelecionada))
            {
                MessageBox.Show("Selecione uma categoria!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (UnidadeSelecionada == "Selecione..." || string.IsNullOrEmpty(UnidadeSelecionada))
            {
                MessageBox.Show("Selecione uma unidade!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            
            return true;
        }

        private void LimparCampos()
        {
            Nome = string.Empty;
            Descricao = string.Empty;
            CodigoBarras = string.Empty;
            Lote = string.Empty;
            DataFabricacao = null;
            DataVencimento = null;
            PrecoCusto = "0,00";
            PrecoVenda = "0,00";
            Estoque = 0;
            EstoqueMinimo = 0;
            CategoriaSelecionada = "Selecione...";
            UnidadeSelecionada = "Selecione...";
            StatusSelecionado = "Ativo";
        }

        private void GerarProximoCodigo()
        {
            // Simple logic based on current list
            if (_todosOsProdutos != null && _todosOsProdutos.Any())
            {
                Codigo = _todosOsProdutos.Max(p => p.Codigo) + 1;
            }
            else
            {
                Codigo = 1;
            }
        }

        private void Voltar(object obj)
        {
            Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
        }
    }
}
