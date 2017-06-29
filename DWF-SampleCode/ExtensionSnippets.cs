    // Page 208
    public void AutoTamperRequestBefore(Session oSession) 
    {
      // Return immediately if no rule is enabled
      if (!bBlockerEnabled) return;
      // ...

    // Page 208
    public void AutoTamperRequestBefore(Session oSession) 
    {
      // Return immediately if no rule is enabled
      if (!bBlockerEnabled) return;
      // ...

    // Page 209
    public void OnLoad()
    {
      myPage = new TabPage("FiddlerScript");
      myPage.ImageIndex = (int)Fiddler.SessionIcons.Script;
      this.lblLoading = new System.Windows.Forms.Label();
      this.lblLoading.Text = "Loading...";
      myPage.Controls.Add(lblLoading);
      FiddlerApplication.UI.tabsViews.TabPages.Add(myPage);

      TabControlEventHandler evtTCEH = null;
      evtTCEH = delegate(object s, TabControlEventArgs e) {
        if (e.TabPage == myTabPage)
        {
          // Create heavyweight components used to display UI
          EnsureReady();

          // Remove the unneeded event handler.
          FiddlerApplication.UI.tabsViews.Selected -= evtTCEH;
        }
      };

      // Subscribe to tab-change events
      FiddlerApplication.UI.tabsViews.Selected += evtTCEH;
    }

    // Page 209
    private void EnsureReady()
    {
      if (null != oEditor) return;    // Exit if we've already been made ready

      lblLoading.Refresh();           // Force repaint of "Loading..." label

      // Create the extension's UI (slow)
      oEditor = new RulesEditor(myPage);
      lblLoading.Visible = false;     // Remove the "Loading..." label
    }

    // Page 211
    void UpdateStatsTab(Session[] _arrSessions)
    {
      // If we're not showing the Stats tab right now, bail out.
      if (FiddlerApplication.UI.tabsViews.SelectedTab !=
          FiddlerApplication.UI.pageStatistics)
      {
        return;
      }

      try
      {
        if (_arrSessions.Length < 1) 
        {
          ClearStatsTab();
          return;
        }

        Dictionary<string, long> dictResponseSizeByContentType;
        long cBytesRecv;
        string sStats = BasicAnalysis.ComputeBasicStatistics(
        _arrSessions, true, out dictResponseSizeByContentType, out cBytesRecv);

        txtReport.Text = String.Format("{0}\r\n, sStats);  
      }
      catch (Exception eX)
      {
         Debug.Assert(false, eX.Message);
      }
    }

    // Page 212
    FiddlerApplication.UI.actSelectSessionsMatchingCriteria( 
      delegate(Session oS) 
      {
        return oS.HTTPMethodIs("POST");
      });

    // Page 212
    FiddlerApplication.UI.actSelectSessionsMatchingCriteria(
      delegate(Session oS) 
      { 
        return (200 == oS.responseCode);
      });

    // Page 214
    oSession.oRequest["HeaderName"] == oSession.oRequest.headers["HeaderName"] 
        == oSession["REQUEST", "HeaderName"];

    oSession.oResponse["HeaderName"] == oSession.oResponse.headers["HeaderName"] 
        == oSession["RESPONSE", "HeaderName"];

    // Page 215
    oSession.oFlags["FlagName"] == oSession["FlagName"] 
        == oSession["SESSION", "FlagName"];

    // Page 215
    if (!oSession.oFlags.ContainsKey("SomeKey") 
    {
      Debug.Assert(oSession["SomeKey"] == null);
      Debug.Assert(oSession.oFlags["SomeKey"] == null);
      Debug.Assert(oSession["Session", "SomeKey"] == String.Empty);
    }


    // Page 219
    bool bHadAnyHTTPErrors = 
        oSession.isAnyFlagSet(SessionFlags.ProtocolViolationInRequest 
                            | SessionFlags.ProtocolViolationInResponse);

    bool bReusedBothConnections =
        oSession.isFlagSet(SessionFlags.ClientPipeReused
                         | SessionFlags.ServerPipeReused);

    // Page 224
    FiddlerApplication.Prefs.SetStringPref("example.str", "Remember me!");
    FiddlerApplication.Prefs.SetBoolPref("example.bool", true);
    FiddlerApplication.Prefs.SetInt32Pref("example.int", 5);

    // Page 224
    FiddlerApplication.Prefs["example.str"] = "value";
    
    // Page 224
    // These three lines are equivalent
    FiddlerApplication.Prefs.RemovePref("NameToRemove");
    FiddlerApplication.Prefs.SetStringPref("NameToRemove", null);
    FiddlerApplication.Prefs["NameToRemove"] = null;

    // Page 224
    string sStr = FiddlerApplication.Prefs.GetStringPref("example.str", "demo");
    bool bBool = FiddlerApplication.Prefs.GetBoolPref("example.bool", false);
    int iNum = FiddlerApplication.Prefs.GetInt32Pref("example.int", 0);

    // Page 224
    string sStr = FiddlerApplication.Prefs["example.str"];
    
    // Page 240
    // See pg240-SampleExtension.cs file
    
    // Page 245
    public void OnLoad()
    {
      oPage = new TabPage("Timeline");
      oPage.ImageIndex = (int)Fiddler.SessionIcons.Timeline;
      FiddlerApplication.UI.tabsViews.TabPages.Add(oPage); 
    }

    // Page 245
    // Extension requires methods introduced in v2.3.9.0...
    [assembly: Fiddler.RequiredVersion("2.3.9.0")]



    
