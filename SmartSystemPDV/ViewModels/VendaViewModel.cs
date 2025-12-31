using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using SmartSystemPDV.Services;
using SmartSystemPDV.Commands;
using SmartSystemPDV.Data.UnitOfWork;
using SmartSystemPDV.Models;
using SmartSystemPDV.ViewModel;
using SmartSystemPDV.View.Caixa;
using RelayCommand = SmartSystemPDV.Commands.RelayCommand;

namespace SmartSystemPDV.ViewModels
{
    public class VendaViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private DispatcherTimer _timer;

        public VendaViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            
            // Inicializar Collections
            ItensCarrinho = new ObservableCollection<ItemVendaViewModel>();
            Produtos = new ObservableCollection<Produto>();
            Clientes = new ObservableCollection<Cliente>();
            FormasPagamento = new ObservableCollection<FormaPagamento>();
            Parcelas = new ObservableCollection<ParcelaInfo>();

            // Inicializar Comandos
            AdicionarProdutoCommand = new RelayCommand(async _ => await AdicionarProduto(), _ => PodeAdicionarProduto());
            BuscarProdutoCommand = new RelayCommand(async _ => await BuscarProdutoPorCodigo());
            BuscarProdutoPorNomeCommand = new RelayCommand(async _ => await BuscarProdutoPorNome());
            RemoverItemCommand = new RelayCommand(RemoverItem, _ => ItemSelecionado != null);
            EditarItemCommand = new RelayCommand(EditarItem, _ => ItemSelecionado != null);
            AplicarDescontoCommand = new RelayCommand(AplicarDesconto, _ => ItemSelecionado != null);
            FinalizarVendaCommand = new RelayCommand(async _ => await FinalizarVenda(), _ => PodeFinalizarVenda());
            CancelarVendaCommand = new RelayCommand(CancelarVenda, _ => ItensCarrinho.Any());
            VoltarCommand = new RelayCommand(Voltar);
            AdicionarNumeroCommand = new RelayCommand(AdicionarNumero);
            BackspaceCommand = new RelayCommand(Backspace);
            DefinirFocoCommand = new RelayCommand(DefinirFoco);
            AbrirBuscaAvancadaCommand = new RelayCommand(_ => AbrirBuscaAvancada());
            
            // Inicializar campos não anuláveis
            _codigoProdutoBusca = string.Empty;
            _valorPago = string.Empty;
            _valorParcela = string.Empty;

            // Timer
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += (s, e) => DataHoraAtual = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            _timer.Start();
            DataHoraAtual = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            // Inicializar Venda
            IniciarNovaVenda();

            OperadorCaixa = SessionManager.CurrentUser?.Nome ?? "Operador";
        }

        // Propriedades
        
        private string _dataHoraAtual;
        public string DataHoraAtual
        {
            get => _dataHoraAtual;
            set => SetProperty(ref _dataHoraAtual, value);
        }

        private Venda _vendaAtual;
        public Venda VendaAtual
        {
            get => _vendaAtual;
            set => SetProperty(ref _vendaAtual, value);
        }

        private string _operadorCaixa;
        public string OperadorCaixa
        {
            get => _operadorCaixa;
            set => SetProperty(ref _operadorCaixa, value);
        }

        // Listas
        public ObservableCollection<ItemVendaViewModel> ItensCarrinho { get; }
        
        private ObservableCollection<Produto> _produtos;
        public ObservableCollection<Produto> Produtos
        {
            get => _produtos;
            set => SetProperty(ref _produtos, value);
        }

        private ObservableCollection<Cliente> _clientes;
        public ObservableCollection<Cliente> Clientes
        {
            get => _clientes;
            set => SetProperty(ref _clientes, value);
        }

        private ObservableCollection<FormaPagamento> _formasPagamento;
        public ObservableCollection<FormaPagamento> FormasPagamento
        {
            get => _formasPagamento;
            set => SetProperty(ref _formasPagamento, value);
        }

        private ObservableCollection<ParcelaInfo> _parcelas;
        public ObservableCollection<ParcelaInfo> Parcelas
        {
            get => _parcelas;
            set => SetProperty(ref _parcelas, value);
        }

        // Seleções e Campos
        private Produto _produtoSelecionado;
        public Produto ProdutoSelecionado
        {
            get => _produtoSelecionado;
            set 
            {
                if (SetProperty(ref _produtoSelecionado, value))
                {
                    if (value != null)
                    {
                        CodigoProdutoBusca = value.Nome; // Mostrar nome no campo de busca se desejado, ou manter separado
                    }
                }
            }
        }

        private Cliente _clienteSelecionado;
        public Cliente ClienteSelecionado
        {
            get => _clienteSelecionado;
            set
            {
                if (SetProperty(ref _clienteSelecionado, value))
                {
                    if (VendaAtual != null && value != null)
                        VendaAtual.ClienteId = value.Id;
                }
            }
        }

        private FormaPagamento _formaPagamentoSelecionada;
        public FormaPagamento FormaPagamentoSelecionada
        {
            get => _formaPagamentoSelecionada;
            set
            {
                if (SetProperty(ref _formaPagamentoSelecionada, value))
                {
                    AtualizarVisibilidadePagamento();
                    if (value != null && VendaAtual != null)
                    {
                        VendaAtual.FormaPagamento = value.Nome;
                        if (value.PermiteParcelas) CalcularParcelas();
                    }
                }
            }
        }

        private ParcelaInfo _parcelaSelecionada;
        public ParcelaInfo ParcelaSelecionada
        {
            get => _parcelaSelecionada;
            set
            {
                if (SetProperty(ref _parcelaSelecionada, value))
                {
                    CalcularParcelas();
                }
            }
        }

        private ItemVendaViewModel _itemSelecionado;
        public ItemVendaViewModel ItemSelecionado
        {
            get => _itemSelecionado;
            set
            {
                SetProperty(ref _itemSelecionado, value);
                ((RelayCommand)RemoverItemCommand).RaiseCanExecuteChanged();
                ((RelayCommand)EditarItemCommand).RaiseCanExecuteChanged();
                ((RelayCommand)AplicarDescontoCommand).RaiseCanExecuteChanged();
            }
        }

        private string _codigoProdutoBusca;
        public string CodigoProdutoBusca
        {
            get => _codigoProdutoBusca;
            set => SetProperty(ref _codigoProdutoBusca, value);
        }

        private string _quantidadeProduto = "1";
        public string QuantidadeProduto
        {
            get => _quantidadeProduto;
            set => SetProperty(ref _quantidadeProduto, value);
        }

        private string _valorPago;
        public string ValorPago
        {
            get => _valorPago;
            set
            {
                if (SetProperty(ref _valorPago, value))
                {
                    CalcularTroco();
                }
            }
        }

        private string _troco = "R$ 0,00";
        public string Troco
        {
            get => _troco;
            set => SetProperty(ref _troco, value);
        }

        private string _valorParcela;
        public string ValorParcela
        {
            get => _valorParcela;
            set => SetProperty(ref _valorParcela, value);
        }

        private string _totalItens = "0";
        public string TotalItens
        {
            get => _totalItens;
            set => SetProperty(ref _totalItens, value);
        }

        private string _descontoTotal = "R$ 0,00";
        public string DescontoTotal
        {
            get => _descontoTotal;
            set => SetProperty(ref _descontoTotal, value);
        }

        private string _totalVenda = "R$ 0,00";
        public string TotalVenda
        {
            get => _totalVenda;
            set => SetProperty(ref _totalVenda, value);
        }

        private decimal _subtotal;
        public decimal Subtotal
        {
            get => _subtotal;
            set => SetProperty(ref _subtotal, value);
        }

        private decimal _pesoTotal;
        public decimal PesoTotal
        {
            get => _pesoTotal;
            set => SetProperty(ref _pesoTotal, value);
        }

        // Visibilidade
        private Visibility _pnlDinheiroVisibility = Visibility.Collapsed;
        public Visibility PnlDinheiroVisibility
        {
            get => _pnlDinheiroVisibility;
            set => SetProperty(ref _pnlDinheiroVisibility, value);
        }

        private Visibility _pnlParcelasVisibility = Visibility.Collapsed;
        public Visibility PnlParcelasVisibility
        {
            get => _pnlParcelasVisibility;
            set => SetProperty(ref _pnlParcelasVisibility, value);
        }

        // Comandos
        public ICommand AdicionarProdutoCommand { get; }
        public ICommand BuscarProdutoCommand { get; }
        public ICommand BuscarProdutoPorNomeCommand { get; }
        public ICommand AbrirBuscaAvancadaCommand { get; }
        public ICommand RemoverItemCommand { get; }
        public ICommand EditarItemCommand { get; }
        public ICommand AplicarDescontoCommand { get; }
        public ICommand FinalizarVendaCommand { get; }
        public ICommand CancelarVendaCommand { get; }
        public ICommand VoltarCommand { get; }
        public ICommand AdicionarNumeroCommand { get; }
        public ICommand BackspaceCommand { get; }
        public ICommand DefinirFocoCommand { get; }

        // Propriedade de Foco
        private string _campoFocoAtual = "CodigoProduto"; // Valor padrão
        public string CampoFocoAtual
        {
            get => _campoFocoAtual;
            set => SetProperty(ref _campoFocoAtual, value);
        }

        private void DefinirFoco(object param)
        {
            if (param is string nomeCampo)
            {
                CampoFocoAtual = nomeCampo;
            }
        }

        private void AdicionarNumero(object param)
        {
            if (param is string numero)
            {
                switch (CampoFocoAtual)
                {
                    case "CodigoProduto":
                        CodigoProdutoBusca = (CodigoProdutoBusca ?? "") + numero;
                        break;
                    case "Quantidade":
                        QuantidadeProduto = (QuantidadeProduto ?? "") + numero;
                        break;
                    case "ValorPago":
                        ValorPago = (ValorPago ?? "") + numero;
                        break;
                }
            }
        }

        private void Backspace(object param)
        {
            switch (CampoFocoAtual)
            {
                case "CodigoProduto":
                    if (!string.IsNullOrEmpty(CodigoProdutoBusca))
                        CodigoProdutoBusca = CodigoProdutoBusca.Substring(0, CodigoProdutoBusca.Length - 1);
                    break;
                case "Quantidade":
                    if (!string.IsNullOrEmpty(QuantidadeProduto))
                        QuantidadeProduto = QuantidadeProduto.Substring(0, QuantidadeProduto.Length - 1);
                    break;
                case "ValorPago":
                    if (!string.IsNullOrEmpty(ValorPago))
                        ValorPago = ValorPago.Substring(0, ValorPago.Length - 1);
                    break;
            }
        }

        // Métodos de Inicialização
        public async Task InitializeAsync()
        {
            try
            {
                var produtos = await _unitOfWork.Produtos.GetProdutosAtivosAsync();
                Produtos = new ObservableCollection<Produto>(produtos);

                var formas = await _unitOfWork.FormasPagamento.GetFormasAtivasAsync();
                FormasPagamento = new ObservableCollection<FormaPagamento>(formas);

                // Mock de Parcelas
                var listaParcelas = Enumerable.Range(1, 12).Select(i => new ParcelaInfo
                {
                    Numero = i,
                    Descricao = i <= 3 ? $"{i}x sem juros" : $"{i}x com juros"
                });
                Parcelas = new ObservableCollection<ParcelaInfo>(listaParcelas);

                // TODO: Carregar Clientes se necessário
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar dados: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void IniciarNovaVenda()
        {
            VendaAtual = new Venda
            {
                DataVenda = DateTime.Now,
                Status = "ABERTA",
                UsuarioId = "Admin", // TODO: Pegar do sistema de login
                ValorTotal = 0,
                ValorDesconto = 0,
                ValorFinal = 0,
                Itens = new List<ItemVenda>()
            };
            ItensCarrinho.Clear();
            AtualizarTotais();
            LimparCamposProduto();
            LimparPagamento();
        }

        // Lógica de Negócio
        private async Task BuscarProdutoPorCodigo()
        {
            if (string.IsNullOrWhiteSpace(CodigoProdutoBusca)) return;

            try
            {
                var produto = await _unitOfWork.Produtos.GetByCodigoAsync(CodigoProdutoBusca);
                if (produto == null)
                {
                    var produtos = await _unitOfWork.Produtos.GetProdutosAtivosAsync();
                    produto = produtos.FirstOrDefault(p => p.CodigoBarras != null && 
                        p.CodigoBarras.Equals(CodigoProdutoBusca, StringComparison.OrdinalIgnoreCase));
                }

                if (produto != null)
                {
                    ProdutoSelecionado = produto;
                    // Opcional: Já adicionar direto ou apenas selecionar?
                    // O comportamento original apenas selecionava se fosse busca, mas aqui podemos focar.
                    // Vamos manter a seleção no combo (que bindamos ProdutoSelecionado)
                }
                else
                {
                    MessageBox.Show("Produto não encontrado!", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao buscar produto: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool PodeAdicionarProduto()
        {
            return ProdutoSelecionado != null || !string.IsNullOrWhiteSpace(CodigoProdutoBusca);
        }

        private async Task AdicionarProduto()
        {
            Produto produto = ProdutoSelecionado;

            if (produto == null && !string.IsNullOrWhiteSpace(CodigoProdutoBusca))
            {
                await BuscarProdutoPorCodigo();
                produto = ProdutoSelecionado;
            }

            if (produto == null)
            {
                MessageBox.Show("Selecione um produto!", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(QuantidadeProduto, NumberStyles.Any, CultureInfo.GetCultureInfo("pt-BR"), out decimal qtd) || qtd <= 0)
            {
                MessageBox.Show("Quantidade inválida!", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (qtd > produto.EstoqueAtual)
            {
                MessageBox.Show($"Estoque insuficiente! Disponível: {produto.EstoqueAtual}", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var itemExistente = ItensCarrinho.FirstOrDefault(i => i.ProdutoId == produto.Id);
            if (itemExistente != null)
            {
                itemExistente.Quantidade += qtd;
                itemExistente.CalcularSubtotal();
            }
            else
            {
                var novoItem = new ItemVendaViewModel
                {
                    ProdutoId = produto.Id,
                    ProdutoCodigo = produto.Codigo.ToString(),
                    ProdutoNome = produto.Nome,
                    Quantidade = qtd,
                    PrecoUnitario = produto.PrecoVenda,
                    Desconto = 0
                };
                novoItem.CalcularSubtotal();
                ItensCarrinho.Add(novoItem);
            }

            AtualizarTotais();
            LimparCamposProduto();
        }

        private async Task BuscarProdutoPorNome()
        {
            if (string.IsNullOrWhiteSpace(CodigoProdutoBusca)) return;
            try
            {
                var resultados = await _unitOfWork.Produtos.SearchAsync(CodigoProdutoBusca);
                var produto = resultados.FirstOrDefault();
                if (produto != null)
                {
                    ProdutoSelecionado = produto;
                }
                else
                {
                    MessageBox.Show("Nenhum produto encontrado pela busca.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro na busca por nome: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LimparCamposProduto()
        {
            ProdutoSelecionado = null;
            CodigoProdutoBusca = string.Empty;
            QuantidadeProduto = "1";
        }

        private void RemoverItem(object param)
        {
            if (ItemSelecionado != null)
            {
                var result = MessageBox.Show($"Remover '{ItemSelecionado.ProdutoNome}' do carrinho?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    ItensCarrinho.Remove(ItemSelecionado);
                    AtualizarTotais();
                }
            }
        }

        private void EditarItem(object param)
        {
             if (ItemSelecionado != null)
             {
                 MessageBox.Show($"Editar item: {ItemSelecionado.ProdutoNome} (Funcionalidade pendente)", "Editar", MessageBoxButton.OK, MessageBoxImage.Information);
             }
        }

        private void AbrirBuscaAvancada()
        {
            try
            {
                var win = new SmartSystemPDV.View.Caixa.BuscaProdutoWindow(this);
                var owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this);
                if (owner != null) win.Owner = owner;
                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir busca avançada: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AplicarDesconto(object param)
        {
            if (ItemSelecionado != null)
            {
                var descontoAplicado = Math.Round(ItemSelecionado.Subtotal * 0.1m, 2);
                ItemSelecionado.Desconto += descontoAplicado;
                ItemSelecionado.CalcularSubtotal();
                AtualizarTotais();
            }
        }

        private void AtualizarTotais()
        {
            decimal total = ItensCarrinho.Sum(i => i.Subtotal);
            decimal desconto = ItensCarrinho.Sum(i => i.Desconto);
            decimal qtd = ItensCarrinho.Sum(i => i.Quantidade);

            TotalVenda = $"R$ {total:F2}";
            DescontoTotal = $"R$ {desconto:F2}";
            TotalItens = qtd.ToString("N0");
            Subtotal = ItensCarrinho.Sum(i => i.PrecoUnitario * i.Quantidade);
            PesoTotal = ItensCarrinho.Sum(i => i.Quantidade);
            
            CalcularTroco();
            CalcularParcelas();
            
            ((RelayCommand)FinalizarVendaCommand).RaiseCanExecuteChanged();
            ((RelayCommand)CancelarVendaCommand).RaiseCanExecuteChanged();
        }

        private void AtualizarVisibilidadePagamento()
        {
            if (FormaPagamentoSelecionada != null)
            {
                PnlDinheiroVisibility = FormaPagamentoSelecionada.Tipo == "DINHEIRO" ? Visibility.Visible : Visibility.Collapsed;
                PnlParcelasVisibility = FormaPagamentoSelecionada.PermiteParcelas ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                PnlDinheiroVisibility = Visibility.Collapsed;
                PnlParcelasVisibility = Visibility.Collapsed;
            }
        }

        private void CalcularTroco()
        {
            if (decimal.TryParse(ValorPago, NumberStyles.Any, CultureInfo.GetCultureInfo("pt-BR"), out decimal valorPago))
            {
                decimal total = ObterTotalVendaDecimal();
                decimal troco = valorPago - total;
                Troco = troco > 0 ? $"R$ {troco:F2}" : "R$ 0,00";
                
                if (VendaAtual != null)
                {
                    VendaAtual.ValorPago = valorPago;
                    VendaAtual.Troco = troco > 0 ? troco : 0;
                }
            }
        }

        private void CalcularParcelas()
        {
            if (ParcelaSelecionada != null && FormaPagamentoSelecionada != null)
            {
                decimal total = ObterTotalVendaDecimal();
                decimal valorParcela = total / ParcelaSelecionada.Numero;

                if (ParcelaSelecionada.Numero > 3 && FormaPagamentoSelecionada.TaxaJuros > 0)
                {
                    decimal juros = FormaPagamentoSelecionada.TaxaJuros / 100;
                    valorParcela *= (1 + juros);
                }

                ValorParcela = $"R$ {valorParcela:F2}";
                if (VendaAtual != null) VendaAtual.NumeroParcelas = ParcelaSelecionada.Numero;
            }
        }

        private decimal ObterTotalVendaDecimal()
        {
            return ItensCarrinho.Sum(i => i.Subtotal);
        }

        private bool PodeFinalizarVenda()
        {
            return ItensCarrinho.Any();
        }

        private async Task FinalizarVenda()
        {
            if (!ItensCarrinho.Any())
            {
                MessageBox.Show("Carrinho vazio!", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var totalVenda = ObterTotalVendaDecimal();
            var finalizarVm = new FinalizarVendaViewModel(totalVenda);
            var finalizarWindow = new FinalizarVendaWindow
            {
                DataContext = finalizarVm,
                Owner = Application.Current.MainWindow
            };

            finalizarWindow.ShowDialog();

            if (finalizarVm.VendaConfirmada)
            {
                try
                {
                    VendaAtual.NumeroVenda = await _unitOfWork.Vendas.GerarNumeroVendaAsync();
                    VendaAtual.Status = "FINALIZADA";
                    VendaAtual.ValorTotal = ItensCarrinho.Sum(i => i.PrecoUnitario * i.Quantidade);
                    VendaAtual.ValorDesconto = ItensCarrinho.Sum(i => i.Desconto);
                    VendaAtual.ValorFinal = totalVenda;
                    
                    // Dados do pagamento
                    VendaAtual.ValorPago = finalizarVm.TotalPago;
                    VendaAtual.Troco = finalizarVm.TemTroco ? finalizarVm.ValorRestanteTroco : 0;
                    VendaAtual.Observacoes = finalizarVm.Observacoes;

                    // Formata string de pagamentos
                    var formasPagamento = finalizarVm.PagamentosAdicionados
                        .Select(p => $"{p.FormaPagamento}: {p.Valor:C2}")
                        .ToList();
                    VendaAtual.FormaPagamento = string.Join(" | ", formasPagamento);
                    if (string.IsNullOrEmpty(VendaAtual.FormaPagamento))
                        VendaAtual.FormaPagamento = "Não Informado";

                    int sequencia = 1;
                    foreach (var item in ItensCarrinho)
                    {
                        VendaAtual.Itens.Add(new ItemVenda
                        {
                            ProdutoId = item.ProdutoId,
                            ProdutoNome = item.ProdutoNome,
                            Quantidade = item.Quantidade,
                            PrecoUnitario = item.PrecoUnitario,
                            Desconto = item.Desconto,
                            Subtotal = item.Subtotal,
                            Sequencia = sequencia++
                        });

                        await _unitOfWork.Produtos.AtualizarEstoqueAsync(item.ProdutoId, -(int)item.Quantidade);
                    }

                    await _unitOfWork.Vendas.AddAsync(VendaAtual);
                    
                    MessageBox.Show(
                        $"Venda realizada com sucesso!\n\n" +
                        $"Número: {VendaAtual.NumeroVenda}\n" +
                        (VendaAtual.Troco > 0 ? $"Troco: {VendaAtual.Troco:C2}" : "Pagamento exato"),
                        "Sucesso", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                    
                    IniciarNovaVenda();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao finalizar venda: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CancelarVenda(object param)
        {
            var result = MessageBox.Show("Cancelar a venda atual?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                IniciarNovaVenda();
            }
        }
        
        private void LimparPagamento()
        {
            FormaPagamentoSelecionada = null;
            ParcelaSelecionada = null;
            ClienteSelecionado = null;
            ValorPago = "";
            Troco = "R$ 0,00";
            ValorParcela = "";
            AtualizarVisibilidadePagamento();
        }

        private void Voltar(object param)
        {
            if (ItensCarrinho.Count > 0)
            {
                var result = MessageBox.Show("Existe uma venda em andamento. Deseja realmente sair?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No) return;
            }
            
            // A View deve tratar o fechamento da janela
            // Aqui podemos enviar uma mensagem ou evento
            Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
        }
    }
}
