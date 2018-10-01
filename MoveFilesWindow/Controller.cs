using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoveFiles.Windows
{
  class Controller
  {
    #region Member Variables

    private MoveFilesWindow m_moveFilesWindow;
    private ProgressWindow m_progressWindow;

    #endregion

    #region Constructors

    public Controller()
    { }

    public Controller(MoveFilesWindow moveFilesWindow, ProgressWindow progressWindow)
    {
      m_moveFilesWindow = moveFilesWindow;
      m_progressWindow = progressWindow;
    }

    #endregion

    #region Private methods
    #endregion
  }
}
