Imports GTA
Imports GTA.Native
Imports System.Windows.Forms

Public Class SoundTester
    Inherits Script

    Private ReadOnly candidates As String()() = {
        New String() {"SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET"},
        New String() {"NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET"},
        New String() {"Remote_Control_Close", "PI_Menu_Sounds"},
        New String() {"Remote_Control_Open", "PI_Menu_Sounds"},
        New String() {"Remote_Control_Fob", "PI_Menu_Sounds"},
        New String() {"unlocked_bleep", "HACKING_DOOR_UNLOCK_SOUNDS"},
        New String() {"CONFIRM_BEEP", "HUD_MINI_GAME_SOUNDSET"},
        New String() {"Garage_Door_Unlocked", "DLC_BTL_Showroom_Sounds"},
        New String() {"door_unlocked", "dlc_sum20_factory_raid_sounds"}
    }
    Private index As Integer = 0

    Public Sub New()
        AddHandler KeyDown, AddressOf OnKeyDown
    End Sub

    Private Sub OnKeyDown(sender As Object, e As KeyEventArgs)
        If e.KeyCode <> Keys.F10 Then Return
        Dim veh As Vehicle = Game.Player.LastVehicle
        If veh Is Nothing Then Return
        Dim c = candidates(index)
        Dim id As Integer = Native.Function.Call(Of Integer)(Hash.GET_SOUND_ID)
        Audio.PlaySoundFrontendAndForget(c(0), c(1))
        Script.Wait(300)
        Native.Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, id, c(0), veh, c(1), False, 0)
        Dim missing As Boolean = True
        For i As Integer = 1 To 10
            Script.Wait(0)
            If Not Native.Function.Call(Of Boolean)(Hash.HAS_SOUND_FINISHED, id) Then missing = False : Exit For
        Next
        Native.Function.Call(Hash.RELEASE_SOUND_ID, id)
        GTA.UI.Screen.ShowSubtitle($"{index + 1}/{candidates.Length} {c(0)} ({c(1)}){If(missing, " MISSING", "")}", 3000)
        index = (index + 1) Mod candidates.Length
    End Sub
End Class