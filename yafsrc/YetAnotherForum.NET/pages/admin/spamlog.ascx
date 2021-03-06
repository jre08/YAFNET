<%@ Control Language="c#" AutoEventWireup="True" EnableViewState="true" Inherits="YAF.Pages.Admin.spamlog"
    CodeBehind="spamlog.ascx.cs" %>

<%@ Import Namespace="YAF.Types.Interfaces" %>
<%@ Import Namespace="YAF.Types.Extensions" %>

<YAF:PageLinks runat="server" ID="PageLinks" />

<script type="text/javascript">
function toggleItem(detailId)
{
    var show = '<i class="fa fa-caret-square-down fa-fw"></i>&nbsp;<%# this.GetText("ADMIN_EVENTLOG", "SHOW")%>';
    var hide = '<i class="fa fa-caret-square-up fa-fw"></i>&nbsp;<%# this.GetText("ADMIN_EVENTLOG", "HIDE")%>';

	jQuery('#Show'+ detailId).html($('#Show'+ detailId).html() == show ? hide : show);

	jQuery('#eventDetails' + detailId).slideToggle('slow');

	return false;

}
</script>


        <div class="row">
            <div class="col-xl-12">
                <h1><YAF:LocalizedLabel ID="LocalizedLabel1" runat="server" LocalizedTag="TITLE" LocalizedPage="ADMIN_SPAMLOG" /></h1>
            </div>
    </div>
    <div class="row">
        <div class="col-xl-12">
            <div class="card mb-3">
                <div class="card-header">
                    <i class="fa fa-shield-alt fa-fw"></i>&nbsp;<YAF:LocalizedLabel ID="LocalizedLabel7" runat="server" LocalizedTag="TITLE" LocalizedPage="ADMIN_SPAMLOG" />
            </div>
                <div class="card-body">
                    <h4>
                        <YAF:HelpLabel ID="SinceDateLabel" runat="server" LocalizedPage="ADMIN_EVENTLOG" LocalizedTag="SINCEDATE" />
                    </h4>
                    <div class='input-group mb-3 date datepickerinput'>
                        <span class="input-group-prepend">
                            <button class="btn btn-secondary datepickerbutton" type="button">
                                <i class="fa fa-calendar-day fa-fw"></i>
                            </button>
                        </span>
                            <asp:TextBox ID="SinceDate" runat="server" CssClass="form-control"></asp:TextBox>
                        </div>
                   <hr />
                    <h4>
                <YAF:HelpLabel ID="ToDateLabel" runat="server" LocalizedPage="ADMIN_EVENTLOG" LocalizedTag="TODATE" />
                        </h4>
                    <div class='input-group mb-3 date datepickerinput'>
                        <span class="input-group-prepend">
                            <button class="btn btn-secondary datepickerbutton" type="button">
                                <i class="fa fa-calendar-day fa-fw"></i>
                            </button>
                        </span>
                            <asp:TextBox ID="ToDate" runat="server" CssClass="form-control"></asp:TextBox>
                        </div>
                   <hr />
        </div>
                <div class="card-footer text-center">
                <YAF:ThemeButton ID="ApplyButton" Type="Primary" OnClick="ApplyButtonClick"
                    TextLocalizedPage="ADMIN_EVENTLOG" TextLocalizedTag="APPLY" Icon="check" runat="server"></YAF:ThemeButton>
            </div>
           </div>
         </div>
    </div>
        <div class="row">
        <div class="col-xl-12">
                <YAF:Pager ID="PagerTop" runat="server" OnPageChange="PagerTopPageChange" />
            <div class="card mb-3">
                <div class="card-header">
                    <i class="fa fa-shield-alt fa-fw"></i>&nbsp;<YAF:LocalizedLabel ID="LocalizedLabel8" runat="server" LocalizedTag="TITLE" LocalizedPage="ADMIN_SPAMLOG" />
            </div>
                <div class="card-body">
        <asp:Repeater runat="server" ID="List">
            <HeaderTemplate>
                <ul class="list-group">
            </HeaderTemplate>
            <ItemTemplate>
                <li class="list-group-item list-group-item-action">
                    <div class="d-flex w-100 justify-content-between text-break" onclick="javascript:toggleItem(<%# this.Eval("EventLogID") %>);">
                        <h5 class="mb-1">
                            <a name="event<%# this.Eval("EventLogID")%>" ></a>
                            <asp:HiddenField ID="EventTypeID" Value='<%# this.Eval("Type")%>' runat="server"/>
                            <YAF:LocalizedLabel ID="LocalizedLabel5" runat="server" 
                                                                               LocalizedTag="SOURCE" 
                                                                               LocalizedPage="ADMIN_EVENTLOG" />:&nbsp;
                            <%# this.HtmlEncode(this.Eval( "Source")).IsSet() ? this.HtmlEncode(this.Eval( "Source")) : "N/A" %>
                        </h5>
                        <small>
                            <a class="showEventItem btn btn-info btn-sm" 
                               href="#event<%# this.Eval("EventLogID")%>" 
                               id="Show<%# this.Eval("EventLogID") %>"><i class="fa fa-caret-square-down fa-fw"></i>&nbsp;<YAF:LocalizedLabel ID="LocalizedLabel1" runat="server" LocalizedTag="SHOW" LocalizedPage="ADMIN_EVENTLOG" /></a>&nbsp;&nbsp;
                            <YAF:ThemeButton runat="server" 
                                             Type="Danger"
                                             CommandName="delete" CommandArgument='<%# this.Eval( "EventLogID") %>'
                                             ReturnConfirmText='<%# this.GetText("ADMIN_EVENTLOG", "CONFIRM_DELETE") %>'
                                             Icon="trash" 
                                             TextLocalizedTag="DELETE">
                            </YAF:ThemeButton>
                        </small>
                    </div>
                    <p class="mb-1">
                        <span class="font-weight-bold"><YAF:LocalizedLabel ID="LocalizedLabel3" runat="server" LocalizedTag="NAME" LocalizedPage="ADMIN_EVENTLOG" />:</span>&nbsp;
                        <%# this.HtmlEncode(this.Eval( "UserName")).IsSet() ? this.HtmlEncode(this.Eval( "UserName")) : "N/A" %>&nbsp;
                        <span><YAF:LocalizedLabel ID="LocalizedLabel6" runat="server" LocalizedTag="TYPE" LocalizedPage="ADMIN_EVENTLOG" />:</span>&nbsp;
                        <%# this.HtmlEncode(this.Eval( "Name")).IsSet() ? this.HtmlEncode(this.Eval( "Name")) : "N/A" %>&nbsp;
                    </p>
                    <small>
                        <span class="font-weight-bold"><YAF:LocalizedLabel ID="LocalizedLabel4" runat="server" 
                                                                           LocalizedTag="TIME" 
                                                                           LocalizedPage="ADMIN_EVENTLOG" />:</span>&nbsp;
                        <%# this.Get<IDateTime>().FormatDateTimeTopic(Container.DataItemToField<DateTime>("EventTime")) %>
                    </small>
                    
                      <div class="EventDetails" id="eventDetails<%# this.Eval("EventLogID") %>" style="display: none;margin:0;padding:0;">
                            <pre class="pre-scrollable">
                                <code>
                                    <%# this.HtmlEncode(this.Eval( "Description")) %>
                                </code>
                            </pre>
                        </div>
                </li>
            </ItemTemplate>
            <FooterTemplate>
               </ul>
            </FooterTemplate>
        </asp:Repeater>
                </div>
            <div class="card-footer text-center">
                <YAF:ThemeButton runat="server" 
                                 Visible="<%# this.List.Items.Count > 0 %>" 
                                 Type="Danger"
                                 Icon="trash" 
                                 OnClick="DeleteAllClick" 
                                 TextLocalizedPage="ADMIN_EVENTLOG" TextLocalizedTag="DELETE_ALLOWED"
                                 ReturnConfirmText='<%#this.GetText("ADMIN_EVENTLOG", "CONFIRM_DELETE_ALL") %>'>
                </YAF:ThemeButton>
            </div>
        </div>
    <YAF:Pager ID="PagerBottom" runat="server" LinkedPager="PagerTop" />
                </div>
        </div>