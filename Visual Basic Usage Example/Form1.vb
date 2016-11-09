Public Class Form1

    Private Sub ButtonConnect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonConnect.Click
        Me.TerminalControl1.UserName = Me.TextBoxUsername.Text
        Me.TerminalControl1.Password = Me.TextBoxPassword.Text
        Me.TerminalControl1.Host = Me.TextBoxServer.Text
        Me.TerminalControl1.Method = WalburySoftware.ConnectionMethod.SSH2

        Me.TerminalControl1.Connect()

        Me.TerminalControl1.SetPaneColors(Color.Blue, Color.Black)
        Me.TerminalControl1.Focus()

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        If Me.TerminalControl1.TerminalPane.ConnectionTag Is Nothing Then
            Return ' make sure we at least have something to edit :)
        End If

        Dim dlg As Poderosa.Forms.EditRenderProfile
        dlg = New Poderosa.Forms.EditRenderProfile(Me.TerminalControl1.TerminalPane.ConnectionTag.RenderProfile)


        If dlg.ShowDialog() = DialogResult.OK Then

        Else
            Return
        End If

        Me.TerminalControl1.TerminalPane.ConnectionTag.RenderProfile = dlg.Result
        Me.TerminalControl1.TerminalPane.ApplyRenderProfile(dlg.Result)

    End Sub
End Class
