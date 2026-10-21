namespace FerreteriaSistema
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            using (var login = new FormLogin())
            {
                if (login.ShowDialog() != DialogResult.OK) return;
            }
            Application.Run(new FormPrincipal());
        }
    }
}