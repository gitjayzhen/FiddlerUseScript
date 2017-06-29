    // Page 101
    FiddlerApplication.ResponseHeadersAvailable += delegate(Fiddler.Session oS)
      {
        // This block enables streaming for files larger than 5mb
        if (oS.oResponse.headers.Exists("Content-Length"))
        {
          int iLen = 0;
          if (int.TryParse(oS.oResponse["Content-Length"], out iLen))
          {
             // File larger than 5mb? Don't save its content
            if (iLen > 5000000)
            {
                oS.bBufferResponse = false;
                oS["log-drop-response-body"] = "save memory";
            }
          }
        }
      };

    // Page 259
    FiddlerApplication.OnNotification += delegate(object s, 
      NotificationEventArgs oNEA) 
    { 
      Console.WriteLine("** NotifyUser: " + oNEA.NotifyString); 
    };

    FiddlerApplication.Log.OnLogString += delegate(object s, LogEventArgs oLEA) 
    { 
      Console.WriteLine("** LogString: " + oLEA.LogString); 
    };

    // Page 259
    FiddlerApplication.BeforeRequest += delegate(Session oS)
    {
      // Buffer response to allow response tampering
      oS.bBufferResponse = true;

      // Use a thread-safe mechanism to update my List<Session>
      Monitor.Enter(oAllSessions);
      oAllSessions.Add(oS);
      Monitor.Exit(oAllSessions);
    };

    // Page 259
    FiddlerApplication.BeforeResponse += delegate(Fiddler.Session oS) 
    {
      oS.utilDecodeResponse(); 
      // Note: This change only takes effect properly if 
      // oS.bBufferResponse was set to true earlier!
      oS.utilReplaceInResponse("<title>", "<title>INJECTED!!");
    };

    // Page 259
    // The default flags are your best bet
    FiddlerCoreStartupFlags oFCSF = FiddlerCoreStartupFlags.Default;

    // ...but if, say, we don't want FiddlerCore to Decrypt
    // HTTPS traffic, we can unset that flag at this point
    oFCSF = (oFCSF & ~FiddlerCoreStartupFlags.DecryptSSL);

    // Page 260
    // Start listening on port 8877
    FiddlerApplication.Startup(8877, oFCSF);

    // Page 261
    FiddlerCoreStartupFlags oFlags = 
        (FiddlerCoreStartupFlags.Default & ~FiddlerCoreStartupFlags.DecryptSSL); 

    // Page 262
    FiddlerApplication.OnValidateServerCertificate += new 
      System.EventHandler<ValidateServerCertificateEventArgs>(CheckCert);

    void CheckCert(object sender, ValidateServerCertificateEventArgs e)
    {
      // If there's an obvious issue with the presented certificate, 
      // it will be rejected unless overridden here.
      if (SslPolicyErrors.None != e.CertificatePolicyErrors)
      {
        return;	// Certificate will be rejected
      }
                
      // Check if the Convergence Certificate Notary services have
      // an opinion about this certificate chain.
      bool bNotariesAffirm = GetNotaryConsensus(e.Session, 
        e.ServerCertificate, e.ServerCertificateChain);
            
      FiddlerApplication.Log.LogFormat("Notaries have indicated that the " 
        + "certificate presented for {0} is {1}", e.ExpectedCN, 
        bNotariesAffirm ? "VALID" : "INVALID");

      if (!bNotariesAffirm)
      {
        e.ValidityState = CertificateValidity.ForceInvalid;
        return;
      }

      e.ValidityState = CertificateValidity.ForceValid;
    }

    // Page 263
    FiddlerApplication.OnReadResponseBuffer += new 
      EventHandler<RawReadEventArgs>(OnRead);

    static void OnRead(object sender, RawReadEventArgs e)
    {
      Console.WriteLine(String.Format("Read {0} response bytes for session {1}", 
        e.iCountOfBytes, e.sessionOwner.id));

      // NOTE: arrDataBuffer is a fixed-size array. Only bytes 0 to 
      // iCountOfBytes should be read/manipulated.
      
      // Just for kicks, lowercase every ASCII char. Note that this will 
      // obviously mangle any binary MIME files and break many types of markup
      for (int i = 0; i < e.iCountOfBytes; i++)
      {
        if ((e.arrDataBuffer[i] > 0x40) && (e.arrDataBuffer[i] < 0x5b))
        {
          e.arrDataBuffer[i] = (byte)(e.arrDataBuffer[i] + (byte)0x20);
        }
      }
    }

    //pg 263
    FiddlerApplication.ResponseHeadersAvailable += delegate(Session oS) 
    {
      // Disable streaming for HTML responses on a target server so that
      // we can modify those responses in the BeforeResponse handler
      if (oS.HostnameIs("example.com") && oS.oResponse.MIMEType.Contains("text/html"))
      { 
        oS.bBufferResponse = true;
      }
    };

    // Page 264
    FiddlerApplication.BeforeReturningError += delegate(Session oS) 
    { 
      string sErrMsg = oS.GetResponseBodyAsString();

      oS.utilSetResponseBody("<!doctype html><title>AcmeCorp Error Page</title>"
        + "<body>Sorry, this page or service is presently unavailable. Please try"
        + " again later. <br /><pre>" + sErrMsg + "</pre></html>");
    };

    // Page 265
    Proxy oSecureEP = FiddlerApplication.CreateProxyEndpoint(8777, true, "localhost");
    if (null != oSecureEP)
    {
      FiddlerApplication.Log.LogString("Created secure endpoint listening "
        + "on port 8777, which will send a HTTPS certificate for 'localhost'");
    }

    // Page 266
    FiddlerApplication.Log.LogFormat("Session {0} received by EndPoint on Port #{1}",
      oSession.id, 
      (null != oSession.oRequest.pipeClient) ? 
         "n/a" : oSession.oRequest.pipeClient.LocalPort
    );

    // Page 268
    // Inside your main object, create a list to hold the Sessions
    // The generic list type requires you are #using System.Collections.Generic
    List<Fiddler.Session> oAllSessions = new List<Fiddler.Session>();
 
    // Add Sessions to the list as they are captured
    Fiddler.FiddlerApplication.BeforeRequest += delegate(Fiddler.Session oS) 
    {
      Monitor.Enter(oAllSessions);
      oAllSessions.Add(oS);
      Monitor.Exit(oAllSessions);
    };

    // Page 268
    Fiddler.URLMonInterop.SetProxyInProcess("127.0.0.1:7777", "<-loopback>");

    // Page 269
    private bool CreateAndTrustRoot()
    {
      // Ensure root exists
      if (!Fiddler.CertMaker.rootCertExists())
      {
        bCreatedRootCertificate = Fiddler.CertMaker.createRootCert();
        if (!bCreatedRootCertificate) return false;
      }

      // Ensure root is trusted
      if (!Fiddler.CertMaker.rootCertIsTrusted())
      {
        bTrustedRootCert = Fiddler.CertMaker.trustRootCert();
        if (!bTrustedRootCert) return false;
      }

      return true;
    }

    // Page 269
    private static bool setMachineTrust(X509Certificate2 oRootCert)
    {
      try
      {
        X509Store certStore = new X509Store(StoreName.Root,
                                            StoreLocation.LocalMachine);
        certStore.Open(OpenFlags.ReadWrite);
        try
        {
          certStore.Add(oRootCert);
        }
        finally
        {
           certStore.Close();
        }
        return true;
      }
      catch (Exception eX)
      {
        return false;
      }
    }

    // Page 270
    Fiddler.FiddlerApplication.BeforeRequest += delegate(Fiddler.Session oS)
    {
      if (oS.uriContains("replaceme.txt"))
      {
        oS.utilCreateResponseAndBypassServer();
        oS.responseBodyBytes = SessionIWantToReturn.responseBodyBytes;
        oS.oResponse.headers = 
          (HTTPResponseHeaders) SessionIWantToReturn.oResponse.headers.Clone();
      }
    };


