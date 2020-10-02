Imports System.Drawing
Imports GTA

Public Class Indicator
    Inherits Script

    Public Shared veh As Vehicle = Nothing

    Private Sub Indicator_Tick(sender As Object, e As EventArgs) Handles Me.Tick
        If veh <> Nothing Then
            If veh.HasBone("indicator_lf") Then World.DrawLightWithRange(veh.GetBoneCoord("indicator_lf"), Color.Orange, 0.5F, 1.0F)
            If veh.HasBone("indicator_rr") Then World.DrawLightWithRange(veh.GetBoneCoord("indicator_rr"), Color.Orange, 0.5F, 1.0F)
            If veh.HasBone("indicator_lr") Then World.DrawLightWithRange(veh.GetBoneCoord("indicator_lr"), Color.Orange, 0.5F, 1.0F)
            If veh.HasBone("indicator_rf") Then World.DrawLightWithRange(veh.GetBoneCoord("indicator_rf"), Color.Orange, 0.5F, 1.0F)
        End If
    End Sub
End Class
