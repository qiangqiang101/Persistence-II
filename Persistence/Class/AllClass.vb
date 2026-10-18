Public Class Language
    Public Property name As String
    Public Property data As New List(Of LanguageData)

    Public Shared Function Defaults() As List(Of Language)
        Return New List(Of Language) From {
            New Language With {
            .name = "american",
            .data = New List(Of LanguageData) From {
                New LanguageData("loaded", "{0} vehicles loaded."),
                New LanguageData("lock", "Press {0} to lock {1}."),
                New LanguageData("unlock", "Press {0} to unlock {1}.")
            }
        }}
    End Function
End Class

Public Class LanguageData
    Public Property name As String
    Public Property value As String

    Public Sub New()

    End Sub

    Public Sub New(name As String, value As String)
        Me.name = name
        Me.value = value
    End Sub

End Class
