using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MWI = MoveFiles.Windows.Interfaces;
using COLG = System.Collections.Generic;

namespace MoveFiles.Windows.Controller
{
  public class Controller : MWI.IController
  {
    #region Member Variables

    private MWI.IView m_view;
    private MWI.IModel m_model;

    #endregion

    #region Constructors

    public Controller(MWI.IView view, MWI.IModel model)
    {
      m_view = view;
      m_model = model;
    }

    #endregion

    #region Public methods


    #endregion
  }
}
