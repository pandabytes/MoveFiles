using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;

using COLG = System.Collections.Generic;
using IO = System.IO;
using WF = System.Windows.Forms;
using REGEX = System.Text.RegularExpressions;

namespace MoveFiles.Windows
{
  /// <summary>
  /// Options to select files by.
  /// </summary>
  public enum SelectFilesByOption
  {
    All = 0,
    Size = 1,
    Date = 2,
    Extension = 3
  }

  /// <summary>
  /// Options to select file size unit.
  /// </summary>
  public enum FileSizeUnit
  {
    Bytes = 0,
    Megabytes = 1,
    Gigabytes = 2
  }

  /// <summary>
  /// Interaction logic for MoveWindow.xaml
  /// </summary>
  public partial class MoveFilesWindow : Window
  {
    #region Member Variables

    /// <summary>
    /// Error message for user.
    /// </summary>
    private const string ErrorMessage = "An error occured when moving files and folders";

    /// <summary>
    /// Invalid path message.
    /// </summary>
    private const string InvalidPathMessage = "This is an invalid path";

    /// <summary>
    /// Message stating a certain path doesn't exist and the program
    /// will create that path.
    /// </summary>
    private const string CreateNonExistentPathMessage = "This path doesn't exists. It will be " + 
                                                        "created during the move operation";

    /// <summary>
    /// Source and destination error message.
    /// </summary>
    private const string SamePathMessage = "Source and destination must be different";

    /// <summary>
    /// The source directory path.
    /// </summary>
    private string m_sourceDirectory;

    /// <summary>
    /// The source directory path.
    /// </summary>
    private string m_destinationDirectory;

    /// <summary>
    /// List of files to be moved.
    /// </summary>
    private COLG.List<IO.FileInfo> m_files;

    /// <summary>
    /// List of directories to be moved.
    /// </summary>
    private COLG.List<IO.DirectoryInfo> m_directories;

    /// <summary>
    /// The number of files & directions that have been moved.
    /// </summary>
    private int m_movedFilesCount;

    /// <summary>
    /// Reference to the ProgressWindow object.
    /// </summary>
    private ProgressWindow m_progressWindow;

    #endregion

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public MoveFilesWindow()
    {
      InitializeComponent();
      Initialize();
    }

    #endregion

    #region Private Methods

    #region Handler Methods

    /// <summary>
    /// Get the source directory text as soon as it's changed.
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Event argument</param>
    private void SourceChangedHandler(object sender, TextChangedEventArgs e)
    {
      m_sourceDirectory = u_sourceTextBox.Text;

      // Enable the move button if boths paths are valid and both paths are not the same
      if (ValidateInputPaths())
      {
        u_moveButton.IsEnabled = true;
      }
      else
      {
        u_moveButton.IsEnabled = false;
      }
    }

    /// <summary>
    /// Get the destination directory text as soon as it's changed.
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Event argument</param>
    private void DestinationChangedHandler(object sender, TextChangedEventArgs e)
    {
      m_destinationDirectory = u_destinationTextBox.Text;

      // Enable the move button if the boths paths are valid
      if (ValidateInputPaths())
      {
        u_moveButton.IsEnabled = true;
      }
      else
      {
        u_moveButton.IsEnabled = false;
      }
    }

    /// <summary>
    /// Perform the moving of files from source to destination.
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Event argument</param>
    private void MoveButtonClickHandler(object sender, RoutedEventArgs e)
    {
      RetrieveContent();

      // If the destination path doesn't exist then create it
      if (!IO.Directory.Exists(m_destinationDirectory))
      {
        IO.Directory.CreateDirectory(m_destinationDirectory);
        ValidateDestination();
      }

      // Reset the moved file count and begin the moving operation in the background
      m_movedFilesCount = 0;
      RunBackgroundThread();
    }

    /// <summary>
    /// Show the the input control of the selected option.
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Event argument</param>
    private void SelectionChangedHandler(object sender, SelectionChangedEventArgs e)
    {
      SelectFilesByOption selection = (SelectFilesByOption)u_selectComboBox.SelectedIndex;
      DisplaySelectionInput(selection);
    }

    /// <summary>
    /// Open a directory browse dialog box for users
    /// to select a source directory.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="e">Event argument.</param>
    private void SourceBrowseButtonClickHandler(object sender, RoutedEventArgs e)
    {
      using (WF.FolderBrowserDialog folderBrowseDialog = new WF.FolderBrowserDialog())
      {
        folderBrowseDialog.ShowDialog();

        if (!string.IsNullOrEmpty(folderBrowseDialog.SelectedPath))
        {
          u_sourceTextBox.Text = folderBrowseDialog.SelectedPath;
        }
      }
    }

    /// <summary>
    /// Open a directory browse dialog box for users
    /// to select a destination directory.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="e">Event argument.</param>
    private void DestinationBrowseButtonClickHandler(object sender, RoutedEventArgs e)
    {
      using (WF.FolderBrowserDialog folderBrowseDialog = new WF.FolderBrowserDialog())
      {
        folderBrowseDialog.ShowDialog();

        if (!string.IsNullOrEmpty(folderBrowseDialog.SelectedPath))
        {
          u_destinationTextBox.Text = folderBrowseDialog.SelectedPath;
        }
      }
    }

    /// <summary>
    /// Handle the input text. Allow only number characters to be passed in.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="e">Event argument</param>
    private void TextBoxNumberInputHandler(object sender, TextCompositionEventArgs e)
    {
      double value;
      bool isValidNumber = Double.TryParse(e.Text, out value);

      // If the input text is not a valid number, then "handle" it here so 
      // XAML won't need to handle it
      e.Handled = !isValidNumber;
    }

    /// <summary>
    /// Reverse the source and destination paths.
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Event argument</param>
    private void ReverseButtonClickHandler(object sender, RoutedEventArgs e)
    {
      string temp = m_sourceDirectory;
      u_sourceTextBox.Text = m_destinationDirectory;
      u_destinationTextBox.Text = temp;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Initalize member variables and perform setups.
    /// </summary>
    private void Initialize()
    {
      // Save the input paths
      m_sourceDirectory = u_sourceTextBox.Text;
      m_destinationDirectory = u_destinationTextBox.Text;

      // Initilize the list of files and directories
      m_files = new COLG.List<IO.FileInfo>();
      m_directories = new COLG.List<IO.DirectoryInfo>();
      m_movedFilesCount = 0;

      this.SizeToContent = SizeToContent.Height;
    }

    /// <summary>
    /// Prevent the main window to be resized.
    /// UNUSED
    /// </summary>
    private async void LockWindowResizeAsync()
    {
      await Task.Delay(1000);

      this.MaxHeight = this.Height;
      this.MaxWidth = this.Width;
      this.MinHeight = this.Height;
      this.MinWidth = this.Width;
    }

    /// <summary>
    /// Display the input control corresponding to the selected option.
    /// </summary>
    /// <param name="selection">The selection the user selects.</param>
    private void DisplaySelectionInput(SelectFilesByOption selection)
    {
      switch (selection)
      {
        case SelectFilesByOption.Size:
          u_datePicker.Visibility = Visibility.Collapsed;
          u_sizeRangeGrid.Visibility = Visibility.Visible;
          u_extensionGrid.Visibility = Visibility.Collapsed;
          break;

        case SelectFilesByOption.Date:
          u_datePicker.Visibility = Visibility.Visible;
          u_sizeRangeGrid.Visibility = Visibility.Collapsed;
          u_extensionGrid.Visibility = Visibility.Collapsed;
          break;

        case SelectFilesByOption.Extension:
          u_datePicker.Visibility = Visibility.Collapsed;
          u_sizeRangeGrid.Visibility = Visibility.Collapsed;
          u_extensionGrid.Visibility = Visibility.Visible;
          break;

        default:
          u_datePicker.Visibility = Visibility.Collapsed;
          u_sizeRangeGrid.Visibility = Visibility.Collapsed;
          u_extensionGrid.Visibility = Visibility.Collapsed;
          break;
      }

      SetElementFullyVisible(u_moveButton, this);
    }

    /// <summary>
    /// Set an element to fully visible to the user if it's hidden because the container is too small.
    /// </summary>
    /// <param name="element">Element to be set visible</param>
    /// <param name="container">Container that contains the element</param>
    private void SetElementFullyVisible(FrameworkElement element, FrameworkElement container)
    {
      if (element != null && container != null)
      {
        Rect elementRect = new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight);
        Rect elementBound = element.TransformToAncestor(container).TransformBounds(elementRect);
        Rect containerBound = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);

        // We only concern with the height of the container
        if (!containerBound.Contains(elementBound))
        {
          this.SizeToContent = SizeToContent.Height;
        }
      }
    }

    /// <summary>
    /// Retrieve the content, files and directories, in the source directory.
    /// </summary>
    private void RetrieveContent()
    {
      m_files.Clear();
      m_directories.Clear();

      IO.DirectoryInfo sourceDirectory = new IO.DirectoryInfo(m_sourceDirectory);
      m_files = sourceDirectory.GetFiles().ToList();
      m_directories = sourceDirectory.GetDirectories().ToList();
    }

    /// <summary>
    /// Move everything from source to destination.
    /// </summary>
    /// <param name="worker">The backgroundworker thread used to update the m_progressWindow</param>
    private void MoveAll(BackgroundWorker worker)
    {
      try
      {
        // Move files
        foreach (IO.FileInfo fileInfo in m_files)
        {
          string destinationFile = IO.Path.Combine(m_destinationDirectory, fileInfo.Name);
          IO.File.Move(fileInfo.FullName, destinationFile);
          worker.ReportProgress(++m_movedFilesCount);
        }

        // Move directories
        foreach (IO.DirectoryInfo dirInfo in m_directories)
        {
          string destinationDirectory = IO.Path.Combine(m_destinationDirectory, dirInfo.Name);
          IO.Directory.Move(dirInfo.FullName, destinationDirectory);
          worker.ReportProgress(++m_movedFilesCount);
        }
      }
      catch (Exception ex)
      {
        throw new Exception(ErrorMessage + "\n\nDetail: " + ex.ToString());
      }
    }

    /// <summary>
    /// Move files and folders by the specified date.
    /// </summary>
    /// <param name="worker">The backgroundworker thread used to update the m_progressWindow</param>
    private void MoveByDate(BackgroundWorker worker)
    {
      try
      {
        string selectedDate = u_datePicker.SelectedDate.Value.ToShortDateString();

        // Move files
        foreach (IO.FileInfo fileInfo in m_files)
        {
          string fileDate = fileInfo.LastWriteTime.ToShortDateString();

          if (fileDate == selectedDate)
          {
            string destinationFile = IO.Path.Combine(m_destinationDirectory, fileInfo.Name);
            IO.File.Move(fileInfo.FullName, destinationFile);
            worker.ReportProgress(++m_movedFilesCount);
          }
        }

        // Move directories
        foreach (IO.DirectoryInfo dirInfo in m_directories)
        {
          string directoryDate = dirInfo.LastWriteTime.ToShortDateString();

          if (directoryDate == selectedDate)
          {
            string destinationDirectory = IO.Path.Combine(m_destinationDirectory, dirInfo.Name);
            IO.Directory.Move(dirInfo.FullName, destinationDirectory);
            worker.ReportProgress(++m_movedFilesCount);
          }
        }
      }
      catch (InvalidOperationException)
      {
        throw new Exception("Please specify a date in this format mm/dd/yyyy.");
      }
      catch (Exception ex)
      {
        throw new Exception(ErrorMessage + "\n\nDetail: " + ex.ToString());
      }
    }

    /// <summary>
    /// Move files and folders by specify its size range.
    /// </summary>
    /// <param name="worker">The backgroundworker thread used to update the m_progressWindow</param>
    private void MoveSize(BackgroundWorker worker)
    {
      double min = 0.0, max = 0.0;
      bool validMin = false, validMax = false;
      FileSizeUnit sizeUnitSelected = FileSizeUnit.Bytes;

      // Access the UI objects from another thread is done via Dispatcher object
      Dispatcher.Invoke(() =>
      {
        // Get the min and max values from XAML form
        ComboBox sizeUnitComboBox = u_sizeRangeGrid.Children.Cast<UIElement>().First(e => e is ComboBox) as ComboBox;
        TextBox minTextBox = u_sizeRangeGrid.Children.Cast<UIElement>().First(e => Grid.GetColumn(e) == 1 && Grid.GetRow(e) == 1) as TextBox;
        TextBox maxTextBox = u_sizeRangeGrid.Children.Cast<UIElement>().First(e => Grid.GetColumn(e) == 1 && Grid.GetRow(e) == 2) as TextBox;

        sizeUnitSelected = (FileSizeUnit)sizeUnitComboBox.SelectedIndex;

        // Convert the string values to double values
        validMin = double.TryParse(minTextBox.Text, out min);
        validMax = double.TryParse(maxTextBox.Text, out max);
      });

      if (!validMin && !validMax)
      {
        throw new ArgumentException("Min and max need to be numbers");
      }

      if (min > max)
      {
        throw new ArgumentException("Min size cannot exceed max size");
      }

      // Get the conversion scale based on the selected Unit 
      double conversion = SizeUnitConversion(sizeUnitSelected);

      try
      {
        // Move files
        foreach (IO.FileInfo fileInfo in m_files)
        {
          double fileSize = conversion * fileInfo.Length;

          if (min <= fileSize && fileSize <= max)
          {
            string destinationFile = IO.Path.Combine(m_destinationDirectory, fileInfo.Name);
            IO.File.Move(fileInfo.FullName, destinationFile);
            worker.ReportProgress(++m_movedFilesCount);
          }
        }

        // Move folders
        foreach (IO.DirectoryInfo dirInfo in m_directories)
        {
          double dirSize = conversion * GetFolderSize(dirInfo);

          if (min <= dirSize && dirSize <= max)
          {
            string destinationDirectory = IO.Path.Combine(m_destinationDirectory, dirInfo.Name);
            IO.Directory.Move(dirInfo.FullName, destinationDirectory);
            worker.ReportProgress(++m_movedFilesCount);
          }
        }
      }
      catch (Exception ex)
      {
        throw new Exception(ErrorMessage + "\n\nDetail: " + ex.ToString());
      }
    }

    /// <summary>
    /// Move files by the specified extensions
    /// </summary>
    /// <param name="worker">The backgroundworker thread used to update the m_progressWindow</param>
    private void MoveExtensions(BackgroundWorker worker)
    {
      // Access the UI objects from another thread is done via Dispatcher object
      string extensionText = null;
      Dispatcher.Invoke(() =>
      {
        extensionText = (u_extensionGrid.Children.Cast<UIElement>().First(e => Grid.GetColumn(e) == 1) as TextBox).Text;
      });

      if (string.IsNullOrWhiteSpace(extensionText))
      {
        throw new ArgumentException("No extension(s) specified");
      }
      else
      {
        string[] extensions = REGEX.Regex.Split(extensionText, @"\s+|,+");
        try
        {
          // Go through each extension
          foreach (string ext in extensions)
          {
            // Ignore white space extension
            if (!string.IsNullOrWhiteSpace(ext))
            {
              IO.DirectoryInfo sourceDirInfo = new IO.DirectoryInfo(m_sourceDirectory);

              // Move files that have matching extensions in the current directory only
              foreach (IO.FileInfo fileInfo in sourceDirInfo.GetFiles("*." + ext, IO.SearchOption.TopDirectoryOnly))
              {
                string destinationFile = IO.Path.Combine(m_destinationDirectory, fileInfo.Name);
                IO.File.Move(fileInfo.FullName, destinationFile);
                worker.ReportProgress(++m_movedFilesCount);
              }
            }
          }
        }
        catch (Exception ex)
        {
          throw new Exception(ErrorMessage + "\n\nDetail: " + ex.ToString());
        }
      }
    }

    /// <summary>
    /// Get the size of a folder.
    /// </summary>
    /// <param name="dirInfo">Object containining data of a folder.</param>
    /// <returns>Size of the given folder.</returns>
    private static double GetFolderSize(IO.DirectoryInfo dirInfo)
    {
      double totalSize = 0.0;

      foreach (IO.FileInfo fileInfo in dirInfo.GetFiles("*", IO.SearchOption.AllDirectories))
      {
        totalSize += fileInfo.Length;
      }

      return totalSize;
    }

    /// <summary>
    /// Display an error message to the user
    /// </summary>
    /// <param name="message">the error message</param>
    /// <param name="exception">the exception that was thrown if available</param>
    private static void DisplayErrorMessage(Exception exception)
    {
      MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    /// <summary>
    /// Validate the source and destination input paths.
    /// </summary>
    /// <returns>True if both input paths are valid. False otherwise.</returns>
    private bool ValidateInputPaths()
    {
      bool validSource = ValidateSource();
      bool validDestination = ValidateDestination();

      // Source and destination must be different
      if (m_sourceDirectory.Trim('\\') == m_destinationDirectory.Trim('\\'))
      {
        // souce
        u_sourceTextBox.BorderBrush = Brushes.Red;
        u_sourceErrorText.Visibility = Visibility.Visible;
        u_sourceErrorText.Text = SamePathMessage;

        // destination
        u_destinationTextBox.BorderBrush = Brushes.Red;
        u_destinationErrorText.Visibility = Visibility.Visible;
        u_destinationErrorText.Text = SamePathMessage;
        u_destinationErrorText.Foreground = Brushes.Red;

        return false;
      }

      return validSource && validDestination;
    }

    /// <summary>
    /// Validate the input source path.
    /// </summary>
    /// <returns>True if the input source path is valid. False otherwise.</returns>
    private bool ValidateSource()
    {
      // Check if the input source path is a valid path
      if (IO.Directory.Exists(m_sourceDirectory))
      {
        u_sourceTextBox.BorderBrush = Brushes.Black;
        u_sourceErrorText.Visibility = Visibility.Hidden;

        return true;
      }
      else
      {
        u_sourceTextBox.BorderBrush = Brushes.Red;

        u_sourceErrorText.Text = InvalidPathMessage;
        u_sourceErrorText.Visibility = Visibility.Visible;

        return false;
      }
    }

    /// <summary>
    /// Validate the input destination path.
    /// </summary>
    /// <returns>True if the input destination path is valid. False otherwise.</returns>
    private bool ValidateDestination()
    {
      // Check if the input destination path is a valid path
      if (IO.Directory.Exists(m_destinationDirectory))
      {
        u_destinationTextBox.BorderBrush = Brushes.Black;
        u_destinationErrorText.Visibility = Visibility.Hidden;

        return true;
      }
      else if (!string.IsNullOrWhiteSpace(m_destinationDirectory))
      {
        // If the directory path doesn't exist and it is not empty/null/whitespace, 
        // then display a message indicating it will be created during the move operation
        u_destinationTextBox.BorderBrush = Brushes.DarkOrange;
        u_destinationErrorText.Visibility = Visibility.Visible;
        u_destinationErrorText.Text = CreateNonExistentPathMessage;
        u_destinationErrorText.Foreground = Brushes.DarkOrange;

        return true;
      }
      else
      {
        // Display an error message stating the input cannot be null, empty, or whitespace
        u_destinationTextBox.BorderBrush = Brushes.Red;
        u_destinationErrorText.Visibility = Visibility.Visible;
        u_destinationErrorText.Text = InvalidPathMessage;
        u_destinationErrorText.Foreground = Brushes.Red;

        return false;
      }
    }

    /// <summary>
    /// Return the conversion scale number.
    /// </summary>
    /// <param name="sizeUnitSelected">The given selected unit</param>
    /// <returns>Return the convertion scale number.</returns>
    private static double SizeUnitConversion(FileSizeUnit sizeUnitSelected)
    {
      double conversion = 0.0;
      switch (sizeUnitSelected)
      {
        case (FileSizeUnit.Bytes):
          conversion = 1.0;
          break;
        case (FileSizeUnit.Megabytes):
          conversion = 0.000001;
          break;
        case (FileSizeUnit.Gigabytes):
          conversion = 0.000000001;
          break;
        default:
          throw new ArgumentException("Invalid size unit");
      }

      return conversion;
    }

    /// <summary>
    /// Run the moving background operation in a background thread.
    /// </summary>
    private void RunBackgroundThread()
    {
      // Create a background thread and register the appropriate handler methods
      BackgroundWorker worker = new BackgroundWorker();
      worker.WorkerReportsProgress = true;
      worker.DoWork += MoveFilesInBackground;
      worker.RunWorkerCompleted += ProgressCompletedHandler;
      worker.ProgressChanged += ProgressChangedHandler;
      
      // Create a ProgressWindow object
      m_progressWindow = new ProgressWindow();
      m_progressWindow.Owner = this;
      m_progressWindow.Canceled += ProgressCanceledHandler;

      // Start the thread in the background and show the ProgressWindow object to user
      // Catch any thrown exception that is not caught in the background thread
      try
      {
        SelectFilesByOption selection = (SelectFilesByOption)u_selectComboBox.SelectedIndex;
        worker.RunWorkerAsync(argument: selection);
        m_progressWindow.ShowDialog();
      }
      catch (Exception ex)
      {
        DisplayErrorMessage(ex);
        m_progressWindow.Canceled -= ProgressCanceledHandler;
      }
    }

    /// <summary>
    /// Do the moving operation in this method.
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Event argument</param>
    private void MoveFilesInBackground(object sender, DoWorkEventArgs e)
    {
      SelectFilesByOption selection = (SelectFilesByOption)e.Argument;
      BackgroundWorker worker = sender as BackgroundWorker;
      
      switch (selection)
      {
        case SelectFilesByOption.All:
          MoveAll(worker);
          break;

        case SelectFilesByOption.Size:
          MoveSize(worker);
          break;

        case SelectFilesByOption.Date:
          MoveByDate(worker);
          break;

        case SelectFilesByOption.Extension:
          MoveExtensions(worker);
          break;

        default:
          break;
      }
    }

    /// <summary>
    /// Handler used for updating the text on the ProgressWindow object to reflect the current progress.
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Event arguments</param>
    private void ProgressChangedHandler(object sender, ProgressChangedEventArgs e)
    {
      m_progressWindow.ProgressTextBlock.Text = string.Format(ProgressWindow.MovingMessageFormat, e.ProgressPercentage);
    }

    /// <summary>
    /// Handler used for indicating to the user that the moving operation is completed, along with any exception if any.
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Event arguments</param>
    private void ProgressCompletedHandler(object sender, RunWorkerCompletedEventArgs e)
    {
      if (e.Error != null)
      {
        DisplayErrorMessage(e.Error);
        m_progressWindow.Close();
      }
      else
      {
        m_progressWindow.OkButton.Visibility = Visibility.Visible;
        m_progressWindow.ProgressBarWindow.IsIndeterminate = false;
        m_progressWindow.ProgressTextBlock.Text = string.Format(ProgressWindow.CompletedMoveMessageFormat, 
                                                                m_movedFilesCount, m_destinationDirectory);
      }
    }

    private void ProgressCanceledHandler(object sender, EventArgs e)
    {
	throw new NotImplementedException();
    }

    #endregion

    #endregion
  }
}
