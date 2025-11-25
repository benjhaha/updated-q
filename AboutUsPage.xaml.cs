namespace EkandidatoAdmin;

public partial class AboutUsPage : ContentPage
{
    public AboutUsPage()
    {
        InitializeComponent();

    }

    private async void CallUs_Clicked(object sender, EventArgs e)
          => await DisplayAlert("Call Us", "This is our number: +63912345678", "OK");
    private async void EmailUs_Clicked(object sender, EventArgs e)
    {
        string email = "";
        string subject = Uri.EscapeDataString("Inquiry from Ekandidato App");
        string body = Uri.EscapeDataString("Hello, I would like to ask about...");

        string gmailUrl = $"https://mail.google.com/mail/?view=cm&fs=1&to={email}&su={subject}&body={body}";

        await Launcher.Default.OpenAsync(gmailUrl);
    }
}