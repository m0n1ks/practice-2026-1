using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FamilyBudget.Models;

namespace FamilyBudget.Pages
{
    public partial class GoalsPage : UserControl
    {
        private int _activeGoalId = -1;

        private static readonly List<string> Icons = new()
        { "✈️","🚗","🏠","🎓","🛡️","💻","📱","🎯","🌴","💍","🎸","🎨","⚽","🏋️","🍕","🌍" };

        public GoalsPage() { InitializeComponent(); }

        public void Refresh()
        {
            TxtTotalSaved.Text  = $"{AppData.TotalSaved:N0} ₽";
            int done   = AppData.Goals.Count(g => g.IsComplete);
            int active = AppData.Goals.Count(g => !g.IsComplete);
            TxtGoalsSub.Text      = $"{AppData.Goals.Count} целей · накоплено {AppData.TotalSaved:N0} ₽";
            TxtGoalsComplete.Text = $"✅ Выполнено: {done}";
            TxtGoalsActive.Text   = $"⏳ В процессе: {active}";

            GoalIconSelect.ItemsSource   = Icons;
            GoalIconSelect.SelectedIndex = 0;

            GoalsList.ItemsSource = null;
            GoalsList.ItemsSource = AppData.Goals.ToList();
            GoalsList.UpdateLayout();
            PatchGoalCards();
        }

        private void PatchGoalCards()
        {
            var goals = AppData.Goals.ToList();
            for (int i = 0; i < GoalsList.Items.Count; i++)
            {
                var cp = GoalsList.ItemContainerGenerator.ContainerFromIndex(i) as ContentPresenter;
                if (cp == null) continue;
                cp.ApplyTemplate();
                var goal = goals.ElementAtOrDefault(i);
                if (goal == null) continue;

                var curTxt  = Find<TextBlock>(cp, "CurAmtTxt");
                var tgtTxt  = Find<TextBlock>(cp, "TgtAmtTxt");
                var pctTxt  = Find<TextBlock>(cp, "ProgPctTxt");
                var progBar = Find<Border>(cp, "GoalProg");

                if (curTxt  != null) curTxt.Text  = $"{goal.CurrentAmount:N0} ₽";
                if (tgtTxt  != null) tgtTxt.Text  = $"{goal.TargetAmount:N0} ₽";
                if (pctTxt  != null) pctTxt.Text  = $"{goal.ProgressText}  ({goal.Remaining:N0} ₽ осталось)";

                if (progBar != null)
                {
                    var parent = VisualTreeHelper.GetParent(progBar) as Grid;
                    if (parent != null)
                    {
                        parent.UpdateLayout();
                        double w = parent.ActualWidth * goal.Progress;
                        progBar.Width = Math.Max(0, w);
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

        // ── Add Goal ────────────────────────────────────────────────────────
        private void AddGoalBtn_Click(object s, RoutedEventArgs e)
        {
            TxtGoalName.Text = ""; TxtGoalDesc.Text = ""; TxtGoalTarget.Text = "";
            GoalIconSelect.SelectedIndex = 0; GC1.IsChecked = true;
            AddGoalOverlay.Visibility = Visibility.Visible;
        }
        private void CloseGoalOverlay_Click(object s, RoutedEventArgs e) => AddGoalOverlay.Visibility = Visibility.Collapsed;

        private void ConfirmAddGoal_Click(object s, RoutedEventArgs e)
        {
            var name = TxtGoalName.Text.Trim();
            if (string.IsNullOrEmpty(name)) { MessageBox.Show("Введите название цели!"); return; }
            if (!decimal.TryParse(TxtGoalTarget.Text.Replace(",", "."), out decimal target) || target <= 0)
            { MessageBox.Show("Введите корректную сумму цели!"); return; }

            string color = GC2.IsChecked==true?"#7B1FA2": GC3.IsChecked==true?"#1565C0":
                           GC4.IsChecked==true?"#FF6B35": GC5.IsChecked==true?"#00796B":"#E91E8C";
            string icon  = GoalIconSelect.SelectedItem?.ToString() ?? "🎯";

            AppData.AddGoal(name, TxtGoalDesc.Text.Trim(), target, null, color, icon);
            AddGoalOverlay.Visibility = Visibility.Collapsed;
            Refresh();
        }

        private void DeleteGoal_Click(object s, RoutedEventArgs e)
        {
            if (s is Button btn && btn.Tag is int id)
            {
                var g = AppData.Goals.FirstOrDefault(x => x.Id == id);
                if (g != null && MessageBox.Show($"Удалить цель «{g.Name}»?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                { AppData.RemoveGoal(id); Refresh(); }
            }
        }

        // ── Add Money ────────────────────────────────────────────────────────
        private void AddMoney_Click(object s, RoutedEventArgs e)
        {
            if (s is Button btn && btn.Tag is int id)
            {
                _activeGoalId = id;
                var g = AppData.Goals.FirstOrDefault(x => x.Id == id);
                TxtAddMoneyTitle.Text = $"💰  Пополнить «{g?.Name}»";
                TxtAddAmount.Text = "";
                AddMoneyOverlay.Visibility = Visibility.Visible;
            }
        }
        private void CloseMoneyOverlay_Click(object s, RoutedEventArgs e) => AddMoneyOverlay.Visibility = Visibility.Collapsed;

        private void ConfirmAddMoney_Click(object s, RoutedEventArgs e)
        {
            if (!decimal.TryParse(TxtAddAmount.Text.Replace(",", "."), out decimal amt) || amt <= 0)
            { MessageBox.Show("Введите корректную сумму!"); return; }
            var goal = AppData.Goals.FirstOrDefault(x => x.Id == _activeGoalId);
            if (goal != null)
            {
                goal.CurrentAmount = Math.Min(goal.TargetAmount, goal.CurrentAmount + amt);
                if (goal.IsComplete) MessageBox.Show($"🎉 Цель «{goal.Name}» достигнута!", "Поздравляем!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            AddMoneyOverlay.Visibility = Visibility.Collapsed;
            Refresh();
        }

        // ── Tasks ────────────────────────────────────────────────────────────
        private void ViewTasks_Click(object s, RoutedEventArgs e)
        {
            if (s is Button btn && btn.Tag is int id)
            {
                _activeGoalId = id;
                var g = AppData.Goals.FirstOrDefault(x => x.Id == id);
                TxtTasksTitle.Text = $"📋  Задачи: {g?.Name}";
                RefreshTasks();
                TasksOverlay.Visibility = Visibility.Visible;
            }
        }
        private void CloseTasksOverlay_Click(object s, RoutedEventArgs e) => TasksOverlay.Visibility = Visibility.Collapsed;

        private void RefreshTasks()
        {
            TasksList.ItemsSource = null;
            TasksList.ItemsSource = AppData.GoalTasks.Where(t => t.GoalId == _activeGoalId).ToList();
        }

        private void AddTask_Click(object s, RoutedEventArgs e)
        {
            var title = TxtNewTask.Text.Trim();
            if (string.IsNullOrEmpty(title)) return;
            AppData.AddGoalTask(_activeGoalId, title);
            TxtNewTask.Text = "";
            RefreshTasks();
        }

        private void DeleteTask_Click(object s, RoutedEventArgs e)
        {
            if (s is Button btn && btn.Tag is int id)
            {
                var t = AppData.GoalTasks.FirstOrDefault(x => x.Id == id);
                if (t != null) { AppData.GoalTasks.Remove(t); RefreshTasks(); }
            }
        }
    }
}
