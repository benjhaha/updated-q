namespace EkandidatoAdmin
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(AdminDashboard), typeof(AdminDashboard));
            Routing.RegisterRoute(nameof(AddNewCandidates), typeof(AddNewCandidates));
            Routing.RegisterRoute(nameof(AboutUsPage), typeof(AboutUsPage));
            Routing.RegisterRoute(nameof(AdminProfile), typeof(AdminProfile));
            Routing.RegisterRoute(nameof(CompareCandidates), typeof(CompareCandidates));
            Routing.RegisterRoute(nameof(ManageSettings), typeof(ManageSettings));
            Routing.RegisterRoute(nameof(VotingPage), typeof(VotingPage));
        }
    }
}
