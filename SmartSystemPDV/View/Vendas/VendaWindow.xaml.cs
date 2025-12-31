using SmartSystemPDV.Data.Context;
using SmartSystemPDV.Data.UnitOfWork;
using SmartSystemPDV.Models;
using SmartSystemPDV.ViewModels;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace SmartSystemPDV.View.Vendas;

public partial class VendaWindow : Window
{
    // Unit of Work para acesso aos repositórios
    private readonly IUnitOfWork _unitOfWork;

    // Collections
    private ObservableCollection<ItemVendaViewModel> _itensCarrinho;

    // Timer
    private DispatcherTimer _timer;

    // Campo focado
    private TextBox _campoFocado;

    // Venda atual
    private Venda _vendaAtual;

    public VendaWindow()
    {
        InitializeComponent();

        // Inicializar Unit of Work
        var context = new AppDbContext();
        _unitOfWork = new UnitOfWork(context);

        InicializarComponentes();
        ConfigurarEventos();
        InicializarTimer();
    }

    private void InicializarComponentes()
    {
        _itensCarrinho = new ObservableCollection<ItemVendaViewModel>();
        dgCarrinho.ItemsSource = _itensCarrinho;

        // Iniciar nova venda
        IniciarNovaVenda();

        // Foco inicial
        txtCodigoProduto.Focus();
    }

    private void IniciarNovaVenda()
    {
        _vendaAtual = new Venda
        {
            DataVenda = DateTime.Now,
            Status = "ABERTA",
            UsuarioId = "Admin", // TODO: Pegar do sistema de login
            ValorTotal = 0,
            ValorDesconto = 0,
            ValorFinal = 0,
            Itens = new System.Collections.Generic.List<ItemVenda>()
        };
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await CarregarDadosIniciaisAsync();
    }

    private async Task CarregarDadosIniciaisAsync()
    {
        try
        {
            // Carregar produtos
            var produtos = await _unitOfWork.Produtos.GetProdutosAtivosAsync();
            var produtosList = produtos.ToList();

            cbProduto.ItemsSource = produtosList.Cast<object>().ToList();
            cbProduto.DisplayMemberPath = "NomeCompleto";

            // Carregar clientes
            //var clientes = await _unitOfWork.Clientes.GetClientesAtivosAsync();
            //var clientesList = clientes.ToList();

            //cbCliente.ItemsSource = clientesList.Cast<object>().ToList();
            cbCliente.DisplayMemberPath = "Nome";

            // Carregar formas de pagamento
            var formasPagamento = await _unitOfWork.FormasPagamento.GetFormasAtivasAsync();
            var formasList = formasPagamento.ToList();

            cbFormaPagamento.ItemsSource = formasList.Cast<object>().ToList();
            cbFormaPagamento.DisplayMemberPath = "Nome";

            // Gerar opções de parcelas (1 a 12)
            var parcelas = Enumerable.Range(1, 12).Select(i => new ParcelaInfo
            {
                Numero = i,
                Descricao = i <= 3 ? $"{i}x sem juros" : $"{i}x com juros"
            }).ToList();

            cbParcelas.ItemsSource = parcelas.Cast<object>().ToList();
            cbParcelas.DisplayMemberPath = "Descricao";

            // Log para debug
            System.Diagnostics.Debug.WriteLine($"Produtos carregados: {produtosList.Count}");
            //System.Diagnostics.Debug.WriteLine($"Clientes carregados: {clientesList.Count}");
            System.Diagnostics.Debug.WriteLine($"Formas de pagamento carregadas: {formasList.Count}");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao carregar dados: {ex.Message}\n\nDetalhes: {ex.InnerException?.Message}",
                "Erro", MessageBoxButton.OK, MessageBoxImage.Error);

            System.Diagnostics.Debug.WriteLine($"ERRO: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
        }
    }

    private void ConfigurarEventos()
    {
        this.KeyDown += Window_KeyDown;

        // Focos para teclado numérico
        txtValorPago.GotFocus += (s, e) => _campoFocado = txtValorPago;
        txtQuantidadeProduto.GotFocus += (s, e) => _campoFocado = txtQuantidadeProduto;
        txtCodigoProduto.GotFocus += (s, e) => _campoFocado = txtCodigoProduto;
    }

    private void InicializarTimer()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += Timer_Tick;
        _timer.Start();

        AtualizarDataHora();
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        AtualizarDataHora();
    }

    private void AtualizarDataHora()
    {
        txtDataHoraAtual.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.F2:
                BtnFinalizarVenda_Click(null, null);
                break;
            case Key.Escape:
                BtnCancelarVenda_Click(null, null);
                break;
            case Key.F3:
                txtCodigoProduto.Focus();
                txtCodigoProduto.SelectAll();
                break;
            case Key.F4:
                cbCliente.Focus();
                break;
        }
    }

    #region Adicionar Produto

    private async void TxtCodigoProduto_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            await BuscarProdutoPorCodigoAsync();
        }
    }

    private async Task BuscarProdutoPorCodigoAsync()
    {
        string codigo = txtCodigoProduto.Text.Trim();

        if (string.IsNullOrEmpty(codigo))
            return;

        try
        {
            // Buscar por código ou código de barras
            var produto = await _unitOfWork.Produtos.GetByCodigoAsync(codigo);

            if (produto == null)
            {
                // Tentar buscar por código de barras
                var produtos = await _unitOfWork.Produtos.GetProdutosAtivosAsync();
                produto = produtos.FirstOrDefault(p =>
                    p.CodigoBarras != null && p.CodigoBarras.Equals(codigo, StringComparison.OrdinalIgnoreCase));
            }

            if (produto != null)
            {
                await AdicionarProdutoAoCarrinhoAsync(produto);
                txtCodigoProduto.Clear();
                txtCodigoProduto.Focus();
            }
            else
            {
                MessageBox.Show("Produto não encontrado!", "Atenção",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCodigoProduto.SelectAll();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao buscar produto: {ex.Message}", "Erro",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CbProduto_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (cbProduto.SelectedItem is Produto produto)
        {
            txtCodigoProduto.Text = produto.Nome;
        }
    }

    private async void BtnAdicionar_Click(object sender, RoutedEventArgs e)
    {
        if (cbProduto.SelectedItem is Produto produto)
        {
            await AdicionarProdutoAoCarrinhoAsync(produto);
        }
        else if (!string.IsNullOrEmpty(txtCodigoProduto.Text))
        {
            await BuscarProdutoPorCodigoAsync();
        }
        else
        {
            MessageBox.Show("Selecione um produto!", "Atenção",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async Task AdicionarProdutoAoCarrinhoAsync(Produto produto)
    {
        try
        {
            if (!decimal.TryParse(txtQuantidadeProduto.Text, NumberStyles.Any, CultureInfo.GetCultureInfo("pt-BR"),
                out decimal quantidade) || quantidade <= 0)
            {
                MessageBox.Show("Quantidade inválida!", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (quantidade > produto.EstoqueAtual)
            {
                MessageBox.Show($"Estoque insuficiente! Disponível: {produto.EstoqueAtual}",
                    "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Verificar se já existe no carrinho
            var itemExistente = _itensCarrinho.FirstOrDefault(i => i.ProdutoId == produto.Id);

            if (itemExistente != null)
            {
                itemExistente.Quantidade += quantidade;
                itemExistente.CalcularSubtotal();
            }
            else
            {
                var novoItem = new ItemVendaViewModel
                {
                    ProdutoId = produto.Id,
                    ProdutoCodigo = produto.Codigo.ToString(),
                    ProdutoNome = produto.Nome,
                    Quantidade = quantidade,
                    PrecoUnitario = produto.PrecoVenda,
                    Desconto = 0
                };
                novoItem.CalcularSubtotal();

                _itensCarrinho.Add(novoItem);
            }

            AtualizarTotais();
            txtQuantidadeProduto.Text = "1";
            txtCodigoProduto.Clear();
            txtCodigoProduto.Focus();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao adicionar produto: {ex.Message}", "Erro",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    #region Manipular Itens

    private void BtnEditarItem_Click(object sender, RoutedEventArgs e)
    {
        if (dgCarrinho.SelectedItem is ItemVendaViewModel item)
        {
            // TODO: Implementar janela de edição
            MessageBox.Show($"Editar item: {item.ProdutoNome}", "Editar",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show("Selecione um item para editar!", "Atenção",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void BtnRemoverItem_Click(object sender, RoutedEventArgs e)
    {
        if (dgCarrinho.SelectedItem is ItemVendaViewModel item)
        {
            var result = MessageBox.Show($"Remover '{item.ProdutoNome}' do carrinho?",
                "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _itensCarrinho.Remove(item);
                AtualizarTotais();
            }
        }
        else
        {
            MessageBox.Show("Selecione um item para remover!", "Atenção",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    #endregion

    #region Pagamento

    private void CbCliente_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (cbCliente.SelectedItem is Cliente cliente)
        {
            _vendaAtual.ClienteId = cliente.Id;
            System.Diagnostics.Debug.WriteLine($"Cliente selecionado: {cliente.Nome}");
        }
    }

    private void CbFormaPagamento_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (cbFormaPagamento.SelectedItem is FormaPagamento forma)
        {
            _vendaAtual.FormaPagamento = forma.Nome;

            // Mostrar/ocultar painéis
            pnlDinheiro.Visibility = forma.Tipo == "DINHEIRO" ? Visibility.Visible : Visibility.Collapsed;
            pnlParcelas.Visibility = forma.PermiteParcelas ? Visibility.Visible : Visibility.Collapsed;

            if (forma.PermiteParcelas)
            {
                CalcularParcelas();
            }

            System.Diagnostics.Debug.WriteLine($"Forma de pagamento: {forma.Nome}, Tipo: {forma.Tipo}");
        }
    }

    private void TxtValorPago_TextChanged(object sender, TextChangedEventArgs e)
    {
        CalcularTroco();
    }

    private void CalcularTroco()
    {
        if (decimal.TryParse(txtValorPago.Text, NumberStyles.Any, CultureInfo.GetCultureInfo("pt-BR"),
            out decimal valorPago))
        {
            decimal totalVenda = ObterTotalVenda();
            decimal troco = valorPago - totalVenda;

            txtTroco.Text = $"R$ {troco:F2}";
            _vendaAtual.ValorPago = valorPago;
            _vendaAtual.Troco = troco > 0 ? troco : 0;
        }
    }

    private void CbParcelas_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        CalcularParcelas();
    }

    private void CalcularParcelas()
    {
        if (cbParcelas.SelectedItem is ParcelaInfo parcela &&
            cbFormaPagamento.SelectedItem is FormaPagamento forma)
        {
            decimal totalVenda = ObterTotalVenda();
            decimal valorParcela = totalVenda / parcela.Numero;

            // Aplicar juros se necessário
            if (parcela.Numero > 3 && forma.TaxaJuros > 0)
            {
                decimal juros = forma.TaxaJuros / 100;
                valorParcela *= (1 + juros);
            }

            txtValorParcela.Text = $"R$ {valorParcela:F2}";
            _vendaAtual.NumeroParcelas = parcela.Numero;
        }
    }

    #endregion

    #region Teclado Numérico

    private void BtnNumerico_Click(object sender, RoutedEventArgs e)
    {
        if (_campoFocado != null && sender is Button btn)
        {
            string numero = btn.Content.ToString();
            _campoFocado.Text += numero;

            if (_campoFocado == txtValorPago)
            {
                CalcularTroco();
            }
        }
    }

    private void BtnBackspace_Click(object sender, RoutedEventArgs e)
    {
        if (_campoFocado != null && _campoFocado.Text.Length > 0)
        {
            _campoFocado.Text = _campoFocado.Text.Substring(0, _campoFocado.Text.Length - 1);

            if (_campoFocado == txtValorPago)
            {
                CalcularTroco();
            }
        }
    }

    #endregion

    #region Ações Principais

    private async void BtnFinalizarVenda_Click(object sender, RoutedEventArgs e)
    {
        if (_itensCarrinho.Count == 0)
        {
            MessageBox.Show("Adicione produtos ao carrinho!", "Atenção",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Validar pagamento
        if (cbFormaPagamento.SelectedItem is FormaPagamento forma)
        {
            if (forma.Tipo == "DINHEIRO")
            {
                if (!decimal.TryParse(txtValorPago.Text, NumberStyles.Any, CultureInfo.GetCultureInfo("pt-BR"),
                    out decimal valorPago) || valorPago < ObterTotalVenda())
                {
                    MessageBox.Show("Valor pago insuficiente!", "Erro",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }
        else
        {
            MessageBox.Show("Selecione uma forma de pagamento!", "Atenção",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Confirmar venda no valor de {txtTotalVenda.Text}?",
            "Finalizar Venda",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            await FinalizarVendaAsync();
        }
    }

    private async Task FinalizarVendaAsync()
    {
        try
        {
            // Gerar número da venda
            _vendaAtual.NumeroVenda = await _unitOfWork.Vendas.GerarNumeroVendaAsync();
            _vendaAtual.Status = "FINALIZADA";
            _vendaAtual.ValorTotal = _itensCarrinho.Sum(i => i.PrecoUnitario * i.Quantidade);
            _vendaAtual.ValorDesconto = _itensCarrinho.Sum(i => i.Desconto);
            _vendaAtual.ValorFinal = ObterTotalVenda();

            // Adicionar itens
            int sequencia = 1;
            foreach (var item in _itensCarrinho)
            {
                _vendaAtual.Itens.Add(new ItemVenda
                {
                    ProdutoId = item.ProdutoId,
                    ProdutoNome = item.ProdutoNome,
                    Quantidade = item.Quantidade,
                    PrecoUnitario = item.PrecoUnitario,
                    Desconto = item.Desconto,
                    Subtotal = item.Subtotal,
                    Sequencia = sequencia++
                });

                // Atualizar estoque
                await _unitOfWork.Produtos.AtualizarEstoqueAsync(
                    item.ProdutoId,
                    -(int)item.Quantidade);
            }

            // Salvar venda
            await _unitOfWork.Vendas.AddAsync(_vendaAtual);

            MessageBox.Show(
                $"Venda finalizada com sucesso!\nNúmero: {_vendaAtual.NumeroVenda}",
                "Sucesso",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            LimparVenda();
            IniciarNovaVenda();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao finalizar venda: {ex.Message}\n\nDetalhes: {ex.InnerException?.Message}", "Erro",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnCancelarVenda_Click(object sender, RoutedEventArgs e)
    {
        if (_itensCarrinho.Count > 0)
        {
            var result = MessageBox.Show("Cancelar a venda atual?", "Confirmação",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                LimparVenda();
                IniciarNovaVenda();
            }
        }
    }

    private void BtnSuspenderVenda_Click(object sender, RoutedEventArgs e)
    {
        if (_itensCarrinho.Count > 0)
        {
            // TODO: Implementar suspensão de venda
            MessageBox.Show("Venda suspensa! (Implementar lógica de recuperação)",
                "Venda Suspensa", MessageBoxButton.OK, MessageBoxImage.Information);

            LimparVenda();
        }
    }

    #endregion

    #region Métodos Auxiliares

    private void AtualizarTotais()
    {
        txtTotalItens.Text = _itensCarrinho.Sum(i => i.Quantidade).ToString("N0");
        txtDescontoTotal.Text = $"R$ {_itensCarrinho.Sum(i => i.Desconto):F2}";
        txtTotalVenda.Text = $"R$ {ObterTotalVenda():F2}";

        CalcularTroco();

        if (pnlParcelas.Visibility == Visibility.Visible)
        {
            CalcularParcelas();
        }
    }

    private decimal ObterTotalVenda()
    {
        return _itensCarrinho.Sum(i => i.Subtotal);
    }

    private void LimparVenda()
    {
        _itensCarrinho.Clear();
        txtCodigoProduto.Clear();
        txtQuantidadeProduto.Text = "1";
        txtValorPago.Clear();
        txtTroco.Text = "R$ 0,00";
        cbProduto.ClearSelection();
        cbCliente.ClearSelection();
        cbFormaPagamento.ClearSelection();

        pnlDinheiro.Visibility = Visibility.Collapsed;
        pnlParcelas.Visibility = Visibility.Collapsed;

        AtualizarTotais();
        txtCodigoProduto.Focus();
    }

    private void BtnVoltar_Click(object sender, RoutedEventArgs e)
    {
        if (_itensCarrinho.Count > 0)
        {
            var result = MessageBox.Show("Existe uma venda em andamento. Deseja realmente sair?",
                "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
                return;
        }

        this.Close();
    }

    #endregion

    #region Validações

    private void TextBox_PreviewTextInputDecimal(object sender, TextCompositionEventArgs e)
    {
        var textBox = sender as TextBox;
        string text = textBox.Text + e.Text;

        // Permitir vírgula ou ponto como separador decimal
        if (e.Text == "," || e.Text == ".")
        {
            e.Handled = text.Count(c => c == ',' || c == '.') > 1;
            return;
        }

        e.Handled = !decimal.TryParse(text.Replace(',', '.'), NumberStyles.Any,
            CultureInfo.InvariantCulture, out _);
    }

    #endregion

    protected override void OnClosed(EventArgs e)
    {
        _timer?.Stop();
        _unitOfWork?.Dispose();
        base.OnClosed(e);
    }
}