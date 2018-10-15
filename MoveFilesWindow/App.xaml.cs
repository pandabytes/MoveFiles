using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using MWI = MoveFiles.Windows.Interfaces;
using MWC = MoveFiles.Windows.Controller;
using MWV = MoveFiles.Windows.View;
using MWM = MoveFiles.Windows.Model;

namespace MoveFiles.Windows
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    public App()
    {}

    [STAThread]
    public static void Main()
    {
      App app = new App();
      MWV.MoveFilesWindow moveFilesWindow = new MWV.MoveFilesWindow();
      MWM.MoveFilesWindowModel moveFilesModel = new MWM.MoveFilesWindowModel();

      MWC.Controller controller = new MWC.Controller(moveFilesWindow, moveFilesModel);
      
    }
  }
}
