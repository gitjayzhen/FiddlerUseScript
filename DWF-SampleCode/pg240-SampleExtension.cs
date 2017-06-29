using Fiddler;
using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text.RegularExpressions;

[assembly: Fiddler.RequiredVersion("2.3.9.0")]
[assembly: AssemblyVersion("1.0.1.0")]
[assembly: AssemblyDescription("Scans for Cookies and P3P")]

public class TagCookies : IAutoTamper2
    {
      private bool bEnabled = false;
      private bool bEnforceP3PValidity = false;
      private bool bCreatedColumn = false;
      private MenuItem miEnabled;
      private MenuItem miEnforceP3PValidity;
      private MenuItem mnuCookieTag;

      private enum P3PState
      {
        NoCookies,
        NoP3PAndSetsCookies,
        P3POk,
        P3PUnsatisfactory,
        P3PMalformed
      }

      public void OnLoad()
      {
        /*
          NB: OnLoad might not get called until ~after~ one of 
          the AutoTamper methods was called, because sessions are
          processed while Fiddler is loading.
          This is okay for us, because we created our mnuCookieTag 
          in the constructor and its simply not
          visible anywhere until this method is called and we 
          merge it onto the Fiddler Main menu.
        */

        FiddlerApplication.UI.mnuMain.MenuItems.Add(mnuCookieTag);
      }

      // We don’t need to do anything on unload, since Fiddler only presently
      // unloads extensions at shutdown, and the GC will dispose of our UI.
      public void OnBeforeUnload() { /*noop*/ }

      private void InitializeMenu()
      {
        this.miEnabled = new MenuItem("&Enabled");
        this.miEnforceP3PValidity = new MenuItem("&Rename P3P header if invalid");
    
        this.miEnabled.Index = 0;
        this.miEnforceP3PValidity.Index = 1;

        this.mnuCookieTag = new MenuItem("Privacy");
        this.mnuCookieTag.MenuItems.AddRange(new MenuItem[] { 
          this.miEnabled, this.miEnforceP3PValidity });

        this.miEnabled.Click += new System.EventHandler(this.miEnabled_Click);
        this.miEnabled.Checked = bEnabled;

        this.miEnforceP3PValidity.Click += 
          new System.EventHandler(this.miEnforceP3PValidity_Click);

        this.miEnforceP3PValidity.Checked = bEnforceP3PValidity;
      }

      public void miEnabled_Click(object sender, EventArgs e)
      {
        miEnabled.Checked = !miEnabled.Checked;
        bEnabled = miEnabled.Checked;
        this.miEnforceP3PValidity.Enabled = bEnabled;
        if (bEnabled) { EnsureColumn(); }
        FiddlerApplication.Prefs.SetBoolPref("extensions.tagcookies.enabled",
                                             bEnabled);
      }

      public void miEnforceP3PValidity_Click(object sender, EventArgs e)
      {
        miEnforceP3PValidity.Checked = !miEnforceP3PValidity.Checked;
        bEnforceP3PValidity = miEnforceP3PValidity.Checked;
        FiddlerApplication.Prefs.SetBoolPref(
          "extensions.tagcookies.EnforceP3PValidity", bEnforceP3PValidity);
      }

      private void EnsureColumn()
      {
        // If we already created the column, bail out.
        if (bCreatedColumn) return;

        // Add a new Column to the Web Sessions list, titled "Privacy Info",
        // that will automatically fill with each Session's X-Privacy flag string
        FiddlerApplication.UI.lvSessions.AddBoundColumn(
                                           "Privacy Info", 1, 120, "X-Privacy");
        bCreatedColumn = true;
      }

      public TagCookies()
      {
        this.bEnabled = FiddlerApplication.Prefs.GetBoolPref(
         "extensions.tagcookies.enabled", false);
        
        this.bEnforceP3PValidity = FiddlerApplication.Prefs.GetBoolPref(
          "extensions.tagcookies.EnforceP3PValidity", true);

         InitializeMenu();

         if (bEnabled) 
             { EnsureColumn(); } else 
             { this.miEnforceP3PValidity.Enabled = false; }
      }

      private void SetP3PStateFromHeader(string sValue, ref P3PState oP3PState)
      {
        // If there was no P3P header, bail out
        if (string.IsNullOrEmpty(sValue)) { return; }

        string sUnsatCat = String.Empty;
        string sUnsatPurpose = String.Empty;
        sValue = sValue.Replace('\'', '"');

        string sCP = null;

        // Use a Regular Expression to search the header for a CP attribute
        Regex r = new Regex("CP\\s?=\\s?[\"]?(?<TokenValue>[^\";]*)");
        Match m = r.Match(sValue);
        if (m.Success && (null != m.Groups["TokenValue"]))
        {
          sCP = m.Groups["TokenValue"].Value;
        }

        // If we didn't find a Compact Policy statement, bail out.
        if (String.IsNullOrEmpty(sCP)) { return; }

        // Okay, we've got a compact policy string. Evaluate each token.
        oP3PState = P3PState.P3POk;
        string[] sTokens = sCP.Split(new char[] { ' ' },
            StringSplitOptions.RemoveEmptyEntries);

        foreach (string sToken in sTokens)
        {
          // Reject clearly invalid tokens...
          if ((sToken.Length < 3) || (sToken.Length > 4))
          {
            oP3PState = P3PState.P3PMalformed;
            return;
          }

          // Track any tokens with "Unacceptable" privacy category
          if (",PHY,ONL,GOV,FIN,".IndexOf("," + sToken + ",",
                StringComparison.OrdinalIgnoreCase) > -1)
          {
            sUnsatCat += (sToken + " ");
            continue;
          }

          // Track any tokens with "Unacceptable" privacy purposes
          if (",SAM,OTR,UNR,PUB,IVA,IVD,CON,TEL,OTP,".IndexOf("," + sToken + ",",
                StringComparison.OrdinalIgnoreCase) > -1)
          {
            sUnsatPurpose += (sToken + " ");
            continue;
          }

          // TODO: Check each token against the list of 70-some valid tokens and 
          // reject if it’s not found.
        }

        // If a cookie contains an unsatisfactory purpose and an unsatisfactory
        // category, tag it. Learn more about "Unsatisfactory cookies" at 
        // http://msdn.microsoft.com/en-us/library/ie/ms537343(v=vs.85).aspx
        if ((sUnsatCat.Length > 0) && (sUnsatPurpose.Length > 0))
        {
          if (oP3PState == P3PState.P3POk)
          {
            oP3PState = P3PState.P3PUnsatisfactory;
          }
        }
      }

      // On each HTTP response, examine the response headers for attempts to set 
      // cookies, and check for a P3P header too. We do this 
      // in OnPeekAtResponseHeaders rather than OnBeforeResponse because we only
      // need the headers and do not need to wait for the responseBodyBytes to 
      // be available.
      public void OnPeekAtResponseHeaders(Session oSession) 
      {
        // If our extension isn't enabled, bail fast
        if (!bEnabled) return;

        P3PState oP3PState = P3PState.NoCookies;
        if (!oSession.oResponse.headers.Exists("Set-Cookie")) { return; }

        oP3PState = P3PState.NoP3PAndSetsCookies;

        if (oSession.oResponse.headers.Exists("P3P"))
        {
          SetP3PStateFromHeader(oSession.oResponse.headers["P3P"], ref oP3PState);
        }

        // Based on the cookie/P3P state, set the background color of item in the
        // Web Sessions list. Also set the X-Privacy flag which is shown in the
        // column that we created.
        switch (oP3PState)
        {
          case P3PState.P3POk:
            oSession["ui-backcolor"] = "#ACDC85";
            oSession["X-Privacy"] = "Sets cookies & P3P";
          break;

          case P3PState.NoP3PAndSetsCookies:
            oSession["ui-backcolor"] = "#FAFDA4";
            oSession["X-Privacy"] = "Sets cookies without P3P";
          break;

          case P3PState.P3PUnsatisfactory:
            oSession["ui-backcolor"] = "#EC921A";
            oSession["X-Privacy"] = "Sets cookies; P3P unsat. for 3rd-party use";
          break;

          case P3PState.P3PMalformed:
            oSession["ui-backcolor"] = "#E90A05";
            if (bEnforceP3PValidity)
            {
              oSession.oResponse.headers["MALFORMED-P3P"] =
                  oSession.oResponse.headers["P3P"];
              oSession["X-Privacy"] = "MALFORMED P3P: " + 
                  oSession.oResponse.headers["P3P"];

              // Delete the invalid header to prevent the client from seeing it.
              oSession.oResponse.headers.Remove("P3P");
            }
          break;
        }
      }

      // Our extension doesn't need any of the other AutoTamper* methods.
      public void AutoTamperRequestBefore(Session oSession) {/*noop*/}
      public void AutoTamperRequestAfter(Session oSession) {/*noop*/}
      public void AutoTamperResponseAfter(Session oSession) {/*noop*/}
      public void AutoTamperResponseBefore(Session oSession) {/*noop*/}
      public void OnBeforeReturningError(Session oSession) {/*noop*/}
}
