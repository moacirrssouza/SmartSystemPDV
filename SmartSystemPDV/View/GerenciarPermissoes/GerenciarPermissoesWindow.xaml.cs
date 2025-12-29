using SmartSystemPDV.Models;
using System.Windows;

namespace SmartSystemPDV.View.GerenciarPermissoes
{
    /// <summary>
    /// Interaction logic for GerenciarPermissoesWindow.xaml
    /// </summary>
    public partial class GerenciarPermissoesWindow : Window
    {
        private Dictionary<string, bool> permissoes;

        public GerenciarPermissoesWindow(Usuario usuario)
        {
            InitializeComponent();
            CarregarDados();
        }

        private void CarregarDados()
        {
            CarregarPermissoesPerfil();
        }

        private void CarregarPermissoesPerfil()
        {
            // Define permissões padrão baseadas no perfil
            permissoes = new Dictionary<string, bool>();
        }

        private void MarcarTodasPermissoes(bool marcar)
        {
            // Produtos
            chkProdutosVisualizar.IsChecked = marcar;
            chkProdutosCadastrar.IsChecked = marcar;
            chkProdutosEditar.IsChecked = marcar;
            chkProdutosExcluir.IsChecked = marcar;
            chkProdutosImportar.IsChecked = marcar;

            // Estoque
            chkEstoqueVisualizar.IsChecked = marcar;
            chkEstoqueEntrada.IsChecked = marcar;
            chkEstoqueSaida.IsChecked = marcar;
            chkEstoqueAjuste.IsChecked = marcar;
            chkEstoqueHistorico.IsChecked = marcar;

            // Vendas
            chkVendasVisualizar.IsChecked = marcar;
            chkVendasRealizar.IsChecked = marcar;
            chkVendasCancelar.IsChecked = marcar;
            chkVendasDesconto.IsChecked = marcar;
            chkVendasDevolucao.IsChecked = marcar;

            // Financeiro
            chkFinanceiroVisualizar.IsChecked = marcar;
            chkFinanceiroContas.IsChecked = marcar;
            chkFinanceiroCaixa.IsChecked = marcar;
            chkFinanceiroRelatorios.IsChecked = marcar;

            // Clientes
            chkClientesVisualizar.IsChecked = marcar;
            chkClientesCadastrar.IsChecked = marcar;
            chkClientesEditar.IsChecked = marcar;
            chkClientesExcluir.IsChecked = marcar;

            // Relatórios
            chkRelatoriosVendas.IsChecked = marcar;
            chkRelatoriosEstoque.IsChecked = marcar;
            chkRelatoriosFinanceiro.IsChecked = marcar;
            chkRelatoriosGerenciais.IsChecked = marcar;
            chkRelatoriosExportar.IsChecked = marcar;

            // Sistema
            chkSistemaUsuarios.IsChecked = marcar;
            chkSistemaPermissoes.IsChecked = marcar;
            chkSistemaConfiguracoes.IsChecked = marcar;
            chkSistemaBackup.IsChecked = marcar;
            chkSistemaLogs.IsChecked = marcar;
        }

        private void BtnMarcarTodas_Click(object sender, RoutedEventArgs e)
        {
            MarcarTodasPermissoes(true);
        }

        private void BtnDesmarcarTodas_Click(object sender, RoutedEventArgs e)
        {
            MarcarTodasPermissoes(false);
        }

        private void BtnRestaurarPadrao_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                $"Deseja restaurar as permissões padrão do perfil ' '?\n\n" +
                "Todas as personalizações serão perdidas.",
                "Confirmar Restauração",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                CarregarPermissoesPerfil();
                MessageBox.Show("Permissões restauradas para o padrão do perfil!", "Sucesso",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnSalvarPermissoes_Click(object sender, RoutedEventArgs e)
        {
            // Salvar permissões
            SalvarPermissoes();

            MessageBox.Show(
                $"Permissões salvas com sucesso para o usuário ''!",
                "Sucesso",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            this.Close();
        }

        private void SalvarPermissoes()
        {
            permissoes = new Dictionary<string, bool>();

            // Produtos
            permissoes["produtos_visualizar"] = chkProdutosVisualizar.IsChecked ?? false;
            permissoes["produtos_cadastrar"] = chkProdutosCadastrar.IsChecked ?? false;
            permissoes["produtos_editar"] = chkProdutosEditar.IsChecked ?? false;
            permissoes["produtos_excluir"] = chkProdutosExcluir.IsChecked ?? false;
            permissoes["produtos_importar"] = chkProdutosImportar.IsChecked ?? false;

            // Estoque
            permissoes["estoque_visualizar"] = chkEstoqueVisualizar.IsChecked ?? false;
            permissoes["estoque_entrada"] = chkEstoqueEntrada.IsChecked ?? false;
            permissoes["estoque_saida"] = chkEstoqueSaida.IsChecked ?? false;
            permissoes["estoque_ajuste"] = chkEstoqueAjuste.IsChecked ?? false;
            permissoes["estoque_historico"] = chkEstoqueHistorico.IsChecked ?? false;

            // Vendas
            permissoes["vendas_visualizar"] = chkVendasVisualizar.IsChecked ?? false;
            permissoes["vendas_realizar"] = chkVendasRealizar.IsChecked ?? false;
            permissoes["vendas_cancelar"] = chkVendasCancelar.IsChecked ?? false;
            permissoes["vendas_desconto"] = chkVendasDesconto.IsChecked ?? false;
            permissoes["vendas_devolucao"] = chkVendasDevolucao.IsChecked ?? false;

            // Financeiro
            permissoes["financeiro_visualizar"] = chkFinanceiroVisualizar.IsChecked ?? false;
            permissoes["financeiro_contas"] = chkFinanceiroContas.IsChecked ?? false;
            permissoes["financeiro_caixa"] = chkFinanceiroCaixa.IsChecked ?? false;
            permissoes["financeiro_relatorios"] = chkFinanceiroRelatorios.IsChecked ?? false;

            // Clientes
            permissoes["clientes_visualizar"] = chkClientesVisualizar.IsChecked ?? false;
            permissoes["clientes_cadastrar"] = chkClientesCadastrar.IsChecked ?? false;
            permissoes["clientes_editar"] = chkClientesEditar.IsChecked ?? false;
            permissoes["clientes_excluir"] = chkClientesExcluir.IsChecked ?? false;

            // Relatórios
            permissoes["relatorios_vendas"] = chkRelatoriosVendas.IsChecked ?? false;
            permissoes["relatorios_estoque"] = chkRelatoriosEstoque.IsChecked ?? false;
            permissoes["relatorios_financeiro"] = chkRelatoriosFinanceiro.IsChecked ?? false;
            permissoes["relatorios_gerenciais"] = chkRelatoriosGerenciais.IsChecked ?? false;
            permissoes["relatorios_exportar"] = chkRelatoriosExportar.IsChecked ?? false;

            // Sistema
            permissoes["sistema_usuarios"] = chkSistemaUsuarios.IsChecked ?? false;
            permissoes["sistema_permissoes"] = chkSistemaPermissoes.IsChecked ?? false;
            permissoes["sistema_configuracoes"] = chkSistemaConfiguracoes.IsChecked ?? false;
            permissoes["sistema_backup"] = chkSistemaBackup.IsChecked ?? false;
            permissoes["sistema_logs"] = chkSistemaLogs.IsChecked ?? false;

            // Em produção, salvar no banco de dados
            // Exemplo: PermissoesDAO.Salvar(usuario.Id, permissoes);

            // Por enquanto, apenas exibe resumo
            int totalPermissoes = permissoes.Count;
            int permissoesAtivas = permissoes.Values.Count(p => p);

            System.Diagnostics.Debug.WriteLine($"Permissões salvas para :");
            System.Diagnostics.Debug.WriteLine($"Total: {permissoesAtivas}/{totalPermissoes} permissões ativas");
        }

        private void BtnFechar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
