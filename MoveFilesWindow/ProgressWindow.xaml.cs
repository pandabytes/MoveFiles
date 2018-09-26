using System;
using System.Windows;
using System.Windows.Controls;

namespace MoveFiles.Windows
{
  /// <summary>
  /// Interaction logic for ProgressWindow.xaml
  /// </summary>
  public partial class ProgressWindow : Window
  {
    #region Member Variables

    /// <summary>
    /// Message indicating the move operation progress.
    /// </summary>
    public const string MovingMessageFormat = "Moving {0} files & directories";

    /// <summary>
    /// Message indicating the move operation is completed.
    /// </summary>
    public const string CompletedMoveMessageFormat = "Moved {0} files & directories to {1}";

    /// <summary>
    /// Get a reference to the progress bar object.
    /// </summary>
    public ProgressBar ProgressBarWindow
    {
      get { return u_progressBar; }
    }

    /// <summary>
    /// Get a reference to the Ok button object.
    /// </summary>
    public Button OkButton
    {
      get { return u_okButton; }
    }

    /// <summary>
    /// Get a reference to the progress text block object.
    /// </summary>
    public TextBlock ProgressTextBlock
    {
      get { return u_progressText; }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Default constructor.
    /// </summary>
    public ProgressWindow()
    {
      InitializeComponent();
      MaxWidth = MinWidth = Width;
      MaxHeight = MinHeight = Height;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Close the "this" object when the Ok button is clicked.
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="e">Event arguments</param>
    private void OkButtonClickHandler(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

    #endregion
  }
}
