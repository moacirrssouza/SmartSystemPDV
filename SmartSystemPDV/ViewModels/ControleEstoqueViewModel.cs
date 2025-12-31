using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SmartSystemPDV.Commands;
using SmartSystemPDV.Data.UnitOfWork;
using SmartSystemPDV.Models;
using SmartSystemPDV.Services;

namespace SmartSystemPDV.ViewModels;

public class ControleEstoqueViewModel : ViewModelBase
{
    private readonly IUnitOfWork _unitOfWork;
    private List<Produto> _todosProdutos;

    private const int ITENS_POR_PAGINA = 10;
    private List<MovimentacaoEstoque> _todasMovimentacoes;
    private List<Produto> _produtosFiltrados;

    public ControleEstoqueViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        
        // Inicializar campos não nulos
        _todosProdutos = new List<Produto>();
        _todasMovimentacoes = new List<MovimentacaoEstoque>();
        _produtosFiltrados = new List<Produto>();
        _dataHoraAtual = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        _movimentacoes = new ObservableCollection<MovimentacaoEstoque>();
        _produtos = new ObservableCollection<Produto>();
        _produtoSelecionado = null!; 
        _filtroProdutoSelecionado = null!;
        _tipoMovimentacao = "Entrada";
        _filtroTexto = string.Empty;
        _filtroTipoMovimentacao = "Todos";
        
        CarregarProdutosCommand = new RelayCommand(async _ => await CarregarProdutos());
        SalvarMovimentacaoCommand = new RelayCommand(async _ => await SalvarMovimentacao(), _ => PodeSalvar());
        BuscarMovimentacoesCommand = new RelayCommand(async _ => await BuscarMovimentacoes());
        LimparCamposCommand = new RelayCommand(_ => LimparCampos());

        // Comandos de Paginação
        ProximaPaginaEstoqueCommand = new RelayCommand(_ => ProximaPaginaEstoque(), _ => PodeAvancarEstoque());
        AnteriorPaginaEstoqueCommand = new RelayCommand(_ => AnteriorPaginaEstoque(), _ => PodeVoltarEstoque());
        ProximaPaginaHistoricoCommand = new RelayCommand(_ => ProximaPaginaHistorico(), _ => PodeAvancarHistorico());
        AnteriorPaginaHistoricoCommand = new RelayCommand(_ => AnteriorPaginaHistorico(), _ => PodeVoltarHistorico());
        
        DataInicio = DateTime.Today.AddDays(-30);
        DataFim = DateTime.Today.AddDays(1).AddSeconds(-1); // Fim do dia atual
        TipoMovimentacao = "Entrada";
        
        // Inicializar listas vazias para evitar null reference (já inicializadas acima)
        Movimentacoes = _movimentacoes;
        Produtos = _produtos;

        // Timer para relógio
        _timer = new System.Windows.Threading.DispatcherTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += (s, e) => DataHoraAtual = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        _timer.Start();
        DataHoraAtual = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
    }

    // Propriedades
    private System.Windows.Threading.DispatcherTimer _timer;

    private string _dataHoraAtual;
    public string DataHoraAtual
    {
        get => _dataHoraAtual;
        set => SetProperty(ref _dataHoraAtual, value);
    }

    private int _novoEstoque;
    public int NovoEstoque
    {
        get => _novoEstoque;
        set => SetProperty(ref _novoEstoque, value);
    }

    private ObservableCollection<MovimentacaoEstoque> _movimentacoes;
    public ObservableCollection<MovimentacaoEstoque> Movimentacoes
    {
        get => _movimentacoes;
        set => SetProperty(ref _movimentacoes, value);
    }

    private ObservableCollection<Produto> _produtos;
    public ObservableCollection<Produto> Produtos
    {
        get => _produtos;
        set => SetProperty(ref _produtos, value);
    }

    private Produto _produtoSelecionado;
    public Produto ProdutoSelecionado
    {
        get => _produtoSelecionado;
        set 
        {
            if (SetProperty(ref _produtoSelecionado, value))
            {
                CalcularNovoEstoque();
                ((RelayCommand)SalvarMovimentacaoCommand).RaiseCanExecuteChanged();
            }
        }
    }
    
    private Produto _filtroProdutoSelecionado;
    public Produto FiltroProdutoSelecionado
    {
        get => _filtroProdutoSelecionado;
        set => SetProperty(ref _filtroProdutoSelecionado, value);
    }

    private string _tipoMovimentacao;
    public string TipoMovimentacao
    {
        get => _tipoMovimentacao;
        set => SetProperty(ref _tipoMovimentacao, value);
    }

    private string _filtroTexto;
    public string FiltroTexto
    {
        get => _filtroTexto;
        set
        {
            if (SetProperty(ref _filtroTexto, value))
            {
                FiltrarProdutos();
            }
        }
    }

    private string _filtroTipoMovimentacao;
    public string FiltroTipoMovimentacao
    {
        get => _filtroTipoMovimentacao;
        set 
        {
            if (SetProperty(ref _filtroTipoMovimentacao, value))
            {
                // Recarregar movimentações quando mudar o filtro
                _ = BuscarMovimentacoes();
            }
        }
    }


    private int _quantidade;
    public int Quantidade
    {
        get => _quantidade;
        set 
        {
            if (SetProperty(ref _quantidade, value))
            {
                ((RelayCommand)SalvarMovimentacaoCommand).RaiseCanExecuteChanged();
            }
        }
    }

    private string _motivo = string.Empty;
    public string Motivo
    {
        get => _motivo;
        set => SetProperty(ref _motivo, value);
    }

    private DateTime _dataInicio;
    public DateTime DataInicio
    {
        get => _dataInicio;
        set => SetProperty(ref _dataInicio, value);
    }

    private DateTime _dataFim;
    public DateTime DataFim
    {
        get => _dataFim;
        set => SetProperty(ref _dataFim, value);
    }

    private int _totalProdutos;
    public int TotalProdutos
    {
        get => _totalProdutos;
        set => SetProperty(ref _totalProdutos, value);
    }

    private decimal _valorTotalEstoque;
    public decimal ValorTotalEstoque
    {
        get => _valorTotalEstoque;
        set => SetProperty(ref _valorTotalEstoque, value);
    }

    private int _estoqueBaixoCount;
    public int EstoqueBaixoCount
    {
        get => _estoqueBaixoCount;
        set => SetProperty(ref _estoqueBaixoCount, value);
    }

    private int _produtosAtivosCount;
    public int ProdutosAtivosCount
    {
        get => _produtosAtivosCount;
        set => SetProperty(ref _produtosAtivosCount, value);
    }

    // Comandos
    public ICommand CarregarProdutosCommand { get; }
    public ICommand SalvarMovimentacaoCommand { get; }
    public ICommand BuscarMovimentacoesCommand { get; }
    public ICommand LimparCamposCommand { get; }

    // Comandos Paginação
    public ICommand ProximaPaginaEstoqueCommand { get; }
    public ICommand AnteriorPaginaEstoqueCommand { get; }
    public ICommand ProximaPaginaHistoricoCommand { get; }
    public ICommand AnteriorPaginaHistoricoCommand { get; }

    // Propriedades Paginação Estoque
    private int _paginaAtualEstoque = 1;
    public int PaginaAtualEstoque
    {
        get => _paginaAtualEstoque;
        set => SetProperty(ref _paginaAtualEstoque, value);
    }

    private int _totalPaginasEstoque = 1;
    public int TotalPaginasEstoque
    {
        get => _totalPaginasEstoque;
        set => SetProperty(ref _totalPaginasEstoque, value);
    }

    // Propriedades Paginação Histórico
    private int _paginaAtualHistorico = 1;
    public int PaginaAtualHistorico
    {
        get => _paginaAtualHistorico;
        set => SetProperty(ref _paginaAtualHistorico, value);
    }

    private int _totalPaginasHistorico = 1;
    public int TotalPaginasHistorico
    {
        get => _totalPaginasHistorico;
        set => SetProperty(ref _totalPaginasHistorico, value);
    }

    // Métodos Públicos para serem chamados pela View se necessário (ex: no Loaded)
    public async Task InitializeAsync()
    {
        await CarregarProdutos();
        await BuscarMovimentacoes();
    }

    private void CalcularNovoEstoque()
    {
        if (ProdutoSelecionado == null)
        {
            NovoEstoque = 0;
            return;
        }

        int estoqueAtual = ProdutoSelecionado.Estoque;
        
        if (TipoMovimentacao == "Entrada")
        {
            NovoEstoque = estoqueAtual + Quantidade;
        }
        else // Saída
        {
            NovoEstoque = estoqueAtual - Quantidade;
        }
    }

    private void FiltrarProdutos()
    {
        if (_todosProdutos == null) return;
        
        if (string.IsNullOrWhiteSpace(FiltroTexto))
        {
            _produtosFiltrados = new List<Produto>(_todosProdutos);
        }
        else
        {
            var filtrados = _todosProdutos.Where(p => 
                (p.Nome != null && p.Nome.Contains(FiltroTexto, StringComparison.OrdinalIgnoreCase)) ||
                (p.Codigo.ToString().Contains(FiltroTexto, StringComparison.OrdinalIgnoreCase)));
            _produtosFiltrados = filtrados.ToList();
        }

        // Resetar paginação
        PaginaAtualEstoque = 1;
        AtualizarPaginaEstoque();
    }

    private void AtualizarPaginaEstoque()
    {
        if (_produtosFiltrados == null || !_produtosFiltrados.Any())
        {
            Produtos = new ObservableCollection<Produto>();
            TotalPaginasEstoque = 1;
            return;
        }

        TotalPaginasEstoque = (int)Math.Ceiling((double)_produtosFiltrados.Count / ITENS_POR_PAGINA);
        
        var itens = _produtosFiltrados
            .Skip((PaginaAtualEstoque - 1) * ITENS_POR_PAGINA)
            .Take(ITENS_POR_PAGINA);
            
        Produtos = new ObservableCollection<Produto>(itens);
        
        ((RelayCommand)ProximaPaginaEstoqueCommand).RaiseCanExecuteChanged();
        ((RelayCommand)AnteriorPaginaEstoqueCommand).RaiseCanExecuteChanged();
    }

    private bool PodeAvancarEstoque() => PaginaAtualEstoque < TotalPaginasEstoque;
    private bool PodeVoltarEstoque() => PaginaAtualEstoque > 1;

    private void ProximaPaginaEstoque()
    {
        if (PodeAvancarEstoque())
        {
            PaginaAtualEstoque++;
            AtualizarPaginaEstoque();
        }
    }

    private void AnteriorPaginaEstoque()
    {
        if (PodeVoltarEstoque())
        {
            PaginaAtualEstoque--;
            AtualizarPaginaEstoque();
        }
    }

    private async Task CarregarProdutos()
    {
        try
        {
            var idSelecionado = ProdutoSelecionado?.Id;

            var produtos = await _unitOfWork.Produtos.GetAllAsync();
            _todosProdutos = produtos.ToList();
            
            FiltrarProdutos(); 

            if (idSelecionado.HasValue)
            {
                ProdutoSelecionado = Produtos.FirstOrDefault(p => p.Id == idSelecionado.Value);
            }

            // Atualizar Resumos
            TotalProdutos = _todosProdutos.Count;
            ValorTotalEstoque = _todosProdutos.Sum(p => p.Estoque * p.PrecoVenda);
            EstoqueBaixoCount = _todosProdutos.Count(p => p.Estoque <= p.EstoqueMinimo);
            ProdutosAtivosCount = _todosProdutos.Count(p => p.Ativo);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao carregar produtos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task BuscarMovimentacoes()
    {
        try
        {
            IEnumerable<MovimentacaoEstoque> result;
            
            // Ajustar DataFim para o final do dia se o usuário selecionou apenas a data (00:00:00)
            DateTime fim = DataFim.Hour == 0 && DataFim.Minute == 0 && DataFim.Second == 0 
                ? DataFim.AddDays(1).AddSeconds(-1) 
                : DataFim;

            result = await _unitOfWork.MovimentacoesEstoque.GetByPeriodoAsync(DataInicio, fim);

            if (FiltroProdutoSelecionado != null)
            {
                 result = result.Where(m => m.ProdutoId == FiltroProdutoSelecionado.Id);
            }

            if (!string.IsNullOrEmpty(FiltroTipoMovimentacao) && FiltroTipoMovimentacao != "Todos os Tipos")
            {
                 result = result.Where(m => m.Tipo.Equals(FiltroTipoMovimentacao, StringComparison.OrdinalIgnoreCase));
            }
            
            // Mapear nome do produto se não vier preenchido (embora o Include deva resolver)
            foreach (var item in result)
            {
                if (item.Produto != null)
                {
                    item.NomeProduto = item.Produto.Nome;
                }
            }
            
            _todasMovimentacoes = result.ToList();
            
            // Resetar paginação
            PaginaAtualHistorico = 1;
            AtualizarPaginaHistorico();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao buscar movimentações: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void AtualizarPaginaHistorico()
    {
        if (_todasMovimentacoes == null || !_todasMovimentacoes.Any())
        {
            Movimentacoes = new ObservableCollection<MovimentacaoEstoque>();
            TotalPaginasHistorico = 1;
            return;
        }

        TotalPaginasHistorico = (int)Math.Ceiling((double)_todasMovimentacoes.Count / ITENS_POR_PAGINA);
        
        var itens = _todasMovimentacoes
            .Skip((PaginaAtualHistorico - 1) * ITENS_POR_PAGINA)
            .Take(ITENS_POR_PAGINA);
            
        Movimentacoes = new ObservableCollection<MovimentacaoEstoque>(itens);
        
        ((RelayCommand)ProximaPaginaHistoricoCommand).RaiseCanExecuteChanged();
        ((RelayCommand)AnteriorPaginaHistoricoCommand).RaiseCanExecuteChanged();
    }

    private bool PodeAvancarHistorico() => PaginaAtualHistorico < TotalPaginasHistorico;
    private bool PodeVoltarHistorico() => PaginaAtualHistorico > 1;

    private void ProximaPaginaHistorico()
    {
        if (PodeAvancarHistorico())
        {
            PaginaAtualHistorico++;
            AtualizarPaginaHistorico();
        }
    }

    private void AnteriorPaginaHistorico()
    {
        if (PodeVoltarHistorico())
        {
            PaginaAtualHistorico--;
            AtualizarPaginaHistorico();
        }
    }

    private bool PodeSalvar()
    {
        return ProdutoSelecionado != null && Quantidade > 0;
    }

    private void LimparCampos()
    {
        ProdutoSelecionado = null;
        Quantidade = 0;
        Motivo = string.Empty;
        TipoMovimentacao = "Entrada";
    }

    private async Task SalvarMovimentacao()
    {
        try 
        {
            var produto = await _unitOfWork.Produtos.GetByIdAsync(ProdutoSelecionado.Id);
            if (produto == null) return;

            int qtdAnterior = produto.Estoque;
            int qtdFinal = qtdAnterior;

            if (TipoMovimentacao == "Entrada")
            {
                qtdFinal += Quantidade;
            }
            else // Saída
            {
                if (qtdAnterior < Quantidade)
                {
                    MessageBox.Show("Estoque insuficiente para realizar esta saída!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                qtdFinal -= Quantidade;
            }

            // Atualiza Produto
            produto.Estoque = qtdFinal;
            // Atualizar EstoqueAtual também se for usado
            produto.EstoqueAtual = qtdFinal;
            
            _unitOfWork.Produtos.Update(produto);

            // Cria Movimentação
            var movimentacao = new MovimentacaoEstoque
            {
                ProdutoId = produto.Id,
                DataHora = DateTime.Now,
                Tipo = TipoMovimentacao,
                Quantidade = Quantidade,
                QuantidadeAnterior = qtdAnterior,
                QuantidadeFinal = qtdFinal,
                Motivo = Motivo ?? string.Empty,
                UsuarioId = SessionManager.CurrentUser?.Id != 0 ? SessionManager.CurrentUser?.Id : null
            };
            
            await _unitOfWork.MovimentacoesEstoque.AddAsync(movimentacao);
            await _unitOfWork.SaveChangesAsync();

            MessageBox.Show("Movimentação registrada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            
            // Limpar campos
            LimparCampos();
            
            // Atualizar listas
            await BuscarMovimentacoes();
            await CarregarProdutos(); // Atualiza estoque na lista de produtos
        }
        catch (Exception ex)
        {
            var innerMessage = ex.InnerException?.Message ?? "";
            MessageBox.Show($"Erro ao salvar movimentação: {ex.Message}\nDetalhes: {innerMessage}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
