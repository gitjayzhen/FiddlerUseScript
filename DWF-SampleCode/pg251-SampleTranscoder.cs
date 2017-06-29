// Sample Transcoder from Debugging with Fiddler pg 251

    using System;
    using System.IO;
    using System.Text;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using Fiddler;

    [assembly: AssemblyVersion("1.0.0.0")]
    [assembly: Fiddler.RequiredVersion("2.3.9.5")]

    // Note that this Transcoder only works when loaded by Fiddler itself; it will
    // not work from a FiddlerCore-based application. The reason is that the output
    // uses the columns shown in Fiddler’s Web Sessions list, and FiddlerCore has
    // no such list.

    // Ensure your class is public, or Fiddler won't see it!
    [ProfferFormat("TAB-Separated Values", "Session List in Tab-Delimited Format")]
    [ProfferFormat("Comma-Separated Values", 
        "Session List in Comma-Delimited Format; import into Excel or other tools")]
    public class CSVTranscoder: ISessionExporter
    {
      public bool ExportSessions(string sFormat, Session[] oSessions, 
          Dictionary<string, object> dictOptions,
          EventHandler<ProgressCallbackEventArgs> evtProgressNotifications)
      {
        bool bResult = false; 
        string chSplit;

        // Determine if we already have a filename 
        // from the dictOptions collection
        string sFilename = null;
        if (null != dictOptions && dictOptions.ContainsKey("Filename"))
        {
          sFilename = dictOptions["Filename"] as string;
        }

        // If we don't yet have a filename, prompt the user
        // with a File Save dialog, using the correct file extension
        // for the export format they selected
        if (sFormat == "Comma-Separated Values")
        {
          chSplit = ",";
          if (string.IsNullOrEmpty(sFilename)) 
            sFilename = Fiddler.Utilities.ObtainSaveFilename(
              "Export As " + sFormat, "CSV Files (*.csv)|*.csv");
        }
        else
        {
          // Ensure caller asked for Tab-delimiting.
          if (sFormat != "TAB-Separated Values") return false;
          chSplit = "\t";
          if (string.IsNullOrEmpty(sFilename)) 
            sFilename = Fiddler.Utilities.ObtainSaveFilename(
              "Export As " + sFormat, "TSV Files (*.tsv)|*.tsv");
        }

        // If we didn't get a filename, user cancelled. If so, bail out.
        if (String.IsNullOrEmpty(sFilename)) return false;

        try
        {
          StreamWriter swOutput = new StreamWriter(sFilename, false, Encoding.UTF8);
          int iCount = 0;
          int iMax = oSessions.Length;

          #region WriteColHeaders
          bool bFirstCol = true;
          foreach (ColumnHeader oLVCol in FiddlerApplication.UI.lvSessions.Columns)
          {
            if (!bFirstCol)
            {
              swOutput.Write(chSplit);
            }
            else
            {
              bFirstCol = false;
            }
        
            // Remove any delimiter characters from the value
            swOutput.Write(oLVCol.Text.Replace(chSplit, ""));
          }

          swOutput.WriteLine();
          #endregion WriteColHeaders

          #region WriteEachSession
          foreach (Session oS in oSessions)
          {
            iCount++;

            // The ViewItem object is the ListViewItem in the Web Sessions list
            // Obviously, this doesn't exist in FiddlerCore-based applications
            if (null != oS.ViewItem)
            {
              bFirstCol = true;
              ListViewItem oLVI = (oS.ViewItem as ListViewItem);
              if (null == oLVI) continue;
              foreach (ListViewItem.ListViewSubItem oLVC in oLVI.SubItems)
              {
                if (!bFirstCol)
                {
                  swOutput.Write(chSplit);
                }
                else
                {
                  bFirstCol = false;
                } 

              // Remove any delimiter characters from the value
              swOutput.Write(oLVC.Text.Replace(chSplit, ""));
            }

            swOutput.WriteLine();
          }

          // Notify the caller of our progress
          if (null != evtProgressNotifications)
          {
             ProgressCallbackEventArgs PCEA = 
               new ProgressCallbackEventArgs((iCount/(float)iMax), 
                "wrote " + iCount.ToString() + " records.");
            evtProgressNotifications(null, PCEA);

            // If the caller tells us to cancel, abort quickly
            if (PCEA.Cancel) { swOutput.Close(); return false; }
          }
        }
        #endregion WriteEachSession

          swOutput.Close();
          bResult = true;
        }
        catch (Exception eX)
        {
          // TODO: Replace alert with FiddlerApplication.Log.LogFormat(...
          MessageBox.Show(eX.Message, "Failed to export");
          bResult = false;
        }
      
        return bResult;
      }

      public void Dispose() { /*no-op*/ }
    }
