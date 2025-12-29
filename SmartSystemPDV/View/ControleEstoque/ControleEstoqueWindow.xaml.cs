using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SmartSystemPDV.View.ControleEstoque;

/// <summary>
/// Interaction logic for StockControl.xaml
/// </summary>
public partial class ControleEstoqueWindow : Window
{
    private ObservableCollection<ProdutoEstoque> listaProdutos;
    private ObservableCollection<MovimentacaoEstoque> listaMovimentacoes;
    private ObservableCollection<MovimentacaoEstoque> movimentacoesCompletas;

    public ControleEstoqueWindow()
    {
        InitializeComponent();
        InicializarDados();
        CarregarDados();
        AtualizarDataHora();
    }

    private void InicializarDados()
    {
        listaProdutos = new ObservableCollection<ProdutoEstoque>();
        listaMovimentacoes = new ObservableCollection<MovimentacaoEstoque>();
        movimentacoesCompletas = new ObservableCollection<MovimentacaoEstoque>();

        // Produtos de exemplo
        listaProdutos.Add(new ProdutoEstoque
        {
            Codigo = 1,
            Nome = "Mouse Gamer RGB",
            Categoria = "Eletrônicos",
            Unidade = "UN",
            Estoque = 25,
            EstoqueMinimo = 5,
            PrecoCusto = 89.90m,
            PrecoVenda = 149.90m,
            Status = "Ativo"
        });

        listaProdutos.Add(new ProdutoEstoque
        {
            Codigo = 2,
            Nome = "Teclado Mecânico",
            Categoria = "Eletrônicos",
            Unidade = "UN",
            Estoque = 15,
            EstoqueMinimo = 3,
            PrecoCusto = 199.90m,
            PrecoVenda = 349.90m,
            Status = "Ativo"
        });

        listaProdutos.Add(new ProdutoEstoque
        {
            Codigo = 3,
            Nome = "Refrigerante Cola 2L",
            Categoria = "Bebidas",
            Unidade = "UN",
            Estoque = 150,
            EstoqueMinimo = 30,
            PrecoCusto = 3.50m,
            PrecoVenda = 6.99m,
            Status = "Ativo"
        });

        listaProdutos.Add(new ProdutoEstoque
        {
            Codigo = 4,
            Nome = "Arroz Tipo 1 5Kg",
            Categoria = "Alimentos",
            Unidade = "PC",
            Estoque = 2,
            EstoqueMinimo = 20,
            PrecoCusto = 12.50m,
            PrecoVenda = 22.90m,
            Status = "Ativo"
        });

        listaProdutos.Add(new ProdutoEstoque
        {
            Codigo = 5,
            Nome = "Detergente Líquido 500ml",
            Categoria = "Limpeza",
            Unidade = "UN",
            Estoque = 0,
            EstoqueMinimo = 50,
            PrecoCusto = 1.50m,
            PrecoVenda = 2.99m,
            Status = "Ativo"
        });

        listaProdutos.Add(new ProdutoEstoque
        {
            Codigo = 6,
            Nome = "Sabonete Líquido 250ml",
            Categoria = "Higiene",
            Unidade = "UN",
            Estoque = 80,
            EstoqueMinimo = 20,
            PrecoCusto = 4.50m,
            PrecoVenda = 8.90m,
            Status = "Ativo"
        });

        // Movimentações de exemplo
        AdicionarMovimentacaoExemplo(1, "Mouse Gamer RGB", "Entrada", 20, 5, 25, "Compra de estoque inicial", DateTime.Now.AddDays(-5));
        AdicionarMovimentacaoExemplo(2, "Teclado Mecânico", "Entrada", 20, 0, 20, "Compra fornecedor XYZ", DateTime.Now.AddDays(-4));
        AdicionarMovimentacaoExemplo(2, "Teclado Mecânico", "Saída", -5, 20, 15, "Venda balcão", DateTime.Now.AddDays(-3));
        AdicionarMovimentacaoExemplo(3, "Refrigerante Cola 2L", "Entrada", 200, 0, 200, "Compra promocional", DateTime.Now.AddDays(-2));
        AdicionarMovimentacaoExemplo(3, "Refrigerante Cola 2L", "Saída", -50, 200, 150, "Venda grande quantidade", DateTime.Now.AddDays(-1));
        AdicionarMovimentacaoExemplo(4, "Arroz Tipo 1 5Kg", "Saída", -18, 20, 2, "Vendas do dia", DateTime.Now);
        AdicionarMovimentacaoExemplo(5, "Detergente Líquido 500ml", "Perda", -50, 50, 0, "Produto vencido", DateTime.Now.AddHours(-2));
    }

    private void AdicionarMovimentacaoExemplo(int codigoProduto, string nomeProduto, string tipo,
        int quantidade, int qtdAnterior, int qtdFinal, string motivo, DateTime dataHora)
    {
        var movimentacao = new MovimentacaoEstoque
        {
            Id = movimentacoesCompletas.Count + 1,
            DataHora = dataHora,
            CodigoProduto = codigoProduto,
            NomeProduto = nomeProduto,
            Tipo = tipo,
            Quantidade = quantidade,
            QuantidadeAnterior = qtdAnterior,
            QuantidadeFinal = qtdFinal,
            Motivo = motivo,
            Usuario = "Admin"
        };

        movimentacoesCompletas.Add(movimentacao);
        listaMovimentacoes.Add(movimentacao);
    }

    private void CarregarDados()
    {
        // Carregar produtos no ComboBox
        cbProduto.Items.Clear();
        cbProduto.Items.Add(new ComboBoxItem { Content = "Selecione um produto...", IsSelected = true });

        foreach (var produto in listaProdutos.OrderBy(p => p.Nome))
        {
            var item = new ComboBoxItem
            {
                Content = $"{produto.Codigo} - {produto.Nome}",
                Tag = produto
            };
            cbProduto.Items.Add(item);
        }

        // Carregar DataGrids
        dgEstoque.ItemsSource = listaProdutos;
        dgHistorico.ItemsSource = listaMovimentacoes.OrderByDescending(m => m.DataHora);

        // Atualizar cards de resumo
        AtualizarResumo();
    }

    private void AtualizarResumo()
    {
        txtTotalProdutos.Text = listaProdutos.Count.ToString();

        decimal valorTotal = listaProdutos.Sum(p => p.ValorTotal);
        txtValorTotal.Text = valorTotal.ToString("C2");

        int estoqueBaixo = listaProdutos.Count(p => p.StatusEstoque == "Crítico" || p.StatusEstoque == "Baixo");
        txtEstoqueBaixo.Text = estoqueBaixo.ToString();

        int produtosAtivos = listaProdutos.Count(p => p.Status == "Ativo");
        txtProdutosAtivos.Text = produtosAtivos.ToString();
    }

    private void AtualizarDataHora()
    {
        txtDataHora.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
    }

    private void CbProduto_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        //if (cbProduto.SelectedIndex > 0)
        //{
        //    var item = (ComboBoxItem)cbProduto.SelectedItem;
        //    var produto = (ProdutoEstoque)item.Tag;
        //    txtEstoqueAtual.Text = produto.Estoque.ToString();
        //    CalcularNovoEstoque();
        //}
        //else
        //{
        //    txtEstoqueAtual.Text = "0";
        //    txtNovoEstoque.Text = "0";
        //}
    }

    private void CbTipoMovimentacao_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        CalcularNovoEstoque();
    }

    private void TxtQuantidade_TextChanged(object sender, TextChangedEventArgs e)
    {
        CalcularNovoEstoque();
    }

    private void CalcularNovoEstoque()
    {
        //if (cbProduto.SelectedIndex > 0 &&
        //    cbTipoMovimentacao.SelectedIndex > 0 &&
        //    int.TryParse(txtQuantidade.Text, out int quantidade))
        //{
        //    int estoqueAtual = int.Parse(txtEstoqueAtual.Text);
        //    int novoEstoque = estoqueAtual;

        //    var tipoItem = (ComboBoxItem)cbTipoMovimentacao.SelectedItem;
        //    string tipo = tipoItem.Tag?.ToString();

        //    switch (tipo)
        //    {
        //        case "entrada":
        //        case "devolucao":
        //            novoEstoque = estoqueAtual + quantidade;
        //            break;
        //        case "saida":
        //        case "perda":
        //            novoEstoque = estoqueAtual - quantidade;
        //            break;
        //        case "ajuste":
        //            novoEstoque = quantidade; // Ajuste define o valor absoluto
        //            break;
        //    }

        //    txtNovoEstoque.Text = novoEstoque.ToString();

        //    // Avisar se o estoque ficará negativo
        //    if (novoEstoque < 0)
        //    {
        //        txtNovoEstoque.Background = System.Windows.Media.Brushes.LightCoral;
        //    }
        //    else
        //    {
        //        txtNovoEstoque.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#E8F5E9");
        //    }
        //}
        //else
        //{
        //    txtNovoEstoque.Text = txtEstoqueAtual.Text;
        //}
    }

    private void BtnRegistrar_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidarCampos())
            return;

        try
        {
            var item = (ComboBoxItem)cbProduto.SelectedItem;
            var produto = (ProdutoEstoque)item.Tag;

            var tipoItem = (ComboBoxItem)cbTipoMovimentacao.SelectedItem;
            string tipo = tipoItem.Content.ToString();
            string tipoTag = tipoItem.Tag?.ToString();

            int quantidade = int.Parse(txtQuantidade.Text);
            int estoqueAnterior = produto.Estoque;
            int novoEstoque = int.Parse(txtNovoEstoque.Text);

            // Verificar se estoque ficará negativo
            if (novoEstoque < 0)
            {
                var result = MessageBox.Show(
                    "O estoque ficará negativo. Deseja continuar?",
                    "Atenção",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.No)
                    return;
            }

            // Calcular quantidade real da movimentação
            int qtdMovimentacao = 0;
            switch (tipoTag)
            {
                case "entrada":
                case "devolucao":
                    qtdMovimentacao = quantidade;
                    break;
                case "saida":
                case "perda":
                    qtdMovimentacao = -quantidade;
                    break;
                case "ajuste":
                    qtdMovimentacao = novoEstoque - estoqueAnterior;
                    break;
            }

            // Registrar movimentação
            var movimentacao = new MovimentacaoEstoque
            {
                Id = movimentacoesCompletas.Count + 1,
                DataHora = DateTime.Now,
                CodigoProduto = produto.Codigo,
                NomeProduto = produto.Nome,
                Tipo = tipo,
                Quantidade = qtdMovimentacao,
                QuantidadeAnterior = estoqueAnterior,
                QuantidadeFinal = novoEstoque,
                Motivo = txtMotivo.Text.Trim(),
                Usuario = "Admin"
            };

            movimentacoesCompletas.Add(movimentacao);
            listaMovimentacoes.Insert(0, movimentacao);

            // Atualizar estoque do produto
            produto.Estoque = novoEstoque;

            // Atualizar DataGrid
            dgEstoque.Items.Refresh();
            dgHistorico.Items.Refresh();

            // Atualizar resumo
            AtualizarResumo();

            MessageBox.Show(
                $"Movimentação registrada com sucesso!\n\n" +
                $"Produto: {produto.Nome}\n" +
                $"Tipo: {tipo}\n" +
                $"Estoque Anterior: {estoqueAnterior}\n" +
                $"Movimentação: {qtdMovimentacao:+#;-#;0}\n" +
                $"Novo Estoque: {novoEstoque}",
                "Sucesso",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            LimparCampos();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao registrar movimentação: {ex.Message}", "Erro",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnLimpar_Click(object sender, RoutedEventArgs e)
    {
        LimparCampos();
    }

    private void LimparCampos()
    {
        cbTipoMovimentacao.SelectedIndex = 0;
        cbProduto.SelectedIndex = 0;
        txtEstoqueAtual.Text = "0";
        txtQuantidade.Clear();
        txtNovoEstoque.Text = "0";
        txtMotivo.Clear();
        AtualizarDataHora();
    }

    private bool ValidarCampos()
    {
        if (cbTipoMovimentacao.SelectedIndex == 0)
        {
            MessageBox.Show("Por favor, selecione o tipo de movimentação.", "Atenção",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            cbTipoMovimentacao.Focus();
            return false;
        }

        if (cbProduto.SelectedIndex == 0)
        {
            MessageBox.Show("Por favor, selecione um produto.", "Atenção",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            cbProduto.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtQuantidade.Text))
        {
            MessageBox.Show("Por favor, informe a quantidade.", "Atenção",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            txtQuantidade.Focus();
            return false;
        }

        if (!int.TryParse(txtQuantidade.Text, out int quantidade) || quantidade <= 0)
        {
            MessageBox.Show("Quantidade inválida.", "Atenção",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            txtQuantidade.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtMotivo.Text))
        {
            MessageBox.Show("Por favor, informe o motivo da movimentação.", "Atenção",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            txtMotivo.Focus();
            return false;
        }

        return true;
    }

    private void CbFiltroTipo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        FiltrarHistorico();
    }

    private void DpData_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
    {
        FiltrarHistorico();
    }

    private void FiltrarHistorico()
    {
        //var filtradas = movimentacoesCompletas.AsEnumerable();

        //// Filtro por tipo
        //if (cbFiltroTipo.SelectedIndex > 0)
        //{
        //    string tipo = ((ComboBoxItem)cbFiltroTipo.SelectedItem).Content.ToString();
        //    filtradas = filtradas.Where(m => m.Tipo == tipo);
        //}

        //// Filtro por data
        //if (dpDataInicio.SelectedDate.HasValue)
        //{
        //    DateTime dataInicio = dpDataInicio.SelectedDate.Value.Date;
        //    filtradas = filtradas.Where(m => m.DataHora.Date >= dataInicio);
        //}

        //if (dpDataFim.SelectedDate.HasValue)
        //{
        //    DateTime dataFim = dpDataFim.SelectedDate.Value.Date.AddDays(1).AddSeconds(-1);
        //    filtradas = filtradas.Where(m => m.DataHora <= dataFim);
        //}

        //dgHistorico.ItemsSource = filtradas.OrderByDescending(m => m.DataHora);
    }

    private void TxtPesquisaEstoque_TextChanged(object sender, TextChangedEventArgs e)
    {
        FiltrarEstoque();
    }

    private void FiltrarEstoque()
    {
        //string filtro = txtPesquisaEstoque.Text.ToLower();

        //if (string.IsNullOrWhiteSpace(filtro))
        //{
        //    dgEstoque.ItemsSource = listaProdutos;
        //}
        //else
        //{
        //    var produtosFiltrados = listaProdutos.Where(p =>
        //        p.Codigo.ToString().Contains(filtro) ||
        //        p.Nome.ToLower().Contains(filtro) ||
        //        p.Categoria.ToLower().Contains(filtro) ||
        //        p.StatusEstoque.ToLower().Contains(filtro)
        //    ).ToList();

        //    dgEstoque.ItemsSource = produtosFiltrados;
        //}
    }

    private void BtnAtualizar_Click(object sender, RoutedEventArgs e)
    {
        dgEstoque.Items.Refresh();
        dgHistorico.Items.Refresh();
        AtualizarResumo();
        MessageBox.Show("Dados atualizados com sucesso!", "Informação",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void TextBox_PreviewTextInputNumeric(object sender, TextCompositionEventArgs e)
    {
        Regex regex = new Regex(@"^[0-9]+$");
        e.Handled = !regex.IsMatch(e.Text);
    }

    private void BtnVoltar_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Deseja voltar para a tela principal?",
            "Confirmar",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question
        );

        if (result == MessageBoxResult.Yes)
        {
            this.Close();
        }
    }
}

// Classe ProdutoEstoque (estendida)
public class ProdutoEstoque
{
    public int Codigo { get; set; }
    public string Nome { get; set; }
    public string Categoria { get; set; }
    public string Unidade { get; set; }
    public int Estoque { get; set; }
    public int EstoqueMinimo { get; set; }
    public decimal PrecoCusto { get; set; }
    public decimal PrecoVenda { get; set; }
    public string Status { get; set; }

    public decimal ValorTotal => Estoque * PrecoVenda;

    public string StatusEstoque
    {
        get
        {
            if (Estoque == 0)
                return "Crítico";
            else if (Estoque <= EstoqueMinimo)
                return "Baixo";
            else
                return "Normal";
        }
    }
}

// Classe MovimentacaoEstoque
public class MovimentacaoEstoque
{
    public int Id { get; set; }
    public DateTime DataHora { get; set; }
    public int CodigoProduto { get; set; }
    public string NomeProduto { get; set; }
    public string Tipo { get; set; }
    public int Quantidade { get; set; }
    public int QuantidadeAnterior { get; set; }
    public int QuantidadeFinal { get; set; }
    public string Motivo { get; set; }
    public string Usuario { get; set; }
}