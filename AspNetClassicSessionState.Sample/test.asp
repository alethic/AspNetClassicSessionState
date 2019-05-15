<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title></title>
</head>
<body>

    <%
        Response.Write AspNetSession("Variable")
        AspNetSession("Variable") = "THIS WAS SET BY ASP CLASSIC"
    %>

    <br />

    I saved a value to to the Variable variable. Go back to the <a href="Test.aspx">ASP.NET page</a> to see the value. 

</body>
</html>
