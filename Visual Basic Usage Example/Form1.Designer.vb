<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.TerminalControl1 = New WalburySoftware.TerminalControl
        Me.TextBoxServer = New System.Windows.Forms.TextBox
        Me.Label1 = New System.Windows.Forms.Label
        Me.Label2 = New System.Windows.Forms.Label
        Me.TextBoxUsername = New System.Windows.Forms.TextBox
        Me.Label3 = New System.Windows.Forms.Label
        Me.TextBoxPassword = New System.Windows.Forms.TextBox
        Me.ButtonConnect = New System.Windows.Forms.Button
        Me.Button1 = New System.Windows.Forms.Button
        Me.SuspendLayout()
        '
        'TerminalControl1
        '
        Me.TerminalControl1.AuthType = Poderosa.ConnectionParam.AuthType.Password
        Me.TerminalControl1.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.TerminalControl1.Host = Nothing
        Me.TerminalControl1.IdentifyFile = ""
        Me.TerminalControl1.Location = New System.Drawing.Point(0, 101)
        Me.TerminalControl1.Method = WalburySoftware.ConnectionMethod.Telnet
        Me.TerminalControl1.Name = "TerminalControl1"
        Me.TerminalControl1.Password = Nothing
        Me.TerminalControl1.Size = New System.Drawing.Size(506, 169)
        Me.TerminalControl1.TabIndex = 0
        Me.TerminalControl1.Text = "TerminalControl1"
        Me.TerminalControl1.UserName = Nothing
        '
        'TextBoxServer
        '
        Me.TextBoxServer.Location = New System.Drawing.Point(82, 10)
        Me.TextBoxServer.Name = "TextBoxServer"
        Me.TextBoxServer.Size = New System.Drawing.Size(100, 20)
        Me.TextBoxServer.TabIndex = 1
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(13, 13)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(63, 13)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "SSH Server"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(19, 39)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(57, 13)
        Me.Label2.TabIndex = 3
        Me.Label2.Text = "UserName"
        '
        'TextBoxUsername
        '
        Me.TextBoxUsername.Location = New System.Drawing.Point(82, 36)
        Me.TextBoxUsername.Name = "TextBoxUsername"
        Me.TextBoxUsername.Size = New System.Drawing.Size(100, 20)
        Me.TextBoxUsername.TabIndex = 4
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(23, 65)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(53, 13)
        Me.Label3.TabIndex = 5
        Me.Label3.Text = "Password"
        '
        'TextBoxPassword
        '
        Me.TextBoxPassword.Location = New System.Drawing.Point(82, 62)
        Me.TextBoxPassword.Name = "TextBoxPassword"
        Me.TextBoxPassword.PasswordChar = Global.Microsoft.VisualBasic.ChrW(42)
        Me.TextBoxPassword.Size = New System.Drawing.Size(100, 20)
        Me.TextBoxPassword.TabIndex = 6
        '
        'ButtonConnect
        '
        Me.ButtonConnect.Location = New System.Drawing.Point(206, 7)
        Me.ButtonConnect.Name = "ButtonConnect"
        Me.ButtonConnect.Size = New System.Drawing.Size(75, 23)
        Me.ButtonConnect.TabIndex = 7
        Me.ButtonConnect.Text = "Connect"
        Me.ButtonConnect.UseVisualStyleBackColor = True
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(206, 39)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(75, 23)
        Me.Button1.TabIndex = 8
        Me.Button1.Text = "Edit Display"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(506, 270)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.ButtonConnect)
        Me.Controls.Add(Me.TextBoxPassword)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.TextBoxUsername)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.TextBoxServer)
        Me.Controls.Add(Me.TerminalControl1)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents TerminalControl1 As WalburySoftware.TerminalControl
    Friend WithEvents TextBoxServer As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents TextBoxUsername As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents TextBoxPassword As System.Windows.Forms.TextBox
    Friend WithEvents ButtonConnect As System.Windows.Forms.Button
    Friend WithEvents Button1 As System.Windows.Forms.Button

End Class
