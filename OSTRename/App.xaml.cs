using AsyncIO.FileSystem;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using BenLib;
using Z.Linq;
using System.Threading;

namespace OSTRename
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        public new static MainWindow MainWindow { get; private set; }

        public static string OSTName { get; private set; }
        public static string[] Pattern { get; private set; }
        public static bool Number { get; set; }
        public static string[][] Translations { get; private set; }

        private void ShowError(bool isPatternError)
        {
            MessageBox.Show(isPatternError ? "L'en-tête du fichier de traductions n'est pas valide" : "Le corps du fichier de traductions n'est pas valide", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(3);
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            string[] file = null;

            if (e.Args.Length > 0 && File.Exists(e.Args[0])) file = await AsyncFile.ReadAllLinesAsync(e.Args[0]);
            else
            {
                var ofd = new OpenFileDialog { Title = "Sélectionnez un fichier de traductions" };

                if (ofd.ShowDialog() == true && File.Exists(ofd.FileName)) file = await AsyncFile.ReadAllLinesAsync(ofd.FileName);
                else
                {
                    Shutdown(2);
                    return;
                }
            }

            if (file.Length < 4) ShowError(true);
            if (await file[0].AllAsync(c => c.Equals('=')) && await file[3].AllAsync(c => c.Equals('=')))
            {
                OSTName = file[1];
                Number = file[2].StartsWith("⇒ ");
                Pattern = file[2].TrimStart("⇒ ", 1).Split(new[] { " ⇔ " }, StringSplitOptions.None);
                if (Pattern.Length <= 1 || await Pattern.AnyAsync(line => line.IsNullOrWhiteSpace()) || await (await Pattern.GroupByAsync(line => line)).AnyAsync(async group => await group.CountAsync() > 1)) ShowError(true);
            }
            else ShowError(true);

            if (file.Length < 6) ShowError(false);

            Translations = (await (await file.SubArray(5, file.Length - 5).SelectAsync(line =>
            {
                if (!Number ^ line.Contains(" ⇒ ") && line.Split(new[] { " ⇔ " }, StringSplitOptions.None).Length == Pattern.Length)
                {
                    if (Number)
                    {
                        var index = line.IndexOf(" ⇒ ");
                        var number = line.Substring(0, index);
                        var litt = line.Substring(index + 3);
                        return new[] { number }.Concat(litt.Split(new[] { " ⇔ " }, StringSplitOptions.None)).ToArray();
                    }
                    else return line.Split(new[] { " ⇔ " }, StringSplitOptions.None);
                }
                else return null;
            })).WhereAsync(line => line != null)).ToArray();

            MainWindow = new MainWindow() { Title = $"{OSTName} - OSTRename" };
            MainWindow.Closed += (sender, args) => Shutdown();
            MainWindow.Show();
        }
    }
}
