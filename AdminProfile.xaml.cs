namespace EkandidatoAdmin;

public partial class AdminProfile : ContentPage
{
    bool _drawerOpen;

    public AdminProfile()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Keep inputs empty on load
        NameEntry.Text = string.Empty;
        StudNoEntry.Text = string.Empty;
        YearSecEntry.Text = string.Empty;
    }

    // ===== Drawer handlers =====
    private async void ToggleDrawerClicked(object sender, EventArgs e)
    {
        if (_drawerOpen) await CloseDrawerAsync();
        else await OpenDrawerAsync();
    }

    private async void Backdrop_Tapped(object sender, TappedEventArgs e)
    {
        if (_drawerOpen) await CloseDrawerAsync();
    }

    private Task OpenDrawerAsync()
    {
        _drawerOpen = true;

        DrawerHost.IsVisible = true;
        Backdrop.Opacity = 0;

        var panelWidth = DrawerPanel.Width > 0 ? DrawerPanel.Width : 260;
        DrawerPanel.TranslationX = -panelWidth;

        var slide = DrawerPanel.TranslateTo(0, 0, 220, Easing.CubicOut);
        var fade = Backdrop.FadeTo(1.0, 220, Easing.CubicOut);
        return Task.WhenAll(slide, fade);
    }

    private Task CloseDrawerAsync()
    {
        _drawerOpen = false;

        var panelWidth = DrawerPanel.Width > 0 ? DrawerPanel.Width : 260;
        var slide = DrawerPanel.TranslateTo(-panelWidth, 0, 180, Easing.CubicIn);
        var fade = Backdrop.FadeTo(0.0, 180, Easing.CubicIn);

        return Task.WhenAll(slide, fade).ContinueWith(_ =>
        {
            MainThread.BeginInvokeOnMainThread(() => DrawerHost.IsVisible = false);
        });
    }

    // ===== Button handlers =====
    private async void ChangePhotoBtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            var photo = await MediaPicker.PickPhotoAsync();

            if (photo != null)
            {
                using var stream = await photo.OpenReadAsync();

                var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                ProfileImage.Source = ImageSource.FromStream(() => memoryStream);

                ProfileImage.IsVisible = true;
                CameraIcon.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void LogoutBtn_Clicked(object sender, EventArgs e)
    {
        bool yes = await DisplayAlert("Logout", "Are you sure you want to log out?", "Yes", "No");
        if (yes)
        {
            await DisplayAlert("Logged out", "You have been logged out.", "OK");
            // TODO: navigate to login page if needed
        }
    }

    // Drawer menu button handlers (stub actions for now)

    private async void HomeBtn_Clicked(object sender, EventArgs e)
    {
        try
        {

            await Shell.Current.GoToAsync("//AdminDashboard");
        }
        catch (Exception ex)
        {

            Console.WriteLine($"Navigation error: {ex.Message}");
        }
    }
    private void ElectionCalendarBtn_Clicked(object sender, EventArgs e)
    {
        // TODO: Add your navigation or logic here
    }

    private async void PollHistoryBtn_Clicked(object sender, EventArgs e)
        => await DisplayAlert("Poll History", "Open poll history.", "OK");

    private async void PublicWallBtn_Clicked(object sender, EventArgs e)
        => await DisplayAlert("Public Wall Opinion", "Open public wall.", "OK");
    private async void ProfileBtn_Clicked(object sender, EventArgs e)
        => await DisplayAlert("Profile", "Already on profile.", "OK");
    private async void AboutUsBtn_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(AboutUsPage)}", true);
    }
}
