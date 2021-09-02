using HLCS01.SDK;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace HLCS01.HMi.Views
{
    /// <summary>
    /// Interaction logic for MainWindowView.xaml
    /// </summary>
    public partial class MainWindowView : UserControl
    {
        private IEventAggregator _eventAggregator;
        public MainWindowView(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            InitializeComponent();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key==Key.Tab)
            {
                myTextBox.Text = myTextBox.Text.Insert(myTextBox.SelectionStart, "    ");
            }

            if(e.Key==Key.Return)
            {
                int selectionIDx = myTextBox.SelectionStart;
                bool found = false;
                var spaces = "";
                for (int i = myTextBox.SelectionStart; i >0; i--)
                {
                    if (found) break;
                    var ch = myTextBox.Text[i];
                    if(ch=='{')
                    {
                        int k = 0;
                        for (int j = i; j > 0; j--)
                        {
                            spaces += " ";
                            if(myTextBox.Text[j]=='\n')
                            {
                                found = true;
                                break;
                            }

                        }
                    }
                }
                var to_add = spaces.Length + 4;
                myTextBox.Text = myTextBox.Text.Insert(myTextBox.SelectionStart, "\n"+spaces+"    ");
                myTextBox.SelectionStart = selectionIDx + to_add;
            }
        }

        private void txbx_TextChanged(object sender, TextChangedEventArgs e)
        {
            _scrollViewer.ScrollToBottom();
            txbx.ScrollToEnd();
        }

        private void ListView_Drop(object sender, DragEventArgs e)
        {
            Console.WriteLine("smth dropped");
            string[] droppedFilePaths = e.Data.GetData(DataFormats.FileDrop, true) as string[];

            foreach (var path in droppedFilePaths)
            {
                FileInfo fi = new FileInfo(path);
                if (fi.Extension == ".upm")
                {
                    _eventAggregator.GetEvent<OnFileDropped>().Publish(path);
                }
            }
        }
    }
}
