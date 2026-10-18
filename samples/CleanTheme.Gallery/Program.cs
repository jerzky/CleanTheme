namespace CleanTheme.Gallery;

public static class Program
{
    [STAThread]
    public static void Main()
    {
        var app = new App();
        app.InitializeComponent();
        app.Run(new MainWindow());
    }
}
