﻿<%@ Control Language="C#" AutoEventWireup="true" Inherits="YAF.Pages.ReportPost"CodeBehind="reportpost.ascx.cs" %>
<%@ Import Namespace="YAF.Types.Interfaces" %>
<%@ Import Namespace="YAF.Types.Extensions" %>

<YAF:PageLinks runat="server" ID="PageLinks" />

<div class="row">
    <div class="col-xl-12">
        <h2><YAF:LocalizedLabel ID="LocalizedLabel2" runat="server" LocalizedTag="HEADER" /></h2>
    </div>
</div>

<div class="row">
    <div class="col">
        <div class="card mb-3">
            <div class="card-header">
                <i class="fa fa-exclamation-triangle fa-fw"></i>&nbsp;<YAF:LocalizedLabel ID="LocalizedLabel3" runat="server" LocalizedTag="HEADER" />
            </div>
            <div class="card-body">
                <asp:Repeater ID="MessageList" runat="server">
                    <ItemTemplate>
                                <YAF:LocalizedLabel ID="PostedByLabel" runat="server" LocalizedTag="POSTEDBY" />
                                <a name="<%# DataBinder.Eval(Container.DataItem, "MessageID") %>" /><strong>
                                    <YAF:UserLink ID="UserLink1" runat="server" UserID='<%# DataBinder.Eval(Container.DataItem, "UserID") %>' />
                                </strong>
                                <strong>
                                    <YAF:LocalizedLabel ID="LocalizedLabel1" runat="server" LocalizedTag="POSTED" />
                                </strong>
                                <%# this.Get<IDateTime>().FormatDateTime( Container.DataItemToField<DateTime>("Posted") )%>
                                <YAF:MessagePostData ID="MessagePreview" runat="server" ShowAttachments="false" ShowSignature="false"
                                                     DataRow="<%# ((System.Data.DataRowView)Container.DataItem).Row %>">
                                </YAF:MessagePostData>
                    </ItemTemplate>
                </asp:Repeater>
                <div class="form-group">
                    <asp:Label runat="server" AssociatedControlID="EditorLine">
                        <YAF:LocalizedLabel ID="EnterReportTextLabel" runat="server" 
                                            LocalizedTag="ENTER_TEXT" />
                    </asp:Label>
                    <asp:PlaceHolder id="EditorLine" runat="server">
                        <asp:Label ID="IncorrectReportLabel" runat="server"></asp:Label>
                        <!-- editor goes here -->
                    </asp:PlaceHolder>
                </div>
            </div>
            <div class="card-footer text-center">
                <YAF:ThemeButton ID="btnReport" runat="server" CssClass="yafcssbigbutton leftItem"
                                 TextLocalizedTag="SEND" TitleLocalizedTag="SEND_TITLE" 
                                 OnClick="BtnReport_Click"
                                 Icon="paper-plane"/>
                <YAF:ThemeButton ID="btnCancel" runat="server"
                                 TextLocalizedTag="CANCEL" TitleLocalizedTag="CANCEL_TITLE" 
                                 OnClick="BtnCancel_Click"
                                 Type="Secondary"
                                 Icon="times"/>
            </div>
        </div>
    </div>
</div>