using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UslmRemoveTags.ViewModel;

namespace UslmRemoveTags
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        //private void StackPanel_Drop(object sender, DragEventArgs e)
        //{
        //    // Check if the data contains file paths
        //    if (e.Data.GetDataPresent(DataFormats.FileDrop))
        //    {
        //        // Get the list of dropped file paths
        //        string[] filePaths = e.Data.GetData(DataFormats.FileDrop) as string[];

        //        if (filePaths != null && filePaths.Length > 0)
        //        {
        //            // Pass the file paths to the DropFileCommand in the ViewModel
        //            var viewModel = (MainViewModel)DataContext;
        //            viewModel.DropFileCommand.Execute(filePaths);
        //        }
        //    }
        //}
    }
}
