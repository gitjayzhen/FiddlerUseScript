    // Page 229
    public override void AddToTab(TabPage oPage)
    {
      // Title my tab
      o.Text = "Raw";                 

      // Create my UserControl and add it to my tab
      myControl = new RawView(this);
      oPage.Controls.Add(myControl);
      oPage.Controls[0].Dock = DockStyle.Fill;
    }
    
    // Page 230
    static byte[] arr_WOFF_MAGIC = 
           new byte[4] {(byte)'w', (byte)'O', (byte)'F', (byte)'F'};

    public override int ScoreForSession(Session oS)
    {
      // Check for WOFF Magic Bytes
      if (Utilities.HasMagicBytes(oS.responseBodyBytes, arr_WOFF_MAGIC)) { 
        return 60;
      }

      // If not found, consult at the response's Content-Type
      return ScoreForContentType(oS.oResponse.MIMEType);
    }

    // Page 231
    public override int ScoreForContentType(string sMIMEType)
    {
      if (sMIMEType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase)) {
        return 60;
      }

      if (sMIMEType.StartsWith("video/", StringComparison.OrdinalIgnoreCase)) {
        return 60;
      }

      // Just in case a site sent a malformed MP3 type, check the whole string
      if (sMIMEType.IndexOf("mp3", StringComparison.OrdinalIgnoreCase) > -1)
        return 60;
      }

      return 0;
    }

    // Page 233
    public override void AssignSession(Session oSession)
    { 
      if ((null == oSession) || !oSession.bHasResponse)
      {
        Clear();
        return;
      }

      UpdateUIFromHeaders(oSession.oResponse.headers);
      UpdateUIFromBody(oSession.responseBodyBytes);
      bool bIsReadOnly = ((oSession.state != SessionStates.HandTamperResponse) 
                        && !oSession.oFlags.ContainsKey("x-Unlocked"));

      UpdateReadOnlyState(bIsReadOnly);
    }

    // Page 234
    public virtual void AssignSession(Session oS)
    {
      if (this is IRequestInspector2)
      {
        IRequestInspector2 oRI = (this as IRequestInspector2);
        oRI.headers = oS.oRequest.headers;
        oRI.body = oS.requestBodyBytes;
        oRI.bReadOnly = ((oS.state != SessionStates.HandTamperRequest) 
                       && !oS.oFlags.ContainsKey("x-Unlocked"));
        return;
      }

      if (this is IResponseInspector2)
      //...


    // Page 235
    if (oHeaders.Exists("Transfer-Encoding") || oHeaders.Exists("Content-Encoding"))
    {
      lblDisplayMyEncodingWarning.Visible = true; 
      return;
    }

    // Page 235
    if (null != oHeaders)
    {
      // Check for no body
      if ((null == value) || (value.Length < 1)) return;

      if (!oHeaders.ExistsAndContains("Content-Type", "application/json") 
          && !oHeaders.ExistsAndContains("Content-Type", "javascript"))
      {
        // Not JSON
        return;
      }

      if (oHeaders.Exists("Transfer-Encoding") || oHeaders.Exists("Content-Encoding"))  
      {
        // Create a copy of the body to avoid corrupting the original
        byte[] arrCopy = (byte[])value.Clone();
        try
        {
          // Decode. Warning: Will throw if value cannot be decoded
          Utilities.utilDecodeHTTPBody(oHeaders, ref arrCopy);
          value = arrCopy;
        }
        catch
        {
          // Leave value alone.
        }
      }
    }

    // Okay, now the body stored in "value" is unchunked 
    // and uncompressed. We need to convert it to a string, 
    // keeping in mind that the HTTP response might have 
    // been in a non-Unicode codepage.

    oEncoding = Utilities.getEntityBodyEncoding(oHeaders, value);
    sJSON = Utilities.GetStringFromArrayRemovingBOM(value, oEncoding);
    myControl.SetJSON(sJSON);

    //...

    // Page 236
    string sRequestBody = oSession.GetRequestBodyAsString();
    string sResponseBody = oSession.GetResponseBodyAsString();

    // Page 237
    // Inspector requires methods introduced in v2.3.9.0...
    [assembly: Fiddler.RequiredVersion("2.3.9.0")]





