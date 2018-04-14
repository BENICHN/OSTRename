using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using Z.Linq;
using AsyncIO.FileSystem;
using AsyncIO.FileSystem.Extensions;
using BenLib;

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

        private async void btn_Add_Click(object sender, RoutedEventArgs e) => await AddFiles();

        private async void btn_Rename_Click(object sender, RoutedEventArgs e)
        {
            if (Files.Count == 0) return;

            var from = App.Pattern.IndexOf(cbx_From.SelectedItem);
            var to = App.Pattern.IndexOf(cbx_To.SelectedItem);

            if (App.Number)
            {
                from++;
                to++;
            }

            int count = 0;
            bar_Main.Maximum = Files.Count;

            foreach (string file in Files.ToList())
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var translation = await App.Translations.FirstOrDefaultAsync(tr =>
                {
                    if (tr[from] == name) return true;
                    if (App.Number)
                    {
                        var numLit = name.Split(new[] { " - " }, 2, StringSplitOptions.None);
                        if (numLit.Length < 2) return false;
                        return numLit[0] == tr[0] && numLit[1] == tr[from];
                    }
                    else return false;
                });

                bar_Main.Value++;
                if (translation.IsNullOrEmpty()) continue;

                var number = chbx_Num.IsChecked == true ? $"{translation[0]} - " : String.Empty;
                var newName = Path.Combine(Path.GetDirectoryName(file), number + translation[to] + Path.GetExtension(file));
                Files[Files.IndexOf(file)] = newName;

                await AsyncFile.MoveAsync(file, newName);
                count++;
            }

            MessageBox.Show($"{count} Fichiers renommés", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            bar_Main.Value = 0;
        }

        protected override async void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Insert:
                    await AddFiles();
                    break;
                case Key.Delete:
                    foreach (string file in await fileList.SelectedItems.OfTypeAsync<string>().ToList()) Files.Remove(file);
                    break;
            }
        }

        private async void Window_Drop(object sender, DragEventArgs e)
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
