using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using FamilyBudget.Pages;

namespace FamilyBudget
{
    public partial class MainWindow : Window
    {
        private DashboardPage?    _dashPage;
        private FamilyPage?       _famPage;
        private TransactionsPage? _transPage;
        private GoalsPage?        _goalsPage;
        private AnalyticsPage?    _analPage;

        private static readonly LinearGradientBrush _activeBg = new LinearGradientBrush(
            Color.FromRgb(0xE9, 0x1E, 0x8C),
            Color.FromRgb(0xAD, 0x14, 0x57), 45);
        private static readonly SolidColorBrush _mutedBg =
            new SolidColorBrush(Color.FromRgb(0x2A, 0x10, 0x40));
        private static readonly SolidColorBrush _activeFg =
            new SolidColorBrush(Color.FromRgb(0xE9, 0x1E, 0x8C));
        private static readonly SolidColorBrush _mutedFg =
            new SolidColorBrush(Color.FromRgb(0x7E, 0x4D, 0x8B));
        private static readonly DropShadowEffect _pinkGlow = new DropShadowEffect
        {
            Color = Color.FromRgb(0xE9, 0x1E, 0x8C),
            BlurRadius = 12, ShadowDepth = 0, Opacity = 0.65
        };

        public MainWindow()
        {
            InitializeComponent();
            TxtDate.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy");
            ShowPage("Dashboard");
        }
        private void NavDashboard_Click(object s, RoutedEventArgs e)  => ShowPage("Dashboard");
        private void NavFamily_Click(object s, RoutedEventArgs e)     => ShowPage("Family");
        private void NavTrans_Click(object s, RoutedEventArgs e)      => ShowPage("Transactions");
        private void NavGoals_Click(object s, RoutedEventArgs e)      => ShowPage("Goals");
        private void NavAnalytics_Click(object s, RoutedEventArgs e)  => ShowPage("Analytics");

        private void ShowPage(string page)
        {
            UserControl? uc = page switch
            {
                "Dashboard"    => _dashPage  ??= new DashboardPage(),
                "Family"       => _famPage   ??= new FamilyPage(),
                "Transactions" => _transPage ??= new TransactionsPage(),
                "Goals"        => _goalsPage ??= new GoalsPage(),
                "Analytics"    => _analPage  ??= new AnalyticsPage(),
                _              => null
            };

            if (uc == null) return;

            if (uc is DashboardPage   dp) dp.Refresh();
            if (uc is FamilyPage      fp) fp.Refresh();
            if (uc is TransactionsPage tp) tp.Refresh();
            if (uc is GoalsPage       gp) gp.Refresh();
            if (uc is AnalyticsPage   ap) ap.Refresh();

            PageContent.Content = uc;
            SetActiveNav(page);
        }

        private void SetActiveNav(string page)
        {
            var items = new[]
            {
                (Brd: BrdDash,  Lbl: LblDash,  Key: "Dashboard"),
                (Brd: BrdFam,   Lbl: LblFam,   Key: "Family"),
                (Brd: BrdTrans, Lbl: LblTrans,  Key: "Transactions"),
                (Brd: BrdGoals, Lbl: LblGoals,  Key: "Goals"),
                (Brd: BrdAnal,  Lbl: LblAnal,   Key: "Analytics"),
            };

            foreach (var item in items)
            {
                bool isActive = item.Key == page;
                item.Brd.Background = isActive ? _activeBg : _mutedBg;
                item.Brd.Effect     = isActive ? _pinkGlow  : null;
                item.Lbl.Foreground = isActive ? _activeFg  : _mutedFg;
                item.Lbl.FontWeight = isActive ? FontWeights.SemiBold : FontWeights.Normal;
            }
        }
        private void TitleBar_MouseDown(object s, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                DragMove();
        }

        private void MinimizeBtn_Click(object s, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;
        private void CloseBtn_Click(object s, RoutedEventArgs e)
            => Close();
    }
}
