    // Page 246
    // Transcoders require methods introduced in v2.3.9.0...
    [assembly: Fiddler.RequiredVersion("2.3.9.0")]

    // Page 246
    [ProfferFormat("HTTPArchive v1.1", "A lossy JSON-based HTTP traffic 
    archive format. Standard is documented @ http://groups.google.com/group/http- 
    archive-specification/web/har-1-1-spec")]

    [ProfferFormat("HTTPArchive v1.2", "A lossy JSON-based HTTP traffic 
    archive format. Standard is documented @ http://groups.google.com/group/http-
    archive-specification/web/har-1-2-spec")]

    public class HTTPArchiveFormatExport: ISessionExporter 
    {
       ///...

    // Page 248
    public bool ExportSessions(string sFormat, Session[] oSessions, 
      Dictionary<string, object> dictOptions, 
      EventHandler<ProgressCallbackEventArgs> evtProgressNotifications)
    {

      //...
      string sFilename = null;
      int iMaxTextBodyLength = DEFAULT_MAX_TEXT_BYTECOUNT;
      int iMaxBinaryBodyLength = DEFAULT_MAX_BINARY_BYTECOUNT;

      if (null != dictOptions)
      { 
        if (dictOptions.ContainsKey("Filename"))
        {
          sFilename = dictOptions["Filename"] as string;
        }

        if (dictOptions.ContainsKey("MaxTextBodyLength"))
        {
          iMaxTextBodyLength = (int)dictOptions["MaxTextBodyLength"];
        }

        if (dictOptions.ContainsKey("MaxBinaryBodyLength"))
        {
           iMaxBinaryBodyLength = (int)dictOptions["MaxBinaryBodyLength"];
        }
      }

    //...

