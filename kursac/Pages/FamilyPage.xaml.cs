using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FamilyBudget.Models;

namespace FamilyBudget.Pages
{
    public partial class FamilyPage : UserControl
    {
        public FamilyPage() { InitializeComponent(); }

        public void Refresh()
        {
            TxtFamilySubtitle.Text = $"{AppData.Members.Count} членов семьи";
            var items = AppData.Members.Select(m => new
            {
                m.Id, m.Name, m.Role, m.AvatarColor, m.Initials,
                Income   = AppData.GetMemberIncome(m.Id),
                Expenses = AppData.GetMemberExpenses(m.Id),
                Balance  = AppData.GetMemberBalance(m.Id)
            }).ToList();
            MembersList.ItemsSource = null;
            MembersList.ItemsSource = items;
            MembersList.UpdateLayout();
            PatchStats();
        }

        private void PatchStats()
        {
            for (int i = 0; i < MembersList.Items.Count; i++)
            {
                var cp = MembersList.ItemContainerGenerator.ContainerFromIndex(i) as ContentPresenter;
                if (cp == null) continue;
                cp.ApplyTemplate();
                var item = MembersList.Items[i];
                var t = item.GetType();
                decimal inc = (decimal)(t.GetProperty("Income")?.GetValue(item) ?? 0m);
                decimal exp = (decimal)(t.GetProperty("Expenses")?.GetValue(item) ?? 0m);
                decimal bal = (decimal)(t.GetProperty("Balance")?.GetValue(item) ?? 0m);

                var incTxt = Find<TextBlock>(cp, "IncTxt");
                var expTxt = Find<TextBlock>(cp, "ExpTxt");
                var balTxt = Find<TextBlock>(cp, "BalTxt");
                if (incTxt != null) incTxt.Text = $"{inc:N0} ₽";
                if (expTxt != null) expTxt.Text = $"{exp:N0} ₽";
                if (balTxt != null)
                {
                    balTxt.Text = $"{bal:N0} ₽";
                    balTxt.Foreground = bal >= 0
                        ? new SolidColorBrush(Color.FromRgb(0x66,0xBB,0x6A))
                        : new SolidColorBrush(Color.FromRgb(0xEF,0x53,0x50));
                }
            }
        }

        private static T? Find<T>(DependencyObject p, string name) where T : FrameworkElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(p); i++)
            {
                var c = VisualTreeHelper.GetChild(p, i);
                if (c is T fe && fe.Name == name) return fe;
                var r = Find<T>(c, name);
                if (r != null) return r;
            }
            return null;
        }

        private void AddMemberBtn_Click(object s, RoutedEventArgs e)
        {
            TxtNewName.Text = ""; TxtNewRole.Text = "";
            AddMemberOverlay.Visibility = Visibility.Visible;
        }
        private void CloseAddOverlay_Click(object s, RoutedEventArgs e)
            => AddMemberOverlay.Visibility = Visibility.Collapsed;

        private void ConfirmAddMember_Click(object s, RoutedEventArgs e)
        {
            var name = TxtNewName.Text.Trim();
            var role = TxtNewRole.Text.Trim();
            if (string.IsNullOrEmpty(name)) { MessageBox.Show("Введите имя!"); return; }
            if (string.IsNullOrEmpty(role)) role = "Член семьи";

            string color = "#E91E8C";
            if (C2.IsChecked == true) color = "#7B1FA2";
            else if (C3.IsChecked == true) color = "#1565C0";
            else if (C4.IsChecked == true) color = "#00796B";
            else if (C5.IsChecked == true) color = "#E65100";
            else if (C6.IsChecked == true) color = "#558B2F";

            AppData.AddMember(name, role, color, "👤");
            AddMemberOverlay.Visibility = Visibility.Collapsed;
            Refresh();
        }

        private void DeleteMember_Click(object s, RoutedEventArgs e)
        {
            if (s is Button btn && btn.Tag is int id)
            {
                var m = AppData.Members.FirstOrDefault(x => x.Id == id);
                if (m != null && MessageBox.Show($"Удалить {m.Name} из семьи?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    AppData.RemoveMember(id);
                    Refresh();
                }
            }
        }
    }
}
