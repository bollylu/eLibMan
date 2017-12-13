using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace FolderManagement {
  public class getListUtils {

    #region Private variables
    private string _pathname;
    private List<string> _extensions;
    private BackgroundWorker _worker;
    private ProgressBar _assignedProgressBar;
    private Control _assignedControl;
    private bool LoadCompleted;
    #endregion Private variables

    #region Public properties
    public TBookCollection BookList { get; set; }
    public string CurrentFolder { get; set; }
    #endregion Public properties

    #region Events
    public event EventHandler onLoadCompleted;
    #endregion Events

    #region Constructors
    public getListUtils( string pathname, string[] aExtension, ProgressBar progressBar, Control control) {
      _pathname = pathname;
      _extensions = new List<string>(aExtension);
      _assignedProgressBar = progressBar;
      BookList = new TBookCollection();
      CurrentFolder = "";
      _assignedControl = control;
    }
    public getListUtils( string pathname, string[] aExtension ) : this(pathname, aExtension, null, null) {
    }
    #endregion Constructors

    #region Public methods
    public void LoadAsync() {
      _worker = new BackgroundWorker();
      if ( _assignedProgressBar != null ) {
        _assignedProgressBar.Minimum = 0;
        _assignedProgressBar.Maximum = Directory.GetDirectories(_pathname).Length;
        _assignedProgressBar.Value = 0;
        _worker.WorkerReportsProgress = true;
        _worker.ProgressChanged += new ProgressChangedEventHandler(_worker_ProgressChanged);
      } else {
        _worker.WorkerReportsProgress = false;
      }
      _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_worker_RunWorkerCompleted);
      _worker.DoWork += new DoWorkEventHandler(_load);
      _worker.RunWorkerAsync(_pathname);
    }
    public void Load() {
      //LoadCompleted = false;
      //LoadAsync();
      //while ( !LoadCompleted ) {
      //  Application.DoEvents();
      //}
      lock ( BookList ) {
        BookList.AddRange(_getSubDirs(_pathname, null, 0,0));
      }
    }
    #endregion Public methods

    #region Private methods
    private void _load( object sender, DoWorkEventArgs e ) {
      string Pathname = (string)e.Argument;
      BackgroundWorker CurrentWorker = sender as BackgroundWorker;
      e.Result = _getSubDirs(Pathname, CurrentWorker, 1, 0);
    }

    private TBookCollection _getSubDirs( string pathname, BackgroundWorker worker, double percentage, int level ) {
      TBookCollection CurrentBookList = new TBookCollection();
      DirectoryInfo oDirInfo = new DirectoryInfo(pathname);
      
      foreach ( DirectoryInfo oSubDirectoryInfo in oDirInfo.GetDirectories() ) {
        CurrentFolder = oSubDirectoryInfo.FullName;
        worker.ReportProgress((int)percentage, oSubDirectoryInfo.FullName);
        try {
          if ( oSubDirectoryInfo.Attributes != FileAttributes.System && oSubDirectoryInfo.GetDirectories().Length > 0 ) {
            Trace.WriteLine(oSubDirectoryInfo.FullName, "trace");
            CurrentBookList.AddRange(_getSubDirs(oSubDirectoryInfo.FullName, worker, percentage, level+1));
          }
          if ( oSubDirectoryInfo.Attributes != FileAttributes.System && oSubDirectoryInfo.GetFiles().Length > 0 ) {
            if ( (oSubDirectoryInfo.GetFiles("thumbs.db", SearchOption.TopDirectoryOnly).Length == 1) && (oSubDirectoryInfo.GetFiles().Length == 1) ) {
              // skip folder
            } else {
              CurrentBookList.Add(new TBook(oSubDirectoryInfo.Name, Path.GetDirectoryName(oSubDirectoryInfo.FullName)));
              CurrentBookList.AddRange(_getFiles(oSubDirectoryInfo));
            }
          }
          if ( level == 0 ) {
            percentage++;
          }
        } catch ( Exception ex ) {
          Trace.WriteLine(string.Format("Error : {0}", ex.Message));
        }
      }
      return CurrentBookList;
    }
    private TBookCollection _getFiles( DirectoryInfo oDirectory ) {
      TBookCollection oRetVal = new TBookCollection();
      foreach ( string sExtension in _extensions ) {
        foreach ( FileInfo oFileInfo in oDirectory.GetFiles("*." + sExtension) ) {
          oRetVal.Add(new TBook(oFileInfo.Name, Path.GetDirectoryName(oDirectory.FullName), TBook.BookTypeEnum.Pdf));
        }
      }
      return oRetVal;
    }
    
    private void _worker_DoWork( object sender, DoWorkEventArgs e ) {
      throw new NotImplementedException();
    }
    private void _worker_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e ) {
      if ( !e.Cancelled ) {
        lock ( BookList ) {
          BookList.AddRange((TBookCollection)e.Result);
        }
        if ( _assignedProgressBar != null ) {
          _assignedProgressBar.Value = _assignedProgressBar.Maximum;
        }
      }
      if ( onLoadCompleted != null ) {
        onLoadCompleted(this, EventArgs.Empty);
      }
      LoadCompleted = true;
      
    }
    private void _worker_ProgressChanged( object sender, ProgressChangedEventArgs e ) {
      if ( _assignedProgressBar != null ) {
        _assignedProgressBar.Value = Math.Min(100, e.ProgressPercentage);
      }
      if ( _assignedControl != null ) {
        _assignedControl.Text = (string)e.UserState;
      }
    }

    #endregion Private methods
  }
}
