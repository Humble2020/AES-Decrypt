Imports System.IO

Public Class Form1
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Try
            Dim jfd As String = Nothing
            Dim oo As New OpenFileDialog
            If oo.ShowDialog = DialogResult.OK Then
                jfd = oo.FileName
            End If
            Dim text As String = ReadFile(jfd)
            Dim aes As New Aes()
            RichTextBox1.Text = aes.Decrypt(text, Me.TextBox2.Text)
        Catch ex As Exception

        End Try
    End Sub
    Public Function ReadFile(ByVal name As String) As String
        ''Dim desktopPath As String = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
        Return File.ReadAllText(name)
    End Function
End Class
