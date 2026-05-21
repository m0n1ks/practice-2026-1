using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FamilyBudget.Models;

namespace FamilyBudget.Pages
{
    public partial class AnalyticsPage : UserControl
    {
        public AnalyticsPage() { InitializeComponent(); }

        public void Refresh()
        {
            var income   = AppData.TotalIncome;
            var expenses = AppData.TotalExpenses;
            var balance  = AppData.TotalBalance;

            TxtAnalSubtitle.Text = $"Всего операций: {AppData.Transactions.Count}  •  Членов семьи: {AppData.Members.Count}";
            TxtIncomeVal.Text    = $"{income:N0} ₽";
            TxtExpenseVal.Text   = $"{expenses:N0} ₽";
            TxtNetBalance.Text   = $"{balance:N0} ₽";
            TxtNetBalance.Foreground = balance >= 0
                ? new SolidColorBrush(Color.FromRgb(0x66, 0xBB, 0x6A))
                : new SolidColorBrush(Color.FromRgb(0xEF, 0x53, 0x50));

            // Draw income/expense bars after layout
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
            {
                DrawMainBars(income, expenses);
                DrawCategoryBars();
                DrawGoalsProgress();
            }));

            // Member contribution
            var memberItems = AppData.Members.Select(m =>
            {
                decimal inc = AppData.GetMemberIncome(m.Id);
                decimal exp = AppData.GetMemberExpenses(m.Id);
                decimal bal = inc - exp;
                var balBrush = bal >= 0
                    ? new SolidColorBrush(Color.FromRgb(0x66, 0xBB, 0x6A))
                    : new SolidColorBrush(Color.FromRgb(0xEF, 0x53, 0x50));
                SolidColorBrush avatarBrush;
                try { avatarBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(m.AvatarColor)); }
                catch { avatarBrush = new SolidColorBrush(Colors.HotPink); }
                return new
                {
                    m.Name, m.Initials,
                    AvatarColorBrush = avatarBrush,
                    IncomeText  = $"+{inc:N0} ₽",
                    ExpenseText = $"-{exp:N0} ₽",
                    BalanceText = $"{bal:N0} ₽",
                    BalanceBrush = balBrush
                };
            }).ToList();
            MemberContrib.ItemsSource = null;
            MemberContrib.ItemsSource = memberItems;

            // Category breakdown
            var cats = AppData.Transactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => new { t.Category, t.CategoryIcon })
                .Select(g => new { g.Key.Category, Icon = g.Key.CategoryIcon, g.Key.Category.Length,
                    Name = g.Key.Category, Raw = g.Sum(t => t.Amount),
                    Amount = $"{g.Sum(t => t.Amount):N0} ₽" })
                .OrderByDescending(x => x.Raw)
                .Take(8)
                .ToList();
            CategoryBreakdown.ItemsSource = null;
            CategoryBreakdown.ItemsSource = cats;

            // Goals progress
            GoalsProgress.ItemsSource = null;
            GoalsProgress.ItemsSource = AppData.Goals.ToList();

            CategoryBreakdown.UpdateLayout();
            GoalsProgress.UpdateLayout();
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
            {
                PatchCategoryBars(cats.Select(x => x.Raw).ToList());
                PatchGoalProgressBars();
            }));
        }

        private void DrawMainBars(decimal income, decimal expenses)
        {
            double maxVal = (double)Math.Max(income, expenses);
            if (maxVal <= 0) return;

            double barMaxWidth = IncomeBar.Parent is Grid g ? g.ActualWidth : 200;
            IncomeBar.Width  = barMaxWidth * ((double)income  / maxVal);
            ExpenseBar.Width = barMaxWidth * ((double)expenses / maxVal);
        }

        private void DrawCategoryBars() { /* patched after layout */ }

        private void PatchCategoryBars(List<decimal> amounts)
        {
            decimal maxAmt = amounts.Count > 0 ? amounts.Max() : 1;
            for (int i = 0; i < CategoryBreakdown.Items.Count; i++)
            {
                var cp = CategoryBreakdown.ItemContainerGenerator.ContainerFromIndex(i) as ContentPresenter;
                if (cp == null) continue;
                cp.ApplyTemplate();
                var bar = Find<Border>(cp, "CatBar");
                if (bar != null)
                {
                    var parent = VisualTreeHelper.GetParent(bar) as Grid;
                    if (parent != null)
                    {
                        parent.UpdateLayout();
                        double ratio = maxAmt > 0 ? (double)(amounts.ElementAtOrDefault(i) / maxAmt) : 0;
                        bar.Width = parent.ActualWidth * ratio;
                    }
                }
            }
        }

        private void DrawGoalsProgress() { /* patched after layout */ }

        private void PatchGoalProgressBars()
        {
            var goals = AppData.Goals.ToList();
            for (int i = 0; i < GoalsProgress.Items.Count; i++)
            {
                var cp = GoalsProgress.ItemContainerGenerator.ContainerFromIndex(i) as ContentPresenter;
                if (cp == null) continue;
                cp.ApplyTemplate();
                var bar = Find<Border>(cp, "GPBar");
                if (bar != null && i < goals.Count)
                {
                    var parent = VisualTreeHelper.GetParent(bar) as Grid;
                    if (parent != null)
                    {
                        parent.UpdateLayout();
                        bar.Width = parent.ActualWidth * goals[i].Progress;
                        bar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(goals[i].Color));
                        bar.Effect = new System.Windows.Media.Effects.DropShadowEffect
                        {
                            Color = (Color)ColorConverter.ConvertFromString(goals[i].Color),
                            BlurRadius = 6, ShadowDepth = 0, Opacity = 0.5
                        };
                    }
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
    }
}
