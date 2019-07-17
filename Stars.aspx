<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Stars.aspx.cs" Inherits="ScrapMaricopa.Retax" %>

<!DOCTYPE html>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta charset="utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <meta name="description" content="" />
    <meta name="author" content="" />
    <title>STARS</title>

    <!-- Bootstrap Core CSS -->
    <link rel="stylesheet" href="Bootstrapp.css" />
    <script type="text/javascript" src="https://ajax.googleapis.com/ajax/libs/jquery/3.3.1/jquery.min.js"></script>
    <script type="text/javascript" src="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js"></script>
    <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jquery/1.10.1/jquery.min.js"></script>
    <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jquery/1.11.0/jquery.min.js"></script>
    <script type="text/javascript" src="http://code.jquery.com/jquery-1.12.4.min.js"></script>
    <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jquery/1.9.1/jquery.min.js"></script>
    <link rel="stylesheet" href="dist/Select2.css" />
    <script type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/select2/4.0.3/js/select2.min.js"></script>
    <script type="text/javascript" src="dist/jquery.loading.min.js"></script>
    <link rel="stylesheet" href="designcss.css" />
    <link href="dist/loading.min.css" rel="stylesheet" type="text/css" />

    <script type="text/javascript">
        $(function () {
            $(".js-example-placeholder-single").select2({
                placeholder: "Select",
                allowClear: true
            });
        });
    </script>

    <style type="text/css">
        .GridLabel {
            font-family: Calibri;
            font-size: 20px;
            font-style: oblique;
            text-decoration: underline;
            cursor: pointer;
        }

        th {
            white-space: nowrap;
            font-weight: 400;
        }

        #bloc1, #bloc2 {
            display: inline;
        }

        .order {
            display: none;
            visibility: hidden;
        }

        .fullwidth {
            backgrounF: #ece9e9;
        }

        .sub-menu {
            padding: 0;
            border: 1px solid black;
            overflow-y: auto;
        }

        #myInput {
            border-box: box-sizing;
            background-image: url(searchicon.png);
            background-position: 14px 12px;
            background-repeat: no-repeat;
            font-size: 16px;
            padding: 14px 20px 12px 45px;
            border: 0px solid black;
            border-bottom: 1px solid #ddd;
            background-repeat: no-repeat;
            background-size: 50px 30px;
        }

            #myInput:focus {
                outline: 3px solid #ddd;
            }
    </style>
</head>
<body class="ff">
    <form id="form1" runat="server">
     <asp:HiddenField runat="server" ID="Cotime" />
        <asp:HiddenField runat="server" ID="Final" />

        <div class="container-fluid">
            <nav class="navbar navbar-default" role="navigation">
                <div class="container">
                    <div class="navbar-header">
                        <button type="button" class="navbar-toggle" data-toggle="collapse" data-target="#navbar-collapse-1">
                            <span class="sr-only">Toggle navigation</span>
                            <span class="icon-bar"></span>
                            <span class="icon-bar"></span>
                            <span class="icon-bar"></span>
                        </button>
                    </div>
                </div>
                <div class="navbar-brand" style="color: #151414cc; bottom: 10px; font-weight: bold; font-style: italic">STARS</div>

                <div class="collapse navbar-collapse" id="navbar-collapse-1">
                    <a class="nav navbar-nav navbar-left">
                        <img src="logo.png" class="img-responsive" alt="logo" />
                    </a>

                    <div class="nav navbar-nav navbar-right" data-toggle="dropdown" style="width: 130px;">
                        <asp:DropDownList class="form-control js-example-placeholder-single col-sm-6 form-group form-inline dropdown-toggle dropdown" runat="server" ID="DropDownList2" AutoPostBack="true" Style="width: 180px;" OnSelectedIndexChanged="DropDownList2_SelectedIndexChanged">
                        </asp:DropDownList>
                    </div>

                    <div class="nav navbar-nav navbar-right">
                        <asp:Button runat="server" ID="btnbulk" Text="Bulk Upload" class="form-control col-sm-6 js-example-placeholder-single form-inline dropdown-toggle dropdown" Style="height:37px;background-color:#d43f3a;color:#fff;padding-left:25px;" OnClick="btnbulk_Click"/>
                    </div>
                </div>
            </nav>
        </div>
        <div id="pbarmain" style="display: none;" class="progress progress-striped active">
            <div id="pbar" class="progress-bar" style="background-color: orange; font-size: larger"></div>
        </div>
        <div class="container">
            <div class="col-lg-12 well">
                <div class="row">

                    <div class="col-sm-12">
                        <div class="row">
                           <div class="col-sm-4 form-group form-inline" style="left: 134px;">
                                <label>State: </label>
                                <asp:DropDownList class="form-control" runat="server" ID="dropState" AutoPostBack="true" Style="width: 170px" OnSelectedIndexChanged="dropState_SelectedIndexChanged">
                                </asp:DropDownList>
                            </div>
                            <div class="col-sm-4 form-group form-inline">
                                <label>County:</label>
                                <asp:DropDownList class="form-control" runat="server" ID="dropCounty" AutoPostBack="true" Style="width: 170px" OnSelectedIndexChanged="dropCounty_SelectedIndexChanged">
                                </asp:DropDownList>
                            </div>
                             <div class="col-sm-4 form-group form-inline">
                                <asp:label runat="server" Visible ="false" id="labelTown">Township:</asp:label>
                                <asp:DropDownList class="form-control" runat="server" ID="dropTownship" AutoPostBack="true" Style="width: 170px" Visible="False">
                                </asp:DropDownList>
                            </div>
                            <div class="col-sm-6 form-group form-inline">
                                <asp:HiddenField ID="hdnField1" runat="server" />
                            </div>
                        </div>
                        <asp:Panel ID="Panel1" runat="server">
                            <div class="form-group form-inline" id="Address" style="margin-left: 6px;">
                                &nbsp;&nbsp;&nbsp;<label>Address:</label>
                                <asp:TextBox runat="server" ID="txtAddress" type="text" placeholder="Enter Address..." class="form-control" Style="margin-left: 40px; width: 593px">
                                </asp:TextBox>
                            </div>
                        </asp:Panel>
                        <asp:Panel ID="Panel2" runat="server">
                            <div id="multi">
                                <div class="col-md-4 form-group form-inline" style="width: 363px">
                                    <label>Street Number: </label>
                                    <asp:TextBox runat="server" ID="txtstreetno" placeholder="Enter StreetNumber..." class="form-control" Style="width: 220px">
                                    </asp:TextBox>
                                </div>

                                <div class="col-md-4 form-group form-inline" style="width: 363px">
                                    <label>Directions:</label>
                                    <asp:TextBox runat="server" ID="txtdirection" placeholder="Enter Directions..." class="form-control" Style="margin-left: 21px; width: 239px"></asp:TextBox>
                                </div>
                                <div class="col-md-4 form-group form-inline" style="width: 363px">
                                    <label>Street Name:</label>
                                    <asp:TextBox runat="server" ID="txtstreetname" placeholder="Enter StreetName..." class="form-control" Style="margin-left: 0px; width: 237px">
                                    </asp:TextBox>
                                </div>

                                <div class="col-sm-4 form-group form-inline" style="width: 363px">
                                    <label>Street Type: </label>
                                    <asp:TextBox runat="server" ID="txtstreettype" placeholder="Enter StreetType..." class="form-control" Style="margin-left: 21px; width: 220px"></asp:TextBox>
                                </div>
                                <div class="col-sm-4 form-group form-inline" style="width: 363px">
                                    <label>Unit/Acc No:</label>
                                    <asp:TextBox runat="server" ID="txtunitnumber" placeholder="Enter Unit/Acc No..." class="form-control" Style="width: 239px; margin-left: 7px;"></asp:TextBox>
                                </div>
                                <div class="col-sm-4 form-group form-inline" style="width: 363px">
                                    <label>City:</label>
                                    <asp:TextBox runat="server" ID="txtcity" placeholder="Enter City..." class="form-control" Style="margin-left: 55px; width: 237px"></asp:TextBox>
                                </div>
                            </div>
                        </asp:Panel>
                        <asp:Panel ID="Panel3" runat="server" DefaultButton="Button1">
                            <div class="row">
                                <div class="col-sm-6 form-group form-inline" style="width: 363px; padding-left: 0px; padding-right: 0px; margin-left: 18px;">
                                    &nbsp;&nbsp;&nbsp;<label>Parcel Number:</label>
                                    <asp:TextBox runat="server" ID="txtparcelno" placeholder="Enter ParcelNumber..." class="form-control" Style="width: 220px;">
                                    </asp:TextBox>
                                </div>
                                <div class="col-sm-6 form-group form-inline" style="width: 363px; padding-left: 0px; padding-right: 0px; margin-left: 12px;">
                                    <label>Owner Name:</label>
                                    <asp:TextBox runat="server" ID="txtownername" placeholder="Enter OwnerName..." class="form-control" Style="width: 241px;">
                                    </asp:TextBox>
                                </div>
                            </div>
                            <div class="form-group form-inline" style="margin-left: 56px;">
                                &nbsp;&nbsp;&nbsp;<label class="order">Order Id:</label>
                                <asp:TextBox runat="server" ID="txtorderno" placeholder="Enter OrderId..." class="form-control order" Style="margin-left: 47px; width: 388px"></asp:TextBox>
                                <asp:Button runat="server" ID="Button1" class="demo3 btn btn-sm btn-success" Text="Go" OnClick="Button1_Click" OnClientClick="makeProgress()" Style="width: 60px;" />
                                <asp:Button runat="server" ID="Button3" Text="Reset" class="btn btn-sm btn-danger" OnClick="Button3_Click" Style="width: 60px" />
                            </div>
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </div>

        <div id="bloc1" runat="server" class="form-group form-inline" style="position: absolute" visible="false">
            <label style="text-align: right; margin-left: 105px">Property Address:</label>
            <asp:Label runat="server" ID="mylabel"></asp:Label>
        </div>
        <div id="bloc2" runat="server" class="form-group form-inline" style="position: absolute" visible="false">
            <label style="text-align: right; width: 1150px;">ScreenShots:</label>
            <asp:HyperLink ID="hyperlink2" Font-Bold="True" Font-Size="Large" NavigateUrl="~/TaxPages.aspx" Target="_blank" Text="Tax Pages" runat="server" Style="text-decoration: underline;" />
        </div>
        <br />
        <br />
         <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="LabelAmrock" CssClass="GridLabel"></asp:Label>
            <br />
               <asp:GridView ID="GridViewAmrock" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
        <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label3" CssClass="GridLabel"></asp:Label>
            <br />
            <asp:GridView ID="GridView3" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
                <Columns>
                    <asp:TemplateField>
                        <HeaderStyle HorizontalAlign="Center" />
                        <ItemTemplate>
                            <asp:CheckBox ID="chkselect" runat="server" class="demo3 demo66" OnCheckedChanged="chkselect_CheckedChanged" OnClick="makeProgress()"
                                AutoPostBack="True" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <div style="margin-left: 38px;">
            <asp:Label runat="server" ID="mobileTax" CssClass="GridLabel"></asp:Label>
            <br />
            <asp:GridView ID="mobiletax_multiparcel" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
                <Columns>
                    <asp:TemplateField>
                        <HeaderStyle HorizontalAlign="Center" />
                        <ItemTemplate>
                            <asp:CheckBox ID="chkselect" runat="server" class="demo3 demo66" OnCheckedChanged="mobile_chkselect_CheckedChanged" OnClick="makeProgress()"
                                AutoPostBack="True" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <div id="mydiv" style="margin-left: 38px;">
            <asp:Label runat="server" ID="Label7" CssClass="GridLabel" onclick="test('GridView7')">     
            </asp:Label>
            <br />
            <asp:GridView ID="GridView7" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true" Style="width: 1280px;">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>

        <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label2" CssClass="GridLabel" onclick="test('GridView2')"></asp:Label>
            <br />
            <asp:GridView ID="GridView2" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
        <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label1" CssClass="GridLabel" onclick="test('GridView1')"></asp:Label>
            <br />
            <asp:GridView ID="GridView1" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
        <div id="div3" style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label4" CssClass="GridLabel" onclick="test('GridView4')"></asp:Label>
            <br />
            <asp:GridView ID="GridView4" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
        <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label5" CssClass="GridLabel" onclick="test('GridView5')"></asp:Label>
            <br />
            <asp:GridView ID="GridView5" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
        <div id="div4" style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label6" CssClass="GridLabel" onclick="test('GridView6')"></asp:Label>
            <br />
            <asp:GridView ID="GridView6" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
        <div id="div5" style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label8" CssClass="GridLabel" onclick="test('GridView8')"></asp:Label>
            <br />
            <asp:GridView ID="GridView8" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
        <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label9" CssClass="GridLabel" onclick="test('GridView9')"></asp:Label>
            <br />
            <asp:GridView ID="GridView9" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
        <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label10" CssClass="GridLabel" onclick="test('GridView10')"></asp:Label>
            <br />
            <asp:GridView ID="GridView10" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
        <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label11" CssClass="GridLabel" onclick="test('GridView11')"></asp:Label>
            <br />
            <asp:GridView ID="GridView11" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
        <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label12" CssClass="GridLabel" onclick="test('GridView12')"></asp:Label>
            <br />
            <asp:GridView ID="GridView12" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
        <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label13" CssClass="GridLabel" onclick="test('GridView13')"></asp:Label>
            <br />
            <asp:GridView ID="GridView13" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
        <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label14" CssClass="GridLabel" onclick="test('GridView14')"></asp:Label>
            <br />
            <asp:GridView ID="GridView14" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
        <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label15" CssClass="GridLabel" onclick="test('GridView14')"></asp:Label>
            <br />
            <asp:GridView ID="GridView15" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
         <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label16" CssClass="GridLabel" onclick="test('GridView14')"></asp:Label>
            <br />
            <asp:GridView ID="GridView16" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
         <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label17" CssClass="GridLabel" onclick="test('GridView14')"></asp:Label>
            <br />
            <asp:GridView ID="GridView17" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
         <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label18" CssClass="GridLabel" onclick="test('GridView14')"></asp:Label>
            <br />
            <asp:GridView ID="GridView18" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
         <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label19" CssClass="GridLabel" onclick="test('GridView14')"></asp:Label>
            <br />
            <asp:GridView ID="GridView19" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
         <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label20" CssClass="GridLabel" onclick="test('GridView14')"></asp:Label>
            <br />
            <asp:GridView ID="GridView20" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
         <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label21" CssClass="GridLabel" onclick="test('GridView14')"></asp:Label>
            <br />
            <asp:GridView ID="GridView21" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
         <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label22" CssClass="GridLabel" onclick="test('GridView14')"></asp:Label>
            <br />
            <asp:GridView ID="GridView22" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
         <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label23" CssClass="GridLabel" onclick="test('GridView14')"></asp:Label>
            <br />
            <asp:GridView ID="GridView23" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
         <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label24" CssClass="GridLabel" onclick="test('GridView14')"></asp:Label>
            <br />
            <asp:GridView ID="GridView24" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
         <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label25" CssClass="GridLabel" onclick="test('GridView14')"></asp:Label>
            <br />
            <asp:GridView ID="GridView25" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div> <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label26" CssClass="GridLabel" onclick="test('GridView14')"></asp:Label>
            <br />
            <asp:GridView ID="GridView26" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div> <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label27" CssClass="GridLabel" onclick="test('GridView14')"></asp:Label>
            <br />
            <asp:GridView ID="GridView27" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div> <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label28" CssClass="GridLabel" onclick="test('GridView14')"></asp:Label>
            <br />
            <asp:GridView ID="GridView28" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div> <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label29" CssClass="GridLabel" onclick="test('GridView14')"></asp:Label>
            <br />
            <asp:GridView ID="GridView29" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div> <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label30" CssClass="GridLabel" onclick="test('GridView14')"></asp:Label>
            <br />
            <asp:GridView ID="GridView30" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
        <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label31" CssClass="GridLabel" onclick="test('GridView14')"></asp:Label>
            <br />
            <asp:GridView ID="GridView31" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
        <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label32" CssClass="GridLabel" onclick="test('GridView14')"></asp:Label>
            <br />
            <asp:GridView ID="GridView32" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
        <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label33" CssClass="GridLabel" onclick="test('GridView14')"></asp:Label>
            <br />
            <asp:GridView ID="GridView33" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
        <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label34" CssClass="GridLabel" onclick="test('GridView14')"></asp:Label>
            <br />
            <asp:GridView ID="GridView34" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
        <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label35" CssClass="GridLabel" onclick="test('GridView14')"></asp:Label>
            <br />
            <asp:GridView ID="GridView35" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
        <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label36" CssClass="GridLabel" onclick="test('GridView14')"></asp:Label>
            <br />
            <asp:GridView ID="GridView36" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
        <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label37" CssClass="GridLabel" onclick="test('GridView14')"></asp:Label>
            <br />
            <asp:GridView ID="GridView37" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
        <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label38" CssClass="GridLabel" onclick="test('GridView14')"></asp:Label>
            <br />
            <asp:GridView ID="GridView38" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
        <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label39" CssClass="GridLabel" onclick="test('GridView14')"></asp:Label>
            <br />
            <asp:GridView ID="GridView39" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
        <div style="margin-left: 38px; margin-right: 30px;">
            <asp:Label runat="server" ID="Label40" CssClass="GridLabel" onclick="test('GridView14')"></asp:Label>
            <br />
            <asp:GridView ID="GridView40" runat="server" CssClass="table table-striped table-bordered table-hover th" GridLines="None" AutoGenerateColumns="true">
                <HeaderStyle BackColor="dimgray" ForeColor="White" />
            </asp:GridView>
        </div>
        <div class="Logout_checklist" id="LogoutReason" runat="server" align="center">
            <asp:Image ID="Image1" runat="server" Height="57px" Width="300px" />
            <asp:Button ID="BtnNew" runat="server" CssClass="btn btn-primary" OnClick="BtnNew_Click"
                Text="New" Width="74px" Height="36px" />
            <br />
            <asp:TextBox ID="TextBox3" runat="server" Height="31px" Width="358px"></asp:TextBox>
            <br />
            <asp:Button ID="Button2" runat="server" Height="31px" Text="Submit" Width="81px"
                CssClass="btn btn-primary demo6" OnClick="Button2_Click" />
            <br />
            <br />
            <asp:HiddenField runat="server" ID="SMS" />
        </div>

        <script type="text/javascript" src="Loader.js"></script>
        <script type="text/javascript" src="Test.js"></script>
        <script type="text/javascript" src="Gridview.js"></script>
        <script type="text/javascript" src="ProgressBar.js"></script>
        <p>
            &nbsp;     
        </p>
    </form>
    <div class="fullwidth">
        <div style="text-align: center; color: black">
            <p>Copyright © 2018,<a href="http://stringinfo.com" target="_blank" style="color: red; clear: both"> String Information Services</a> All rights reserved</p>
        </div>
    </div>
    <script>
        function filterFunction() {
            var input, filter, ul, li, a, i;
            input = document.getElementById("myInput");
            filter = input.value.toUpperCase();
            ul = document.getElementById("myDropdown");
            a = ul.getElementsByTagName("li");
            for (i = 0; i < a.length; i++) {
                if (a[i].innerHTML.toUpperCase().indexOf(filter) > -1) {
                    a[i].style.display = "";
                } else {
                    a[i].style.display = "none";
                }
            }
        }
    </script>
</body>
</html>
