using BenLib.Standard;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Z.Linq;

namespace OSTRename
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<string> Files { get; set; } = new ObservableCollection<string>();

        public MainWindow()
        {
            InitializeComponent();
            fileList.MaxHeight = SystemParameters.PrimaryScreenHeight / 2;
            DataContext = this;
        }

        public async Task AddFiles(IEnumerable<string> collection = default)
        {
            OpenFileDialog ofd = null;
            if (collection != null || (ofd = new OpenFileDialog() { Multiselect = true }).ShowDialog() == true)
            {
                Files = new ObservableCollection<string>(await Files.UnionAsync(collection ?? ofd.FileNames));
                if (chbx_DisplayFilesInList.IsChecked == true) fileList.GetBindingExpression(ItemsControl.ItemsSourceProperty).UpdateTarget();
            }
        }

        public async Task RenameFiles()
        {
            if (Files.Count == 0) return;

            int from = App.Pattern.IndexOf(cbx_From.SelectedItem);
            int to = App.Pattern.IndexOf(cbx_To.SelectedItem);

            if (App.Number)
            {
                from++;
                to++;
            }

            int count = 0;
            bar_Main.Maximum = Files.Count;

            foreach (string file in Files.ToList())
            {
                try
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    string[] translation = await App.Translations.FirstOrDefaultAsync(tr =>
                    {
                        if (tr[from] == name) return true;
                        if (App.Number && name.StartsWith(tr[0]))
                        {
                            string num = name.Substring(0, tr[0].Length);
                            string lit = name.Substring(tr[0].Length);
                            return lit == $" - {tr[from]}";
                        }
                        else return false;
                    });

                    bar_Main.Value++;
                    if (translation.IsNullOrEmpty()) continue;

                    string number = chbx_Num.IsChecked == true ? $"{translation[0]} - " : string.Empty;
                    string newName = Path.Combine(Path.GetDirectoryName(file), number + translation[to] + Path.GetExtension(file));
                    Files[Files.IndexOf(file)] = newName;

                    File.Move(file, newName);
                    count++;
                }
                catch { }
            }

            MessageBox.Show($"{count} Fichiers renommés", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            bar_Main.Value = 0;
        }

        private async void btn_Add_Click(object sender, RoutedEventArgs e) => await AddFiles();

        private async void btn_Rename_Click(object sender, RoutedEventArgs e) => await RenameFiles();

        protected override async void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Insert:
                    await AddFiles();
                    break;
                case Key.F5:
                    await RenameFiles();
                    break;
                case Key.Delete:
                    foreach (string file in await fileList.SelectedItems.OfTypeAsync<string>().ToList()) Files.Remove(file);
                    break;
            }
        }

        protected override async void OnDrop(DragEventArgs e)
        {
            var files = ((string[])e.Data.GetData(DataFormats.FileDrop, false)).ToList();
            foreach (string folder in files.Where(folder => Directory.Exists(folder)).ToList())
            {
                files.Remove(folder);
                if (Keyboard.Modifiers == ModifierKeys.Shift) files.AddRange(Directory.GetFiles(folder, "*", SearchOption.AllDirectories));
                else files.AddRange(Directory.GetFiles(folder));
            }
            await AddFiles(files);
        }
    }
}
