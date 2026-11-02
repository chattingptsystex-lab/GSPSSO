<%@ Page Language="C#" Async="true" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="GSPSSO.Default" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>SSO Result</title>
</head>
<body>
    <form id="form1" runat="server">
        <asp:Literal ID="litResult" runat="server"></asp:Literal>
        <asp:Button ID="btnLogout" runat="server" Text="登出" OnClick="btnLogout_Click" />
    </form>
</body>
</html>