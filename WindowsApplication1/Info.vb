Imports System.Net.Mail

Public Class Info

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        System.Diagnostics.Process.Start("explorer", (Application.StartupPath & "\log.txt"))
    End Sub

    Private Sub Form4_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Label3.Text = "QIS Bot Version: " & Application.ProductVersion
    End Sub

End Class