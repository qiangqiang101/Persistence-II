Imports System.Drawing
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports GTA
Imports GTA.Math
Imports Metadata
Imports Newtonsoft.Json

Public Class UserSave

    Public DisplayVehicleName As Boolean = False
    Public ShowBlips As Boolean = True
    Public AlarmVol As Integer = 100
    Public SaveDamage As Boolean = True
    Public SaveKey As Control = Control.Context
    Public Vehicles As List(Of PersonalVehicle) = New List(Of PersonalVehicle)

    Public Function Load(filename As String) As UserSave
        Return JsonConvert.DeserializeObject(Of UserSave)(IO.File.ReadAllText(filename))
    End Function

    Public Sub Save(filename As String)
        IO.File.WriteAllText(filename, JsonConvert.SerializeObject(Me, Formatting.Indented))
    End Sub

End Class

Public Class PersonalVehicle

    Public MakeName As String
    Public DisplayName As String
    Public Hash As Integer
    Public Position As Vector3
    Public Rotation As Vector3
    Public Owner As Integer
    Public Identification As Long

    Public Aerials As Integer
    Public Suspension As Integer
    Public Armor As Integer
    Public Brakes As Integer
    Public Engine As Integer
    Public Transmission As Integer
    Public FrontBumper As Integer
    Public RearBumper As Integer
    Public SideSkirt As Integer
    Public Trim As Integer
    Public EngineBlock As Integer
    Public AirFilter As Integer
    Public Struts As Integer
    Public ColumnShifterLevers As Integer
    Public Dashboard As Integer
    Public DialDesign As Integer
    Public Ornaments As Integer
    Public Seats As Integer
    Public SteeringWheels As Integer
    Public TrimDesign As Integer
    Public TrimColor As Integer
    Public PlateHolder As Integer
    Public VanityPlates As Integer
    Public LicensePlateStyle As Integer
    Public LicensePlate As String
    Public WheelType As Integer
    Public FrontWheel As Integer
    Public RearWheel As Integer
    Public FrontWheelVariation As Boolean
    Public RearWheelVariation As Boolean
    Public XenonHeadlights As Boolean
    Public FrontNeon As Boolean
    Public BackNeon As Boolean
    Public LeftNeon As Boolean
    Public RightNeon As Boolean
    Public ArchCover As Integer
    Public Exhaust As Integer
    Public Fender As Integer
    Public RightFender As Integer
    Public DoorSpeakers As Integer
    Public Frame As Integer
    Public Grille As Integer
    Public Hood As Integer
    Public Horns As Integer
    Public Hydraulics As Integer
    Public Livery As Integer
    Public Plaques As Integer
    Public Roof As Integer
    Public Speakers As Integer
    Public Spoilers As Integer
    Public Tank As Integer
    Public Trunk As Integer
    Public Windows As Integer
    Public Turbo As Boolean
    Public WindowTint As Integer
    Public PrimaryColor As Integer
    Public SecondaryColor As Integer
    Public PearlescentColor As Integer
    Public RimColor As Integer
    Public DashboardColor As Integer
    Public NeonLightsColor As VsColor
    Public TireSmokeColor As VsColor
    Public TireSmoke As Boolean
    Public Livery2 As Integer
    Public XenonLightsColor As Integer
    Public BulletProofTyres As Boolean
    Public CustomPrimaryColor As VsColor
    Public CustomSecondaryColor As VsColor
    Public IsPrimaryColorCustom As Boolean
    Public IsSecondaryColorCustom As Boolean
    Public ColorCombination As Integer
    Public Extras As List(Of Extra)
    Public DirtLevel As Single
    Public BodyHealth As Single
    Public EngineHealth As Single
    Public PetrolTankHealth As Single
    Public FuelLevel As Single
    Public RoofState As Integer
    Public SteeringScale As Single
    Public SteeringAngle As Single
    Public Nitrous As Boolean
    Public Livery1 As Integer
    Public SubWoofer As Boolean
    Public Hydraulics2 As Boolean

    Public Function LoadVehicle(Optional addblip As Boolean = True) As Vehicle
        Dim model As New Model(Hash)
        Dim veh = World.CreateVehicle(model, Position, Rotation.ToHeading)

        With veh
            .IsPersistent = True
            .SetInt(modDecor, GetPlayerCharacter())
            .SetBool(modDecor2, True)
            .SetInt(modDecor3, Identification)

            With veh.Mods
                .InstallModKit()
                .Item(VehicleModType.Spoilers).Index = Spoilers
                .Item(VehicleModType.FrontBumper).Index = FrontBumper
                .Item(VehicleModType.RearBumper).Index = RearBumper
                .Item(VehicleModType.SideSkirt).Index = SideSkirt
                .Item(VehicleModType.Exhaust).Index = Exhaust
                .Item(VehicleModType.Frame).Index = Frame
                .Item(VehicleModType.Grille).Index = Grille
                .Item(VehicleModType.Hood).Index = Hood
                .Item(VehicleModType.Fender).Index = Fender
                .Item(VehicleModType.RightFender).Index = RightFender
                .Item(VehicleModType.Roof).Index = Roof
                .Item(VehicleModType.Engine).Index = Engine
                .Item(VehicleModType.Brakes).Index = Brakes
                .Item(VehicleModType.Transmission).Index = Transmission
                .Item(VehicleModType.Horns).Index = Horns
                .Item(VehicleModType.Suspension).Index = Suspension
                .Item(VehicleModType.Armor).Index = Armor
                .Item(VehicleModType.FrontWheel).Index = FrontWheel
                .Item(VehicleModType.FrontWheel).Variation = FrontWheelVariation
                .Item(VehicleModType.RearWheel).Index = RearWheel
                .Item(VehicleModType.RearWheel).Variation = RearWheelVariation
                .Item(VehicleModType.PlateHolder).Index = PlateHolder
                .Item(VehicleModType.VanityPlates).Index = VanityPlates
                .Item(VehicleModType.TrimDesign).Index = TrimDesign
                .Item(VehicleModType.Ornaments).Index = Ornaments
                .Item(VehicleModType.Dashboard).Index = Dashboard
                .Item(VehicleModType.DialDesign).Index = DialDesign
                .Item(VehicleModType.DoorSpeakers).Index = DoorSpeakers
                .Item(VehicleModType.Seats).Index = Seats
                .Item(VehicleModType.SteeringWheels).Index = SteeringWheels
                .Item(VehicleModType.ColumnShifterLevers).Index = ColumnShifterLevers
                .Item(VehicleModType.Plaques).Index = Plaques
                .Item(VehicleModType.Speakers).Index = Speakers
                .Item(VehicleModType.Trunk).Index = Trunk
                .Item(VehicleModType.Hydraulics).Index = Hydraulics
                .Item(VehicleModType.EngineBlock).Index = EngineBlock
                .Item(VehicleModType.AirFilter).Index = AirFilter
                .Item(VehicleModType.Struts).Index = Struts
                .Item(VehicleModType.ArchCover).Index = ArchCover
                .Item(VehicleModType.Aerials).Index = Aerials
                .Item(VehicleModType.Trim).Index = Trim
                .Item(VehicleModType.Tank).Index = Tank
                .Item(VehicleModType.Windows).Index = Windows
                .Item(VehicleModType.Livery).Index = Livery

                .Item(CType(VehicleToggleModTypeEx.Nitrous, VehicleToggleModType)).IsInstalled = Nitrous
                .Item(VehicleToggleModType.Turbo).IsInstalled = Turbo
                .Item(CType(VehicleToggleModTypeEx.Subwoofer, VehicleToggleModType)).IsInstalled = SubWoofer
                .Item(VehicleToggleModType.TireSmoke).IsInstalled = TireSmoke
                .Item(CType(VehicleToggleModTypeEx.Hydraulics, VehicleToggleModType)).IsInstalled = Hydraulics2
                .Item(VehicleToggleModType.XenonHeadlights).IsInstalled = XenonHeadlights

                .SetNeonLightsOn(VehicleNeonLight.Front, FrontNeon)
                .SetNeonLightsOn(VehicleNeonLight.Back, BackNeon)
                .SetNeonLightsOn(VehicleNeonLight.Left, LeftNeon)
                .SetNeonLightsOn(VehicleNeonLight.Right, RightNeon)

                .LicensePlateStyle = LicensePlateStyle
                .LicensePlate = LicensePlate
                .TrimColor = TrimColor
                .WheelType = WheelType
                .WindowTint = WindowTint
                .PrimaryColor = PrimaryColor
                .SecondaryColor = SecondaryColor
                .PearlescentColor = PearlescentColor
                .RimColor = RimColor
                .DashboardColor = DashboardColor
                .NeonLightsColor = NeonLightsColor.ToColor
                .TireSmokeColor = TireSmokeColor.ToColor
                .Livery = Livery1
                If IsPrimaryColorCustom Then .CustomPrimaryColor = CustomPrimaryColor.ToColor
                If IsSecondaryColorCustom Then .CustomSecondaryColor = CustomSecondaryColor.ToColor
                .ColorCombination = ColorCombination
            End With

            .CanTiresBurst = BulletProofTyres
            .XenonLightsColor(XenonLightsColor)
            Extras.ForEach(Sub(x) .ToggleExtra(x.Index, x.ExtraOn))
            .DirtLevel = DirtLevel
            .BodyHealth = BodyHealth
            .EngineHealth = EngineHealth
            .PetrolTankHealth = PetrolTankHealth
            .FuelLevel = FuelLevel
            .RoofState = RoofState
            .SteeringScale = SteeringScale
            .SteeringAngle = SteeringAngle
            .Livery2(Livery2)

            If addblip Then
                .AddBlip()
                .AttachedBlip.Sprite = .GetSprite
                Select Case Owner
                    Case 0
                        .AttachedBlip.Color = BlipColor.Michael
                    Case 1
                        .AttachedBlip.Color = BlipColor.Franklin
                    Case 2
                        .AttachedBlip.Color = BlipColor.Trevor
                    Case Else
                        .AttachedBlip.Color = BlipColor.NetPlayer1
                End Select
                .AttachedBlip.IsShortRange = True
                .AttachedBlip.Name = $"{MakeName} {DisplayName}"
            End If

        End With

        Return veh
    End Function

    Public Sub New(veh As Vehicle, _owner As Integer)
        Try
            MakeName = Vehicle.GetModelMakeName(veh.Model)
            DisplayName = Vehicle.GetModelDisplayName(veh.Model)
            Hash = veh.Model.Hash
            Position = veh.Position
            Rotation = veh.Rotation
            Owner = _owner
            Identification = If(veh.GetInt(modDecor3) = 0, Now.Ticks, veh.GetInt(modDecor3))

            With veh.Mods
                Spoilers = .Item(VehicleModType.Spoilers).Index
                FrontBumper = .Item(VehicleModType.FrontBumper).Index
                RearBumper = .Item(VehicleModType.RearBumper).Index
                SideSkirt = .Item(VehicleModType.SideSkirt).Index
                Exhaust = .Item(VehicleModType.Exhaust).Index
                Frame = .Item(VehicleModType.Frame).Index
                Grille = .Item(VehicleModType.Grille).Index
                Hood = .Item(VehicleModType.Hood).Index
                Fender = .Item(VehicleModType.Fender).Index
                RightFender = .Item(VehicleModType.RightFender).Index
                Roof = .Item(VehicleModType.Roof).Index
                Engine = .Item(VehicleModType.Engine).Index
                Brakes = .Item(VehicleModType.Brakes).Index
                Transmission = .Item(VehicleModType.Transmission).Index
                Horns = .Item(VehicleModType.Horns).Index
                Suspension = .Item(VehicleModType.Suspension).Index
                Armor = .Item(VehicleModType.Armor).Index
                FrontWheel = .Item(VehicleModType.FrontWheel).Index
                FrontWheelVariation = .Item(VehicleModType.FrontWheel).Variation
                RearWheel = .Item(VehicleModType.RearWheel).Index
                RearWheelVariation = .Item(VehicleModType.RearWheel).Variation
                PlateHolder = .Item(VehicleModType.PlateHolder).Index
                VanityPlates = .Item(VehicleModType.VanityPlates).Index
                TrimDesign = .Item(VehicleModType.TrimDesign).Index
                Ornaments = .Item(VehicleModType.Ornaments).Index
                Dashboard = .Item(VehicleModType.Dashboard).Index
                DialDesign = .Item(VehicleModType.DialDesign).Index
                DoorSpeakers = .Item(VehicleModType.DoorSpeakers).Index
                Seats = .Item(VehicleModType.Seats).Index
                SteeringWheels = .Item(VehicleModType.SteeringWheels).Index
                ColumnShifterLevers = .Item(VehicleModType.ColumnShifterLevers).Index
                Plaques = .Item(VehicleModType.Plaques).Index
                Speakers = .Item(VehicleModType.Speakers).Index
                Trunk = .Item(VehicleModType.Trunk).Index
                Hydraulics = .Item(VehicleModType.Hydraulics).Index
                EngineBlock = .Item(VehicleModType.EngineBlock).Index
                AirFilter = .Item(VehicleModType.AirFilter).Index
                Struts = .Item(VehicleModType.Struts).Index
                ArchCover = .Item(VehicleModType.ArchCover).Index
                Aerials = .Item(VehicleModType.Aerials).Index
                Trim = .Item(VehicleModType.Trim).Index
                Tank = .Item(VehicleModType.Tank).Index
                Windows = .Item(VehicleModType.Windows).Index
                Livery = .Item(VehicleModType.Livery).Index

                Nitrous = .Item(CType(VehicleToggleModTypeEx.Nitrous, VehicleToggleModType)).IsInstalled
                Turbo = .Item(VehicleToggleModType.Turbo).IsInstalled
                SubWoofer = .Item(CType(VehicleToggleModTypeEx.Subwoofer, VehicleToggleModType)).IsInstalled
                TireSmoke = .Item(VehicleToggleModType.TireSmoke).IsInstalled
                Hydraulics2 = .Item(CType(VehicleToggleModTypeEx.Hydraulics, VehicleToggleModType)).IsInstalled
                XenonHeadlights = .Item(VehicleToggleModType.XenonHeadlights).IsInstalled

                FrontNeon = .HasNeonLight(VehicleNeonLight.Front)
                BackNeon = .HasNeonLight(VehicleNeonLight.Back)
                LeftNeon = .HasNeonLight(VehicleNeonLight.Left)
                RightNeon = .HasNeonLight(VehicleNeonLight.Right)

                LicensePlateStyle = .LicensePlateStyle
                LicensePlate = .LicensePlate
                TrimColor = .TrimColor
                WheelType = .WheelType
                WindowTint = .WindowTint
                PrimaryColor = .PrimaryColor
                SecondaryColor = .SecondaryColor
                PearlescentColor = .PearlescentColor
                RimColor = .RimColor
                DashboardColor = .DashboardColor
                NeonLightsColor = .NeonLightsColor.ToVsColor
                TireSmokeColor = .TireSmokeColor.ToVsColor
                Livery1 = .Livery
                CustomPrimaryColor = .CustomPrimaryColor.ToVsColor
                CustomSecondaryColor = .CustomSecondaryColor.ToVsColor
                IsPrimaryColorCustom = .IsPrimaryColorCustom
                IsSecondaryColorCustom = .IsSecondaryColorCustom
                ColorCombination = .ColorCombination
            End With

            BulletProofTyres = veh.CanTiresBurst
            XenonLightsColor = veh.XenonLightsColor()

            Dim xtras As New List(Of Extra)
            For i As Integer = 0 To 15
                If veh.ExtraExists(i) Then xtras.Add(New Extra(i, veh.IsExtraOn(i)))
            Next
            Extras = xtras

            DirtLevel = veh.DirtLevel
            BodyHealth = veh.BodyHealth
            EngineHealth = veh.EngineHealth
            PetrolTankHealth = veh.PetrolTankHealth
            FuelLevel = veh.FuelLevel
            RoofState = veh.RoofState
            SteeringScale = veh.SteeringScale
            SteeringAngle = veh.SteeringAngle
            Livery2 = veh.Livery2
        Catch ex As Exception
            Logger.Log($"{ex.Message}{ex.HResult}{ex.StackTrace}")
        End Try
    End Sub

End Class

Public Class VsColor

    Public Red As Integer
    Public Green As Integer
    Public Blue As Integer

    Public Sub New(r As Integer, g As Integer, b As Integer)
        Red = r
        Green = g
        Blue = b
    End Sub

    Public Function ToColor() As Color
        Return Color.FromArgb(Red, Green, Blue)
    End Function

End Class

Public Class Extra

    Public Index As Integer
    Public ExtraOn As Boolean

    Public Sub New(idx As Integer, ison As Boolean)
        Index = idx
        ExtraOn = ison
    End Sub

End Class

Public Enum VehicleToggleModTypeEx
    Nitrous = 17
    Turbo
    Subwoofer
    TireSmoke
    Hydraulics
    XenonHeadlights
End Enum