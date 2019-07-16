Imports System.Drawing
Imports System.IO
Imports System.Media
Imports System.Runtime.CompilerServices
Imports GTA
Imports GTA.Math
Imports GTA.Native
Imports Metadata

Module Helper

    'Config
    Public config As ScriptSettings = ScriptSettings.Load("scripts\Persistence\Persistence.ini")
    Public dispVehName As Boolean = False
    Public saveKey As GTA.Control = GTA.Control.Context
    Public showBlips As Boolean = True
    Public alarmVolume As Integer = 100
    Public saveDamage As Boolean = True
    'Public todayPassword As String = "password"
    'Public todayPasswordWeb As String = "password"

    'Path
    Public xmlPath As String = ".\scripts\Persistence\Vehicles\"
    Public soundPath As String = ".\scripts\Persistence\Sound\"

    'Memory
    Public listOfVeh As New List(Of Vehicle)
    Public listOfTrl As New List(Of Vehicle)
    Public IsVehicleLoaded As Boolean = False
    Public IsVehicleLoading As Boolean = False

    'Decor
    Public modDecor As String = "inm_persistence"
    Public nitroModDecor As String = "inm_nitro_active"
    Public flatbedModDecor As String = "inm_flatbed_installed"
    Public lastFbVehDecor As String = "inm_flatbed_last"

    Public carKeyModel As Model = "lr_prop_carkey_fob"

    Public Function GetPlayerCharacter() As Integer
        Select Case Game.Player.Character.Model
            Case PedHash.Michael
                Return 0
            Case PedHash.Franklin
                Return 1
            Case PedHash.Trevor
                Return 2
            Case Else
                Return 3
        End Select
    End Function

    Public Function Cheating(Cheat As String) As Boolean
        Return Native.Function.Call(Of Boolean)(Hash._0x557E43C447E700A8, Game.GenerateHash(Cheat))
    End Function

    <Extension>
    Public Function Make(vehicle As Vehicle) As String
        Return Game.GetGXTEntry(vehicle.Model.Hash.GetVehicleMakeName)
    End Function

    <Extension>
    Public Function WheelsVariation(vehicle As Vehicle) As Boolean
        Return Native.Function.Call(Of Boolean)(Native.Hash.GET_VEHICLE_MOD_VARIATION, vehicle, VehicleMod.FrontWheels)
    End Function

    <Extension>
    Public Function XenonHeadlightsColor(ByVal veh As Vehicle) As Integer
        Return Native.Function.Call(Of Integer)(&H3DFF319A831E0CDB, veh.Handle)
    End Function

    <Extension()>
    Public Sub XenonHeadlightsColor(ByVal veh As Vehicle, colorID As Integer)
        Native.Function.Call(&HE41033B25D003A07UL, veh.Handle, colorID)
    End Sub

    <Extension()>
    Public Function Livery2(veh As Vehicle) As Integer
        Return Native.Function.Call(Of Integer)(DirectCast(&H60190048C0764A26UL, Hash), veh.Handle)
    End Function

    <Extension()>
    Public Sub Livery2(veh As Vehicle, liv As Integer)
        Native.Function.Call(DirectCast(&HA6D3A8750DC73270UL, Hash), veh.Handle, liv)
    End Sub

    <Extension>
    Public Function GetSprite(vehicle As Vehicle) As BlipSprite
        Select Case True
            Case vehicle.Model.IsBicycle
                Return BlipSprite.RaceBike
            Case vehicle.Model.IsBike
                Return BlipSprite.PersonalVehicleBike
            Case vehicle.Model.IsBoat
                Return BlipSprite.Boat
            Case vehicle.Model.IsCargobob
                Return BlipSprite.Cargobob
            Case vehicle.Model.IsHelicopter
                Return BlipSprite.Helicopter
            Case vehicle.Model.IsPlane
                Return BlipSprite.Plane
            Case vehicle.Model.IsQuadbike
                Return BlipSprite.QuadBike
        End Select
        Return BlipSprite.PersonalVehicleCar
    End Function

    <Extension>
    Public Function GetOwnerColor(owner As Integer) As BlipColor
        Select Case owner
            Case 0
                Return BlipColor.Michael
            Case 1
                Return BlipColor.Franklin
            Case 2
                Return BlipColor.Trevor
            Case 3
                Return BlipColor.NetPlayer1
        End Select
        Return BlipColor.NetPlayer1
    End Function

    <Extension>
    Public Function GetOwnerName(owner As Integer) As String
        Select Case owner
            Case 0
                Return Game.GetGXTEntry("ACCNA_MIKE")
            Case 1
                Return Game.GetGXTEntry("ACCNA_FRANKLIN")
            Case 2
                Return Game.GetGXTEntry("ACCNA_TREVOR")
            Case 3
                Return Game.Player.Name
        End Select
        Return Game.Player.Name
    End Function

    Public Sub DisplayHelpTextThisFrame(helpText As String, Optional Shape As Integer = -1)
        Native.Function.Call(Native.Hash._SET_TEXT_COMPONENT_FORMAT, "CELL_EMAIL_BCON")
        Const maxStringLength As Integer = 99

        Dim i As Integer = 0
        While i < helpText.Length
            Native.Function.Call(Native.Hash._0x6C188BE134E074AA, helpText.Substring(i, System.Math.Min(maxStringLength, helpText.Length - i)))
            i += maxStringLength
        End While
        Native.Function.Call(Native.Hash._DISPLAY_HELP_TEXT_FROM_STRING_LABEL, 0, 0, 1, Shape)
    End Sub

    <Extension>
    Public Function ToColor(vs As VsColor) As Color
        Return Color.FromArgb(vs.Red, vs.Green, vs.Blue)
    End Function

    <Extension>
    Public Function ToVsColor(col As Color) As VsColor
        Return New VsColor(col.R, col.G, col.B)
    End Function

    <Extension>
    Public Sub LockVehicle(vehicle As Vehicle, ped As Ped)
        ped.SetPedCurrentWeaponVisible
        Dim carkey As Prop = World.CreateProp(carKeyModel, ped.Position + ped.ForwardVector, Vector3.Zero, True, False)
        carkey.AttachTo(ped, ped.GetBoneIndex(Bone.PH_R_Hand), Vector3.Zero, Vector3.Zero)
        ped.Task.PlayAnimation("anim@mp_player_intmenu@key_fob@", "fob_click_fp", 10.0F, 1500, 49)
        vehicle.IndicatorsOnOff
        Script.Wait(500)
        Using Stream As New WaveStream(File.OpenRead($"{soundPath}lock.wav"))
            Stream.Volume = alarmVolume
            Using player As New SoundPlayer(Stream)
                player.Play()
            End Using
        End Using
        vehicle.LockStatus = VehicleLockStatus.LockedForPlayer
        vehicle.HasAlarm = True
        Script.Wait(1000)
        carkey.Detach()
        carkey.Delete()
    End Sub

    <Extension>
    Public Sub UnlockVehicle(vehicle As Vehicle, ped As Ped)
        ped.SetPedCurrentWeaponVisible
        Dim carkey As Prop = World.CreateProp(carKeyModel, ped.Position + ped.ForwardVector, Vector3.Zero, True, False)
        carkey.AttachTo(ped, ped.GetBoneIndex(Bone.PH_R_Hand), Vector3.Zero, Vector3.Zero)
        ped.Task.PlayAnimation("anim@mp_player_intmenu@key_fob@", "fob_click_fp", 10.0F, 1500, 49)
        vehicle.IndicatorsOnOff
        Script.Wait(500)
        Using Stream As New WaveStream(File.OpenRead($"{soundPath}unlock.wav"))
            Stream.Volume = alarmVolume
            Using player As New SoundPlayer(Stream)
                player.Play()
            End Using
        End Using
        vehicle.LockStatus = VehicleLockStatus.Unlocked
        Script.Wait(1000)
        carkey.Detach()
        carkey.Delete()
    End Sub

    Public Function GetNearestChopper() As Vehicle
        Try
            Dim loe = listOfVeh.ToArray.Where(Function(x) System.Math.Abs(x.Position.DistanceTo(Game.Player.Character.Position)) >= 50.0F).Where(Function(x) x.Model.IsHelicopter).Where(Function(x) x.FreezePosition)
            If loe.Count = 0 Then
                Return New Vehicle(0)
            Else
                Return loe.First
            End If
        Catch ex As Exception
            Logger.Log($"{ex.Message}{ex.HResult}{ex.StackTrace}")
        End Try
        Return New Vehicle(0)
    End Function

    Public Function GetNearestCar() As Vehicle
        Try
            Dim loe = listOfVeh.ToArray.OrderBy(Function(x) System.Math.Abs(x.Position.DistanceTo(Game.Player.Character.Position)))
            If loe.Count = 0 Then
                Return New Vehicle(0)
            Else
                Return loe.First
            End If
        Catch ex As Exception
            Logger.Log($"{ex.Message}{ex.HResult}{ex.StackTrace}")
        End Try
        Return New Vehicle(0)
    End Function

    Public Sub SoundPlayer(waveFile As String)
        Using stream As New WaveStream(IO.File.OpenRead(waveFile))
            stream.Volume = alarmVolume
            Using player As New SoundPlayer(stream)
                player.Play()
            End Using
        End Using
    End Sub

    <Extension>
    Public Function GetInteriorID(pos As Vector3) As Integer
        Return Native.Function.Call(Of Integer)(Native.Hash.GET_INTERIOR_AT_COORDS, pos.X, pos.Y, pos.Z)
    End Function

    Public Sub LoadSettings()
        CreateConfig()
        dispVehName = config.GetValue(Of Boolean)("GENERAL", "DisplayVehicleName", False)
        showBlips = config.GetValue(Of Boolean)("GENERAL", "ShowBlips", True)
        alarmVolume = config.GetValue(Of Integer)("GENERAL", "AlarmVol", 100)
        saveDamage = config.GetValue(Of Boolean)("GENERAL", "SaveDamage", True)
        saveKey = config.GetValue(Of GTA.Control)("CONTROL", "SaveKey", GTA.Control.Context)
        'todayPassword = config.GetValue(Of String)("PASSWORD", "TodaysPassword", "password")
    End Sub

    Private Sub CreateConfig()
        If Not File.Exists("scripts\Persistence\Persistence.ini") Then
            config.SetValue(Of Boolean)("GENERAL", "DisplayVehicleName", False)
            config.SetValue(Of Boolean)("GENERAL", "ShowBlips", True)
            config.SetValue(Of Integer)("GENERAL", "AlarmVol", 100)
            config.SetValue(Of Boolean)("GENERAL", "SaveDamage", True)
            config.SetValue(Of GTA.Control)("CONTROL", "SaveKey", GTA.Control.Context)
            'config.SetValue(Of String)("PASSWORD", "TodaysPassword", "password")
            config.Save()
        End If
    End Sub

    <Extension>
    Public Function GetButtonIcon(control As GTA.Control) As String
        Return String.Format("~{0}~", [Enum].GetName(GetType(ControlButtonIcon), control))
    End Function

    Enum ControlButtonIcon
        INPUT_NEXT_CAMERA
        INPUT_LOOK_LR
        INPUT_LOOK_UD
        INPUT_LOOK_UP_ONLY
        INPUT_LOOK_DOWN_ONLY
        INPUT_LOOK_LEFT_ONLY
        INPUT_LOOK_RIGHT_ONLY
        INPUT_CINEMATIC_SLOWMO
        INPUT_SCRIPTED_FLY_UD
        INPUT_SCRIPTED_FLY_LR
        INPUT_SCRIPTED_FLY_ZUP
        INPUT_SCRIPTED_FLY_ZDOWN
        INPUT_WEAPON_WHEEL_UD
        INPUT_WEAPON_WHEEL_LR
        INPUT_WEAPON_WHEEL_NEXT
        INPUT_WEAPON_WHEEL_PREV
        INPUT_SELECT_NEXT_WEAPON
        INPUT_SELECT_PREV_WEAPON
        INPUT_SKIP_CUTSCENE
        INPUT_CHARACTER_WHEEL
        INPUT_MULTIPLAYER_INFO
        INPUT_SPRINT
        INPUT_JUMP
        INPUT_ENTER
        INPUT_ATTACK
        INPUT_AIM
        INPUT_LOOK_BEHIND
        INPUT_PHONE
        INPUT_SPECIAL_ABILITY
        INPUT_SPECIAL_ABILITY_SECONDARY
        INPUT_MOVE_LR
        INPUT_MOVE_UD
        INPUT_MOVE_UP_ONLY
        INPUT_MOVE_DOWN_ONLY
        INPUT_MOVE_LEFT_ONLY
        INPUT_MOVE_RIGHT_ONLY
        INPUT_DUCK
        INPUT_SELECT_WEAPON
        INPUT_PICKUP
        INPUT_SNIPER_ZOOM
        INPUT_SNIPER_ZOOM_IN_ONLY
        INPUT_SNIPER_ZOOM_OUT_ONLY
        INPUT_SNIPER_ZOOM_IN_SECONDARY
        INPUT_SNIPER_ZOOM_OUT_SECONDARY
        INPUT_COVER
        INPUT_RELOAD
        INPUT_TALK
        INPUT_DETONATE
        INPUT_HUD_SPECIAL
        INPUT_ARREST
        INPUT_ACCURATE_AIM
        INPUT_CONTEXT
        INPUT_CONTEXT_SECONDARY
        INPUT_WEAPON_SPECIAL
        INPUT_WEAPON_SPECIAL_TWO
        INPUT_DIVE
        INPUT_DROP_WEAPON
        INPUT_DROP_AMMO
        INPUT_THROW_GRENADE
        INPUT_VEH_MOVE_LR
        INPUT_VEH_MOVE_UD
        INPUT_VEH_MOVE_UP_ONLY
        INPUT_VEH_MOVE_DOWN_ONLY
        INPUT_VEH_MOVE_LEFT_ONLY
        INPUT_VEH_MOVE_RIGHT_ONLY
        INPUT_VEH_SPECIAL
        INPUT_VEH_GUN_LR
        INPUT_VEH_GUN_UD
        INPUT_VEH_AIM
        INPUT_VEH_ATTACK
        INPUT_VEH_ATTACK2
        INPUT_VEH_ACCELERATE
        INPUT_VEH_BRAKE
        INPUT_VEH_DUCK
        INPUT_VEH_HEADLIGHT
        INPUT_VEH_EXIT
        INPUT_VEH_HANDBRAKE
        INPUT_VEH_HOTWIRE_LEFT
        INPUT_VEH_HOTWIRE_RIGHT
        INPUT_VEH_LOOK_BEHIND
        INPUT_VEH_CIN_CAM
        INPUT_VEH_NEXT_RADIO
        INPUT_VEH_PREV_RADIO
        INPUT_VEH_NEXT_RADIO_TRACK
        INPUT_VEH_PREV_RADIO_TRACK
        INPUT_VEH_RADIO_WHEEL
        INPUT_VEH_HORN
        INPUT_VEH_FLY_THROTTLE_UP
        INPUT_VEH_FLY_THROTTLE_DOWN
        INPUT_VEH_FLY_YAW_LEFT
        INPUT_VEH_FLY_YAW_RIGHT
        INPUT_VEH_PASSENGER_AIM
        INPUT_VEH_PASSENGER_ATTACK
        INPUT_VEH_SPECIAL_ABILITY_FRANKLIN
        INPUT_VEH_STUNT_UD
        INPUT_VEH_CINEMATIC_UD
        INPUT_VEH_CINEMATIC_UP_ONLY
        INPUT_VEH_CINEMATIC_DOWN_ONLY
        INPUT_VEH_CINEMATIC_LR
        INPUT_VEH_SELECT_NEXT_WEAPON
        INPUT_VEH_SELECT_PREV_WEAPON
        INPUT_VEH_ROOF
        INPUT_VEH_JUMP
        INPUT_VEH_GRAPPLING_HOOK
        INPUT_VEH_SHUFFLE
        INPUT_VEH_DROP_PROJECTILE
        INPUT_VEH_MOUSE_CONTROL_OVERRIDE
        INPUT_VEH_FLY_ROLL_LR
        INPUT_VEH_FLY_ROLL_LEFT_ONLY
        INPUT_VEH_FLY_ROLL_RIGHT_ONLY
        INPUT_VEH_FLY_PITCH_UD
        INPUT_VEH_FLY_PITCH_UP_ONLY
        INPUT_VEH_FLY_PITCH_DOWN_ONLY
        INPUT_VEH_FLY_UNDERCARRIAGE
        INPUT_VEH_FLY_ATTACK
        INPUT_VEH_FLY_SELECT_NEXT_WEAPON
        INPUT_VEH_FLY_SELECT_PREV_WEAPON
        INPUT_VEH_FLY_SELECT_TARGET_LEFT
        INPUT_VEH_FLY_SELECT_TARGET_RIGHT
        INPUT_VEH_FLY_VERTICAL_FLIGHT_MODE
        INPUT_VEH_FLY_DUCK
        INPUT_VEH_FLY_ATTACK_CAMERA
        INPUT_VEH_FLY_MOUSE_CONTROL_OVERRIDE
        INPUT_VEH_SUB_TURN_LR
        INPUT_VEH_SUB_TURN_LEFT_ONLY
        INPUT_VEH_SUB_TURN_RIGHT_ONLY
        INPUT_VEH_SUB_PITCH_UD
        INPUT_VEH_SUB_PITCH_UP_ONLY
        INPUT_VEH_SUB_PITCH_DOWN_ONLY
        INPUT_VEH_SUB_THROTTLE_UP
        INPUT_VEH_SUB_THROTTLE_DOWN
        INPUT_VEH_SUB_ASCEND
        INPUT_VEH_SUB_DESCEND
        INPUT_VEH_SUB_TURN_HARD_LEFT
        INPUT_VEH_SUB_TURN_HARD_RIGHT
        INPUT_VEH_SUB_MOUSE_CONTROL_OVERRIDE
        INPUT_VEH_PUSHBIKE_PEDAL
        INPUT_VEH_PUSHBIKE_SPRINT
        INPUT_VEH_PUSHBIKE_FRONT_BRAKE
        INPUT_VEH_PUSHBIKE_REAR_BRAKE
        INPUT_MELEE_ATTACK_LIGHT
        INPUT_MELEE_ATTACK_HEAVY
        INPUT_MELEE_ATTACK_ALTERNATE
        INPUT_MELEE_BLOCK
        INPUT_PARACHUTE_DEPLOY
        INPUT_PARACHUTE_DETACH
        INPUT_PARACHUTE_TURN_LR
        INPUT_PARACHUTE_TURN_LEFT_ONLY
        INPUT_PARACHUTE_TURN_RIGHT_ONLY
        INPUT_PARACHUTE_PITCH_UD
        INPUT_PARACHUTE_PITCH_UP_ONLY
        INPUT_PARACHUTE_PITCH_DOWN_ONLY
        INPUT_PARACHUTE_BRAKE_LEFT
        INPUT_PARACHUTE_BRAKE_RIGHT
        INPUT_PARACHUTE_SMOKE
        INPUT_PARACHUTE_PRECISION_LANDING
        INPUT_MAP
        INPUT_SELECT_WEAPON_UNARMED
        INPUT_SELECT_WEAPON_MELEE
        INPUT_SELECT_WEAPON_HANDGUN
        INPUT_SELECT_WEAPON_SHOTGUN
        INPUT_SELECT_WEAPON_SMG
        INPUT_SELECT_WEAPON_AUTO_RIFLE
        INPUT_SELECT_WEAPON_SNIPER
        INPUT_SELECT_WEAPON_HEAVY
        INPUT_SELECT_WEAPON_SPECIAL
        INPUT_SELECT_CHARACTER_MICHAEL
        INPUT_SELECT_CHARACTER_FRANKLIN
        INPUT_SELECT_CHARACTER_TREVOR
        INPUT_SELECT_CHARACTER_MULTIPLAYER
        INPUT_SAVE_REPLAY_CLIP
        INPUT_SPECIAL_ABILITY_PC
        INPUT_CELLPHONE_UP
        INPUT_CELLPHONE_DOWN
        INPUT_CELLPHONE_LEFT
        INPUT_CELLPHONE_RIGHT
        INPUT_CELLPHONE_SELECT
        INPUT_CELLPHONE_CANCEL
        INPUT_CELLPHONE_OPTION
        INPUT_CELLPHONE_EXTRA_OPTION
        INPUT_CELLPHONE_SCROLL_FORWARD
        INPUT_CELLPHONE_SCROLL_BACKWARD
        INPUT_CELLPHONE_CAMERA_FOCUS_LOCK
        INPUT_CELLPHONE_CAMERA_GRID
        INPUT_CELLPHONE_CAMERA_SELFIE
        INPUT_CELLPHONE_CAMERA_DOF
        INPUT_CELLPHONE_CAMERA_EXPRESSION
        INPUT_FRONTEND_DOWN
        INPUT_FRONTEND_UP
        INPUT_FRONTEND_LEFT
        INPUT_FRONTEND_RIGHT
        INPUT_FRONTEND_RDOWN
        INPUT_FRONTEND_RUP
        INPUT_FRONTEND_RLEFT
        INPUT_FRONTEND_RRIGHT
        INPUT_FRONTEND_AXIS_X
        INPUT_FRONTEND_AXIS_Y
        INPUT_FRONTEND_RIGHT_AXIS_X
        INPUT_FRONTEND_RIGHT_AXIS_Y
        INPUT_FRONTEND_PAUSE
        INPUT_FRONTEND_PAUSE_ALTERNATE
        INPUT_FRONTEND_ACCEPT
        INPUT_FRONTEND_CANCEL
        INPUT_FRONTEND_X
        INPUT_FRONTEND_Y
        INPUT_FRONTEND_LB
        INPUT_FRONTEND_RB
        INPUT_FRONTEND_LT
        INPUT_FRONTEND_RT
        INPUT_FRONTEND_LS
        INPUT_FRONTEND_RS
        INPUT_FRONTEND_LEADERBOARD
        INPUT_FRONTEND_SOCIAL_CLUB
        INPUT_FRONTEND_SOCIAL_CLUB_SECONDARY
        INPUT_FRONTEND_DELETE
        INPUT_FRONTEND_ENDSCREEN_ACCEPT
        INPUT_FRONTEND_ENDSCREEN_EXPAND
        INPUT_FRONTEND_SELECT
        INPUT_SCRIPT_LEFT_AXIS_X
        INPUT_SCRIPT_LEFT_AXIS_Y
        INPUT_SCRIPT_RIGHT_AXIS_X
        INPUT_SCRIPT_RIGHT_AXIS_Y
        INPUT_SCRIPT_RUP
        INPUT_SCRIPT_RDOWN
        INPUT_SCRIPT_RLEFT
        INPUT_SCRIPT_RRIGHT
        INPUT_SCRIPT_LB
        INPUT_SCRIPT_RB
        INPUT_SCRIPT_LT
        INPUT_SCRIPT_RT
        INPUT_SCRIPT_LS
        INPUT_SCRIPT_RS
        INPUT_SCRIPT_PAD_UP
        INPUT_SCRIPT_PAD_DOWN
        INPUT_SCRIPT_PAD_LEFT
        INPUT_SCRIPT_PAD_RIGHT
        INPUT_SCRIPT_SELECT
        INPUT_CURSOR_ACCEPT
        INPUT_CURSOR_CANCEL
        INPUT_CURSOR_X
        INPUT_CURSOR_Y
        INPUT_CURSOR_SCROLL_UP
        INPUT_CURSOR_SCROLL_DOWN
        INPUT_ENTER_CHEAT_CODE
        INPUT_INTERACTION_MENU
        INPUT_MP_TEXT_CHAT_ALL
        INPUT_MP_TEXT_CHAT_TEAM
        INPUT_MP_TEXT_CHAT_FRIENDS
        INPUT_MP_TEXT_CHAT_CREW
        INPUT_PUSH_TO_TALK
        INPUT_CREATOR_LS
        INPUT_CREATOR_RS
        INPUT_CREATOR_LT
        INPUT_CREATOR_RT
        INPUT_CREATOR_MENU_TOGGLE
        INPUT_CREATOR_ACCEPT
        INPUT_CREATOR_DELETE
        INPUT_ATTACK2
        INPUT_RAPPEL_JUMP
        INPUT_RAPPEL_LONG_JUMP
        INPUT_RAPPEL_SMASH_WINDOW
        INPUT_PREV_WEAPON
        INPUT_NEXT_WEAPON
        INPUT_MELEE_ATTACK1
        INPUT_MELEE_ATTACK2
        INPUT_WHISTLE
        INPUT_MOVE_LEFT
        INPUT_MOVE_RIGHT
        INPUT_MOVE_UP
        INPUT_MOVE_DOWN
        INPUT_LOOK_LEFT
        INPUT_LOOK_RIGHT
        INPUT_LOOK_UP
        INPUT_LOOK_DOWN
        INPUT_SNIPER_ZOOM_IN
        INPUT_SNIPER_ZOOM_OUT
        INPUT_SNIPER_ZOOM_IN_ALTERNATE
        INPUT_SNIPER_ZOOM_OUT_ALTERNATE
        INPUT_VEH_MOVE_LEFT
        INPUT_VEH_MOVE_RIGHT
        INPUT_VEH_MOVE_UP
        INPUT_VEH_MOVE_DOWN
        INPUT_VEH_GUN_LEFT
        INPUT_VEH_GUN_RIGHT
        INPUT_VEH_GUN_UP
        INPUT_VEH_GUN_DOWN
        INPUT_VEH_LOOK_LEFT
        INPUT_VEH_LOOK_RIGHT
        INPUT_REPLAY_START_STOP_RECORDING
        INPUT_REPLAY_START_STOP_RECORDING_SECONDARY
        INPUT_SCALED_LOOK_LR
        INPUT_SCALED_LOOK_UD
        INPUT_SCALED_LOOK_UP_ONLY
        INPUT_SCALED_LOOK_DOWN_ONLY
        INPUT_SCALED_LOOK_LEFT_ONLY
        INPUT_SCALED_LOOK_RIGHT_ONLY
        INPUT_REPLAY_MARKER_DELETE
        INPUT_REPLAY_CLIP_DELETE
        INPUT_REPLAY_PAUSE
        INPUT_REPLAY_REWIND
        INPUT_REPLAY_FFWD
        INPUT_REPLAY_NEWMARKER
        INPUT_REPLAY_RECORD
        INPUT_REPLAY_SCREENSHOT
        INPUT_REPLAY_HIDEHUD
        INPUT_REPLAY_STARTPOINT
        INPUT_REPLAY_ENDPOINT
        INPUT_REPLAY_ADVANCE
        INPUT_REPLAY_BACK
        INPUT_REPLAY_TOOLS
        INPUT_REPLAY_RESTART
        INPUT_REPLAY_SHOWHOTKEY
        INPUT_REPLAY_CYCLEMARKERLEFT
        INPUT_REPLAY_CYCLEMARKERRIGHT
        INPUT_REPLAY_FOVINCREASE
        INPUT_REPLAY_FOVDECREASE
        INPUT_REPLAY_CAMERAUP
        INPUT_REPLAY_CAMERADOWN
        INPUT_REPLAY_SAVE
        INPUT_REPLAY_TOGGLETIME
        INPUT_REPLAY_TOGGLETIPS
        INPUT_REPLAY_PREVIEW
        INPUT_REPLAY_TOGGLE_TIMELINE
        INPUT_REPLAY_TIMELINE_PICKUP_CLIP
        INPUT_REPLAY_TIMELINE_DUPLICATE_CLIP
        INPUT_REPLAY_TIMELINE_PLACE_CLIP
        INPUT_REPLAY_CTRL
        INPUT_REPLAY_TIMELINE_SAVE
        INPUT_REPLAY_PREVIEW_AUDIO
        INPUT_VEH_DRIVE_LOOK
        INPUT_VEH_DRIVE_LOOK2
        INPUT_VEH_FLY_ATTACK2
        INPUT_RADIO_WHEEL_UD
        INPUT_RADIO_WHEEL_LR
        INPUT_VEH_SLOWMO_UD
        INPUT_VEH_SLOWMO_UP_ONLY
        INPUT_VEH_SLOWMO_DOWN_ONLY
        INPUT_MAP_POI
        INPUT_REPLAY_SNAPMATIC_PHOTO
        INPUT_VEH_CAR_JUMP
        INPUT_VEH_ROCKET_BOOST
        INPUT_VEH_PARACHUTE
        INPUT_VEH_BIKE_WINGS
    End Enum

    <Extension>
    Public Function FullName(vehicle As Vehicle) As String
        Dim make As String = vehicle.Make
        Dim name As String = vehicle.FriendlyName
        Dim full As String = $"{make} {name}"
        If make = "NULL" Then full = name
        Return full
    End Function

    <Extension>
    Public Function FullName(vehicle As Vehicles) As String
        Dim make As String = vehicle.Make
        Dim name As String = vehicle.Name
        Dim full As String = $"{make} {name}"
        If make = "NULL" Then full = name
        Return full
    End Function

    Public Sub LoadVehicles(files As String())
        Dim procFile As String = Nothing
        Try
            For Each file As String In files
                IsVehicleLoading = True
                procFile = file
                Dim pv As PVehicle = New PVehicle(file).Instance
                Dim v As Vehicles = pv.PlayerVehicles
                Dim t As Vehicles
                Dim veh As Vehicle = World.CreateVehicle(v.Hash, v.Position)
                Dim trl As Vehicle = Nothing
                With veh
                    .Rotation = v.Rotation
                    .WheelType = v.WheelType
                    .InstallModKit()
                    .SetMod(VehicleMod.Aerials, v.Aerials, True)
                    .SetMod(VehicleMod.Suspension, v.Suspension, True)
                    .SetMod(VehicleMod.Brakes, v.Brakes, True)
                    .SetMod(VehicleMod.Engine, v.Engine, True)
                    .SetMod(VehicleMod.Transmission, v.Transmission, True)
                    .SetMod(VehicleMod.FrontBumper, v.FrontBumper, True)
                    .SetMod(VehicleMod.RearBumper, v.RearBumper, True)
                    .SetMod(VehicleMod.SideSkirt, v.SideSkirt, True)
                    .SetMod(VehicleMod.Trim, v.Trim, True)
                    .SetMod(VehicleMod.EngineBlock, v.EngineBlock, True)
                    .SetMod(VehicleMod.AirFilter, v.AirFilter, True)
                    .SetMod(VehicleMod.Struts, v.Struts, True)
                    .SetMod(VehicleMod.ColumnShifterLevers, v.ColumnShifterLevers, True)
                    .SetMod(VehicleMod.Dashboard, v.Dashboard, True)
                    .SetMod(VehicleMod.DialDesign, v.DialDesign, True)
                    .SetMod(VehicleMod.Ornaments, v.Ornaments, True)
                    .SetMod(VehicleMod.Seats, v.Seats, True)
                    .SetMod(VehicleMod.SteeringWheels, v.SteeringWheel, True)
                    .SetMod(VehicleMod.TrimDesign, v.TrimDesign, True)
                    .SetMod(VehicleMod.PlateHolder, v.PlateHolder, True)
                    .SetMod(VehicleMod.VanityPlates, v.VanityPlates, True)
                    .SetMod(VehicleMod.FrontWheels, v.Frontwheels, v.WheelsVariation)
                    .SetMod(VehicleMod.BackWheels, v.BackWheels, v.WheelsVariation)
                    .SetMod(VehicleMod.ArchCover, v.ArchCover, True)
                    .SetMod(VehicleMod.Exhaust, v.Exhaust, True)
                    .SetMod(VehicleMod.Fender, v.Fender, True)
                    .SetMod(VehicleMod.RightFender, v.RightFender, True)
                    .SetMod(VehicleMod.DoorSpeakers, v.DoorSpeakers, True)
                    .SetMod(VehicleMod.Frame, v.Frame, True)
                    .SetMod(VehicleMod.Grille, v.Grille, True)
                    .SetMod(VehicleMod.Hood, v.Hood, True)
                    .SetMod(VehicleMod.Horns, v.Horn, True)
                    .SetMod(VehicleMod.Hydraulics, v.Hydraulics, True)
                    .SetMod(VehicleMod.Livery, v.Livery, True)
                    .SetMod(VehicleMod.Plaques, v.Plaques, True)
                    .SetMod(VehicleMod.Roof, v.Roof, True)
                    .SetMod(VehicleMod.Speakers, v.Speakers, True)
                    .SetMod(VehicleMod.Spoilers, v.Spoiler, True)
                    .SetMod(VehicleMod.Tank, v.Tank, True)
                    .SetMod(VehicleMod.Trunk, v.Trunk, True)
                    .SetMod(VehicleMod.Windows, v.Windows, True)
                    .ToggleMod(VehicleToggleMod.XenonHeadlights, v.Headlights)
                    .ToggleMod(VehicleToggleMod.Turbo, v.Turbo)
                    .ToggleMod(VehicleToggleMod.TireSmoke, v.Tiresmoke)
                    .TrimColor = v.TrimColor
                    .NumberPlateType = v.NumberPlate
                    .NumberPlate = v.PlateNumber
                    .SetNeonLightsOn(VehicleNeonLight.Front, v.FrontNeon)
                    .SetNeonLightsOn(VehicleNeonLight.Back, v.BackNeon)
                    .SetNeonLightsOn(VehicleNeonLight.Left, v.LeftNeon)
                    .SetNeonLightsOn(VehicleNeonLight.Right, v.RightNeon)
                    .WindowTint = v.Tint
                    .PrimaryColor = v.PrimaryColor
                    .SecondaryColor = v.SecondaryColor
                    .PearlescentColor = v.PearlescentColor
                    .RimColor = v.RimColor
                    .DashboardColor = v.LightsColor
                    .NeonLightsColor = v.NeonLightsColor.ToColor
                    .TireSmokeColor = v.TireSmokeColor.ToColor
                    .Livery2(v.Livery2)
                    .Livery = v.Livery1
                    .XenonHeadlightsColor(v.HeadlightsColor)
                    .CanTiresBurst = v.BulletProofTires
                    If v.HasCustomPrimaryColor Then .CustomPrimaryColor = v.CustomPrimaryColor.ToColor
                    If v.HasCustomSecondaryColor Then .CustomSecondaryColor = v.CustomSecondaryColor.ToColor
                    .ToggleExtra(1, v.Extra1)
                    .ToggleExtra(2, v.Extra2)
                    .ToggleExtra(3, v.Extra3)
                    .ToggleExtra(4, v.Extra4)
                    .ToggleExtra(5, v.Extra5)
                    .ToggleExtra(6, v.Extra6)
                    .ToggleExtra(7, v.Extra7)
                    .ToggleExtra(8, v.Extra8)
                    .ToggleExtra(9, v.Extra9)
                    .DirtLevel = v.DirtLevel
                    If saveDamage Then
                        .BodyHealth = v.BodyHealth
                        .EngineHealth = v.EngineHealth
                        .PetrolTankHealth = v.PetrolTankHealth
                    End If
                    .FuelLevel = v.FuelLevel
                    .RoofState = v.RoofState
                    .SteeringScale = v.SteeringScale
                    If IsNitroModInstalled() Then .SetBool(nitroModDecor, v.HasNitro)
                    .IsPersistent = True
                    If showBlips Then
                        .AddBlip()
                        .CurrentBlip.Sprite = veh.GetSprite
                        Select Case v.Owner
                            Case 0
                                .CurrentBlip.Color = BlipColor.Michael
                            Case 1
                                .CurrentBlip.Color = BlipColor.Franklin
                            Case 2
                                .CurrentBlip.Color = BlipColor.Trevor
                            Case 3
                                .CurrentBlip.Color = BlipColor.NetPlayer1
                        End Select
                        .CurrentBlip.IsShortRange = True
                        .CurrentBlip.Name = If(dispVehName, v.FullName, Game.GetGXTEntry("PVEHICLE"))
                    End If
                    .SetInt(modDecor, v.Owner)
                    .LockStatus = VehicleLockStatus.LockedForPlayer
                    .HasAlarm = True
                    If .Model.IsHelicopter Then .FreezePosition = True
                End With
                listOfVeh.Add(veh)
                If v.HasTrailer Then
                    t = pv.TrailerVehicles
                    trl = World.CreateVehicle(t.Hash, t.Position)
                    With trl
                        .Rotation = t.Rotation
                        .WheelType = t.WheelType
                        .InstallModKit()
                        .SetMod(VehicleMod.Aerials, t.Aerials, True)
                        .SetMod(VehicleMod.Suspension, t.Suspension, True)
                        .SetMod(VehicleMod.Brakes, t.Brakes, True)
                        .SetMod(VehicleMod.Engine, t.Engine, True)
                        .SetMod(VehicleMod.Transmission, t.Transmission, True)
                        .SetMod(VehicleMod.FrontBumper, t.FrontBumper, True)
                        .SetMod(VehicleMod.RearBumper, t.RearBumper, True)
                        .SetMod(VehicleMod.SideSkirt, t.SideSkirt, True)
                        .SetMod(VehicleMod.Trim, t.Trim, True)
                        .SetMod(VehicleMod.EngineBlock, t.EngineBlock, True)
                        .SetMod(VehicleMod.AirFilter, t.AirFilter, True)
                        .SetMod(VehicleMod.Struts, t.Struts, True)
                        .SetMod(VehicleMod.ColumnShifterLevers, t.ColumnShifterLevers, True)
                        .SetMod(VehicleMod.Dashboard, t.Dashboard, True)
                        .SetMod(VehicleMod.DialDesign, t.DialDesign, True)
                        .SetMod(VehicleMod.Ornaments, t.Ornaments, True)
                        .SetMod(VehicleMod.Seats, t.Seats, True)
                        .SetMod(VehicleMod.SteeringWheels, t.SteeringWheel, True)
                        .SetMod(VehicleMod.TrimDesign, t.TrimDesign, True)
                        .SetMod(VehicleMod.PlateHolder, t.PlateHolder, True)
                        .SetMod(VehicleMod.VanityPlates, t.VanityPlates, True)
                        .SetMod(VehicleMod.FrontWheels, t.Frontwheels, t.WheelsVariation)
                        .SetMod(VehicleMod.BackWheels, t.BackWheels, t.WheelsVariation)
                        .SetMod(VehicleMod.ArchCover, t.ArchCover, True)
                        .SetMod(VehicleMod.Exhaust, t.Exhaust, True)
                        .SetMod(VehicleMod.Fender, t.Fender, True)
                        .SetMod(VehicleMod.RightFender, t.RightFender, True)
                        .SetMod(VehicleMod.DoorSpeakers, t.DoorSpeakers, True)
                        .SetMod(VehicleMod.Frame, t.Frame, True)
                        .SetMod(VehicleMod.Grille, t.Grille, True)
                        .SetMod(VehicleMod.Hood, t.Hood, True)
                        .SetMod(VehicleMod.Horns, t.Horn, True)
                        .SetMod(VehicleMod.Hydraulics, t.Hydraulics, True)
                        .SetMod(VehicleMod.Livery, t.Livery, True)
                        .SetMod(VehicleMod.Plaques, t.Plaques, True)
                        .SetMod(VehicleMod.Roof, t.Roof, True)
                        .SetMod(VehicleMod.Speakers, t.Speakers, True)
                        .SetMod(VehicleMod.Spoilers, t.Spoiler, True)
                        .SetMod(VehicleMod.Tank, t.Tank, True)
                        .SetMod(VehicleMod.Trunk, t.Trunk, True)
                        .SetMod(VehicleMod.Windows, t.Windows, True)
                        .ToggleMod(VehicleToggleMod.XenonHeadlights, t.Headlights)
                        .ToggleMod(VehicleToggleMod.Turbo, t.Turbo)
                        .ToggleMod(VehicleToggleMod.TireSmoke, t.Tiresmoke)
                        .TrimColor = t.TrimColor
                        .NumberPlateType = t.NumberPlate
                        .NumberPlate = t.PlateNumber
                        .SetNeonLightsOn(VehicleNeonLight.Front, t.FrontNeon)
                        .SetNeonLightsOn(VehicleNeonLight.Back, t.BackNeon)
                        .SetNeonLightsOn(VehicleNeonLight.Left, t.LeftNeon)
                        .SetNeonLightsOn(VehicleNeonLight.Right, t.RightNeon)
                        .WindowTint = t.Tint
                        .PrimaryColor = t.PrimaryColor
                        .SecondaryColor = t.SecondaryColor
                        .PearlescentColor = t.PearlescentColor
                        .RimColor = t.RimColor
                        .DashboardColor = t.LightsColor
                        .NeonLightsColor = t.NeonLightsColor.ToColor
                        .TireSmokeColor = t.TireSmokeColor.ToColor
                        .Livery2(t.Livery2)
                        .XenonHeadlightsColor(t.HeadlightsColor)
                        .CanTiresBurst = t.BulletProofTires
                        If .IsPrimaryColorCustom Then .CustomPrimaryColor = t.CustomPrimaryColor.ToColor
                        If .IsSecondaryColorCustom Then .CustomSecondaryColor = t.CustomSecondaryColor.ToColor
                        .ToggleExtra(1, t.Extra1)
                        .ToggleExtra(2, t.Extra2)
                        .ToggleExtra(3, t.Extra3)
                        .ToggleExtra(4, t.Extra4)
                        .ToggleExtra(5, t.Extra5)
                        .ToggleExtra(6, t.Extra6)
                        .ToggleExtra(7, t.Extra7)
                        .ToggleExtra(8, t.Extra8)
                        .ToggleExtra(9, t.Extra9)
                        .DirtLevel = t.DirtLevel
                        If saveDamage Then
                            .BodyHealth = t.BodyHealth
                            .EngineHealth = t.EngineHealth
                            .PetrolTankHealth = t.PetrolTankHealth
                        End If
                        .FuelLevel = t.FuelLevel
                        .RoofState = t.RoofState
                        .SteeringScale = t.SteeringScale
                        If IsNitroModInstalled() Then .SetBool(nitroModDecor, t.HasNitro)
                        .IsPersistent = True
                        .SetInt(modDecor, t.Owner)
                    End With
                    listOfTrl.Add(trl)
                    veh.AttachToTrailer(trl)
                End If
                If v.HasTowing Then
                    t = pv.TrailerVehicles
                    trl = World.CreateVehicle(t.Hash, t.Position)
                    With trl
                        .Rotation = t.Rotation
                        .WheelType = t.WheelType
                        .InstallModKit()
                        .SetMod(VehicleMod.Aerials, t.Aerials, True)
                        .SetMod(VehicleMod.Suspension, t.Suspension, True)
                        .SetMod(VehicleMod.Brakes, t.Brakes, True)
                        .SetMod(VehicleMod.Engine, t.Engine, True)
                        .SetMod(VehicleMod.Transmission, t.Transmission, True)
                        .SetMod(VehicleMod.FrontBumper, t.FrontBumper, True)
                        .SetMod(VehicleMod.RearBumper, t.RearBumper, True)
                        .SetMod(VehicleMod.SideSkirt, t.SideSkirt, True)
                        .SetMod(VehicleMod.Trim, t.Trim, True)
                        .SetMod(VehicleMod.EngineBlock, t.EngineBlock, True)
                        .SetMod(VehicleMod.AirFilter, t.AirFilter, True)
                        .SetMod(VehicleMod.Struts, t.Struts, True)
                        .SetMod(VehicleMod.ColumnShifterLevers, t.ColumnShifterLevers, True)
                        .SetMod(VehicleMod.Dashboard, t.Dashboard, True)
                        .SetMod(VehicleMod.DialDesign, t.DialDesign, True)
                        .SetMod(VehicleMod.Ornaments, t.Ornaments, True)
                        .SetMod(VehicleMod.Seats, t.Seats, True)
                        .SetMod(VehicleMod.SteeringWheels, t.SteeringWheel, True)
                        .SetMod(VehicleMod.TrimDesign, t.TrimDesign, True)
                        .SetMod(VehicleMod.PlateHolder, t.PlateHolder, True)
                        .SetMod(VehicleMod.VanityPlates, t.VanityPlates, True)
                        .SetMod(VehicleMod.FrontWheels, t.Frontwheels, t.WheelsVariation)
                        .SetMod(VehicleMod.BackWheels, t.BackWheels, t.WheelsVariation)
                        .SetMod(VehicleMod.ArchCover, t.ArchCover, True)
                        .SetMod(VehicleMod.Exhaust, t.Exhaust, True)
                        .SetMod(VehicleMod.Fender, t.Fender, True)
                        .SetMod(VehicleMod.RightFender, t.RightFender, True)
                        .SetMod(VehicleMod.DoorSpeakers, t.DoorSpeakers, True)
                        .SetMod(VehicleMod.Frame, t.Frame, True)
                        .SetMod(VehicleMod.Grille, t.Grille, True)
                        .SetMod(VehicleMod.Hood, t.Hood, True)
                        .SetMod(VehicleMod.Horns, t.Horn, True)
                        .SetMod(VehicleMod.Hydraulics, t.Hydraulics, True)
                        .SetMod(VehicleMod.Livery, t.Livery, True)
                        .SetMod(VehicleMod.Plaques, t.Plaques, True)
                        .SetMod(VehicleMod.Roof, t.Roof, True)
                        .SetMod(VehicleMod.Speakers, t.Speakers, True)
                        .SetMod(VehicleMod.Spoilers, t.Spoiler, True)
                        .SetMod(VehicleMod.Tank, t.Tank, True)
                        .SetMod(VehicleMod.Trunk, t.Trunk, True)
                        .SetMod(VehicleMod.Windows, t.Windows, True)
                        .ToggleMod(VehicleToggleMod.XenonHeadlights, t.Headlights)
                        .ToggleMod(VehicleToggleMod.Turbo, t.Turbo)
                        .ToggleMod(VehicleToggleMod.TireSmoke, t.Tiresmoke)
                        .TrimColor = t.TrimColor
                        .NumberPlateType = t.NumberPlate
                        .NumberPlate = t.PlateNumber
                        .SetNeonLightsOn(VehicleNeonLight.Front, t.FrontNeon)
                        .SetNeonLightsOn(VehicleNeonLight.Back, t.BackNeon)
                        .SetNeonLightsOn(VehicleNeonLight.Left, t.LeftNeon)
                        .SetNeonLightsOn(VehicleNeonLight.Right, t.RightNeon)
                        .WindowTint = t.Tint
                        .PrimaryColor = t.PrimaryColor
                        .SecondaryColor = t.SecondaryColor
                        .PearlescentColor = t.PearlescentColor
                        .RimColor = t.RimColor
                        .DashboardColor = t.LightsColor
                        .NeonLightsColor = t.NeonLightsColor.ToColor
                        .TireSmokeColor = t.TireSmokeColor.ToColor
                        .Livery2(t.Livery2)
                        .XenonHeadlightsColor(t.HeadlightsColor)
                        .CanTiresBurst = t.BulletProofTires
                        If .IsPrimaryColorCustom Then .CustomPrimaryColor = t.CustomPrimaryColor.ToColor
                        If .IsSecondaryColorCustom Then .CustomSecondaryColor = t.CustomSecondaryColor.ToColor
                        .ToggleExtra(1, t.Extra1)
                        .ToggleExtra(2, t.Extra2)
                        .ToggleExtra(3, t.Extra3)
                        .ToggleExtra(4, t.Extra4)
                        .ToggleExtra(5, t.Extra5)
                        .ToggleExtra(6, t.Extra6)
                        .ToggleExtra(7, t.Extra7)
                        .ToggleExtra(8, t.Extra8)
                        .ToggleExtra(9, t.Extra9)
                        .DirtLevel = t.DirtLevel
                        If saveDamage Then
                            .BodyHealth = t.BodyHealth
                            .EngineHealth = t.EngineHealth
                            .PetrolTankHealth = t.PetrolTankHealth
                        End If
                        .FuelLevel = t.FuelLevel
                        .RoofState = t.RoofState
                        .SteeringScale = t.SteeringScale
                        If IsNitroModInstalled() Then .SetBool(nitroModDecor, t.HasNitro)
                        .IsPersistent = True
                        .SetInt(modDecor, t.Owner)
                    End With
                    listOfTrl.Add(trl)
                    veh.TowVehicle(trl, False)
                End If
            Next
        Catch ex As Exception
            Logger.Log($"{ex.Message} {procFile}{ex.StackTrace}")
        Finally
            If listOfVeh.Count = files.Count Then
                IsVehicleLoaded = True
                IsVehicleLoading = False
                Try
                    UI.Notify(String.Format(GetLangEntry("loaded"), files.Count))
                Catch
                End Try
            Else
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
        End Try
    End Sub

    Public Sub PatchRedBlips()
        Try
            For Each vehicle As Vehicle In listOfVeh
                If vehicle.CurrentBlip.Sprite <> vehicle.GetSprite Then
                    vehicle.CurrentBlip.Sprite = vehicle.GetSprite
                    vehicle.CurrentBlip.Color = GetOwnerColor(vehicle.GetInt(modDecor))
                    vehicle.CurrentBlip.IsShortRange = True
                    vehicle.CurrentBlip.Name = If(dispVehName, vehicle.FullName, Game.GetGXTEntry("PVEHICLE"))
                End If
            Next
        Catch ex As Exception
            Logger.Log($"{ex.Message}{ex.HResult}{ex.StackTrace}")
        End Try
    End Sub

    Public Function GetLangEntry(lang As String) As String
        Dim result As String = ReadCfgValue(lang, $".\scripts\Persistence\Language\{Game.Language.ToString}.cfg")
        Dim real_result As String
        If result = Nothing Then
            real_result = "NULL"
        Else
            real_result = result
        End If
        Return real_result
    End Function

    <Extension>
    Public Function HasTrailer(vh As Vehicle) As Boolean
        Return Native.Function.Call(Of Boolean)(Native.Hash.IS_VEHICLE_ATTACHED_TO_TRAILER, vh)
    End Function

    <Extension>
    Public Function HasTowing(vh As Vehicle) As Boolean
        Return If(vh.TowedVehicle.Model.Hash <> 0, True, False)
    End Function

    <Extension>
    Public Function Trailer(vh As Vehicle) As Vehicle
        Dim out As New OutputArgument()
        Native.Function.Call(Native.Hash.GET_VEHICLE_TRAILER_VEHICLE, vh, out)
        Return out.GetResult(Of Vehicle)()
    End Function

    <Extension>
    Public Sub AttachToTrailer(veh As Vehicle, trl As Vehicle)
        Native.Function.Call(Native.Hash.ATTACH_VEHICLE_TO_TRAILER, veh, trl, 1.0F)
    End Sub

    Public Sub DisableControls()
        Game.DisableControlThisFrame(0, Control.Talk)
    End Sub

    <Extension>
    Public Sub IndicatorsOnOff(veh As Vehicle)
        If veh.HasBone("indicator_lf") Then World.DrawLightWithRange(veh.GetBoneCoord("indicator_lf"), Color.Orange, 1.0F, 2.0F)
        If veh.HasBone("indicator_rr") Then World.DrawLightWithRange(veh.GetBoneCoord("indicator_rr"), Color.Orange, 1.0F, 2.0F)
        If veh.HasBone("indicator_lr") Then World.DrawLightWithRange(veh.GetBoneCoord("indicator_lr"), Color.Orange, 1.0F, 2.0F)
        If veh.HasBone("indicator_rf") Then World.DrawLightWithRange(veh.GetBoneCoord("indicator_rf"), Color.Orange, 1.0F, 2.0F)
    End Sub

    Public Function IsNitroModInstalled() As Boolean
        Return Decor.Registered(nitroModDecor, Decor.eDecorType.Bool)
    End Function

    <Extension()>
    Public Sub SetPedCurrentWeaponVisible(ped As Ped)
        Native.Function.Call(Native.Hash.SET_PED_CURRENT_WEAPON_VISIBLE, ped.Handle, False, True, True, True)
    End Sub

    Public Function IsFlatbedModInstalled() As Boolean
        Return Decor.Registered(flatbedModDecor, Decor.eDecorType.Bool)
    End Function

    <Extension>
    Public Function LastFlatbed(ped As Ped) As Vehicle
        Return New Vehicle(ped.GetInt(lastFbVehDecor))
    End Function

    Public Sub RegisterDecor(d As String, t As Decor.eDecorType)
        If Not Decor.Registered(d, t) Then
            Decor.Unlock()
            Decor.Register(d, t)
            Decor.Lock()
        End If
    End Sub

End Module
