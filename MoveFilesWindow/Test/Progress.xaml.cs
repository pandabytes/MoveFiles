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
using System.Windows.Shapes;
using System.ComponentModel;
using System.Threading;

namespace MoveFilesTest
{
  /// <summary>
  /// Interaction logic for Progress.xaml
  /// </summary>
  public partial class Progress : Window
  {
    private Main m_main;

    public Progress(Main main)
    {
      InitializeComponent();
      u_progressBar.Minimum = 0;
      u_progressBar.Maximum = 100;
      m_main = main;
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
      //e.Cancel = true;
      //this.Visibility = Visibility.Hidden;
    }
  }
}
