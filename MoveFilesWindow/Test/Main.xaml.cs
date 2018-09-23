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
using System.Threading;
using System.ComponentModel;

namespace MoveFilesTest
{
  /// <summary>
  /// Interaction logic for Main.xaml
  /// </summary>
  public partial class Main : Window
  {
    public Main()
    {

    }

    private int m_count = 0;
    Progress m_progress;

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      m_progress = new Progress(this);
      DoSomething();
      m_progress.ShowDialog();
      
    }

    public void DoSomething()
    {
      BackgroundWorker worker = new BackgroundWorker();
      worker.WorkerReportsProgress = true;
      worker.DoWork += worker_DoWork;
      worker.ProgressChanged += worker_ProgressChanged;
      worker.RunWorkerCompleted += Completed;
      worker.RunWorkerAsync();
    }

    private void Completed(object sender, RunWorkerCompletedEventArgs e)
    {
      MessageBox.Show("Completed");
    }

    void worker_DoWork(object sender, DoWorkEventArgs e)
    {
      for (int i = 0; i < 100; i++)
      {
        (sender as BackgroundWorker).ReportProgress(i);
        Thread.Sleep(10);
      }
    }

    void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      m_progress.u_progressBar.Value = e.ProgressPercentage;
      m_progress.Title = "Progress: " + e.ProgressPercentage.ToString() + "%";
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
      //m_progress.Close();
    }
  }
}
