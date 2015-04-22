Project Description
This project is based on http://www.telerik.com/fiddler/fiddlercore. You can use our Mock Server to mock HTTP response. This tool is mainly designed for test engineers. The goal is to make it easy to use for even junior test engineers to test their client apps. 

Supported OS
Windows 7 and above
.Net Framework 4.0 required

Why not Fiddler's AutoResponder?
Fiddler's AutoResponder is a little difficult for test engineers to use.
Fiddler's AutoResponder is UI driven. Mock Server is file driven, which gives you more extensibility.
Fiddler's AutoResponder cannot be used to mock response if the request is encrypted and you want to tamper only if the request body contains certain string. It doesn't work either if the response need to be encrypted but you expect to set a plain text response. But the Mock Server can.
This mock server is open source so that you can add additional functions to support your test team.
Other enhancements are provided in Mock Server, such as support for HTTP header/body split in Rule, comment in Rule etc.

Hello World
Add this in Rules.txt (note it's tab instead of white space between url and file name)
[startwith]http://www.jd.com 1.txt
Add a 1.txt file under the Response folder with below content:
HTTP/1.1 200 OK
Content-Length: 15445
Content-Type: text/html; charset=utf-8
Vary: Accept-Encoding
Server: BWS/1.1 Microsoft-HTTPAPI/2.0
Date: Tue, 10 Jun 2014 08:15:53 GMT

hello world!
Launch MockServer.exe
Access http://www.jd.com in browser
You'll see the tampered response