Imports GTA
Imports System.IO
Imports Metadata
Imports GTA.Native
Imports System.Net
Imports System.Drawing
Imports GTA.Math

Public Class Persistence
    Inherits Script

    Public PP As Ped
    Public LV As Vehicle
    Public NV As Vehicle

    Public Sub New()
        PP = Game.Player.Character
        LV = Game.Player.Character.LastVehicle

        LoadSettings()

        Decor.Unlock()
        Decor.Register(modDecor, Decor.eDecorType.Int)
        Decor.Register(lastVehDecor, Decor.eDecorType.Bool)
        Decor.Register(modDecor2, Decor.eDecorType.Bool)
        Decor.Lock()
    End Sub

    Private Sub Persistence_Tick(sender As Object, e As EventArgs) Handles Me.Tick
        RegisterDecor(modDecor, Decor.eDecorType.Int)
        RegisterDecor(lastVehDecor, Decor.eDecorType.Bool)
        RegisterDecor(modDecor2, Decor.eDecorType.Bool)

        PP = Game.Player.Character
        LV = Game.Player.Character.LastVehicle
        NV = World.GetClosestVehicle(PP.Position, 10.0F)

        If Not Game.IsLoading Then
            If Not IsVehicleLoaded AndAlso Not IsVehicleLoading Then
                LoadVehicles(Directory.GetFiles(xmlPath, "*.xml"))
            End If

            If IsFlatbedModInstalled() Then
                If PP.Position.DistanceToSquared(PP.LastFlatbed.Position) >= 50.0F Then PersistenceScriptRun()
            Else
                PersistenceScriptRun()
            End If

            If showBlips Then PatchRedBlips()
        End If

        If Cheating("reload persistence2") Then
            Try
                For Each veh As Vehicle In listOfVeh
                    If veh.ExistsOn(modDecor) Then
                        If showBlips Then veh.CurrentBlip.Remove()
                        veh.Delete()
                    End If
                Next
                listOfVeh = New List(Of Vehicle)
                For Each trl As Vehicle In listOfTrl
                    If trl.ExistsOn(modDecor) Then
                        trl.Delete()
                    End If
                Next
                listOfTrl = New List(Of Vehicle)
                IsVehicleLoaded = False
                IsVehicleLoading = False
            Catch ex As Exception
                Logger.Log($"{ex.Message}{ex.HResult}{ex.StackTrace}")
            End Try
        End If
    End Sub

    Private Sub PersistenceScriptRun()
        If GetNearestChopper.FreezePosition Then GetNearestChopper.FreezePosition = False

        If NV = LV AndAlso Not listOfTrl.Contains(LV) Then
            If Not PP.IsInVehicle AndAlso PP.Position.DistanceToSquared(LV.Position) <= 10.0F AndAlso LV.LockStatus = VehicleLockStatus.Unlocked AndAlso Not listOfVeh.Contains(LV) Then
                DisableControls()
                DisplayHelpTextThisFrame(String.Format(GetLangEntry("lock"), saveKey.GetButtonIcon, LV.FullName))
                If Game.IsControlJustReleased(0, saveKey) Then
                    Try
                        Dim veh As New Vehicles(LV, GetPlayerCharacter)
                        If Not listOfVeh.Contains(LV) Then
                            listOfVeh.Add(LV)
                            LV.IsPersistent = True
                            LV.HasAlarm = True
                            If showBlips Then
                                LV.AddBlip()
                                LV.CurrentBlip.Sprite = LV.GetSprite
                                Select Case veh.Owner
                                    Case 0
                                        LV.CurrentBlip.Color = BlipColor.Michael
                                    Case 1
                                        LV.CurrentBlip.Color = BlipColor.Franklin
                                    Case 2
                                        LV.CurrentBlip.Color = BlipColor.Trevor
                                    Case 3
                                        LV.CurrentBlip.Color = BlipColor.NetPlayer1
                                End Select
                                LV.CurrentBlip.IsShortRange = True
                                LV.CurrentBlip.Name = If(dispVehName, LV.FullName, Game.GetGXTEntry("PVEHICLE"))
                            End If
                            LV.SetInt(modDecor, CInt(GetPlayerCharacter()))
                            LV.SetBool(modDecor2, True)

                            Dim newFile As String = Path.Combine(xmlPath, $"{LV.GetInt(modDecor)}_{LV.Model.Hash}_{LV.NumberPlate}.xml")
                            Dim newpVeh As New PVehicle(newFile)
                            newpVeh.PlayerVehicles = New Vehicles(LV, LV.GetInt(modDecor))
                            If LV.HasTrailer Then
                                newpVeh.TrailerVehicles = New Vehicles(LV.Trailer, LV.GetInt(modDecor))
                                LV.Trailer.IsPersistent = True
                                LV.Trailer.SetInt(modDecor, CInt(GetPlayerCharacter()))
                                LV.Trailer.SetBool(modDecor2, True)
                                listOfTrl.Add(LV.Trailer)
                            End If
                            If LV.HasTowing Then
                                newpVeh.TrailerVehicles = New Vehicles(LV.TowedVehicle, LV.GetInt(modDecor))
                                LV.TowedVehicle.IsPersistent = True
                                LV.TowedVehicle.SetInt(modDecor, CInt(GetPlayerCharacter()))
                                LV.TowedVehicle.SetBool(modDecor2, True)
                                listOfTrl.Add(LV.TowedVehicle)
                            End If
                            newpVeh.Save()
                            LV.LockVehicle(PP)
                            Script.Wait(1000)
                        End If
                    Catch ex As Exception
                        Logger.Log($"{ex.Message}{ex.HResult}{ex.StackTrace}")
                    End Try
                End If
            End If
        End If

        If NV = GetNearestCar() Then
            If Not PP.IsInVehicle AndAlso NV.Position.DistanceToSquared(PP.Position) <= 10.0F AndAlso NV.ExistsOn(modDecor) AndAlso NV.LockStatus = VehicleLockStatus.LockedForPlayer AndAlso NV.GetInt(modDecor) = GetPlayerCharacter() Then
                DisableControls()
                DisplayHelpTextThisFrame(String.Format(GetLangEntry("unlock"), saveKey.GetButtonIcon, NV.FullName))
                If Game.IsControlJustReleased(0, saveKey) Then
                    Try
                        NV.UnlockVehicle(PP)
                        Script.Wait(1000)
                        Dim fileToDelete As String = Path.Combine(xmlPath, $"{GetOwnerName(NV.GetInt(modDecor))}{NV.Make}{NV.FriendlyName}{NV.NumberPlate}{NV.Model.Hash}.xml")
                        Dim fileToDelete2 As String = Path.Combine(xmlPath, $"{LV.GetInt(modDecor)}_{LV.Model.Hash}_{LV.NumberPlate}.xml")
                        If File.Exists(fileToDelete) Then File.Delete(fileToDelete) Else If File.Exists(fileToDelete2) Then File.Delete(fileToDelete2)

                        If showBlips Then NV.CurrentBlip.Remove()
                        NV.SetBool(modDecor2, False)
                        NV.IsPersistent = False
                        NV.HasAlarm = False
                        Select Case NV.GetInt(modDecor)
                            Case 0, 1, 2, 3
                                If NV.HasTrailer Then If listOfTrl.Contains(NV.Trailer) Then listOfTrl.Remove(NV.Trailer)
                                If NV.HasTowing Then If listOfTrl.Contains(NV.TowedVehicle) Then listOfTrl.Remove(NV.TowedVehicle)
                                listOfVeh.Remove(NV)
                                Decor.Unlock()
                                NV.Remove(modDecor)
                                Decor.Lock()
                        End Select
                    Catch ex As Exception
                        Logger.Log($"{ex.Message}{ex.HResult}{ex.StackTrace}")
                    End Try
                End If
            End If
        End If

        If Not LV.IsVehiclePersist Then
            ReleasePersistLastVehicle()
            LV.SetVehicleIsPersist
        End If
    End Sub

    Private Sub Persistence_Aborted(sender As Object, e As EventArgs) Handles Me.Aborted
        Try
            For Each veh As Vehicle In listOfVeh
                If veh.ExistsOn(modDecor) Then
                    If showBlips Then veh.CurrentBlip.Remove()
                    veh.Delete()
                End If
            Next
            For Each trl As Vehicle In listOfTrl
                If trl.ExistsOn(modDecor) Then
                    trl.Delete()
                End If
            Next
        Catch ex As Exception
            Logger.Log($"{ex.Message}{ex.HResult}{ex.StackTrace}")
        End Try
    End Sub

End Class
