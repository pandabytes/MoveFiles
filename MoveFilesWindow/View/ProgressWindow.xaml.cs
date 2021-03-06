﻿using System;
using System.Windows;
using System.Windows.Controls;

namespace MoveFiles.Windows.View
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
    /// Message indicating the move operation is being canceled.
    /// </summary>
    public const string CanceledMessage = "Move operation is canceled";

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

    /// <summary>
    /// The Canceled event.
    /// </summary>
    public event EventHandler Canceled;

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

    /// <summary>
    /// Cancel the moving operation when the Cancel button is clicked.
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="e">Event arguments</param>
    private void CancelButtonClickHandler(object sender, RoutedEventArgs e)
    {
      OnCanceled(EventArgs.Empty);
    }

    /// <summary>
    /// Raise the Canceled event
    /// </summary>
    /// <param name="e">Event argument</param>
    protected virtual void OnCanceled(EventArgs e)
    {
      Canceled?.Invoke(this, e);
    }

    #endregion
  }
}
