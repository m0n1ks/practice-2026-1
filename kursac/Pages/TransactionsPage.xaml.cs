using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FamilyBudget.Models;

namespace FamilyBudget.Pages
{
    public partial class TransactionsPage : UserControl
    {
        private string _filter = "All";
        private int    _memberFilter = -1;
        private bool   _isIncome = true;

        private static readonly List<(string Name, string Icon, string Cat)> Categories = new()
        {
            ("Зарплата",    "💼", "Зарплата"),    ("Подработка",   "💻", "Подработка"),
            ("Стипендия",   "🎓", "Стипендия"),   ("Продажи",      "🛍️","Продажи"),
            ("Карманные",   "🎁", "Карманные"),   ("Продукты",     "🛒", "Продукты"),
            ("Одежда",      "👗", "Одежда"),      ("Транспорт",    "🚌", "Транспорт"),
            ("Здоровье",    "💊", "Здоровье"),    ("Коммунальные", "🏠", "Коммунальные"),
            ("Ресторан",    "🍽️","Ресторан"),    ("Развлечения",  "🎮", "Развлечения"),
            ("Бензин",      "⛽", "Бензин"),      ("Учёба",        "📚", "Учёба"),
            ("Хобби",       "🎨", "Хобби"),       ("Другое",       "💰", "Другое"),
        };

        public TransactionsPage() { InitializeComponent(); }

        // Вспомогательный класс для ComboBox (анонимные типы не работают с DisplayMemberPath)
        private class MemberItem
        {
            public int    Id   { get; set; }
            public string Name { get; set; } = "";
            public override string ToString() => Name;
        }

        public void Refresh()
        {
            // Инициализация стиля фильтров
            int activeIdx = _filter == "Income" ? 1 : _filter == "Expense" ? 2 : 0;
            FilterAll.Style     = (Style)FindResource(activeIdx == 0 ? "PrimaryBtn" : "FlatBtn");
            FilterIncome.Style  = (Style)FindResource(activeIdx == 1 ? "PrimaryBtn" : "FlatBtn");
            FilterExpense.Style = (Style)FindResource(activeIdx == 2 ? "PrimaryBtn" : "FlatBtn");
            SetFilterText(FilterAll,     "Все",     activeIdx == 0);
            SetFilterText(FilterIncome,  "Доходы",  activeIdx == 1);
            SetFilterText(FilterExpense, "Расходы", activeIdx == 2);

            // Member filter combobox
            var members = new List<MemberItem> { new MemberItem { Id = -1, Name = "Все члены семьи" } };
            members.AddRange(AppData.Members.Select(m => new MemberItem { Id = m.Id, Name = m.Name }));
            MemberFilter.DisplayMemberPath = "Name";
            MemberFilter.SelectedValuePath = "Id";
            MemberFilter.ItemsSource = members;
            MemberFilter.SelectedIndex = 0;

            // Member select in form
            var formMembers = AppData.Members.Select(m => new MemberItem { Id = m.Id, Name = m.Name }).ToList();
            MemberSelect.DisplayMemberPath = "Name";
            MemberSelect.SelectedValuePath = "Id";
            MemberSelect.ItemsSource = formMembers;
            if (MemberSelect.Items.Count > 0) MemberSelect.SelectedIndex = 0;

            // Category select
            CategorySelect.DisplayMemberPath = "Name";
            CategorySelect.ItemsSource = Categories;
            if (CategorySelect.Items.Count > 0) CategorySelect.SelectedIndex = 0;

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var q = AppData.Transactions.AsEnumerable();
            if (_filter == "Income")  q = q.Where(t => t.Type == TransactionType.Income);
            if (_filter == "Expense") q = q.Where(t => t.Type == TransactionType.Expense);
            if (_memberFilter > 0)    q = q.Where(t => t.MemberId == _memberFilter);
            var list = q.OrderByDescending(t => t.Date).ToList();
            TransList.ItemsSource = null;
            TransList.ItemsSource = list;
            TxtEmpty.Visibility   = list.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            var income   = q.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            var expenses = q.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
            TxtTransSubtitle.Text = $"Доходы: {income:N0} ₽  •  Расходы: {expenses:N0} ₽";
        }

        // Filters
        private void FilterAll_Click(object s, RoutedEventArgs e)     { _filter = "All";     ApplyFilterAndStyle(0); }
        private void FilterIncome_Click(object s, RoutedEventArgs e)  { _filter = "Income";  ApplyFilterAndStyle(1); }
        private void FilterExpense_Click(object s, RoutedEventArgs e) { _filter = "Expense"; ApplyFilterAndStyle(2); }

        private void ApplyFilterAndStyle(int active)
        {
            FilterAll.Style     = (Style)FindResource(active == 0 ? "PrimaryBtn" : "FlatBtn");
            FilterIncome.Style  = (Style)FindResource(active == 1 ? "PrimaryBtn" : "FlatBtn");
            FilterExpense.Style = (Style)FindResource(active == 2 ? "PrimaryBtn" : "FlatBtn");
            SetFilterText(FilterAll,     "Все",     active == 0);
            SetFilterText(FilterIncome,  "Доходы",  active == 1);
            SetFilterText(FilterExpense, "Расходы", active == 2);
            ApplyFilter();
        }

        private void SetFilterText(Button btn, string text, bool isActive)
        {
            if (isActive)
                btn.Content = text;
            else
                btn.Content = new TextBlock
                {
                    Text       = text,
                    Foreground = (System.Windows.Media.Brush)FindResource("TextMutedBrush"),
                    FontSize   = 12,
                    FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                    FontWeight = FontWeights.SemiBold
                };
        }

        private void MemberFilter_Changed(object s, SelectionChangedEventArgs e)
        {
            if (MemberFilter.SelectedItem is MemberItem item)
            {
                _memberFilter = item.Id;
                ApplyFilter();
            }
        }

        // Add transaction
        private void AddTransBtn_Click(object s, RoutedEventArgs e)
        {
            TxtAmount.Text = ""; TxtDesc.Text = "";
            _isIncome = true;
            TypeIncomeBtn.Style  = (Style)FindResource("GreenBtn");
            TypeExpenseBtn.Style = (Style)FindResource("FlatBtn");
            if (MemberSelect.Items.Count > 0) MemberSelect.SelectedIndex = 0;
            CategorySelect.SelectedIndex = 0;
            AddTransOverlay.Visibility = Visibility.Visible;
        }

        private void CloseTransOverlay_Click(object s, RoutedEventArgs e)
            => AddTransOverlay.Visibility = Visibility.Collapsed;

        private void SetTypeIncome_Click(object s, RoutedEventArgs e)
        {
            _isIncome = true;
            TypeIncomeBtn.Style  = (Style)FindResource("GreenBtn");
            TypeExpenseBtn.Style = (Style)FindResource("FlatBtn");
        }

        private void SetTypeExpense_Click(object s, RoutedEventArgs e)
        {
            _isIncome = false;
            TypeExpenseBtn.Style = (Style)FindResource("DangerBtn");
            TypeIncomeBtn.Style  = (Style)FindResource("FlatBtn");
        }

        private void ConfirmAddTrans_Click(object s, RoutedEventArgs e)
        {
            if (!decimal.TryParse(TxtAmount.Text.Replace(",", "."), out decimal amount) || amount <= 0)
            { MessageBox.Show("Введите корректную сумму!"); return; }
            var desc = TxtDesc.Text.Trim();
            if (string.IsNullOrEmpty(desc)) { MessageBox.Show("Введите описание!"); return; }

            if (MemberSelect.SelectedItem is not MemberItem memberItem)
            { MessageBox.Show("Выберите члена семьи!"); return; }
            var member = AppData.Members.FirstOrDefault(m => m.Id == memberItem.Id);
            if (member == null) return;

            var catItem = Categories[CategorySelect.SelectedIndex];
            AppData.AddTransaction(member.Id, member.Name, member.AvatarColor, amount,
                _isIncome ? TransactionType.Income : TransactionType.Expense,
                catItem.Cat, catItem.Icon, desc, DateTime.Now);

            AddTransOverlay.Visibility = Visibility.Collapsed;
            Refresh();
        }

        private void DeleteTransaction_Click(object s, RoutedEventArgs e)
        {
            if (s is Button btn && btn.Tag is int id)
            {
                AppData.RemoveTransaction(id);
                ApplyFilter();
            }
        }
    }
}
