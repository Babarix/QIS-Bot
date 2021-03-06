﻿Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Net.Mail

Public Class QIS_Bot

    Dim state As Integer = 0
    Dim button As HtmlElement
    Dim tableneu As List(Of List(Of String))
    Dim tablealt As List(Of List(Of String))
    Dim maxzalt As Integer = -1
    Dim maxzneu As Integer = -1
    Dim maxspalten As Integer = 10
    Dim rest As Integer = 0
    Dim ticks As Integer = 0
    Dim fund As Integer = 0

    ''' <summary>
    ''' Klick auf Button1 der den Bot startet
    ''' </summary>
    ''' <param name="sender">StandartÃ¼bergabe</param>
    ''' <param name="e">StandartÃ¼bergabe</param>
    ''' <remarks></remarks>
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        If (TextBox1.Text <> "" And TextBox2.Text <> "" And TextBox3.Text > 59) Then
            If (File.Exists(TextBox4.Text) = True And TextBox4.Text.EndsWith(".wav") = True) Then
                Timer1.Interval = TextBox3.Text * 60000
                GroupBox1.Enabled = False
                'Timer1.Interval = 5000
                Timer1.Stop()
                Timer2.Stop()
                ProgressBar1.Value = 0
                ProgressBar1.Maximum = 50
                DataGridView1.Visible = False
                GroupBox3.Enabled = False
                Label3.Text = "Datenabrufen"
                log("Bot gestartet")
                log("Timer: " + TextBox3.Text)
                log("Alarm Ton: " + TextBox4.Text)
                log("Alarm An: " + CheckBox1.Checked.ToString)
                writeOptions()
                Timer3.Start()
                WebBrowser1.Navigate("https://qis.hs-albsig.de/qisserver/rds?state=user&type=0")
            Else
                MsgBox("Angegebene Datei existirt nicht oder ist keine WAV Datei!")
            End If
        Else
            MsgBox("Einstellungen ungÃ¼ltig")
        End If
    End Sub

    Sub run()
        log("run")
        GroupBox3.Enabled = True
        DataGridView1.Visible = True
        reload()
        readSave()
        Dim tex As List(Of List(Of String)) = New List(Of List(Of String))
        For Each t As List(Of String) In tableneu
            Dim b As Boolean = False
            For Each tt As List(Of String) In tablealt
                If (t(0) = tt(0)) Then
                    b = True
                End If
            Next
            If (b = False) Then
                tex.Add(t)
            End If
            'If (tablealt.Contains(t) = False) Then
            '    tex.Add(t)
            'End If
        Next

        If (tex.Count > 0) Then 'vergleich
            'meldung
            If (CheckBox1.Checked) Then
                My.Computer.Audio.Play(TextBox4.Text)
            End If
            Dim mel As String = "Neuer Eintrag: " + Environment.NewLine()
            For Each l As List(Of String) In tex
                mel += l(1) + Environment.NewLine() + "Note: " + l(3) + Environment.NewLine() + "Status: " + l(4) + Environment.NewLine() + Environment.NewLine()
            Next

            log(mel)
            If (CheckBox2.Checked = True) Then
                sendMail(mel)
            End If
            MsgBox(mel) '3
        End If

        writeSave()
        rest = 0
        Label3.Text = "Warten auf nÃ¤chtes abfragen, Restzeit: " + (TextBox3.Text - rest).ToString
        ProgressBar1.Value = 0
        ProgressBar1.Maximum = TextBox3.Text * 10
        Timer2.Start()
        Timer1.Start()
    End Sub

    Private Sub WebBrowser1_DocumentCompleted(ByVal sender As System.Object, ByVal e As System.Windows.Forms.WebBrowserDocumentCompletedEventArgs) Handles WebBrowser1.DocumentCompleted
        If (state = 0) Then
            Label3.Text = "Datenabrufen: Step " + state.ToString
            ProgressBar1.PerformStep()
            WebBrowser1.Document.GetElementById("Benutzername").SetAttribute("Value", TextBox1.Text)
            WebBrowser1.Document.GetElementById("pass").SetAttribute("Value", TextBox2.Text)
            button = WebBrowser1.Document.GetElementById("submit")
            button.InvokeMember("click")
            state = 1
        ElseIf (state = 1) Then
            Label3.Text = "Datenabrufen: Step " + state.ToString
            ProgressBar1.PerformStep()
            button = WebBrowser1.Document.GetElementById("makronavigation").Children(0).Children(2).Children(1)
            button.InvokeMember("click")
            Timer3.Stop()
            state += 1
        ElseIf (state = 2) Then
            Label3.Text = "Datenabrufen: Step " + state.ToString
            ProgressBar1.PerformStep()
            button = WebBrowser1.Document.GetElementById("wrapper").Children(7).Children(1).Children(3).Children(0).Children(0).Children(0).Children(1).Children(1)
            button.InvokeMember("click")
            state += 1
        ElseIf (state = 3) Then
            Label3.Text = "Datenabrufen: Step " + state.ToString
            ProgressBar1.PerformStep()
            button = WebBrowser1.Document.Forms(0).Children(2).Children(0).Children(2)
            button.InvokeMember("click")
            state += 1
        ElseIf (state = 4) Then
            Label3.Text = "Datenabrufen: Step " + state.ToString
            ProgressBar1.PerformStep()
            ' tableneuLoad(WebBrowser1.Document.Forms(1).Children(4).Children(0).InnerHtml)
            tableneuLoad(WebBrowser1.Document.Forms(0).Children(6).Children(0).InnerHtml)
            button = WebBrowser1.Document.GetElementById("wrapper").Children(4).Children(2)
            button.InvokeMember("click")
            state += 1
            run()
        ElseIf (state = 5) Then
            state += 1
        ElseIf (state = 6) Then
            state = 0
        End If
        log("State: " + state.ToString)
    End Sub

    Sub tableneuLoad(ByVal tex As String)
        Label3.Text = "Datenverarbeiten"
        log("Datenverarbeiten")
        ProgressBar1.PerformStep()
        tableneu.Clear()
        Dim zeilen As String() = tex.Split(Environment.NewLine())
        Dim i As Integer = 0
        Dim x As Integer = -1 'zeilen
        Dim y As Integer = 0 'spalten
        maxzneu = -3
        Try
            While (i >= 0)
                If (zeilen(i).Contains("<TR>")) Then
                    maxzneu += 1
                End If
                i += 1
            End While
        Catch ex As Exception
        End Try

        For d As Integer = 0 To maxzneu
            tableneu.Add(New List(Of String))
        Next

        i = 13
        While (x < maxzneu)
            x += 1
            i += 1
            y = 0
            While (y < maxspalten)
                Dim t As String = Regex_Replace_Example(zeilen(i))
                If t.EndsWith(" ") Then
                    t = t.Substring(0, t.Length - 1)
                End If
                tableneu(x).Add(t)
                y += 1
                i += 1
            End While
        End While
        i = 0
        log("maxzneu: " + maxzneu.ToString)
    End Sub

    Function Regex_Replace_Example(ByVal sInput As String) As String 'Quelle http://dotnetdud.blogspot.com/2008/06/remove-html-tags-from-string-using-net.html
        Dim sOut As String
        sOut = Regex.Replace(sInput, "<[^<>]+>", "")
        sOut = Regex.Replace(sOut, "&nbsp;", "")
        sOut = Regex.Replace(sOut, Environment.NewLine, "")
        sOut = Regex.Replace(sOut, "\n", "")
        Return sOut
    End Function

    Sub reload()
        Dim x As Integer = -1 'zeilen
        Dim y As Integer = 0 'spalten
        DataGridView1.Rows.Clear()
        While (x < maxzneu)
            x += 1
            y = 0
            Dim dgvRow As New DataGridViewRow
            Dim dgvCell As DataGridViewCell
            While (y < maxspalten)
                dgvCell = New DataGridViewTextBoxCell()
                dgvCell.Value = tableneu(x)(y)
                dgvRow.Cells.Add(dgvCell)
                y += 1
            End While
            DataGridView1.Rows.Add(dgvRow)
        End While
    End Sub

    Function checklist(ByVal talt(,) As String, ByVal tneu(,) As String) As String
        log("Checklist")
        Dim x As Integer = 0 'zeilen
        Dim lalt As New ArrayList()
        Dim lneu As New ArrayList()

        If (maxzalt <> maxzneu) Then
            While (x < maxzalt)
                x += 1
                lalt.Add(talt(x, 1)) 'alt
            End While
            x = 0
            While (x < maxzneu)
                x += 1
                lneu.Add(tneu(x, 1)) 'neu
            End While
            For Each e In lneu
                If (lalt.Contains(e)) Then
                Else
                    fund = (lneu.IndexOf(e) + 1)
                    Return e
                End If
            Next
            Return "none"
        Else
            Return "none"
        End If
    End Function

    Sub readSave()
        log("ReadSave")
        Try
            Dim objDateiLeser As StreamReader
            Dim x As Integer = -1 'zeilen
            Dim y As Integer = 0 'spalten
            Dim ii As Integer = 0
            objDateiLeser = New StreamReader("save.txt")
            maxzalt = objDateiLeser.ReadLine()    '1 Zeilen
            maxspalten = objDateiLeser.ReadLine()   '2 Spalten
            tablealt.Clear()
            For d As Integer = 0 To maxzalt
                tablealt.Add(New List(Of String))
            Next
            While (x < maxzalt)
                x += 1
                y = 0
                While (y < maxspalten)
                    tablealt(x).Add(objDateiLeser.ReadLine().Substring(2))
                    y += 1
                End While
            End While
            objDateiLeser.Close()
            objDateiLeser = Nothing
        Catch ex As Exception
        End Try
        log("maxzalt: " + maxzalt.ToString)
    End Sub

    Sub readOptions()
        '1 Name
        '2 Password
        '3 Zeit
        '4 Alarm Ton
        '5 Alarm An
        '6 Email adresse
        '7 Email an
        log("readOptions")
        Try
            Dim objDateiLeser As StreamReader
            objDateiLeser = New StreamReader("options.txt")
            TextBox1.Text = objDateiLeser.ReadLine
            TextBox2.Text = objDateiLeser.ReadLine
            TextBox3.Text = objDateiLeser.ReadLine
            TextBox4.Text = objDateiLeser.ReadLine
            CheckBox1.Checked = (objDateiLeser.ReadLine = "True")
            TextBox5.Text = objDateiLeser.ReadLine
            CheckBox2.Checked = (objDateiLeser.ReadLine = "True")
            objDateiLeser.Close()
            objDateiLeser = Nothing
        Catch ex As Exception
            log("Fehler: " + ex.ToString)
        End Try

    End Sub

    Sub writeOptions()
        log("WriteSave")
        Try
            Dim writer As StreamWriter
            writer = New StreamWriter("options.txt", False)
            writer.WriteLine(TextBox1.Text)
            writer.WriteLine(TextBox2.Text)
            writer.WriteLine(TextBox3.Text)
            writer.WriteLine(TextBox4.Text)
            writer.WriteLine(CheckBox1.Checked.ToString)
            writer.WriteLine(TextBox5.Text)
            writer.WriteLine(CheckBox2.Checked.ToString)
            writer.Close()
            writer = Nothing
        Catch ex As Exception
            log("Fehler: " + ex.ToString)
        End Try
    End Sub

    Sub writeSave()
        log("WriteSave")
        Dim writer As StreamWriter
        Dim x As Integer = -1 'zeilen
        Dim y As Integer = 0 'spalten
        Dim ii As Integer = 0
        Try
            writer = New StreamWriter("save.txt", False)
            writer.WriteLine(maxzneu)     '1 Zeilen
            writer.WriteLine(maxspalten)    '2 Spalten
            While (x < maxzneu)
                x += 1
                y = 0
                ii = 0
                While (y < maxspalten)
                    writer.WriteLine(ii.ToString + " " + tableneu(x)(y))
                    y += 1
                    ii += 1
                End While
            End While
            writer.Close()
            writer = Nothing
        Catch ex As Exception
        End Try


        GC.Collect()
        GC.WaitForPendingFinalizers()
    End Sub

    Sub log(ByVal tex As String)
        Dim writer As StreamWriter
        writer = New StreamWriter("log.txt", True)
        writer.WriteLine(Date.Now + " " + tex)     '1 Zeilen
        writer.Flush()
        writer.Close()
        writer = Nothing
    End Sub


    Private Sub QIS_Bot_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Label3.Text = "Bot aus"
        TextBox4.Text = Application.StartupPath + "\submarinealert.wav"
        log("##Programm start##")
        log("Version: " + Application.ProductVersion)
        tableneu = New List(Of List(Of String))
        tablealt = New List(Of List(Of String))
        readOptions()
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Timer1.Stop()
        Timer2.Stop()
        ticks += 1
        log("#Tick#" + Environment.NewLine)
        log("Ticks: " + ticks.ToString)
        ProgressBar1.Value = 0
        ProgressBar1.Maximum = 50
        DataGridView1.Visible = False
        GroupBox3.Enabled = False
        Label3.Text = "Datenabrufen"
        state = 0
        Timer3.Start()
        WebBrowser1.Navigate("https://qis.hs-albsig.de/qisserver/rds?state=user&type=0")
        'Dim objDateiLeser As StreamReader
        'objDateiLeser = New StreamReader("M:\\test.txt")
        'tableneuLoad(objDateiLeser.ReadToEnd())
        'objDateiLeser.Close()
        'objDateiLeser = Nothing
        'run()
    End Sub

    Private Sub Timer2_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer2.Tick
        rest += 1
        ProgressBar1.PerformStep()
        Label3.Text = "Warten auf nÃ¤chtes abfragen, Restzeit: " + (TextBox3.Text - rest).ToString
    End Sub

    Private Sub QIS_Bot_FormClosed(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles MyBase.FormClosed
        Timer1.Stop()
        Timer2.Stop()
        log("##Programm Ende##" + Environment.NewLine + Environment.NewLine)
    End Sub

    Private Sub QIS_Bot_KeyUp(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles MyBase.KeyUp
        If e.KeyCode = Keys.F1 Then
            Info.ShowDialog()
        End If
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Dim tex As String
        OpenFileDialog1.ShowDialog()
        tex = OpenFileDialog1.FileName
        While (File.Exists(tex) = False Or tex.EndsWith(".wav") = False)
            MsgBox("Angegebene Datei existirt nicht oder ist keine WAV Datei!")
            OpenFileDialog1.ShowDialog()
            tex = OpenFileDialog1.FileName
        End While
        TextBox4.Text = tex
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        TextBox4.Text = Application.StartupPath + "\submarinealert.wav"
    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        Me.Close()
    End Sub

    Private Sub sendMail(ByVal tex)
        Try
            log("EMAIL mit: " + tex)
            Dim myClient As New Net.Mail.SmtpClient("smtp.web.de")
            myClient.Credentials = New Net.NetworkCredential("QIS_info@web.de", AscToString(49, 50, 51, 52, 53, 54, 113, 119, 101))
            myClient.Send("QIS_info@web.de", TextBox5.Text, "QIS BOT", tex)
        Catch ex As Exception
            MsgBox("Fehler beim senden der E-Mail!")
            log("Fehler beim sender der E-Mail: " + ex.Message)
        End Try
    End Sub

    Private Sub Button5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button5.Click
        sendMail("QIS BOT TEST E-MAIL")
        MsgBox("Test E-Mail gesendet")
    End Sub

    Private Sub Timer3_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer3.Tick
        Timer3.Stop()
        MsgBox("Fehler auf der QIS Seite. Vieleicht Wartungen.")
        log("Fehler auf der QIS Seite. Vieleicht Wartungen.")
        Me.Close()
    End Sub

    'Quelle: http://www.vbarchiv.net/tipps/details.php?id=825
    Public Function AscToString(ByVal ParamArray nAsc() As Object) As String
        Dim i As Integer
        Dim sString As String

        For i = 0 To UBound(nAsc)
            sString = sString & Chr(nAsc(i))
        Next i
        AscToString = sString
    End Function
End Class