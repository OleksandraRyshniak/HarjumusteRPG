namespace HarjumusteRPG.Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Pages opened on top of other pages (with "..") are registered here.
            Routing.RegisterRoute("habit-edit", typeof(Views.HabitEditPage));
        }
    }
}