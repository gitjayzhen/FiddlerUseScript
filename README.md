# FiddlerUseScript

### 简介：抓包工具，调试接口除了已有的功能，还可以使用灵活的脚本来处理一些场景

### 欢迎各位朋友 request 自己的经验方案

**************************************************************************************************************
** 官网脚本api示例教程：http://docs.telerik.com/fiddler/KnowledgeBase/FiddlerScript/ModifyRequestOrResponse  

** Fiddler命令行教程：http://docs.telerik.com/fiddler/knowledgebase/quickexec  

** FiddlerScript的api：http://fiddlerbook.com/Fiddler/dev/ScriptSamples.asp

** jscript.net官网教程：https://msdn.microsoft.com/en-us/library/91td9cas(v=vs.80).aspx

** google网上论坛：https://groups.google.com/forum/?fromgroups#%21forum/httpfiddler
*************************************************************************************************************

[TOC]


### 该文件在window的文件目录位置：C:\Users\"you_pc_name"\Documents\Fiddler2\Scripts

# FiddlerScript的场景应用(20180412, 更新：20200817)：

## +++++++++++++++++beforerequest+++++++++++++++++++++++

1. 在此处【设置代理网络限速】1KB的量 50Kb/s需要delay 160ms <br>
  带宽：mbps kbps (比特流)  网速：KB/s MB/s （字节流）   <br>
  修改完记得勾选【simulate modem speeds】[randInt(1,50) 模拟网络抖动]<br>
  
```
上传带宽 = 1KB/300ms = (1 * 8/1000) /0.300 ≈  0.027Mbps
下载带宽 = 1KB/150ms = (1 * 8/1000) /0.150 ≈ 0.053Mbps
（1MB = 1024 KB ≈ 1000 KB 这里为了运算简便就用了1000的倍数，忽略误差）
```
  
```
    static function randInt(min, max) {
        return Math.round(Math.random()*(max-min)+min);
    }
    if (m_SimulateModem) {
        // Delay sends by 300ms per KB uploaded.
        oSession["request-trickle-delay"] = ""+randInt(1,50);
        // Delay receives by 150ms per KB downloaded.
        oSession["response-trickle-delay"] = ""+randInt(1,50);
    }
```
2. 在此处【过滤并高亮显示host】
```
   if( oSession.host.IndexOf("host") > -1 || oSession.host.IndexOf("host") > -1){
     	oSession["ui-color"] = "green";
    }
```

3. 在此处【过滤url并高亮显示】
```
   if(oSession.url.IndexOf("url_path") > -1){
     oSession["ui-color"] = "yellow";
    }
```
4. 在此处【重定向urlplace】host和url的判断  
```
  if(oSession.HostnameIs("host") && oSession.url.IndexOf("url_path") > -1){
	    oSession.hostname = "api.mobile.xxx.com"
		  }
```
5. 在此处【设置请求的header】，测试网络爬虫时候会用，
```
// TSET FOR Spider： 根据网站来限定请求
if (oSession.HostnameIs("test.com")) {
    // 模拟修改请求的用户端ip，这种情况对独立的网络有效，对于公司级的网络，还是有一些问题，需要借助vpn
    oSession.oRequest["X-Forwarded-For"]="16.12.23.16";
    // 修改请求的header
    oSession.oRequest["User-Agent"] = "spider Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.81 Safari/537.36 SE 2.X MetaSr 1.0";

}
```


2019.04.26 手机模拟添加cookie（区别于种cookie 在onBeforeResponse里）

* 删除所有的cookie
`oSession.oRequest.headers.Remove("Cookie");`

* 新建cookie
`oSession.oRequest.headers.Add("Cookie", "username=testname;testpassword=P@ssword1");`

注意: Fiddler script不能直接删除或者编辑单独的一个cookie， 你需要用replace方法或者正则表达式的方法去操作cookie的string

```
if (oSession.HostnameIs("******") && oSession.oRequest.headers.Exists("Cookie") ) {

    var sCookie = oSession.oRequest["Cookie"]; 
    //用replace方法或者正则表达式的方法去操作cookie的string
    //sCookie = sCookie.Replace("cookieName=", "ignoreme="); 
    sCookie = sCookie + ";tt_spver=1";

    oSession.oRequest["Cookie"] = sCookie; 
}
```

## +++++++++++++++++beforerespond+++++++++++++++++++++

5. 需要在返回头这里就设置buffer处理，否则，后续无法在onBeforeResponse中修改body（修改的动作不会阻塞原来的返回）

```
    static function OnPeekAtResponseHeaders(oSession: Session) {
        if (oSession.HostnameIs("cmshow.qq.com") && oSession.oResponse.headers.ExistsAndContains("Content-Type","text/html")){
            oSession.bBufferResponse = true;    
        }
    }	
```
6. 在此处修改response的bady内容【使用正则匹配方式】
```
   if(oSession.HostnameIs("host") && oSession.url.IndexOf("url_path") > -1){
        // 获取response中的body字符串
        var strBody=oSession.GetResponseBodyAsString();
        // 用正则表达式或者replace方法去修改string
        var regx = '"stream_mode":\d*?'
        strBody=strBody.replace(regx,'"stream_mode":0');
        // 弹个对话框检查下修改后的body               
        FiddlerObject.alert(strBody);
        // 将修改后的body，重新写回Request中
        oSession.utilSetResponseBody(strBody);
    }
```

7. 在此处修改json中的数据【修改接口字段的值】
```
    if(oSession.HostnameIs("host") && oSession.url.IndexOf("url_path") > -1){
        // 获取Response Body中JSON字符串
        var responseStringOriginal =  oSession.GetResponseBodyAsString();
        // 转换为可编辑的JSONObject变量
        var responseJSON = Fiddler.WebFormats.JSON.JsonDecode(responseStringOriginal);
        // 修改JSONObject变量，修改字段数据
        responseJSON.JSONObject["new_core"] = "True";  
        responseJSON.JSONObject["stream_mode"] = 5;
        // 重新设置Response Body
        var responseStringDestinal = Fiddler.WebFormats.JSON.JsonEncode(responseJSON.JSONObject);
        oSession.utilSetResponseBody(responseStringDestinal);
	}
```
8. 在此处修改json中的数据【增加接口字段=值】
```
    if(oSession.HostnameIs("host") && oSession.url.IndexOf("url_path") > -1){
        // 获取Response Body中JSON字符串
        var responseStringOriginal =  oSession.GetResponseBodyAsString();
        // 转换为可编辑的JSONObject变量
        var responseJSON = Fiddler.WebFormats.JSON.JsonDecode(responseStringOriginal);
        // 修改JSONObject变量，修改字段数据
        responseJSON.JSONObject["type_arr"] = ["bullet"];
        // 重新设置Response Body
        var responseStringDestinal = Fiddler.WebFormats.JSON.JsonEncode(responseJSON.JSONObject);
        oSession.utilSetResponseBody(responseStringDestinal);
        }
```
9. 使指定URL支持CORS跨域请求有时候，你调用一个 json 接口，发现跨域了，你需要去找接口的开发人支持跨域，显然傻傻等待后端开发完毕再联调是低效率的，
这个时候就就要在后台改完之前就自己实现跨域的模拟，此时 fiddler 显然是再好不过的利器。支要持 CORS 跨域，
就是要为请求的返回头增加  Access-Control-Allow-Origin 属性，因此需要修改 OnBeforeResponse函数，在该函数的末尾添加你的 CORS 逻辑

```
    static function OnBeforeResponse(oSession: Session) {
        	...

        	if(oSession.uriContains("要处理的url")){
        		oSession.oResponse["Access-Control-Allow-Origin"] =  "允许的域名";
        		oSession.oResponse["Access-Control-Allow-Credentials"] = true;
        	}
    }
```
10. 同一域名不同端口或目录转发到不同服务器某些情况下，一个域名部署了多个业务的应用，但有时候你只需要修改自己的应用，这个时候你会使用hosts把该域名指向开发机，但问题来了，该域名下所有的应用都指向了同一个开发机，如何使得其他应用仍然指向正式环境？显然依靠传统的hosts工具无法解决这个问题，这时就需要编写fiddler规则脚本了：
```
    static function OnBeforeResponse(oSession: Session) {
        ...

        if(oSession.host == "www.google.com:80"){
            oSession["x-overrideHost"] = "123.123.123.123";
        }
        if(oSession.pathAndQuery.contains("/path1/"){
            oSession["x-overrideHost"] = "124.124.124.124";
        }else if(oSession.pathAndQuery.contains("/path2/"){
            oSession["x-overrideHost"] = "125.125.125.125";
        }
    }
```

11. 场景--同时将接口返回修改成404;这种方式可以解决bpu命令的单个调试的 缺点；全部断点需要操作的步骤太多，浪费时间
```
    if(oSession.HostnameIs("host1") && oSession.url.IndexOf("url_path1") > -1){
        //说明已经拿到了播放请求接口,将其返回网络状态码修改成：404
        oSession.oResponse.headers.HTTPResponseCode = 404;
        oSession.oResponse.headers.HTTPResponseStatus = "use fiddler change responed code";
	
     }
    if(oSession.HostnameIs("host2") && oSession.url.IndexOf("url_path2") > -1){
        //同上
        oSession.oResponse.headers.HTTPResponseCode = 404;
        oSession.oResponse.headers.HTTPResponseStatus = "use fiddler change responed code";
    }
```
12. 自定义菜单
```
    //这里是新加的菜单项       
    RulesString("Override &Allow-Origin", true)             //一级菜单名称  
    RulesStringValue(1,"http://xx", "http://xx.com", true)          //指定几个默认的的选项
    RulesStringValue(2,"https://xx", "https://xx.com", true)          //指定几个默认的的选项

    //RulesStringValue(1,"xx_test", "https://test.xx.com", true)          //指定几个默认的的选项  
    RulesStringValue(3,"&Custom...", "%CUSTOM%")        //允许用户自已定义,点击时弹出输入  

    //如果加第4个参数为true的话,会把当前规则当作默认规则,每次启动都会生效,如:  
    //RulesStringValue(5,"菜单项显示内容","菜单项选中对应值",true)//将会默认选中此项  
    public static var sAllowOriginss: String = null;      //定义变量名称              
```

13. 使用自定义得变量（请求跨域操作）
```angular2html
    // 12.自定义菜单得内容里，最后一行 public static var sAllowOriginss: String = null;      //定义变量名称
    // 对这个sAllowOriginss变量进行使用（在OnBeforeResponse里使用）
    if (sAllowOrigin && !oSession.oResponse.headers.Exists("Access-Control-Allow-Origin"))
        {
            oSession.oResponse.headers.Add("Access-Control-Allow-Credentials", true);
            oSession.oResponse.headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
            oSession.oResponse.headers.Add("Access-Control-Allow-Origin", sAllowOrigin);
        
        } 
```

14. 对fiddler里的session按照设备进行过滤
```
	public static var gs_FilterDevice: boolean = false;	// 是否过滤单设备请求标志
	public static var gs_FilterClientIP: String = null;	// 显示请求的设备的 ClientIP

	static function IsUnMatchClientIP(oS:Session):Boolean {
		return (oS.m_clientIP != gs_FilterClientIP);
	}
	public static ContextAction("开/关过滤单设备请求")
	function ToggleDeviceFilter(oSessions: Fiddler.Session[]){
		if (gs_FilterDevice) {
			gs_FilterDevice = false;
			return;
		}
	var oS: Session = FiddlerApplication.UI.GetFirstSelectedSession();
	if (null == oS) return;
	if (!gs_FilterDevice) {
		gs_FilterDevice = true;
	}
	gs_FilterClientIP = oS.clientIP;
	// 删除当前已显示的非所关心设备的请求
	FiddlerApplication.UI.actSelectSessionsMatchingCriteria(IsUnMatchClientIP);
	FiddlerApplication.UI.actRemoveSelectedSessions();
}

Fiddler修改脚本进行对fiddler里的session按照设备进行过滤
```
