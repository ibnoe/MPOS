﻿Imports System.Data
Imports System.Data.SqlClient
Imports DevExpress.XtraEditors
Imports DevExpress.Utils
Imports DevExpress.XtraEditors.Repository

Imports Elliteserv.Fungsi.Ini
Imports Elliteserv.Fungsi.Utils
Imports Elliteserv.SQLServer.Connect
Imports mPOS.clsCetakReportDevExpress


Public Class frmLaporanPenjualanPerSales
    Public FormName As String = Me.Name
    Public Judul As String = "Daftar "
    Public NoID As Long = -1

    Dim SQL As String = ""
    Dim repckedit As New RepositoryItemCheckEdit
    Dim repdateedit As New RepositoryItemDateEdit
    Dim reptextedit As New RepositoryItemTextEdit
    Dim reppicedit As New RepositoryItemPictureEdit

    Private Sub frmDaftarMaster_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        GridView1.SaveLayoutToXml(FolderLayouts & "\" & FormName & GridView1.Name & ".xml")
        LayoutControl1.SaveLayoutToXml(FolderLayouts & "\" & FormName & LayoutControl1.Name & ".xml")
    End Sub

    Private Sub RefreshDataKontak()
        Dim ds As New DataSet
        Try
            SQL = "SELECT HKategori.NoID, HKategori.Kode, HKategori.Nama FROM HKategori WHERE HKategori.IsAktif=1 "
            EksekusiDataset(ds, "Data", SQL)
            txtIDKategori.Properties.DataSource = ds.Tables("Data")
            txtIDKategori.Properties.DisplayMember = "Kode"
            txtIDKategori.Properties.ValueMember = "NoID"
        Catch ex As Exception
            XtraMessageBox.Show("Info Kesalahan : " & ex.Message, NamaAplikasi, MessageBoxButtons.OK)
        Finally
            ds.Dispose()
        End Try
    End Sub

    Private Sub frmDaftarMaster_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Me.Text = Judul
        LabelControl1.Text = Judul.ToUpper

        RefreshDataKontak()
        DateEdit1.DateTime = TanggalSystem
        DateEdit2.DateTime = TanggalSystem
        RefreshData()

        If System.IO.File.Exists(FolderLayouts & "\" & FormName & LayoutControl1.Name & ".xml") Then
            LayoutControl1.RestoreLayoutFromXml(FolderLayouts & "\" & FormName & LayoutControl1.Name & ".xml")
        End If
        If System.IO.File.Exists(FolderLayouts & "\" & FormName & GridView1.Name & ".xml") Then
            GridView1.RestoreLayoutFromXml(FolderLayouts & "\" & FormName & GridView1.Name & ".xml")
        End If
    End Sub

    Private Sub cmdClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdClose.Click
        DialogResult = Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub
    Dim dsReport As New DataSet
    Private Sub RefreshData()
        Dim ds As New DataSet
        Try
            SQL = "SELECT HBarang.NoID, HKontak.Kode + ' ' + HKontak.Nama AS Salesman, HBarang.Kode, HBarang.Barcode, HBarang.Nama, HKategori.Nama AS Kategori, SUM(HJualD.Qty*HJualD.Konversi) AS QtyPcs, SUM(HJualD.Jumlah) AS Total" & vbCrLf & _
                  " FROM HJual " & vbCrLf & _
                  " INNER JOIN HJualD ON HJual.NoID=HJualD.IDHeader " & vbCrLf & _
                  " INNER JOIN (HBarang LEFT JOIN HKategori ON HKategori.NoID=HBarang.IDKategori) ON HBarang.NoID=HJualD.IDBarang " & vbCrLf & _
                  " INNER JOIN HKontak ON HKontak.NoID=HJual.IDSalesman " & vbCrLf & _
                  " WHERE HJual.Tanggal>='" & DateEdit1.DateTime.ToString("yyyy-MM-dd") & "' AND HJual.Tanggal<'" & DateEdit2.DateTime.AddDays(1).ToString("yyyy-MM-dd") & "' " & vbCrLf & _
                  IIf(txtIDKategori.Text <> "", " AND HBarang.IDKategori=" & NullToLong(txtIDKategori.EditValue), "") & vbCrLf & _
                  " GROUP BY HBarang.NoID, HBarang.Kode, HBarang.Barcode, HBarang.Nama, HKategori.Nama, HKontak.Kode + ' ' + HKontak.Nama " & vbCrLf & _
                  " ORDER BY SUM(HJualD.Jumlah) DESC"
            EksekusiDataset(ds, "Data", SQL)
            GridControl1.DataSource = ds.Tables("Data")
            dsReport = ds
            With GridView1
                For i As Integer = 0 To .Columns.Count - 1
                    Select Case .Columns(i).ColumnType.Name.ToLower
                        Case "int32", "int64", "int"
                            .Columns(i).DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric
                            .Columns(i).DisplayFormat.FormatString = "n0"
                        Case "decimal", "single", "money", "double"
                            .Columns(i).DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric
                            .Columns(i).DisplayFormat.FormatString = "n2"
                        Case "string"
                            .Columns(i).DisplayFormat.FormatType = DevExpress.Utils.FormatType.None
                            .Columns(i).DisplayFormat.FormatString = ""
                        Case "date"
                            .Columns(i).DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime
                            .Columns(i).DisplayFormat.FormatString = "dd-MM-yyyy"
                        Case "datetime"
                            .Columns(i).DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime
                            .Columns(i).DisplayFormat.FormatString = "dd-MM-yyyy"
                        Case "byte[]"
                            reppicedit.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Squeeze
                            .Columns(i).OptionsColumn.AllowGroup = False
                            .Columns(i).OptionsColumn.AllowSort = False
                            .Columns(i).OptionsFilter.AllowFilter = False
                            .Columns(i).ColumnEdit = reppicedit
                        Case "boolean"
                            .Columns(i).ColumnEdit = repckedit
                    End Select
                    .ShowFindPanel()
                Next
            End With

            SQL = "SELECT HKontak.Kode + ' ' + HKontak.Nama AS Salesman, SUM(HJualD.Jumlah) AS Total" & vbCrLf & _
                  " FROM HJual " & vbCrLf & _
                  " INNER JOIN HJualD ON HJual.NoID=HJualD.IDHeader " & vbCrLf & _
                  " INNER JOIN (HBarang LEFT JOIN HKategori ON HKategori.NoID=HBarang.IDKategori) ON HBarang.NoID=HJualD.IDBarang " & vbCrLf & _
                  " INNER JOIN HKontak ON HKontak.NoID=HJual.IDSalesman " & vbCrLf & _
                  " WHERE HJual.Tanggal>='" & DateEdit1.DateTime.ToString("yyyy-MM-dd") & "' AND HJual.Tanggal<'" & DateEdit2.DateTime.AddDays(1).ToString("yyyy-MM-dd") & "' " & vbCrLf & _
                  IIf(txtIDKategori.Text <> "", " AND HBarang.IDKategori=" & NullToLong(txtIDKategori.EditValue), "") & vbCrLf & _
                  " GROUP BY HKontak.Kode + ' ' + HKontak.Nama" & vbCrLf & _
                  " ORDER BY SUM(HJualD.Jumlah) DESC"
            EksekusiDataset(ds, "Salesman", SQL)
            CC1.Series(0).DataSource = ds.Tables("Salesman")
            CC1.Series(0).ArgumentDataMember = "Salesman"
            CC1.Series(0).ValueDataMembers(0) = "Total"
            CC1.Series(0).ArgumentScaleType = DevExpress.XtraCharts.ScaleType.Qualitative
            CC1.Series(0).ValueScaleType = DevExpress.XtraCharts.ScaleType.Numerical
            CC1.Series(0).PointOptions.PointView = DevExpress.XtraCharts.PointView.ArgumentAndValues
            CC1.Legend.Visible = False
            If txtIDKategori.Text <> "" Then
                CC1.Titles(0).Text = "Penjualan Per Sales [" & txtIDKategori.Text & "]"
            Else
                CC1.Titles(0).Text = "Penjualan Per Sales"
            End If
            CC1.RefreshData()

        Catch ex As Exception
            XtraMessageBox.Show("Info Kesalahan : " & ex.Message, NamaAplikasi, MessageBoxButtons.OK, MessageBoxIcon.Information)
        Finally
            ds.Dispose()
        End Try
    End Sub

    Private Sub cmdRefresh_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdRefresh.Click
        RefreshData()
    End Sub

    Private Sub cmdHapus_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdHapus.Click

    End Sub

    Private Sub cmdEdit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdEdit.Click

    End Sub

    Private Sub cmdBaru_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdBaru.Click

    End Sub

    Private Sub cmdPreview_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdPreview.Click
        Dim NamaFile As String = Application.StartupPath & "\Report\LaporanPenjualanPerSales.repx"
        Try
            ViewXtraReport(Me.MdiParent, IIf(IsEditReport, Action_.Edit, Action_.Preview), NamaFile, "Laporan Penjualan Per Sales", "LaporanPenjualanPerSales.repx", dsReport, "A4", "TglDari=Datetime=#" & DateEdit1.DateTime.ToString("yyyy-MM-dd") & "#|TglSampai=Datetime=#" & DateEdit2.DateTime.ToString("yyyy-MM-dd") & "#|KategoriBarang=String='" & FixApostropi(txtIDKategori.Text) & "'")
        Catch ex As Exception
            XtraMessageBox.Show("Info Kesalahan : " & ex.Message, NamaAplikasi, MessageBoxButtons.OK, MessageBoxIcon.Information)
        End Try
    End Sub
End Class