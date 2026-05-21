using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace FamilyBudget.Models
{
    // ─────────────────────────────── FamilyMember ────────────────────────────
    public class FamilyMember : INotifyPropertyChanged
    {
        private string _name = "";
        private string _role = "";
        private string _avatarColor = "#E91E8C";
        private string _emoji = "👤";

        public int    Id          { get; set; }
        public string Name        { get => _name;        set { _name = value;        PC(nameof(Name)); PC(nameof(Initials)); } }
        public string Role        { get => _role;        set { _role = value;        PC(nameof(Role)); } }
        public string AvatarColor { get => _avatarColor; set { _avatarColor = value; PC(nameof(AvatarColor)); } }
        public string Emoji       { get => _emoji;       set { _emoji = value;       PC(nameof(Emoji)); } }

        public string Initials => Name.Length > 0 ? Name[0].ToString().ToUpper() : "?";

        public event PropertyChangedEventHandler? PropertyChanged;
        void PC(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    // ─────────────────────────────── Transaction ─────────────────────────────
    public enum TransactionType { Income, Expense }

    public class Transaction : INotifyPropertyChanged
    {
        public int             Id          { get; set; }
        public int             MemberId    { get; set; }
        public string          MemberName  { get; set; } = "";
        public string          MemberColor { get; set; } = "#E91E8C";
        public decimal         Amount      { get; set; }
        public TransactionType Type        { get; set; }
        public string          Category    { get; set; } = "";
        public string          CategoryIcon{ get; set; } = "💰";
        public string          Description { get; set; } = "";
        public DateTime        Date        { get; set; }

        public string FormattedAmount  => Type == TransactionType.Income
            ? $"+{Amount:N0} ₽" : $"-{Amount:N0} ₽";
        public string FormattedDate    => Date.ToString("dd MMM");
        public string AmountColor      => Type == TransactionType.Income ? "#66BB6A" : "#EF5350";

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    // ─────────────────────────────── SavingsGoal ─────────────────────────────
    public class SavingsGoal : INotifyPropertyChanged
    {
        private decimal _currentAmount;

        public int     Id            { get; set; }
        public string  Name          { get; set; } = "";
        public string  Description   { get; set; } = "";
        public decimal TargetAmount  { get; set; }
        public decimal CurrentAmount
        {
            get => _currentAmount;
            set { _currentAmount = value; PC(nameof(CurrentAmount)); PC(nameof(Progress)); PC(nameof(ProgressText)); PC(nameof(Remaining)); PC(nameof(IsComplete)); }
        }
        public DateTime? Deadline { get; set; }
        public string    Color    { get; set; } = "#E91E8C";
        public string    Icon     { get; set; } = "🎯";

        public double  Progress     => TargetAmount > 0 ? Math.Min(1.0, (double)(CurrentAmount / TargetAmount)) : 0;
        public string  ProgressText => $"{Progress * 100:F0}%";
        public decimal Remaining    => Math.Max(0, TargetAmount - CurrentAmount);
        public bool    IsComplete   => CurrentAmount >= TargetAmount;

        public string DeadlineText => Deadline.HasValue
            ? $"До {Deadline.Value:dd MMM yyyy}" : "Без срока";

        public event PropertyChangedEventHandler? PropertyChanged;
        void PC(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    // ─────────────────────────────── GoalTask ────────────────────────────────
    public class GoalTask : INotifyPropertyChanged
    {
        private bool _isDone;
        public int    Id     { get; set; }
        public int    GoalId { get; set; }
        public string Title  { get; set; } = "";
        public bool   IsDone { get => _isDone; set { _isDone = value; PC(nameof(IsDone)); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        void PC(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    // ─────────────────────────────── AppData ─────────────────────────────────
    public static class AppData
    {
        public static ObservableCollection<FamilyMember> Members      { get; } = new();
        public static ObservableCollection<Transaction>  Transactions { get; } = new();
        public static ObservableCollection<SavingsGoal>  Goals        { get; } = new();
        public static ObservableCollection<GoalTask>     GoalTasks    { get; } = new();

        private static int _mId = 1, _tId = 1, _gId = 1, _tkId = 1;

        // ── Aggregates ─────────────────────────────────────────────────────
        public static decimal TotalIncome   => Transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        public static decimal TotalExpenses => Transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        public static decimal TotalBalance  => TotalIncome - TotalExpenses;
        public static decimal TotalSaved    => Goals.Sum(g => g.CurrentAmount);

        public static decimal GetMemberIncome(int id)   => Transactions.Where(t => t.MemberId == id && t.Type == TransactionType.Income).Sum(t => t.Amount);
        public static decimal GetMemberExpenses(int id) => Transactions.Where(t => t.MemberId == id && t.Type == TransactionType.Expense).Sum(t => t.Amount);
        public static decimal GetMemberBalance(int id)  => GetMemberIncome(id) - GetMemberExpenses(id);

        // ── CRUD ────────────────────────────────────────────────────────────
        public static FamilyMember AddMember(string name, string role, string color, string emoji)
        {
            var m = new FamilyMember { Id = _mId++, Name = name, Role = role, AvatarColor = color, Emoji = emoji };
            Members.Add(m);
            return m;
        }

        public static void RemoveMember(int id)
        {
            var m = Members.FirstOrDefault(x => x.Id == id);
            if (m != null) Members.Remove(m);
            var toRemove = Transactions.Where(t => t.MemberId == id).ToList();
            foreach (var t in toRemove) Transactions.Remove(t);
        }

        public static void AddTransaction(int memberId, string memberName, string memberColor,
            decimal amount, TransactionType type, string category, string icon, string description, DateTime date)
        {
            Transactions.Add(new Transaction
            {
                Id = _tId++, MemberId = memberId, MemberName = memberName, MemberColor = memberColor,
                Amount = amount, Type = type, Category = category, CategoryIcon = icon,
                Description = description, Date = date
            });
        }

        public static void RemoveTransaction(int id)
        {
            var t = Transactions.FirstOrDefault(x => x.Id == id);
            if (t != null) Transactions.Remove(t);
        }

        public static SavingsGoal AddGoal(string name, string description, decimal target, DateTime? deadline, string color, string icon)
        {
            var g = new SavingsGoal { Id = _gId++, Name = name, Description = description, TargetAmount = target, Deadline = deadline, Color = color, Icon = icon };
            Goals.Add(g);
            return g;
        }

        public static void RemoveGoal(int id)
        {
            var g = Goals.FirstOrDefault(x => x.Id == id);
            if (g != null) Goals.Remove(g);
            var toRemove = GoalTasks.Where(t => t.GoalId == id).ToList();
            foreach (var t in toRemove) GoalTasks.Remove(t);
        }

        public static GoalTask AddGoalTask(int goalId, string title)
        {
            var t = new GoalTask { Id = _tkId++, GoalId = goalId, Title = title };
            GoalTasks.Add(t);
            return t;
        }

        // Статический конструктор — данные не добавляются, приложение стартует чистым
        static AppData() { }
    }
}
