<%@ Page Language="C#" Async="true" AutoEventWireup="true" CodeBehind="Logout.aspx.cs" Inherits="GSPSSO.Logout" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>登出中</title>
</head>
<body>
    <form id="logoutForm" method="post" action="<%= LogoutEndpoint %>">
        <input type="hidden" name="id_token_hint" value="<%= IdTokenHint %>" />
        <noscript>
            <button type="submit">登出</button>
        </noscript>
    </form>

    <script>
        document.getElementById("logoutForm").submit();
    </script>

    <p>登出處理中...</p>
</body>
</html>