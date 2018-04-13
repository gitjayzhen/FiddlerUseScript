# FiddlerUseScript

### 简介：抓包工具，调试接口除了已有的功能，还可以使用灵活的脚本来处理一些场景

**************************************************************************************************************
** 官网脚本api示例教程：http://docs.telerik.com/fiddler/KnowledgeBase/FiddlerScript/ModifyRequestOrResponse  

** Fiddler命令行教程：http://docs.telerik.com/fiddler/knowledgebase/quickexec  

** FiddlerScript的api：http://fiddlerbook.com/Fiddler/dev/ScriptSamples.asp

** jscript.net官网教程：https://msdn.microsoft.com/en-us/library/91td9cas(v=vs.80).aspx

** google网上论坛：https://groups.google.com/forum/?fromgroups#%21forum/httpfiddler
*************************************************************************************************************

### 该文件在window的文件目录位置：C:\Users\"you_pc_name"\Documents\Fiddler2\Scripts

# FiddlerScript的场景应用(20180412)：

## +++++++++++++++++beforerequest+++++++++++++++++++++++

1. 在此处【设置代理网络限速】1KB的量 50Kb/s需要delay 160ms <br>
  带宽：mbps kbps (比特流)  网速：KB/s MB/s （字节流）   <br>
  修改完记得勾选【simulate modem speeds】[randInt(1,50) 模拟网络抖动]<br>
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
	    oSession.hostname = "api.mobile.youku.com"
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
