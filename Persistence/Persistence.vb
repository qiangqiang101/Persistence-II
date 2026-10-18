Imports GTA
Imports System.IO
Imports GTA.Native
Imports Newtonsoft.Json

Public Class Persistence
    Inherits Script

    Public PP As Ped
    Public LV As Vehicle
    Public NV As Vehicle

    Public Sub New()
        Logger.Delete()

        PP = Game.Player.Character
        LV = Game.Player.LastVehicle

        LoadLanguages()
        LoadSettings()

        RegisterDecor(modDecor, DecoratorType.Int)
        RegisterDecor(lastVehDecor, DecoratorType.Bool)
        RegisterDecor(modDecor2, DecoratorType.Bool)
    End Sub

    Private Sub Persistence_Tick(sender As Object, e As EventArgs) Handles Me.Tick
        RegisterDecor(modDecor, DecoratorType.Int)
        RegisterDecor(lastVehDecor, DecoratorType.Bool)
        RegisterDecor(modDecor2, DecoratorType.Bool)

        PP = Game.Player.Character
        LV = Game.Player.LastVehicle
        NV = World.GetClosestVehicle(PP.Position, 10.0F)

        If Not Native.Function.Call(Of Boolean)(Hash.GET_IS_LOADING_SCREEN_ACTIVE) Then
            If Not IsVehicleLoaded AndAlso Not IsVehicleLoading Then
                LoadVehicles(Directory.GetFiles(xmlPath, "*.xml"))
            End If

            PersistenceScriptRun()

            If showBlips Then PatchRedBlips()
        End If

        If Game.WasCheatStringJustEntered("pause persistence2") Then
            isPause = Not isPause
            GTA.UI.Screen.ShowHelpText($"Persistence II mod is {If(isPause, "Paused.", "Running.")}")
        End If

        If Game.WasCheatStringJustEntered("reload persistence2") Then
            Try
                For Each veh As Vehicle In listOfVeh
                    If DecoratorInterface.ExistsOn(veh, modDecor) Then
                        If showBlips Then veh.AttachedBlip?.Delete()
                        veh.Delete()
                    End If
                Next
                listOfVeh = New List(Of Vehicle)
                For Each trl As Vehicle In listOfTrl
                    If DecoratorInterface.ExistsOn(trl, modDecor) Then
                        trl.Delete()
                    End If
                Next
                listOfTrl = New List(Of Vehicle)
                IsVehicleLoaded = False
                IsVehicleLoading = False
            Catch ex As Exception
                Logger.Log(ex)
            End Try
        End If
    End Sub

    Private Sub PersistenceScriptRun()

        'v3 returns Nothing where v2 returned an empty entity, so every vehicle is null-checked before use.
        Dim chopper As Vehicle = GetNearestChopper()
        If chopper IsNot Nothing AndAlso chopper.IsPositionFrozen Then chopper.IsPositionFrozen = False

        If LV IsNot Nothing AndAlso NV = LV AndAlso Not listOfTrl.Contains(LV) Then
            If Not PP.IsInVehicle AndAlso PP.Position.DistanceToSquared(LV.Position) <= 10.0F AndAlso LV.LockStatus = VehicleLockStatus.Unlocked AndAlso Not listOfVeh.Contains(LV) Then
                DisableControls()
                If Not isPause Then DisplayHelpTextThisFrame(String.Format(GetLangEntry("lock"), saveKey.GetButtonIcon, LV.FullName))
                If IsSaveKeyJustReleased() Then
                    Try
                        Dim veh As New Vehicles(LV, GetPlayerCharacter)
                        If Not listOfVeh.Contains(LV) Then
                            listOfVeh.Add(LV)
                            LV.IsPersistent = True
                            LV.IsAlarmSet = True
                            If showBlips Then
                                LV.AddBlip()
                                If LV.AttachedBlip IsNot Nothing Then
                                    LV.AttachedBlip.Sprite = LV.GetSprite
                                    Select Case veh.Owner
                                        Case 0
                                            LV.AttachedBlip.Color = BlipColor.Michael
                                        Case 1
                                            LV.AttachedBlip.Color = BlipColor.Franklin
                                        Case 2
                                            LV.AttachedBlip.Color = BlipColor.Trevor
                                        Case 3
                                            LV.AttachedBlip.Color = BlipColor.NetPlayer1
                                    End Select
                                    LV.AttachedBlip.IsShortRange = True
                                    LV.AttachedBlip.Name = If(dispVehName, LV.FullName, Game.GetLocalizedString("PVEHICLE"))
                                End If
                            End If
                            DecoratorInterface.SetInt(LV, modDecor, CInt(GetPlayerCharacter()))
                            DecoratorInterface.SetBool(LV, modDecor2, True)

                            Dim newFile As String = Path.Combine(xmlPath, $"{DecoratorInterface.GetInt(LV, modDecor)}_{LV.Model.Hash}_{LV.Mods.LicensePlate}.xml")
                            Dim newpVeh As New PVehicle(newFile)
                            newpVeh.PlayerVehicles = New Vehicles(LV, DecoratorInterface.GetInt(LV, modDecor))
                            If LV.HasTrailer Then
                                newpVeh.TrailerVehicles = New Vehicles(LV.Trailer, DecoratorInterface.GetInt(LV, modDecor))
                                LV.Trailer.IsPersistent = True
                                DecoratorInterface.SetInt(LV.Trailer, modDecor, CInt(GetPlayerCharacter()))
                                DecoratorInterface.SetBool(LV.Trailer, modDecor2, True)
                                listOfTrl.Add(LV.Trailer)
                            End If
                            If LV.HasTowing Then
                                newpVeh.TrailerVehicles = New Vehicles(LV.TowedVehicle, DecoratorInterface.GetInt(LV, modDecor))
                                LV.TowedVehicle.IsPersistent = True
                                DecoratorInterface.SetInt(LV.TowedVehicle, modDecor, CInt(GetPlayerCharacter()))
                                DecoratorInterface.SetBool(LV.TowedVehicle, modDecor2, True)
                                listOfTrl.Add(LV.TowedVehicle)
                            End If
                            newpVeh.Save()
                            LV.LockVehicle(PP)
                            Script.Wait(1000)
                        End If
                    Catch ex As Exception
                        Logger.Log(ex)
                    End Try
                End If
            End If
        End If

        If NV IsNot Nothing AndAlso NV = GetNearestCar() Then
            If Not PP.IsInVehicle AndAlso NV.Position.DistanceToSquared(PP.Position) <= 10.0F AndAlso DecoratorInterface.ExistsOn(NV, modDecor) AndAlso NV.LockStatus = VehicleLockStatus.PlayerCannotEnter AndAlso DecoratorInterface.GetInt(NV, modDecor) = GetPlayerCharacter() Then
                DisableControls()
                If Not isPause Then DisplayHelpTextThisFrame(String.Format(GetLangEntry("unlock"), saveKey.GetButtonIcon, NV.FullName))
                If IsSaveKeyJustReleased() Then
                    Try
                        NV.UnlockVehicle(PP)
                        Script.Wait(1000)
                        Dim fileToDelete As String = Path.Combine(xmlPath, $"{GetOwnerName(DecoratorInterface.GetInt(NV, modDecor))}{NV.Make}{NV.LocalizedName}{NV.Mods.LicensePlate}{NV.Model.Hash}.xml")
                        Dim fileToDelete2 As String = Path.Combine(xmlPath, $"{DecoratorInterface.GetInt(NV, modDecor)}_{NV.Model.Hash}_{NV.Mods.LicensePlate}.xml")
                        If File.Exists(fileToDelete) Then File.Delete(fileToDelete) Else If File.Exists(fileToDelete2) Then File.Delete(fileToDelete2)

                        If showBlips Then NV.AttachedBlip?.Delete()
                        DecoratorInterface.SetBool(NV, modDecor2, False)
                        NV.IsPersistent = False
                        NV.IsAlarmSet = False
                        Select Case DecoratorInterface.GetInt(NV, modDecor)
                            Case 0, 1, 2, 3
                                If NV.HasTrailer Then If listOfTrl.Contains(NV.Trailer) Then listOfTrl.Remove(NV.Trailer)
                                If NV.HasTowing Then If listOfTrl.Contains(NV.TowedVehicle) Then listOfTrl.Remove(NV.TowedVehicle)
                                listOfVeh.Remove(NV)
                                RemoveDecor(NV, modDecor)
                        End Select
                    Catch ex As Exception
                        Logger.Log(ex)
                    End Try
                End If
            End If
        End If

        If LV IsNot Nothing AndAlso Not LV.IsVehiclePersist Then
            ReleasePersistLastVehicle()
            LV.SetVehicleIsPersist
        End If
    End Sub

    Private Sub Persistence_Aborted(sender As Object, e As EventArgs) Handles Me.Aborted
        Try
            For Each veh As Vehicle In listOfVeh
                If DecoratorInterface.ExistsOn(veh, modDecor) Then
                    If showBlips Then veh.AttachedBlip?.Delete()
                    veh.Delete()
                End If
            Next
            For Each trl As Vehicle In listOfTrl
                If DecoratorInterface.ExistsOn(trl, modDecor) Then
                    trl.Delete()
                End If
            Next
        Catch ex As Exception
            Logger.Log(ex)
        End Try
    End Sub

End Class
