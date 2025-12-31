using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace SmartSystemPDV.Controls
{
    public partial class SearchableComboBox : UserControl
    {
        // Propriedades de Dependência
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register("Placeholder", typeof(string), typeof(SearchableComboBox),
                new PropertyMetadata("Digite para buscar...", OnPlaceholderChanged));

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable<object>), typeof(SearchableComboBox),
                new PropertyMetadata(null, OnItemsSourceChanged));

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(SearchableComboBox),
                new PropertyMetadata(null, OnSelectedItemChanged));

        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register("DisplayMemberPath", typeof(string), typeof(SearchableComboBox),
                new PropertyMetadata("Nome"));

        // Propriedades Públicas
        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        public IEnumerable<object> ItemsSource
        {
            get => (IEnumerable<object>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public string DisplayMemberPath
        {
            get => (string)GetValue(DisplayMemberPathProperty);
            set => SetValue(DisplayMemberPathProperty, value);
        }

        // Eventos
        public event SelectionChangedEventHandler SelectionChanged;
        public event TextChangedEventHandler SearchTextChanged;

        // Campos privados
        private List<object> _allItems;
        private DispatcherTimer _searchTimer;
        private bool _isUpdating;
        private bool _isSelectingItem; // Flag para controlar seleção

        public SearchableComboBox()
        {
            InitializeComponent();
            InitializeSearchTimer();
            UpdatePlaceholder();

            // Adicionar handler para fechar popup ao clicar fora
            this.Loaded += (s, e) =>
            {
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    window.PreviewMouseDown += Window_PreviewMouseDown;
                }
            };

            this.Unloaded += (s, e) =>
            {
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    window.PreviewMouseDown -= Window_PreviewMouseDown;
                }
            };
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (popupResults.IsOpen && !_isSelectingItem)
            {
                var source = e.OriginalSource as DependencyObject;

                // Verificar se o clique foi dentro do controle
                while (source != null)
                {
                    if (source == this || source == popupResults.Child)
                        return;

                    source = VisualTreeHelper.GetParent(source);
                }

                // Clique foi fora - fechar popup
                popupResults.IsOpen = false;
                System.Diagnostics.Debug.WriteLine("Popup fechado - clique fora");
            }
        }

        private void InitializeSearchTimer()
        {
            _searchTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            _searchTimer.Tick += SearchTimer_Tick;
        }

        private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SearchableComboBox control)
            {
                control.UpdatePlaceholder();
            }
        }

        private void UpdatePlaceholder()
        {
            if (txtSearch != null)
            {
                txtSearch.Tag = Placeholder;
            }
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SearchableComboBox control)
            {
                control._allItems = e.NewValue as List<object> ??
                                   (e.NewValue as IEnumerable<object>)?.ToList() ??
                                   new List<object>();

                control.ConfigureItemTemplate();
                control.UpdateResults(control._allItems);

                System.Diagnostics.Debug.WriteLine($"[SearchableComboBox] ItemsSource atualizado: {control._allItems.Count} itens");
            }
        }

        private void ConfigureItemTemplate()
        {
            if (_allItems == null || _allItems.Count == 0)
            {
                listResults.ItemTemplate = CreateGenericTemplate();
                return;
            }

            var firstItem = _allItems.First();
            var itemType = firstItem.GetType();

            System.Diagnostics.Debug.WriteLine($"[SearchableComboBox] Tipo detectado: {itemType.Name}");

            var properties = itemType.GetProperties();
            bool hasPrecoVenda = properties.Any(p => p.Name == "PrecoVenda");
            bool hasCodigo = properties.Any(p => p.Name == "Codigo");

            if (hasPrecoVenda && hasCodigo)
            {
                listResults.ItemTemplate = CreateProdutoTemplate();
                System.Diagnostics.Debug.WriteLine("[SearchableComboBox] Template Produto aplicado");
            }
            else
            {
                listResults.ItemTemplate = CreateGenericTemplate();
                System.Diagnostics.Debug.WriteLine("[SearchableComboBox] Template Genérico aplicado");
            }
        }

        private DataTemplate CreateProdutoTemplate()
        {
            var template = new DataTemplate();

            var gridFactory = new FrameworkElementFactory(typeof(Grid));

            var col1 = new FrameworkElementFactory(typeof(ColumnDefinition));
            col1.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Auto));
            var col2 = new FrameworkElementFactory(typeof(ColumnDefinition));
            col2.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
            var col3 = new FrameworkElementFactory(typeof(ColumnDefinition));
            col3.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Auto));

            gridFactory.AppendChild(col1);
            gridFactory.AppendChild(col2);
            gridFactory.AppendChild(col3);

            var txtCodigo = new FrameworkElementFactory(typeof(TextBlock));
            txtCodigo.SetBinding(TextBlock.TextProperty, new Binding("Codigo"));
            txtCodigo.SetValue(Grid.ColumnProperty, 0);
            txtCodigo.SetValue(TextBlock.FontWeightProperty, FontWeights.SemiBold);
            txtCodigo.SetValue(TextBlock.FontSizeProperty, 12.0);
            txtCodigo.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush(Color.FromRgb(102, 102, 102)));
            txtCodigo.SetValue(TextBlock.MarginProperty, new Thickness(0, 0, 10, 0));
            txtCodigo.SetValue(TextBlock.MinWidthProperty, 60.0);
            gridFactory.AppendChild(txtCodigo);

            var txtNome = new FrameworkElementFactory(typeof(TextBlock));
            txtNome.SetBinding(TextBlock.TextProperty, new Binding(DisplayMemberPath));
            txtNome.SetValue(Grid.ColumnProperty, 1);
            txtNome.SetValue(TextBlock.FontSizeProperty, 13.0);
            txtNome.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis);
            gridFactory.AppendChild(txtNome);

            var txtPreco = new FrameworkElementFactory(typeof(TextBlock));
            var precoBinding = new Binding("PrecoVenda") { StringFormat = "R$ {0:F2}" };
            txtPreco.SetBinding(TextBlock.TextProperty, precoBinding);
            txtPreco.SetValue(Grid.ColumnProperty, 2);
            txtPreco.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
            txtPreco.SetValue(TextBlock.FontSizeProperty, 13.0);
            txtPreco.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush(Color.FromRgb(40, 167, 69)));
            txtPreco.SetValue(TextBlock.MarginProperty, new Thickness(10, 0, 0, 0));
            gridFactory.AppendChild(txtPreco);

            template.VisualTree = gridFactory;
            return template;
        }

        private DataTemplate CreateGenericTemplate()
        {
            var template = new DataTemplate();

            var txtBlock = new FrameworkElementFactory(typeof(TextBlock));
            txtBlock.SetBinding(TextBlock.TextProperty, new Binding(DisplayMemberPath));
            txtBlock.SetValue(TextBlock.FontSizeProperty, 13.0);
            txtBlock.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis);

            template.VisualTree = txtBlock;
            return template;
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SearchableComboBox control && !control._isUpdating)
            {
                control.UpdateSelectedDisplay();
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchTimer.Stop();
            _searchTimer.Start();

            SearchTextChanged?.Invoke(this, e);
        }

        private void SearchTimer_Tick(object sender, EventArgs e)
        {
            _searchTimer.Stop();
            PerformSearch();
        }

        private void PerformSearch()
        {
            if (_allItems == null || _allItems.Count == 0)
            {
                ShowNoResults();
                return;
            }

            string searchText = txtSearch.Text?.Trim().ToLower() ?? "";

            if (string.IsNullOrEmpty(searchText))
            {
                UpdateResults(_allItems);
                return;
            }

            ShowLoading();

            Task.Run(() =>
            {
                try
                {
                    var filtered = _allItems.Where(item =>
                    {
                        if (item == null) return false;

                        var properties = item.GetType().GetProperties();
                        foreach (var prop in properties)
                        {
                            try
                            {
                                var value = prop.GetValue(item)?.ToString()?.ToLower();
                                if (value != null && value.Contains(searchText))
                                    return true;
                            }
                            catch { }
                        }
                        return false;
                    }).ToList();

                    Dispatcher.Invoke(() =>
                    {
                        HideLoading();
                        UpdateResults(filtered);
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[SearchableComboBox] Erro na busca: {ex.Message}");
                    Dispatcher.Invoke(() =>
                    {
                        HideLoading();
                        ShowNoResults();
                    });
                }
            });
        }

        private void UpdateResults(List<object> items)
        {
            if (listResults != null)
            {
                listResults.ItemsSource = items;

                if (items == null || items.Count == 0)
                {
                    ShowNoResults();
                }
                else
                {
                    txtNoResults.Visibility = Visibility.Collapsed;
                    listResults.Visibility = Visibility.Visible;
                    pnlLoading.Visibility = Visibility.Collapsed;
                }

                System.Diagnostics.Debug.WriteLine($"[SearchableComboBox] Resultados atualizados: {items?.Count ?? 0} itens");
            }
        }

        private void ShowNoResults()
        {
            if (listResults != null && txtNoResults != null)
            {
                listResults.Visibility = Visibility.Collapsed;
                txtNoResults.Visibility = Visibility.Visible;
                pnlLoading.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowLoading()
        {
            if (listResults != null && pnlLoading != null)
            {
                listResults.Visibility = Visibility.Collapsed;
                txtNoResults.Visibility = Visibility.Collapsed;
                pnlLoading.Visibility = Visibility.Visible;
            }
        }

        private void HideLoading()
        {
            if (pnlLoading != null)
            {
                pnlLoading.Visibility = Visibility.Collapsed;
            }
        }

        private void TxtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[SearchableComboBox] TxtSearch_GotFocus");

            if (SelectedItem == null && _allItems != null && _allItems.Count > 0)
            {
                popupResults.IsOpen = true;
                UpdateResults(_allItems);
                System.Diagnostics.Debug.WriteLine($"[SearchableComboBox] Popup aberto com {_allItems.Count} itens");
            }
        }

        private void TxtSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            // NÃO fechar automaticamente - deixar o StaysOpen=True e Window_PreviewMouseDown gerenciar
            System.Diagnostics.Debug.WriteLine("[SearchableComboBox] TxtSearch_LostFocus (não fechando popup)");
        }

        private void TxtSearch_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down:
                    if (popupResults.IsOpen && listResults.Items.Count > 0)
                    {
                        listResults.Focus();
                        listResults.SelectedIndex = 0;
                        var item = listResults.ItemContainerGenerator.ContainerFromIndex(0) as ListBoxItem;
                        item?.Focus();
                        e.Handled = true;
                    }
                    else if (!popupResults.IsOpen)
                    {
                        popupResults.IsOpen = true;
                        UpdateResults(_allItems);
                    }
                    break;

                case Key.Up:
                    if (popupResults.IsOpen && listResults.Items.Count > 0)
                    {
                        listResults.Focus();
                        listResults.SelectedIndex = listResults.Items.Count - 1;
                        var item = listResults.ItemContainerGenerator.ContainerFromIndex(listResults.Items.Count - 1) as ListBoxItem;
                        item?.Focus();
                        e.Handled = true;
                    }
                    break;

                case Key.Escape:
                    popupResults.IsOpen = false;
                    e.Handled = true;
                    break;

                case Key.Enter:
                    if (listResults.SelectedItem != null)
                    {
                        SelectItem(listResults.SelectedItem);
                        e.Handled = true;
                    }
                    break;
            }
        }

        private void ListResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Não fazer nada aqui
        }

        private void ListResults_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isSelectingItem = true;
            System.Diagnostics.Debug.WriteLine("[SearchableComboBox] PreviewMouseDown na lista");
        }

        private void ListResults_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[SearchableComboBox] MouseLeftButtonUp na lista");

            if (listResults.SelectedItem != null)
            {
                SelectItem(listResults.SelectedItem);
            }

            _isSelectingItem = false;
        }

        private void PopupBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Prevenir que o clique na borda feche o popup
            e.Handled = true;
        }

        private void SelectItem(object item)
        {
            System.Diagnostics.Debug.WriteLine($"[SearchableComboBox] Item selecionado: {GetDisplayValue(item)}");

            _isUpdating = true;
            SelectedItem = item;
            _isUpdating = false;

            UpdateSelectedDisplay();
            popupResults.IsOpen = false;

            SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(
                Selector.SelectionChangedEvent,
                new List<object>(),
                new List<object> { item }));
        }

        private void UpdateSelectedDisplay()
        {
            if (SelectedItem != null)
            {
                var displayValue = GetDisplayValue(SelectedItem);
                txtSelectedItem.Text = displayValue;
                borderSelected.Visibility = Visibility.Visible;
                txtSearch.Visibility = Visibility.Collapsed;

                System.Diagnostics.Debug.WriteLine($"[SearchableComboBox] Item exibido: {displayValue}");
            }
            else
            {
                borderSelected.Visibility = Visibility.Collapsed;
                txtSearch.Visibility = Visibility.Visible;
                txtSearch.Clear();
            }
        }

        private string GetDisplayValue(object item)
        {
            if (item == null) return string.Empty;

            if (!string.IsNullOrEmpty(DisplayMemberPath))
            {
                try
                {
                    var prop = item.GetType().GetProperty(DisplayMemberPath);
                    if (prop != null)
                    {
                        return prop.GetValue(item)?.ToString() ?? string.Empty;
                    }
                }
                catch { }
            }

            return item.ToString();
        }

        private void BtnClearSelection_Click(object sender, RoutedEventArgs e)
        {
            ClearSelection();
        }

        public void ClearSelection()
        {
            System.Diagnostics.Debug.WriteLine("[SearchableComboBox] ClearSelection chamado");

            _isUpdating = true;
            SelectedItem = null;
            _isUpdating = false;
            UpdateSelectedDisplay();
            txtSearch.Focus();
        }

        public new void Focus()
        {
            if (SelectedItem == null)
            {
                txtSearch?.Focus();
            }
        }
    }
}