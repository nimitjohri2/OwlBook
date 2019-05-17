<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="OwlBook._Default" %>

<%@ Register Assembly="System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" Namespace="System.Web.UI.DataVisualization.Charting" TagPrefix="asp" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
        <div class="row">
            <div class="col-md-6">
    <div class="jumbotron">
        <h1>OwlBook</h1>
        
        <p>
            <asp:Button ID="btn_Run" runat="server" Text="Suggest Friends" OnClick="btn_Run_Click" /></p>
            <p><asp:Label ID="lbl_friendsmade" runat="server" Text=""></asp:Label></p>
            <p><asp:GridView ID="gv_Display" runat="server">
            </asp:GridView>
        </p>
    </div>
        </div>



        <div class="col-md-6">
            <p>
                <asp:Button ID="btn_loadchart" runat="server" OnClick="btn_loadchart_Click" Text="Plot Clustering Coefficient" /><asp:Button ID="btn_Clustering" runat="server" Text="Calculate Clustering Coefficient" OnClick="btn_Clustering_Click" /></p>
            <p>
                <asp:Label ID="lbl_clustering" runat="server" Text=""></asp:Label></p>
            <p>
                <asp:GridView ID="gv_Clustering" runat="server"></asp:GridView>
            </p>
            <p>
                <asp:Chart ID="ch_clustering" runat="server" Height="600px" Width="600px">
                    <Series>
                        <asp:Series Name="Series1" ChartType="Line"></asp:Series>
                    </Series>
                    <ChartAreas>
                        <asp:ChartArea Name="ChartArea1"></asp:ChartArea>
                    </ChartAreas>
                </asp:Chart>
            </p>
        </div>
    </div>

</asp:Content>
