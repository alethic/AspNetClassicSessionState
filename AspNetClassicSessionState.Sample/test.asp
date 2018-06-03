<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title></title>
</head>
<body>

    <%
        Dim o
        Set o = Server.CreateObject("AspNetClassicSessionState.COM.AspSessionStateClient")
        o.Load()
        Response.Write "ASP.NET Variable: "
        Response.Write Session("ASPNetVar")
        Session("ASPClassicVar") = "THIS WAS SET BY ASP CLASSIC"
        o.Save()
    %>

    <br />

    I saved a value to to the ASPClassicVar variable. Go back to the <a href="Test.aspx">ASP.NET page</a> to see the value. 

</body>
</html>
