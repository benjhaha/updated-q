using System.Collections.ObjectModel;

namespace EkandidatoAdmin;

public partial class AddNewCandidates : ContentPage
{

    string? _partyCoverPath, _presPhoto, _vpPhoto, _secPhoto, _treasPhoto;

    string? _proPhoto, _shRepPhoto, _fyRepPhoto, _syRepPhoto, _tyRepPhoto, _foyRepPhoto;
    public AddNewCandidates()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
    }

    /* ========= Image pickers ========= */
    async void OnChooseCoverClicked(object? sender, EventArgs e)
    {
        _partyCoverPath = await PickImageAsync("Choose your cover photo");
        TrySetHint("CoverHint", _partyCoverPath);
    }

    async void OnPickPresidentPhoto(object? s, EventArgs e) { _presPhoto = await PickImageAsync("Choose president photo"); TrySetHint("PresHint", _presPhoto); }
    async void OnPickVPPhoto(object? s, EventArgs e) { _vpPhoto = await PickImageAsync("Choose vice-president photo"); TrySetHint("VpHint", _vpPhoto); }
    async void OnPickSecPhoto(object? s, EventArgs e) { _secPhoto = await PickImageAsync("Choose secretary photo"); TrySetHint("SecHint", _secPhoto); }
    async void OnPickTreasPhoto(object? s, EventArgs e) { _treasPhoto = await PickImageAsync("Choose treasurer photo"); TrySetHint("TreasHint", _treasPhoto); }
    async void OnPickProPhoto(object? s, EventArgs e) { _proPhoto = await PickImageAsync("Choose PRO photo"); TrySetHint("ProHint", _proPhoto); }
    async void OnPickSHRepPhoto(object? s, EventArgs e) { _shRepPhoto = await PickImageAsync("Choose Senior High Rep photo"); TrySetHint("SHRepHint", _shRepPhoto); }
    async void OnPickFYRepPhoto(object? s, EventArgs e) { _fyRepPhoto = await PickImageAsync("Choose First Year Rep photo"); TrySetHint("FYRepHint", _fyRepPhoto); }
    async void OnPickSYRepPhoto(object? s, EventArgs e) { _syRepPhoto = await PickImageAsync("Choose Second Year Rep photo"); TrySetHint("SYRepHint", _syRepPhoto); }
    async void OnPickTYRepPhoto(object? s, EventArgs e) { _tyRepPhoto = await PickImageAsync("Choose Third Year Rep photo"); TrySetHint("TYRepHint", _tyRepPhoto); }
    async void OnPickFoYRepPhoto(object? s, EventArgs e) { _foyRepPhoto = await PickImageAsync("Choose Fourth Year Rep photo"); TrySetHint("FoYRepHint", _foyRepPhoto); }

    static async Task<string?> PickImageAsync(string title)
    {
        try
        {
            var res = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = title
            });
            return res?.FullPath;
        }
        catch { return null; }
    }

    void TrySetHint(string labelName, string? fullPath)
    {
        var lbl = this.FindByName<Label>(labelName);
        if (lbl != null)
            lbl.Text = string.IsNullOrWhiteSpace(fullPath) ? "Choose your photo" : System.IO.Path.GetFileName(fullPath);
    }

    /* ========= Submit ========= */
    async void OnSubmitClicked(object? sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;

        if (string.IsNullOrWhiteSpace(PartyNameEntry?.Text))
        {
            ErrorLabel.Text = "Party List Name is required.";
            ErrorLabel.IsVisible = true;
            return;
        }

        // Build a simple model you can save (or send to a repository/viewmodel)
        var party = new PartyList
        {
            Name = PartyNameEntry.Text.Trim(),
            CoverPath = _partyCoverPath,
            Candidates = new ObservableCollection<Candidate>
            {
                new Candidate("President",               PresidentNameEntry?.Text ?? "",   PresidentYS?.Text ?? "",   _presPhoto),
                new Candidate("Vice-President",          VPNameEntry?.Text ?? "",          VPYS?.Text ?? "",          _vpPhoto),
                new Candidate("Secretary",               SecNameEntry?.Text ?? "",         SecYS?.Text ?? "",         _secPhoto),
                new Candidate("Treasurer",               TreasNameEntry?.Text ?? "",       TreasYS?.Text ?? "",       _treasPhoto),
                new Candidate("PRO",                     ProNameEntry?.Text ?? "",         ProYS?.Text ?? "",         _proPhoto),
                new Candidate("Senior High Representative", SHRepNameEntry?.Text ?? "",    SHRepYS?.Text ?? "",       _shRepPhoto),
                new Candidate("First Year Representative",  FYRepNameEntry?.Text ?? "",    FYRepYS?.Text ?? "",       _fyRepPhoto),
                new Candidate("Second Year Representative", SYRepNameEntry?.Text ?? "",    SYRepYS?.Text ?? "",       _syRepPhoto),
                new Candidate("Third Year Representative",  TYRepNameEntry?.Text ?? "",    TYRepYS?.Text ?? "",       _tyRepPhoto),
                new Candidate("Fourth Year Representative", FoYRepNameEntry?.Text ?? "",   FoYRepYS?.Text ?? "",      _foyRepPhoto),
            }

        };

        // ========= push into the shared repository so VotingPage shows them =========


        CandidateRepository.PartyListName = party.Name;
        CandidateRepository.PartyCoverPhotoPath = party.CoverPath;

        // Clear previous lists then map the new ones
        CandidateRepository.ClearAll();

        void AddTo(System.Collections.ObjectModel.ObservableCollection<CandidateVm> bucket, Candidate c)
        {
            bucket.Add(new CandidateVm
            {
                Name = c.Name,
                YearSection = c.YearSection,
                Party = party.Name,            // show partylist as Party
                Slogan = "",                   // not collected on this screen
                PhotoPath = c.PhotoPath
            });
        }

        Candidate? Pick(string contains) =>
            party.Candidates.FirstOrDefault(x => x.Position.Contains(contains, StringComparison.OrdinalIgnoreCase));

        // Core positions
        var pres = Pick("President");            // picks "President" (exact) first occurrence
        var vp = party.Candidates.FirstOrDefault(x => x.Position.Contains("Vice", StringComparison.OrdinalIgnoreCase));
        var sec = Pick("Secretary");
        var tre = Pick("Treasurer");
        var pro = Pick("PRO");

        if (pres != null && pres.Position.Equals("President", StringComparison.OrdinalIgnoreCase))
            AddTo(CandidateRepository.President, pres);
        if (vp != null) AddTo(CandidateRepository.VicePresident, vp);
        if (sec != null) AddTo(CandidateRepository.Secretary, sec);
        if (tre != null) AddTo(CandidateRepository.Treasurer, tre);
        if (pro != null) AddTo(CandidateRepository.PRO, pro);

        // Representatives (optional buckets)
        void TryAddRep(string key, System.Collections.ObjectModel.ObservableCollection<CandidateVm> bucket)
        {
            var c = party.Candidates.FirstOrDefault(x => x.Position.Contains(key, StringComparison.OrdinalIgnoreCase));
            if (c != null) AddTo(bucket, c);
        }

        TryAddRep("Senior High", CandidateRepository.SeniorHighRep);
        TryAddRep("First Year", CandidateRepository.FirstYearRep);
        TryAddRep("Second Year", CandidateRepository.SecondYearRep);
        TryAddRep("Third Year", CandidateRepository.ThirdYearRep);
        TryAddRep("Fourth Year", CandidateRepository.FourthYearRep);
        // ========= END =========

        // TODO: Save 'party' (e.g., to a repository, database, or pass forward).
        await DisplayAlert("Submitted", "Party list and candidate details were captured.", "OK");

        // (Optional) Navigate to Voting so user can see them
        await Shell.Current.GoToAsync(nameof(VotingPage));

        // Optional: reset the form
        PartyNameEntry.Text = string.Empty;
        _partyCoverPath = _presPhoto = _vpPhoto = _secPhoto = _treasPhoto =
            _proPhoto = _shRepPhoto = _fyRepPhoto = _syRepPhoto = _tyRepPhoto = _foyRepPhoto = null;

        TrySetHint("CoverHint", null);
        TrySetHint("PresHint", null); TrySetHint("VpHint", null); TrySetHint("SecHint", null);
        TrySetHint("TreasHint", null); TrySetHint("ProHint", null); TrySetHint("SHRepHint", null);
        TrySetHint("FYRepHint", null); TrySetHint("SYRepHint", null); TrySetHint("TYRepHint", null);
        TrySetHint("FoYRepHint", null);
    }
}



/* These simple models match what you already had */
public class PartyList
{
    public string Name { get; set; } = string.Empty;
    public string? CoverPath { get; set; }
    public ObservableCollection<Candidate> Candidates { get; set; } = new();
}

public class Candidate
{
    public string Position { get; set; }
    public string Name { get; set; }
    public string YearSection { get; set; }
    public string? PhotoPath { get; set; }

    public Candidate(string position, string name, string yearSection, string? photoPath)
    {
        Position = position;
        Name = name;
        YearSection = yearSection;
        PhotoPath = photoPath;
    }
}

