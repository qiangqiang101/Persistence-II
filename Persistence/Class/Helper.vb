Imports System.Drawing
Imports System.IO
Imports System.Media
Imports System.Runtime.CompilerServices
Imports GTA
Imports GTA.Math
Imports GTA.Native
Imports Newtonsoft.Json

Module Helper

    'Config
    Public config As ScriptSettings = ScriptSettings.Load("scripts\Persistence\Persistence.ini")
    Public langDict As New List(Of Language)
    Public dispVehName As Boolean = False
    Public saveKey As Control = Control.Context
    Public showBlips As Boolean = True
    Public saveDamage As Boolean = True

    'Path
    Public xmlPath As String = ".\scripts\Persistence\Vehicles\"

    'Memory
    Public listOfVeh As New List(Of Vehicle)
    Public listOfTrl As New List(Of Vehicle)
    Public IsVehicleLoaded As Boolean = False
    Public IsVehicleLoading As Boolean = False

    'Decor
    Public modDecor As String = "inm_persistence"
    Public lastVehDecor As String = "inm_persistence_last_vehicle"
    Public modDecor2 As String = "inm_persistence_2"
    Public nitroModDecor As String = "inm_nitro_active"
    Public flatbedModDecor As String = "inm_flatbed_installed"
    Public carCarrierModDecor As String = "inm_cctm_installed"
    Public lastFbVehDecor As String = "inm_flatbed_last"
    Public lastCCTruckDecor As String = "inm_cctm_truck"

    Public carKeyModel As Model = "lr_prop_carkey_fob"
    Public isPause As Boolean = False 'Renamed from pause: in v3 it clashed with Script.Pause()

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

    <Extension>
    Public Function Make(vehicle As Vehicle) As String
        Return Game.GetLocalizedString(Native.Function.Call(Of String)(Hash.GET_MAKE_NAME_FROM_VEHICLE_MODEL, vehicle.Model.Hash))
    End Function

    <Extension>
    Public Function WheelsVariation(vehicle As Vehicle) As Boolean
        Return Native.Function.Call(Of Boolean)(Native.Hash.GET_VEHICLE_MOD_VARIATION, vehicle, CInt(VehicleModType.FrontWheel))
    End Function

    <Extension>
    Public Function XenonHeadlightsColor(ByVal veh As Vehicle) As Integer
        Return Native.Function.Call(Of Integer)(Hash.GET_VEHICLE_XENON_LIGHT_COLOR_INDEX, veh.Handle)
    End Function

    <Extension()>
    Public Sub XenonHeadlightsColor(ByVal veh As Vehicle, colorID As Integer)
        Native.Function.Call(Hash.SET_VEHICLE_XENON_LIGHT_COLOR_INDEX, veh.Handle, colorID)
    End Sub

    <Extension()>
    Public Function Livery2(veh As Vehicle) As Integer
        Return Native.Function.Call(Of Integer)(Hash.GET_VEHICLE_LIVERY2, veh.Handle)
    End Function

    <Extension()>
    Public Sub Livery2(veh As Vehicle, liv As Integer)
        Native.Function.Call(Hash.SET_VEHICLE_LIVERY2, veh.Handle, liv)
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
            Case vehicle.Model.IsQuadBike
                Return BlipSprite.QuadBike
            Case Else
                Return BlipSprite.PersonalVehicleCar
        End Select
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
                Return Game.GetLocalizedString("ACCNA_MIKE")
            Case 1
                Return Game.GetLocalizedString("ACCNA_FRANKLIN")
            Case 2
                Return Game.GetLocalizedString("ACCNA_TREVOR")
            Case 3
                Return Game.Player.Name
        End Select
        Return Game.Player.Name
    End Function

    Public Sub DisplayHelpTextThisFrame(helpText As String, Optional Shape As Integer = -1)
        Native.Function.Call(Native.Hash.BEGIN_TEXT_COMMAND_DISPLAY_HELP, "CELL_EMAIL_BCON")
        Const maxStringLength As Integer = 99

        Dim i As Integer = 0
        While i < helpText.Length
            Native.Function.Call(Native.Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, helpText.Substring(i, System.Math.Min(maxStringLength, helpText.Length - i)))
            i += maxStringLength
        End While
        Native.Function.Call(Native.Hash.END_TEXT_COMMAND_DISPLAY_HELP, 0, False, True, Shape)
    End Sub

    <Extension>
    Public Function ToColor(vs As VsColor) As Color
        Return Color.FromArgb(vs.Red, vs.Green, vs.Blue)
    End Function

    <Extension>
    Public Function ToVsColor(col As Color) As VsColor
        Return New VsColor(col.R, col.G, col.B)
    End Function

    Public Function StartAudioScene(scene As String)
        Native.Function.Call(Hash.START_AUDIO_SCENE, scene)
    End Function

    <Extension>
    Public Sub LockVehicle(vehicle As Vehicle, ped As Ped)
        ped.SetPedCurrentWeaponVisible
        Dim carkey As Prop = World.CreateProp(carKeyModel, ped.Position + ped.ForwardVector, Vector3.Zero, True, False)
        carkey.AttachTo(ped.Bones(Bone.PHRightHand), Vector3.Zero, Vector3.Zero)
        ped.Task.PlayAnimation("anim@mp_player_intmenu@key_fob@", "fob_click_fp", 10.0F, 1500, CType(49, AnimationFlags))
        Indicator.veh = vehicle
        Script.Wait(500)
        Audio.PlaySoundFromEntityAndForget(ped, "Remote_Control_Fob", "PI_Menu_Sounds")
        Audio.PlaySoundFromEntityAndForget(vehicle, "Remote_Control_Close", "PI_Menu_Sounds")
        vehicle.LockStatus = VehicleLockStatus.PlayerCannotEnter
        vehicle.IsAlarmSet = True
        Script.Wait(500)
        carkey.Detach()
        carkey.Delete()
        Indicator.veh = Nothing
    End Sub

    <Extension>
    Public Sub UnlockVehicle(vehicle As Vehicle, ped As Ped)
        ped.SetPedCurrentWeaponVisible
        Dim carkey As Prop = World.CreateProp(carKeyModel, ped.Position + ped.ForwardVector, Vector3.Zero, True, False)
        carkey.AttachTo(ped.Bones(Bone.PHRightHand), Vector3.Zero, Vector3.Zero)
        ped.Task.PlayAnimation("anim@mp_player_intmenu@key_fob@", "fob_click_fp", 10.0F, 1500, CType(49, AnimationFlags))
        Indicator.veh = vehicle
        Script.Wait(500)
        Audio.PlaySoundFromEntityAndForget(ped, "Remote_Control_Fob", "PI_Menu_Sounds")
        Audio.PlaySoundFromEntityAndForget(vehicle, "Remote_Control_Open", "PI_Menu_Sounds")
        vehicle.LockStatus = VehicleLockStatus.Unlocked
        Script.Wait(500)
        carkey.Detach()
        carkey.Delete()
        Indicator.veh = Nothing
    End Sub

    Public Function GetNearestChopper() As Vehicle
        Try
            Dim loe = listOfVeh.ToArray.Where(Function(x) System.Math.Abs(x.Position.DistanceTo(Game.Player.Character.Position)) >= 50.0F).Where(Function(x) x.Model.IsHelicopter).Where(Function(x) x.IsPositionFrozen)
            If loe.Count = 0 Then
                Return Nothing
            Else
                Return loe.First
            End If
        Catch ex As Exception
            Logger.Log(ex)
        End Try
        Return Nothing
    End Function

    Public Function GetNearestCar() As Vehicle
        Try
            Dim loe = listOfVeh.ToArray.OrderBy(Function(x) System.Math.Abs(x.Position.DistanceTo(Game.Player.Character.Position)))
            If loe.Count = 0 Then
                Return Nothing
            Else
                Return loe.First
            End If
        Catch ex As Exception
            Logger.Log(ex)
        End Try
        Return Nothing
    End Function

    <Extension>
    Public Function GetInteriorID(pos As Vector3) As Integer
        Return Native.Function.Call(Of Integer)(Native.Hash.GET_INTERIOR_AT_COORDS, pos.X, pos.Y, pos.Z)
    End Function

    Public Sub LoadSettings()
        CreateConfig()
        dispVehName = config.GetValue(Of Boolean)("GENERAL", "DisplayVehicleName", False)
        showBlips = config.GetValue(Of Boolean)("GENERAL", "ShowBlips", True)
        saveDamage = config.GetValue(Of Boolean)("GENERAL", "SaveDamage", True)
        saveKey = config.GetValue(Of Control)("CONTROL", "SaveKey", Control.Context)
    End Sub

    Private Sub CreateConfig()
        If Not File.Exists("scripts\Persistence\Persistence.ini") Then
            config.SetValue(Of Boolean)("GENERAL", "DisplayVehicleName", False)
            config.SetValue(Of Boolean)("GENERAL", "ShowBlips", True)
            config.SetValue(Of Boolean)("GENERAL", "SaveDamage", True)
            config.SetValue(Of Control)("CONTROL", "SaveKey", Control.Context)
            config.Save()
        End If
    End Sub

    Public Sub LoadLanguages()
        Try
            langDict = JsonConvert.DeserializeObject(Of List(Of Language))(File.ReadAllText($".\scripts\Persistence\language.json"))
        Catch ex As Exception
            Logger.Log(ex)
        End Try
        If langDict Is Nothing OrElse langDict.Count = 0 Then langDict = Language.Defaults()
    End Sub

    <Extension>
    Public Function GetButtonIcon(control As Control) As String
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
        Dim make As String = Vehicle.GetModelMakeName(vehicle.Model)
        Dim name As String = vehicle.LocalizedName
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
                    .Mods.WheelType = v.WheelType
                    .Mods.InstallModKit()
                    .Mods(VehicleModType.Aerials).Index = v.Aerials
                    .Mods(VehicleModType.Suspension).Index = v.Suspension
                    .Mods(VehicleModType.Brakes).Index = v.Brakes
                    .Mods(VehicleModType.Engine).Index = v.Engine
                    .Mods(VehicleModType.Transmission).Index = v.Transmission
                    .Mods(VehicleModType.FrontBumper).Index = v.FrontBumper
                    .Mods(VehicleModType.RearBumper).Index = v.RearBumper
                    .Mods(VehicleModType.SideSkirt).Index = v.SideSkirt
                    .Mods(VehicleModType.Trim).Index = v.Trim
                    .Mods(VehicleModType.EngineBlock).Index = v.EngineBlock
                    .Mods(VehicleModType.AirFilter).Index = v.AirFilter
                    .Mods(VehicleModType.Struts).Index = v.Struts
                    .Mods(VehicleModType.ColumnShifterLevers).Index = v.ColumnShifterLevers
                    .Mods(VehicleModType.Dashboard).Index = v.Dashboard
                    .Mods(VehicleModType.DialDesign).Index = v.DialDesign
                    .Mods(VehicleModType.Ornaments).Index = v.Ornaments
                    .Mods(VehicleModType.Seats).Index = v.Seats
                    .Mods(VehicleModType.SteeringWheels).Index = v.SteeringWheel
                    .Mods(VehicleModType.TrimDesign).Index = v.TrimDesign
                    .Mods(VehicleModType.PlateHolder).Index = v.PlateHolder
                    .Mods(VehicleModType.VanityPlates).Index = v.VanityPlates
                    .Mods(VehicleModType.FrontWheel).Index = v.Frontwheels
                    .Mods(VehicleModType.FrontWheel).Variation = v.WheelsVariation
                    .Mods(VehicleModType.RearWheel).Index = v.BackWheels
                    .Mods(VehicleModType.RearWheel).Variation = v.WheelsVariation
                    .Mods(VehicleModType.ArchCover).Index = v.ArchCover
                    .Mods(VehicleModType.Exhaust).Index = v.Exhaust
                    .Mods(VehicleModType.Fender).Index = v.Fender
                    .Mods(VehicleModType.RightFender).Index = v.RightFender
                    .Mods(VehicleModType.DoorSpeakers).Index = v.DoorSpeakers
                    .Mods(VehicleModType.Frame).Index = v.Frame
                    .Mods(VehicleModType.Grille).Index = v.Grille
                    .Mods(VehicleModType.Hood).Index = v.Hood
                    .Mods(VehicleModType.Horns).Index = v.Horn
                    .Mods(VehicleModType.Hydraulics).Index = v.Hydraulics
                    .Mods(VehicleModType.Livery).Index = v.Livery
                    .Mods(VehicleModType.Plaques).Index = v.Plaques
                    .Mods(VehicleModType.Roof).Index = v.Roof
                    .Mods(VehicleModType.Speakers).Index = v.Speakers
                    .Mods(VehicleModType.Spoilers).Index = v.Spoiler
                    .Mods(VehicleModType.Tank).Index = v.Tank
                    .Mods(VehicleModType.Trunk).Index = v.Trunk
                    .Mods(VehicleModType.Windows).Index = v.Windows
                    .Mods(VehicleToggleModType.XenonHeadlights).IsInstalled = v.Headlights
                    .Mods(VehicleToggleModType.Turbo).IsInstalled = v.Turbo
                    .Mods(VehicleToggleModType.TireSmoke).IsInstalled = v.Tiresmoke
                    .Mods.TrimColor = v.TrimColor
                    .Mods.LicensePlateStyle = v.NumberPlate
                    .Mods.LicensePlate = v.PlateNumber
                    .Mods.SetNeonLightsOn(VehicleNeonLight.Front, v.FrontNeon)
                    .Mods.SetNeonLightsOn(VehicleNeonLight.Back, v.BackNeon)
                    .Mods.SetNeonLightsOn(VehicleNeonLight.Left, v.LeftNeon)
                    .Mods.SetNeonLightsOn(VehicleNeonLight.Right, v.RightNeon)
                    .Mods.WindowTint = v.Tint
                    .Mods.PrimaryColor = v.PrimaryColor
                    .Mods.SecondaryColor = v.SecondaryColor
                    .Mods.PearlescentColor = v.PearlescentColor
                    .Mods.RimColor = v.RimColor
                    .Mods.DashboardColor = v.LightsColor
                    .Mods.NeonLightsColor = v.NeonLightsColor.ToColor
                    .Mods.TireSmokeColor = v.TireSmokeColor.ToColor
                    .Livery2(v.Livery2)
                    .Mods.Livery = v.Livery1
                    .XenonHeadlightsColor(v.HeadlightsColor)
                    .CanTiresBurst = v.BulletProofTires
                    If v.HasCustomPrimaryColor Then .Mods.CustomPrimaryColor = v.CustomPrimaryColor.ToColor
                    If v.HasCustomSecondaryColor Then .Mods.CustomSecondaryColor = v.CustomSecondaryColor.ToColor
                    '.Extras(VehicleExtraIndex.Extra1).Enabled = v.Extra0
                    '.Extras(VehicleExtraIndex.Extra2).Enabled = v.Extra1
                    '.Extras(VehicleExtraIndex.Extra3).Enabled = v.Extra2
                    '.Extras(VehicleExtraIndex.Extra4).Enabled = v.Extra3
                    '.Extras(VehicleExtraIndex.Extra5).Enabled = v.Extra4
                    '.Extras(VehicleExtraIndex.Extra6).Enabled = v.Extra5
                    '.Extras(VehicleExtraIndex.Extra7).Enabled = v.Extra6
                    '.Extras(VehicleExtraIndex.Extra8).Enabled = v.Extra7
                    '.Extras(VehicleExtraIndex.Extra9).Enabled = v.Extra8
                    '.Extras(VehicleExtraIndex.Extra10).Enabled = v.Extra9
                    '.Extras(VehicleExtraIndex.Extra11).Enabled = v.Extra10
                    '.Extras(VehicleExtraIndex.Extra12).Enabled = v.Extra11
                    '.Extras(VehicleExtraIndex.Extra13).Enabled = v.Extra12
                    '.Extras(VehicleExtraIndex.Extra14).Enabled = v.Extra13
                    '.Extras(VehicleExtraIndex.Extra15).Enabled = v.Extra14
                    '.Extras(VehicleExtraIndex.Extra16).Enabled = v.Extra15
                    .DirtLevel = v.DirtLevel
                    If saveDamage Then
                        .BodyHealth = v.BodyHealth
                        .EngineHealth = v.EngineHealth
                        .PetrolTankHealth = v.PetrolTankHealth
                    End If
                    .FuelLevel = v.FuelLevel
                    .RoofState = v.RoofState
                    .SteeringScale = v.SteeringScale
                    If IsNitroModInstalled() Then DecoratorInterface.SetInt(veh, nitroModDecor, v.HasNitro)
                    .IsPersistent = True
                    If showBlips Then
                        .AddBlip()
                        If .AttachedBlip IsNot Nothing Then
                            .AttachedBlip.Sprite = veh.GetSprite
                            Select Case v.Owner
                                Case 0
                                    .AttachedBlip.Color = BlipColor.Michael
                                Case 1
                                    .AttachedBlip.Color = BlipColor.Franklin
                                Case 2
                                    .AttachedBlip.Color = BlipColor.Trevor
                                Case 3
                                    .AttachedBlip.Color = BlipColor.NetPlayer1
                            End Select
                            .AttachedBlip.IsShortRange = True
                            .AttachedBlip.Name = If(dispVehName, v.FullName, Game.GetLocalizedString("PVEHICLE"))
                        End If
                    End If
                    DecoratorInterface.SetInt(veh, modDecor, v.Owner)
                    DecoratorInterface.SetBool(veh, modDecor2, True)
                    .LockStatus = VehicleLockStatus.PlayerCannotEnter
                    .IsAlarmSet = True
                    If .Model.IsHelicopter Then .IsPositionFrozen = True
                End With
                listOfVeh.Add(veh)
                If v.HasTrailer Then
                    t = pv.TrailerVehicles
                    trl = World.CreateVehicle(t.Hash, t.Position)
                    With trl
                        .Rotation = t.Rotation
                        .Mods.WheelType = t.WheelType
                        .Mods.InstallModKit()
                        .Mods(VehicleModType.Aerials).Index = t.Aerials
                        .Mods(VehicleModType.Suspension).Index = t.Suspension
                        .Mods(VehicleModType.Brakes).Index = t.Brakes
                        .Mods(VehicleModType.Engine).Index = t.Engine
                        .Mods(VehicleModType.Transmission).Index = t.Transmission
                        .Mods(VehicleModType.FrontBumper).Index = t.FrontBumper
                        .Mods(VehicleModType.RearBumper).Index = t.RearBumper
                        .Mods(VehicleModType.SideSkirt).Index = t.SideSkirt
                        .Mods(VehicleModType.Trim).Index = t.Trim
                        .Mods(VehicleModType.EngineBlock).Index = t.EngineBlock
                        .Mods(VehicleModType.AirFilter).Index = t.AirFilter
                        .Mods(VehicleModType.Struts).Index = t.Struts
                        .Mods(VehicleModType.ColumnShifterLevers).Index = t.ColumnShifterLevers
                        .Mods(VehicleModType.Dashboard).Index = t.Dashboard
                        .Mods(VehicleModType.DialDesign).Index = t.DialDesign
                        .Mods(VehicleModType.Ornaments).Index = t.Ornaments
                        .Mods(VehicleModType.Seats).Index = t.Seats
                        .Mods(VehicleModType.SteeringWheels).Index = t.SteeringWheel
                        .Mods(VehicleModType.TrimDesign).Index = t.TrimDesign
                        .Mods(VehicleModType.PlateHolder).Index = t.PlateHolder
                        .Mods(VehicleModType.VanityPlates).Index = t.VanityPlates
                        .Mods(VehicleModType.FrontWheel).Index = t.Frontwheels
                        .Mods(VehicleModType.FrontWheel).Variation = t.WheelsVariation
                        .Mods(VehicleModType.RearWheel).Index = t.BackWheels
                        .Mods(VehicleModType.RearWheel).Variation = t.WheelsVariation
                        .Mods(VehicleModType.ArchCover).Index = t.ArchCover
                        .Mods(VehicleModType.Exhaust).Index = t.Exhaust
                        .Mods(VehicleModType.Fender).Index = t.Fender
                        .Mods(VehicleModType.RightFender).Index = t.RightFender
                        .Mods(VehicleModType.DoorSpeakers).Index = t.DoorSpeakers
                        .Mods(VehicleModType.Frame).Index = t.Frame
                        .Mods(VehicleModType.Grille).Index = t.Grille
                        .Mods(VehicleModType.Hood).Index = t.Hood
                        .Mods(VehicleModType.Horns).Index = t.Horn
                        .Mods(VehicleModType.Hydraulics).Index = t.Hydraulics
                        .Mods(VehicleModType.Livery).Index = t.Livery
                        .Mods(VehicleModType.Plaques).Index = t.Plaques
                        .Mods(VehicleModType.Roof).Index = t.Roof
                        .Mods(VehicleModType.Speakers).Index = t.Speakers
                        .Mods(VehicleModType.Spoilers).Index = t.Spoiler
                        .Mods(VehicleModType.Tank).Index = t.Tank
                        .Mods(VehicleModType.Trunk).Index = t.Trunk
                        .Mods(VehicleModType.Windows).Index = t.Windows
                        .Mods(VehicleToggleModType.XenonHeadlights).IsInstalled = t.Headlights
                        .Mods(VehicleToggleModType.Turbo).IsInstalled = t.Turbo
                        .Mods(VehicleToggleModType.TireSmoke).IsInstalled = t.Tiresmoke
                        .Mods.TrimColor = t.TrimColor
                        .Mods.LicensePlateStyle = t.NumberPlate
                        .Mods.LicensePlate = t.PlateNumber
                        .Mods.SetNeonLightsOn(VehicleNeonLight.Front, t.FrontNeon)
                        .Mods.SetNeonLightsOn(VehicleNeonLight.Back, t.BackNeon)
                        .Mods.SetNeonLightsOn(VehicleNeonLight.Left, t.LeftNeon)
                        .Mods.SetNeonLightsOn(VehicleNeonLight.Right, t.RightNeon)
                        .Mods.WindowTint = t.Tint
                        .Mods.PrimaryColor = t.PrimaryColor
                        .Mods.SecondaryColor = t.SecondaryColor
                        .Mods.PearlescentColor = t.PearlescentColor
                        .Mods.RimColor = t.RimColor
                        .Mods.DashboardColor = t.LightsColor
                        .Mods.NeonLightsColor = t.NeonLightsColor.ToColor
                        .Mods.TireSmokeColor = t.TireSmokeColor.ToColor
                        .Livery2(t.Livery2)
                        .XenonHeadlightsColor(t.HeadlightsColor)
                        .CanTiresBurst = t.BulletProofTires
                        If .Mods.IsPrimaryColorCustom Then .Mods.CustomPrimaryColor = t.CustomPrimaryColor.ToColor
                        If .Mods.IsSecondaryColorCustom Then .Mods.CustomSecondaryColor = t.CustomSecondaryColor.ToColor
                        '.Extras(VehicleExtraIndex.Extra1).Enabled = t.Extra0
                        '.Extras(VehicleExtraIndex.Extra2).Enabled = t.Extra1
                        '.Extras(VehicleExtraIndex.Extra3).Enabled = t.Extra2
                        '.Extras(VehicleExtraIndex.Extra4).Enabled = t.Extra3
                        '.Extras(VehicleExtraIndex.Extra5).Enabled = t.Extra4
                        '.Extras(VehicleExtraIndex.Extra6).Enabled = t.Extra5
                        '.Extras(VehicleExtraIndex.Extra7).Enabled = t.Extra6
                        '.Extras(VehicleExtraIndex.Extra8).Enabled = t.Extra7
                        '.Extras(VehicleExtraIndex.Extra9).Enabled = t.Extra8
                        '.Extras(VehicleExtraIndex.Extra10).Enabled = t.Extra9
                        '.Extras(VehicleExtraIndex.Extra11).Enabled = t.Extra10
                        '.Extras(VehicleExtraIndex.Extra12).Enabled = t.Extra11
                        '.Extras(VehicleExtraIndex.Extra13).Enabled = t.Extra12
                        '.Extras(VehicleExtraIndex.Extra14).Enabled = t.Extra13
                        '.Extras(VehicleExtraIndex.Extra15).Enabled = t.Extra14
                        '.Extras(VehicleExtraIndex.Extra16).Enabled = t.Extra15
                        .DirtLevel = t.DirtLevel
                        If saveDamage Then
                            .BodyHealth = t.BodyHealth
                            .EngineHealth = t.EngineHealth
                            .PetrolTankHealth = t.PetrolTankHealth
                        End If
                        .FuelLevel = t.FuelLevel
                        .RoofState = t.RoofState
                        .SteeringScale = t.SteeringScale
                        If IsNitroModInstalled() Then DecoratorInterface.SetInt(trl, nitroModDecor, t.HasNitro)
                        .IsPersistent = True
                        DecoratorInterface.SetInt(trl, modDecor, t.Owner)
                        DecoratorInterface.SetBool(trl, modDecor2, True)
                    End With
                    listOfTrl.Add(trl)
                    veh.AttachToTrailer(trl)
                    Script.Wait(500)
                End If
                If v.HasTowing Then
                    t = pv.TrailerVehicles
                    trl = World.CreateVehicle(t.Hash, t.Position)
                    With trl
                        .Rotation = t.Rotation
                        .Mods.WheelType = t.WheelType
                        .Mods.InstallModKit()
                        .Mods(VehicleModType.Aerials).Index = t.Aerials
                        .Mods(VehicleModType.Suspension).Index = t.Suspension
                        .Mods(VehicleModType.Brakes).Index = t.Brakes
                        .Mods(VehicleModType.Engine).Index = t.Engine
                        .Mods(VehicleModType.Transmission).Index = t.Transmission
                        .Mods(VehicleModType.FrontBumper).Index = t.FrontBumper
                        .Mods(VehicleModType.RearBumper).Index = t.RearBumper
                        .Mods(VehicleModType.SideSkirt).Index = t.SideSkirt
                        .Mods(VehicleModType.Trim).Index = t.Trim
                        .Mods(VehicleModType.EngineBlock).Index = t.EngineBlock
                        .Mods(VehicleModType.AirFilter).Index = t.AirFilter
                        .Mods(VehicleModType.Struts).Index = t.Struts
                        .Mods(VehicleModType.ColumnShifterLevers).Index = t.ColumnShifterLevers
                        .Mods(VehicleModType.Dashboard).Index = t.Dashboard
                        .Mods(VehicleModType.DialDesign).Index = t.DialDesign
                        .Mods(VehicleModType.Ornaments).Index = t.Ornaments
                        .Mods(VehicleModType.Seats).Index = t.Seats
                        .Mods(VehicleModType.SteeringWheels).Index = t.SteeringWheel
                        .Mods(VehicleModType.TrimDesign).Index = t.TrimDesign
                        .Mods(VehicleModType.PlateHolder).Index = t.PlateHolder
                        .Mods(VehicleModType.VanityPlates).Index = t.VanityPlates
                        .Mods(VehicleModType.FrontWheel).Index = t.Frontwheels
                        .Mods(VehicleModType.FrontWheel).Variation = t.WheelsVariation
                        .Mods(VehicleModType.RearWheel).Index = t.BackWheels
                        .Mods(VehicleModType.RearWheel).Variation = t.WheelsVariation
                        .Mods(VehicleModType.ArchCover).Index = t.ArchCover
                        .Mods(VehicleModType.Exhaust).Index = t.Exhaust
                        .Mods(VehicleModType.Fender).Index = t.Fender
                        .Mods(VehicleModType.RightFender).Index = t.RightFender
                        .Mods(VehicleModType.DoorSpeakers).Index = t.DoorSpeakers
                        .Mods(VehicleModType.Frame).Index = t.Frame
                        .Mods(VehicleModType.Grille).Index = t.Grille
                        .Mods(VehicleModType.Hood).Index = t.Hood
                        .Mods(VehicleModType.Horns).Index = t.Horn
                        .Mods(VehicleModType.Hydraulics).Index = t.Hydraulics
                        .Mods(VehicleModType.Livery).Index = t.Livery
                        .Mods(VehicleModType.Plaques).Index = t.Plaques
                        .Mods(VehicleModType.Roof).Index = t.Roof
                        .Mods(VehicleModType.Speakers).Index = t.Speakers
                        .Mods(VehicleModType.Spoilers).Index = t.Spoiler
                        .Mods(VehicleModType.Tank).Index = t.Tank
                        .Mods(VehicleModType.Trunk).Index = t.Trunk
                        .Mods(VehicleModType.Windows).Index = t.Windows
                        .Mods(VehicleToggleModType.XenonHeadlights).IsInstalled = t.Headlights
                        .Mods(VehicleToggleModType.Turbo).IsInstalled = t.Turbo
                        .Mods(VehicleToggleModType.TireSmoke).IsInstalled = t.Tiresmoke
                        .Mods.TrimColor = t.TrimColor
                        .Mods.LicensePlateStyle = t.NumberPlate
                        .Mods.LicensePlate = t.PlateNumber
                        .Mods.SetNeonLightsOn(VehicleNeonLight.Front, t.FrontNeon)
                        .Mods.SetNeonLightsOn(VehicleNeonLight.Back, t.BackNeon)
                        .Mods.SetNeonLightsOn(VehicleNeonLight.Left, t.LeftNeon)
                        .Mods.SetNeonLightsOn(VehicleNeonLight.Right, t.RightNeon)
                        .Mods.WindowTint = t.Tint
                        .Mods.PrimaryColor = t.PrimaryColor
                        .Mods.SecondaryColor = t.SecondaryColor
                        .Mods.PearlescentColor = t.PearlescentColor
                        .Mods.RimColor = t.RimColor
                        .Mods.DashboardColor = t.LightsColor
                        .Mods.NeonLightsColor = t.NeonLightsColor.ToColor
                        .Mods.TireSmokeColor = t.TireSmokeColor.ToColor
                        .Livery2(t.Livery2)
                        .XenonHeadlightsColor(t.HeadlightsColor)
                        .CanTiresBurst = t.BulletProofTires
                        If .Mods.IsPrimaryColorCustom Then .Mods.CustomPrimaryColor = t.CustomPrimaryColor.ToColor
                        If .Mods.IsSecondaryColorCustom Then .Mods.CustomSecondaryColor = t.CustomSecondaryColor.ToColor
                        '.Extras(VehicleExtraIndex.Extra1).Enabled = t.Extra0
                        '.Extras(VehicleExtraIndex.Extra2).Enabled = t.Extra1
                        '.Extras(VehicleExtraIndex.Extra3).Enabled = t.Extra2
                        '.Extras(VehicleExtraIndex.Extra4).Enabled = t.Extra3
                        '.Extras(VehicleExtraIndex.Extra5).Enabled = t.Extra4
                        '.Extras(VehicleExtraIndex.Extra6).Enabled = t.Extra5
                        '.Extras(VehicleExtraIndex.Extra7).Enabled = t.Extra6
                        '.Extras(VehicleExtraIndex.Extra8).Enabled = t.Extra7
                        '.Extras(VehicleExtraIndex.Extra9).Enabled = t.Extra8
                        '.Extras(VehicleExtraIndex.Extra10).Enabled = t.Extra9
                        '.Extras(VehicleExtraIndex.Extra11).Enabled = t.Extra10
                        '.Extras(VehicleExtraIndex.Extra12).Enabled = t.Extra11
                        '.Extras(VehicleExtraIndex.Extra13).Enabled = t.Extra12
                        '.Extras(VehicleExtraIndex.Extra14).Enabled = t.Extra13
                        '.Extras(VehicleExtraIndex.Extra15).Enabled = t.Extra14
                        '.Extras(VehicleExtraIndex.Extra16).Enabled = t.Extra15
                        .DirtLevel = t.DirtLevel
                        If saveDamage Then
                            .BodyHealth = t.BodyHealth
                            .EngineHealth = t.EngineHealth
                            .PetrolTankHealth = t.PetrolTankHealth
                        End If
                        .FuelLevel = t.FuelLevel
                        .RoofState = t.RoofState
                        .SteeringScale = t.SteeringScale
                        If IsNitroModInstalled() Then DecoratorInterface.SetInt(trl, nitroModDecor, t.HasNitro)
                        .IsPersistent = True
                        DecoratorInterface.SetInt(trl, modDecor, t.Owner)
                        DecoratorInterface.SetBool(trl, modDecor2, True)
                    End With
                    listOfTrl.Add(trl)
                    veh.TowVehicle(trl, Vector3.Zero)
                    Script.Wait(500)
                End If
            Next
        Catch ex As Exception
            Logger.Log(ex, procFile)
        Finally
            If listOfVeh.Count = files.Count Then
                IsVehicleLoaded = True
                IsVehicleLoading = False
                Try
                    GTA.UI.Notification.PostTicker(String.Format(GetLangEntry("loaded"), files.Count), False)
                Catch
                End Try
            Else
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
        End Try
    End Sub

    Public Sub PatchRedBlips()
        Try
            For Each vehicle As Vehicle In listOfVeh
                If vehicle.AttachedBlip IsNot Nothing Then
                    If vehicle.AttachedBlip.Sprite <> vehicle.GetSprite Then
                        vehicle.AttachedBlip.Sprite = vehicle.GetSprite
                        vehicle.AttachedBlip.Color = GetOwnerColor(DecoratorInterface.GetInt(vehicle, modDecor))
                        vehicle.AttachedBlip.IsShortRange = True
                        vehicle.AttachedBlip.Name = If(dispVehName, vehicle.FullName, Game.GetLocalizedString("PVEHICLE"))
                    End If
                End If
            Next
        Catch ex As Exception
            Logger.Log(ex)
        End Try
    End Sub

    Public Function GetLangEntry(key As String) As String
        Dim gameLang As String = Game.Language.ToString.ToLower()
        Dim lang = If(langDict?.FirstOrDefault(Function(x) x.name = gameLang), langDict?.FirstOrDefault(Function(x) x.name = "american"))
        Dim value As String = lang?.data?.FirstOrDefault(Function(x) x.name = key)?.value
        Return If(value, "NULL")
    End Function

    <Extension>
    Public Function HasTrailer(vh As Vehicle) As Boolean
        Return Native.Function.Call(Of Boolean)(Native.Hash.IS_VEHICLE_ATTACHED_TO_TRAILER, vh)
    End Function

    <Extension>
    Public Function HasTowing(vh As Vehicle) As Boolean
        Dim towed As Vehicle = vh.TowedVehicle
        Return towed IsNot Nothing AndAlso towed.Exists()
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
        Native.Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, CInt(Control.Talk), True)
    End Sub

    'The mod disables the key it uses, so read the disabled control state.
    Public Function IsSaveKeyJustReleased() As Boolean
        Return Native.Function.Call(Of Boolean)(Hash.IS_DISABLED_CONTROL_JUST_RELEASED, 0, CInt(saveKey))
    End Function

    Public Function IsNitroModInstalled() As Boolean
        Return DecoratorInterface.IsRegisteredAsType(nitroModDecor, DecoratorType.Int)
    End Function

    <Extension()>
    Public Sub SetPedCurrentWeaponVisible(ped As Ped)
        Native.Function.Call(Native.Hash.SET_PED_CURRENT_WEAPON_VISIBLE, ped.Handle, False, True, True, True)
    End Sub

    Public Function IsFlatbedModInstalled() As Boolean
        Return DecoratorInterface.IsRegisteredAsType(flatbedModDecor, DecoratorType.Bool)
    End Function

    Public Function IsCarCarrierModInstalled() As Boolean
        Return DecoratorInterface.IsRegisteredAsType(carCarrierModDecor, DecoratorType.Bool)
    End Function

    <Extension>
    Public Function LastFlatbed(ped As Ped) As Vehicle
        Return TryCast(Entity.FromHandle(DecoratorInterface.GetInt(ped, lastFbVehDecor)), Vehicle)
    End Function

    <Extension>
    Public Function LastTrailerTruck(ped As Ped) As Vehicle
        Return TryCast(Entity.FromHandle(DecoratorInterface.GetInt(ped, lastCCTruckDecor)), Vehicle)
    End Function

    Public Sub RegisterDecor(d As String, t As DecoratorType)
        If Not DecoratorInterface.IsRegisteredAsType(d, t) Then
            SetDecorLocked(False)
            DecoratorInterface.Register(d, t)
            SetDecorLocked(True)
        End If
    End Sub

    Public Sub SetDecorLocked(locked As Boolean)
        Try
            DecoratorInterface.IsLocked = locked
        Catch ex As Exception
            'Thrown when SHVDN could not find the decorator lock flag in this game build.
            Logger.Log(ex)
        End Try
    End Sub

    'DecoratorInterface.Remove calls DECOR_EXIST_ON instead of DECOR_REMOVE, so call the native directly.
    Public Function RemoveDecor(ent As Entity, d As String) As Boolean
        Return Native.Function.Call(Of Boolean)(Hash.DECOR_REMOVE, ent, d)
    End Function

    Private Function LastVehicleHandle() As Integer
        Dim lastVeh As Vehicle = Game.Player.LastVehicle
        Return If(lastVeh Is Nothing, 0, lastVeh.Handle)
    End Function

    Public Sub ReleasePersistLastVehicle()
        Dim lastHandle As Integer = LastVehicleHandle()
        Dim vehs = World.GetAllVehicles.Where(Function(x) x.IsVehiclePersist And x.IsPersistent And Not x.IsVehicleHasOwner And x.Handle <> lastHandle)
        If Not vehs.Count = 0 Then
            For Each veh In vehs
                DecoratorInterface.SetBool(veh, lastVehDecor, False)
                veh.IsPersistent = False
                veh.MarkAsNoLongerNeeded()
            Next
        End If
    End Sub

    <Extension>
    Public Function IsVehiclePersist(veh As Vehicle) As Boolean
        If DecoratorInterface.GetBool(veh, lastVehDecor) = Nothing Then Return False
        Return DecoratorInterface.GetBool(veh, lastVehDecor)
    End Function

    <Extension>
    Public Function IsVehicleHasOwner(veh As Vehicle) As Boolean
        If DecoratorInterface.GetBool(veh, modDecor2) = Nothing Then Return False
        Return DecoratorInterface.GetBool(veh, modDecor2)
    End Function

    <Extension>
    Public Sub SetVehicleIsPersist(veh As Vehicle)
        DecoratorInterface.SetBool(veh, lastVehDecor, True)
        veh.IsPersistent = True
    End Sub

    Public Function HowManyVehicleSpawnedByModOnWorldRightNow() As Integer
        Dim lastHandle As Integer = LastVehicleHandle()
        Return World.GetAllVehicles.Where(Function(x) x.IsVehiclePersist And x.IsPersistent And Not x.IsVehicleHasOwner And x.Handle <> lastHandle).Count
    End Function


End Module

Public Module Logger

    Public Sub Log(message As Object)
        IO.File.AppendAllText(".\Persistence.log", DateTime.Now & ":" & message & Environment.NewLine)
    End Sub

    Public Sub Log(ex As Exception, Optional extra As String = Nothing, Optional hasMore As Boolean = False)
        Dim n = Environment.NewLine
        If hasMore Then
            IO.File.AppendAllText(".\Persistence.log", $"+ {ex.Message}{ex.StackTrace}{n}{ex.Source}{If(Not String.IsNullOrEmpty(extra), n, "")}{extra}{n}{n}")
        Else
            IO.File.AppendAllText(".\Persistence.log", $"{Now}:{ex.Message}{ex.StackTrace}{n}{ex.Source}{If(Not String.IsNullOrEmpty(extra), n, "")}{extra}{n}{n}")
        End If

        If ex.InnerException IsNot Nothing Then Log(ex.InnerException, hasMore:=True)
    End Sub

    Public Sub Delete()
        If File.Exists(".\Persistence.log") Then File.Delete(".\Persistence.log")
    End Sub

End Module
