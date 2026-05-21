using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using FamilyBudget.Models;

namespace FamilyBudget.Pages
{
    public partial class DashboardPage : UserControl
    {
        public DashboardPage() { InitializeComponent(); }

        public void Refresh()
        {
            var bal = AppData.TotalBalance;
            TxtBalance.Text   = $"{bal:N0} ₽";
            TxtBalance.Foreground = bal >= 0
                ? new SolidColorBrush(Colors.White)
                : new SolidColorBrush(Color.FromRgb(0xFF, 0xAA, 0xAA));

            TxtIncome.Text   = $"+{AppData.TotalIncome:N0} ₽";
            TxtExpenses.Text = $"-{AppData.TotalExpenses:N0} ₽";
            TxtMembers.Text  = AppData.Members.Count.ToString();
            TxtGoalsCount.Text = AppData.Goals.Count.ToString();
            TxtSaved.Text    = $"{AppData.TotalSaved:N0} ₽";

            // Members overview with computed balances
            var memberItems = AppData.Members.Select(m => new
            {
                m.Name, m.Role, m.AvatarColor, m.Initials,
                Balance = AppData.GetMemberBalance(m.Id)
            }).ToList();

            MembersOverview.ItemsSource = null;
            MembersOverview.ItemsSource = memberItems;

            // Patch member balance text after items render
            MembersOverview.UpdateLayout();
            PatchMemberBalances();

            // Recent transactions (last 8, newest first)
            var recent = AppData.Transactions
                .OrderByDescending(t => t.Date)
                .Take(8)
                .ToList();
            RecentTransactions.ItemsSource = null;
            RecentTransactions.ItemsSource = recent;

            // Goals summary
            var goals = AppData.Goals.Take(4).ToList();
            GoalsSummary.ItemsSource = null;
            GoalsSummary.ItemsSource = goals;

            // Patch progress bars after layout
            GoalsSummary.UpdateLayout();
            PatchGoalProgressBars();
        }

        private void PatchMemberBalances()
        {
            // Walk visual tree to set balance text colors
            for (int i = 0; i < MembersOverview.Items.Count; i++)
            {
                var container = MembersOverview.ItemContainerGenerator.ContainerFromIndex(i) as ContentPresenter;
                if (container == null) continue;
                container.ApplyTemplate();
                var txt = FindVisualChild<TextBlock>(container, "BalTxt");
                if (txt == null) continue;

                var item = MembersOverview.Items[i];
                var balProp = item.GetType().GetProperty("Balance");
                if (balProp?.GetValue(item) is decimal bal)
                {
                    txt.Text       = $"{bal:N0} ₽";
                    txt.Foreground = bal >= 0
                        ? new SolidColorBrush(Color.FromRgb(0x66, 0xBB, 0x6A))
                        : new SolidColorBrush(Color.FromRgb(0xEF, 0x53, 0x50));
                }
            }
        }

        private void PatchGoalProgressBars()
        {
            for (int i = 0; i < GoalsSummary.Items.Count; i++)
            {
                var container = GoalsSummary.ItemContainerGenerator.ContainerFromIndex(i) as ContentPresenter;
                if (container == null) continue;
                container.ApplyTemplate();

                var goal = AppData.Goals.ElementAtOrDefault(i);
                if (goal == null) continue;

                // Find the progress border
                var progBar = FindVisualChild<Border>(container, "ProgBar");
                if (progBar != null)
                {
                    var parent = VisualTreeHelper.GetParent(progBar) as Grid;
                    if (parent != null)
                    {
                        parent.UpdateLayout();
                        double totalWidth = parent.ActualWidth;
                        progBar.Width = totalWidth * goal.Progress;
                        progBar.Background = new SolidColorBrush(
                            (Color)System.Windows.Media.ColorConverter.ConvertFromString(goal.Color));
                    }
                }
            }
        }

        private static T? FindVisualChild<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T fe && fe.Name == name) return fe;
                var result = FindVisualChild<T>(child, name);
                if (result != null) return result;
            }
            return null;
        }
    }
}
