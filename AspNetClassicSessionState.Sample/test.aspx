﻿<%@ Page Language="C#" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title></title>
</head>
<body>
    <%
        Response.Write("ASP Classic Variable: ");
        Response.Write(Session["ASPClassicVar"]);
        Session["ASPNetVar"] = "THIS WAS SET BY ASP.NET";
    %>

    <br />

    I saved a value to to the ASPNetVar variable. Go back to the <a href="Test.asp">ASP classic page</a> to see the value. 
</body>
</html>
