Imports System.Drawing
Imports GTA

Public Class Indicator
    Inherits Script

    Public Shared veh As Vehicle = Nothing

    Private Sub Indicator_Tick(sender As Object, e As EventArgs) Handles Me.Tick
        If veh <> Nothing Then
            If veh.Bones.Contains("indicator_lf") Then World.DrawLightWithRange(veh.Bones.Item("indicator_lf").Position, Color.Orange, 0.5F, 1.0F)
            If veh.Bones.Contains("indicator_rr") Then World.DrawLightWithRange(veh.Bones.Item("indicator_rr").Position, Color.Orange, 0.5F, 1.0F)
            If veh.Bones.Contains("indicator_lr") Then World.DrawLightWithRange(veh.Bones.Item("indicator_lr").Position, Color.Orange, 0.5F, 1.0F)
            If veh.Bones.Contains("indicator_rf") Then World.DrawLightWithRange(veh.Bones.Item("indicator_rf").Position, Color.Orange, 0.5F, 1.0F)
        End If
    End Sub
End Class
