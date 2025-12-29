using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace SmartSystemPDV.View.Vendas
{
    public partial class VendaWindow : Window
    {
        // Collections
        private ObservableCollection<ItemVenda> _itensCarrinho;
        private List<Produto> _produtos;

        // Timer para atualizar data/hora
        private DispatcherTimer _timer;

        // Campo focado para teclado numérico
        private TextBox _campoFocado;

        public VendaWindow()
        {
            InitializeComponent();
            InicializarComponentes();
            CarregarDados();
            ConfigurarEventos();
            InicializarTimer();
        }

        private void InicializarComponentes()
        {
            _itensCarrinho = new ObservableCollection<ItemVenda>();
            dgCarrinho.ItemsSource = _itensCarrinho;

            _produtos = new List<Produto>();

            // Foco inicial no campo de código
            txtCodigoProduto.Focus();
        }

        private void CarregarDados()
        {
            // Simular carregamento de produtos
            _produtos = new List<Produto>
            {
                new Produto { Codigo = "001", Nome = "Coca-Cola 2L", PrecoVenda = 8.50m, Estoque = 50 },
                new Produto { Codigo = "002", Nome = "Arroz Tipo 1 5kg", PrecoVenda = 25.90m, Estoque = 30 },
                new Produto { Codigo = "003", Nome = "Feijão Preto 1kg", PrecoVenda = 7.80m, Estoque = 40 },
                new Produto { Codigo = "004", Nome = "Óleo de Soja 900ml", PrecoVenda = 6.90m, Estoque = 60 },
                new Produto { Codigo = "005", Nome = "Macarrão Espaguete 500g", PrecoVenda = 4.50m, Estoque = 80 },
                new Produto { Codigo = "7891234567890", Nome = "Leite Integral 1L", PrecoVenda = 4.20m, Estoque = 100 },
                new Produto { Codigo = "7891234567891", Nome = "Açúcar Cristal 1kg", PrecoVenda = 3.80m, Estoque = 70 },
                new Produto { Codigo = "7891234567892", Nome = "Café Torrado 500g", PrecoVenda = 12.90m, Estoque = 45 },
            };

            // Preencher ComboBox de produtos
            cbProduto.Items.Clear();
            cbProduto.Items.Add(new ComboBoxItem { Content = "Selecione ou digite código...", IsSelected = true });

            foreach (var produto in _produtos)
            {
                var item = new ComboBoxItem
                {
                    Content = $"{produto.Codigo} - {produto.Nome} - R$ {produto.PrecoVenda:F2}",
                    Tag = produto
                };
                cbProduto.Items.Add(item);
            }

            // Carregar clientes (exemplo)
            cbCliente.Items.Clear();
            cbCliente.Items.Add(new ComboBoxItem { Content = "Cliente Padrão", IsSelected = true });
            cbCliente.Items.Add(new ComboBoxItem { Content = "João Silva - 123.456.789-00" });
            cbCliente.Items.Add(new ComboBoxItem { Content = "Maria Santos - 987.654.321-00" });
        }

        private void ConfigurarEventos()
        {
            // Teclas de atalho
            this.KeyDown += Window_KeyDown;

            // Eventos de foco para teclado numérico
            txtValorPago.GotFocus += (s, e) => _campoFocado = txtValorPago;
            txtQuantidadeProduto.GotFocus += (s, e) => _campoFocado = txtQuantidadeProduto;
            txtCodigoProduto.GotFocus += (s, e) => _campoFocado = txtCodigoProduto;
        }

        private void InicializarTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
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
            // F2 - Finalizar Venda
            if (e.Key == Key.F2)
            {
                BtnFinalizarVenda_Click(null, null);
            }
            // ESC - Cancelar Venda
            else if (e.Key == Key.Escape)
            {
                BtnCancelarVenda_Click(null, null);
            }
            // F3 - Buscar Produto
            else if (e.Key == Key.F3)
            {
                txtCodigoProduto.Focus();
                txtCodigoProduto.SelectAll();
            }
            // F4 - Cliente
            else if (e.Key == Key.F4)
            {
                cbCliente.Focus();
                cbCliente.IsDropDownOpen = true;
            }
        }

        #region Eventos - Adicionar Produto

        private void TxtCodigoProduto_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuscarProdutoPorCodigo();
            }
        }

        private void BuscarProdutoPorCodigo()
        {
            string codigo = txtCodigoProduto.Text.Trim();

            if (string.IsNullOrEmpty(codigo))
                return;

            var produto = _produtos.FirstOrDefault(p => p.Codigo == codigo);

            if (produto != null)
            {
                AdicionarProdutoAoCarrinho(produto);
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

        private void CbProduto_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbProduto.SelectedItem is ComboBoxItem item && item.Tag is Produto produto)
            {
                txtCodigoProduto.Text = produto.Codigo;
            }
        }

        private void BtnAdicionar_Click(object sender, RoutedEventArgs e)
        {
            if (cbProduto.SelectedItem is ComboBoxItem item && item.Tag is Produto produto)
            {
                AdicionarProdutoAoCarrinho(produto);
            }
            else if (!string.IsNullOrEmpty(txtCodigoProduto.Text))
            {
                BuscarProdutoPorCodigo();
            }
            else
            {
                MessageBox.Show("Selecione um produto!", "Atenção",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AdicionarProdutoAoCarrinho(Produto produto)
        {
            if (!int.TryParse(txtQuantidadeProduto.Text, out int quantidade) || quantidade <= 0)
            {
                MessageBox.Show("Quantidade inválida!", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (quantidade > produto.Estoque)
            {
                MessageBox.Show($"Estoque insuficiente! Disponível: {produto.Estoque}",
                    "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Verificar se o produto já está no carrinho
            var itemExistente = _itensCarrinho.FirstOrDefault(i => i.Codigo == produto.Codigo);

            if (itemExistente != null)
            {
                itemExistente.Quantidade += quantidade;
                dgCarrinho.Items.Refresh();
            }
            else
            {
                var novoItem = new ItemVenda
                {
                    Codigo = produto.Codigo,
                    Nome = produto.Nome,
                    Quantidade = quantidade,
                    PrecoUnitario = produto.PrecoVenda,
                    Desconto = 0
                };

                _itensCarrinho.Add(novoItem);
            }

            AtualizarTotais();
            txtQuantidadeProduto.Text = "1";
            txtCodigoProduto.Clear();
            txtCodigoProduto.Focus();
        }

        #endregion

        #region Eventos - Manipular Itens

        private void BtnEditarItem_Click(object sender, RoutedEventArgs e)
        {
            if (dgCarrinho.SelectedItem is ItemVenda item)
            {
                // Implementar janela de edição
                MessageBox.Show($"Editar item: {item.Nome}", "Editar",
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
            if (dgCarrinho.SelectedItem is ItemVenda item)
            {
                var result = MessageBox.Show($"Remover '{item.Nome}' do carrinho?",
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

        #region Eventos - Pagamento

        private void CbFormaPagamento_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (cbFormaPagamento.SelectedItem is ComboBoxItem item)
            //{
            //    string formaPagamento = item.Content.ToString();

            //    // Mostrar/ocultar painéis conforme forma de pagamento
            //    pnlDinheiro.Visibility = formaPagamento == "Dinheiro" ?
            //        Visibility.Visible : Visibility.Collapsed;

            //    pnlParcelas.Visibility = formaPagamento == "Cartão de Crédito" ?
            //        Visibility.Visible : Visibility.Collapsed;

            //    if (formaPagamento == "Cartão de Crédito")
            //    {
            //        CalcularParcelas();
            //    }
            //}
        }

        private void TxtValorPago_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalcularTroco();
        }

        private void CalcularTroco()
        {
            //if (decimal.TryParse(txtValorPago.Text.Replace("R$", "").Replace(".", "").Replace(",", ".").Trim(),
            //    out decimal valorPago))
            //{
            //    decimal totalVenda = ObterTotalVenda();
            //    decimal troco = valorPago - totalVenda;

            //    txtTroco.Text = troco >= 0 ? $"R$ {troco:F2}" : "R$ 0,00";
            //    txtTroco.Foreground = troco >= 0 ?
            //        System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
            //}
        }

        private void CbParcelas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CalcularParcelas();
        }

        private void CalcularParcelas()
        {
            //if (cbParcelas.SelectedItem is ComboBoxItem item)
            //{
            //    string parcela = item.Content.ToString();
            //    int numParcelas = int.Parse(parcela.Split('x')[0]);

            //    decimal totalVenda = ObterTotalVenda();
            //    decimal valorParcela = totalVenda / numParcelas;

            //    // Aplicar juros se necessário
            //    if (parcela.Contains("com juros"))
            //    {
            //        valorParcela *= 1.05m; // 5% de juros
            //    }

            //    txtValorParcela.Text = $"R$ {valorParcela:F2}";
            //}
        }

        #endregion

        #region Eventos - Teclado Numérico

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

        #region Eventos - Ações Principais

        private void BtnFinalizarVenda_Click(object sender, RoutedEventArgs e)
        {
            if (_itensCarrinho.Count == 0)
            {
                MessageBox.Show("Adicione produtos ao carrinho!", "Atenção",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar pagamento
            if (cbFormaPagamento.SelectedItem is ComboBoxItem item)
            {
                string formaPagamento = item.Content.ToString();

                if (formaPagamento == "Dinheiro")
                {
                    if (!decimal.TryParse(txtValorPago.Text.Replace("R$", "").Replace(".", "").Replace(",", ".").Trim(),
                        out decimal valorPago) || valorPago < ObterTotalVenda())
                    {
                        MessageBox.Show("Valor pago insuficiente!", "Erro",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }

            var result = MessageBox.Show(
                $"Confirmar venda no valor de {txtTotalVenda.Text}?",
                "Finalizar Venda",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Aqui você implementaria a lógica de salvar a venda no banco de dados

                MessageBox.Show("Venda finalizada com sucesso!", "Sucesso",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                LimparVenda();
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
                }
            }
        }

        private void BtnSuspenderVenda_Click(object sender, RoutedEventArgs e)
        {
            if (_itensCarrinho.Count > 0)
            {
                MessageBox.Show("Venda suspensa! (Implementar lógica de recuperação)",
                    "Venda Suspensa", MessageBoxButton.OK, MessageBoxImage.Information);

                LimparVenda();
            }
        }

        private void BtnNovoCliente_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Abrir janela de cadastro de cliente", "Novo Cliente",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Métodos Auxiliares

        private void AtualizarTotais()
        {
            txtTotalItens.Text = _itensCarrinho.Sum(i => i.Quantidade).ToString();
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
            cbProduto.SelectedIndex = 0;
            cbCliente.SelectedIndex = 0;
            cbFormaPagamento.SelectedIndex = 0;

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

        #region Validações de Input

        private void TextBox_PreviewTextInputNumeric(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void TextBox_PreviewTextInputDecimal(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            string text = textBox.Text + e.Text;
            e.Handled = !decimal.TryParse(text.Replace(",", "."), out _) && e.Text != ",";
        }

        #endregion

        protected override void OnClosed(EventArgs e)
        {
            _timer?.Stop();
            base.OnClosed(e);
        }
    }

    #region Classes de Modelo

    public class ItemVenda
    {
        public string Codigo { get; set; }
        public string Nome { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal Desconto { get; set; }

        public decimal Subtotal => (PrecoUnitario * Quantidade) - Desconto;
    }

    public class Produto
    {
        public string Codigo { get; set; }
        public string Nome { get; set; }
        public decimal PrecoVenda { get; set; }
        public int Estoque { get; set; }
    }

    #endregion
}