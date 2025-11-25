using System.Collections.ObjectModel;
using System.Collections.Specialized;

using System.ComponentModel;
using System.Globalization;

using System.Windows.Input;


namespace EkandidatoAdmin;


public partial class AdminDashboard : ContentPage
{
    private int _totalStudents;
    private int _votesCast;
    private int _totalCandidates;

    public AdminDashboardVm VM { get; } = new();

    bool _drawerOpen;

    BoxView? BackdropView => this.FindByName<BoxView>("Backdrop");
    Grid? DrawerHostView => this.FindByName<Grid>("DrawerHost");
    Border? DrawerPanelView => this.FindByName<Border>("DrawerPanel");


    public AdminDashboard()
    {
        InitializeComponent();
        // Converters used by XAML (StaticResource)
        if (!Resources.ContainsKey("BoolToColor")) Resources.Add("BoolToColor", new BoolToColorConverter());
        if (!Resources.ContainsKey("BoolToTextColor")) Resources.Add("BoolToTextColor", new BoolToTextColorConverter());
        if (!Resources.ContainsKey("BoolToOpacity")) Resources.Add("BoolToOpacity", new BoolToOpacityConverter());

        BindingContext = VM;

        // Keep analytics up to date
        VM.Candidates.CollectionChanged += OnCandidatesChanged;

    }

    private void OnCandidatesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
            foreach (var c in e.OldItems.OfType<AdminCandidate>())
                c.PropertyChanged -= VM.Candidate_PropertyChanged;

        if (e.NewItems != null)
            foreach (var c in e.NewItems.OfType<AdminCandidate>())
                c.PropertyChanged += VM.Candidate_PropertyChanged;

        VM.RefreshTotals();
    }


    // ===== Drawer handlers =====
    async void ToggleDrawerClicked(object? sender, EventArgs e)
    {
        if (_drawerOpen) await CloseDrawerAsync();
        else await OpenDrawerAsync();
    }

    async void Backdrop_Tapped(object? sender, TappedEventArgs e)
    {
        if (_drawerOpen) await CloseDrawerAsync();
    }

    Task OpenDrawerAsync()
    {
        var host = DrawerHostView;
        var panel = DrawerPanelView;
        var backdrop = BackdropView;
        if (host is null || panel is null || backdrop is null)
            return Task.CompletedTask; // XAML not ready yet

        _drawerOpen = true;

        host.IsVisible = true;
        backdrop.Opacity = 0;

        var width = panel.Width > 0 ? panel.Width : 260;
        panel.TranslationX = -width;

        var slide = panel.TranslateTo(0, 0, 220, Easing.CubicOut);
        var fade = backdrop.FadeTo(1.0, 220, Easing.CubicOut);
        return Task.WhenAll(slide, fade);
    }

    Task CloseDrawerAsync()
    {
        var host = DrawerHostView;
        var panel = DrawerPanelView;
        var backdrop = BackdropView;
        if (host is null || panel is null || backdrop is null)
            return Task.CompletedTask;

        _drawerOpen = false;

        var width = panel.Width > 0 ? panel.Width : 260;
        var slide = panel.TranslateTo(-width, 0, 180, Easing.CubicIn);
        var fade = backdrop.FadeTo(0.0, 180, Easing.CubicIn);

        return Task.WhenAll(slide, fade).ContinueWith(_ =>
        {
            MainThread.BeginInvokeOnMainThread(() => host.IsVisible = false);
        });
    }
}

/* =====================  VIEWMODEL  ===================== */

public class AdminDashboardVm : BindableObject
{
    // Analytics
    private int _totalCandidates;
    private int _votesCast;
    public int TotalCandidates { get => _totalCandidates; private set { _totalCandidates = value; OnPropertyChanged(); } }
    public int VotesCast { get => _votesCast; private set { _votesCast = value; OnPropertyChanged(); } }

    public ObservableCollection<AdminCandidate> Candidates { get; } = new();

    // Commands

    public ICommand LogoutCommand { get; }
    public ICommand AddCandidateCommand { get; }
    public ICommand GoToVotingCommand { get; }
    public ICommand ManageSettingsCommand { get; }
    public ICommand PrevMonthCommand { get; }
    public ICommand NextMonthCommand { get; }
    public ICommand AddEventCommand { get; }
    public ICommand DeleteEventCommand { get; }
    public ICommand AboutUsCommand { get; }
    public ICommand ProfileCommand { get; }
    public ICommand PollHistoryCommand { get; }



    // Calendar
    public ObservableCollection<DayCell> Days { get; } = new();
    private DateTime _displayMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    public string MonthTitle => _displayMonth.ToString("MMMM yyyy").ToUpperInvariant();

    private DayCell? _selectedDay;
    public DayCell? SelectedDay
    {
        get => _selectedDay;
        set
        {
            if (_selectedDay == value) return;
            if (_selectedDay != null) _selectedDay.IsSelected = false;
            _selectedDay = value;
            if (_selectedDay != null) _selectedDay.IsSelected = true;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedDateLabel));
        }
    }

    private readonly ObservableCollection<CalendarEvent> _events = new();
    public string SelectedDateLabel =>
        SelectedDay is null
            ? "Select a date…"
            : $"{SelectedDay.Date:MMMM d, yyyy}  •  {_events.Count(e => e.Date.Date == SelectedDay.Date.Date)} event(s)";

    public AdminDashboardVm()
    {
        // Helper to get a Page for popups when not using Shell
        Page? Page() => Application.Current?.Windows.FirstOrDefault()?.Page;

        LogoutCommand = new Command(async () =>
            await (Page() ?? Shell.Current)?.DisplayAlert("Logout", "Implement your logout flow here.", "OK"));

        // Navigation calls 
        AddCandidateCommand = new Command(async () =>
        {
            await Shell.Current.GoToAsync(nameof(AddNewCandidates));
        });
        GoToVotingCommand = new Command(async () =>
        {
            await Shell.Current.GoToAsync(nameof(VotingPage));
        });

        ManageSettingsCommand = new Command(async () =>
        {
            await Shell.Current.GoToAsync(nameof(ManageSettings));
        });
        AboutUsCommand = new Command(async () =>
        {
            await Shell.Current.GoToAsync(nameof(AboutUsPage));
        });

        ProfileCommand = new Command(async () =>
        {
            await Shell.Current.GoToAsync(nameof(AdminProfile));
        });

        PollHistoryCommand = new Command(async () =>
        {
            await Shell.Current.GoToAsync(nameof(CompareCandidates));

        });

        PrevMonthCommand = new Command(() =>
        {
            _displayMonth = _displayMonth.AddMonths(-1);
            RebuildMonth();
            KeepDayNumber();
            OnPropertyChanged(nameof(MonthTitle));
        });

        NextMonthCommand = new Command(() =>
        {
            _displayMonth = _displayMonth.AddMonths(1);
            RebuildMonth();
            KeepDayNumber();
            OnPropertyChanged(nameof(MonthTitle));
        });

        AddEventCommand = new Command(async () =>
        {
            if (SelectedDay is null) { await (Page() ?? Shell.Current)?.DisplayAlert("Calendar", "Pick a date first.", "OK"); return; }

            var desc = await (Page() ?? Shell.Current)?.DisplayPromptAsync(
                "Add Event", "",
                accept: "Confirm", cancel: "Cancel",
                placeholder: "Event Description…", maxLength: 200, keyboard: Keyboard.Text);

            if (string.IsNullOrWhiteSpace(desc)) return;

            _events.Add(new CalendarEvent { Date = SelectedDay.Date, Title = desc.Trim() });
            MarkEventDots();
            OnPropertyChanged(nameof(SelectedDateLabel));
        });

        DeleteEventCommand = new Command(async () =>
        {
            if (SelectedDay is null) { await (Page() ?? Shell.Current)?.DisplayAlert("Calendar", "Pick a date first.", "OK"); return; }
            var last = _events.Where(e => e.Date.Date == SelectedDay.Date.Date).LastOrDefault();
            if (last is null)
            {
                await (Page() ?? Shell.Current)?.DisplayAlert("Delete Event", "No events on selected date.", "OK");
                return;
            }
            _events.Remove(last);
            MarkEventDots();
            OnPropertyChanged(nameof(SelectedDateLabel));
        });


        // Init
        RebuildMonth();
        SelectedDay = Days.FirstOrDefault(d => d.Date.Date == DateTime.Today)
                   ?? Days.FirstOrDefault(d => d.InCurrentMonth)
                   ?? Days.FirstOrDefault();

        RefreshTotals();
    }

    public void RefreshTotals()
    {
        TotalCandidates = Candidates.Count;
        VotesCast = Candidates.Sum(c => c.Votes);
    }

    public void Candidate_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AdminCandidate.Votes))
            RefreshTotals();
    }

    private void RebuildMonth()
    {
        Days.Clear();
        var first = new DateTime(_displayMonth.Year, _displayMonth.Month, 1);
        int diff = (7 + (int)first.DayOfWeek - (int)DayOfWeek.Sunday) % 7;
        var start = first.AddDays(-diff);

        for (int i = 0; i < 42; i++)
        {
            var d = start.AddDays(i);
            Days.Add(new DayCell
            {
                Date = d,
                DayNumber = d.Day,
                InCurrentMonth = d.Month == _displayMonth.Month,
                IsToday = d.Date == DateTime.Today
            });
        }
        MarkEventDots();
    }

    private void KeepDayNumber()
    {
        var wanted = SelectedDay?.DayNumber ?? 1;
        var safeDay = Math.Min(wanted, DateTime.DaysInMonth(_displayMonth.Year, _displayMonth.Month));
        SetSelectedDate(new DateTime(_displayMonth.Year, _displayMonth.Month, safeDay));
    }

    private void SetSelectedDate(DateTime date)
    {
        if (date.Year != _displayMonth.Year || date.Month != _displayMonth.Month)
        {
            _displayMonth = new DateTime(date.Year, date.Month, 1);
            RebuildMonth();
            OnPropertyChanged(nameof(MonthTitle));
        }

        SelectedDay = Days.FirstOrDefault(d => d.Date.Date == date.Date)
                   ?? Days.FirstOrDefault(d => d.InCurrentMonth)
                   ?? Days.FirstOrDefault();
    }

    private void MarkEventDots()
    {
        var map = _events.GroupBy(e => e.Date.Date).ToDictionary(g => g.Key, g => g.Count());
        foreach (var day in Days)
            day.HasEvents = map.ContainsKey(day.Date.Date);
    }
}


/* ===== Models ===== */
public class AdminCandidate : BindableObject
{
    private string _name = "";
    private string _position = "";
    private string _partyName = "";
    private int _votes;
    private string? _photoPath;

    public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
    public string Position { get => _position; set { _position = value; OnPropertyChanged(); } }
    public string PartyName { get => _partyName; set { _partyName = value; OnPropertyChanged(); } }
    public int Votes { get => _votes; set { _votes = value; OnPropertyChanged(); } }
    public string? PhotoPath { get => _photoPath; set { _photoPath = value; OnPropertyChanged(); } }
}

public class DayCell : BindableObject
{
    private bool _isSelected;
    private bool _hasEvents;

    public DateTime Date { get; set; }
    public int DayNumber { get; set; }
    public bool InCurrentMonth { get; set; }
    public bool IsToday { get; set; }

    public bool IsSelected { get => _isSelected; set { _isSelected = value; OnPropertyChanged(); } }
    public bool HasEvents { get => _hasEvents; set { _hasEvents = value; OnPropertyChanged(); } }
}

public class CalendarEvent
{
    public DateTime Date { get; set; }
    public string Title { get; set; } = "";
}

/* ===== Converters ===== */
public sealed class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var isTrue = value is bool b && b;
        if (!isTrue) return Colors.Transparent;

        if (parameter is string s)
        {
            try { return Color.FromArgb(s); } catch { }
        }
        return Colors.Green;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

public sealed class BoolToTextColorConverter : IValueConverter
{
    static readonly Color DefaultText = Color.FromArgb("#2A2E2D");
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var isTrue = value is bool b && b;
        if (isTrue)
        {
            if (parameter is string s)
            {
                try { return Color.FromArgb(s); } catch { }
            }
            return Colors.White;
        }
        return DefaultText;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

public sealed class BoolToOpacityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        (value is bool b && b) ? 1.0 : 0.45;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();

}
