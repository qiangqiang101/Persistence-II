Imports System.Drawing
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports GTA
Imports GTA.Math
Imports Metadata
Imports Newtonsoft.Json

Public Class VirtualGarage

    Public Property Vehicles() As List(Of PersonalVehicle)

    Public Function Load(filename As String) As VirtualGarage
        Return JsonConvert.DeserializeObject(Of VirtualGarage)(IO.File.ReadAllText(filename))
    End Function

    Public Sub Save(filename As String)
        IO.File.WriteAllText(filename, JsonConvert.SerializeObject(Me, Formatting.Indented))
    End Sub

End Class

Public Class PersonalVehicle

    Public Property MakeName() As String
    Public Property DisplayName() As String
    Public Property Hash() As Integer
    Public Property Position() As Vector3
    Public Property Rotation() As Vector3
    Public Property Owner() As Integer
    Public Property Identification() As Long

    Public Property Aerials() As Integer
    Public Property Suspension() As Integer
    Public Property Armor() As Integer
    Public Property Brakes() As Integer
    Public Property Engine() As Integer
    Public Property Transmission() As Integer
    Public Property FrontBumper() As Integer
    Public Property RearBumper() As Integer
    Public Property SideSkirt() As Integer
    Public Property Trim() As Integer
    Public Property EngineBlock() As Integer
    Public Property AirFilter() As Integer
    Public Property Struts() As Integer
    Public Property ColumnShifterLevers() As Integer
    Public Property Dashboard() As Integer
    Public Property DialDesign() As Integer
    Public Property Ornaments() As Integer
    Public Property Seats() As Integer
    Public Property SteeringWheels() As Integer
    Public Property TrimDesign() As Integer
    Public Property TrimColor() As Integer
    Public Property PlateHolder() As Integer
    Public Property VanityPlates() As Integer
    Public Property LicensePlateType() As Integer
    Public Property LicensePlateStyle() As Integer
    Public Property LicensePlate() As String
    Public Property WheelType() As Integer
    Public Property FrontWheel() As Integer
    Public Property RearWheel() As Integer
    Public Property FrontWheelVariation() As Boolean
    Public Property RearWheelVariation() As Boolean
    Public Property XenonHeadlights() As Boolean
    Public Property FrontNeon() As Boolean
    Public Property BackNeon() As Boolean
    Public Property LeftNeon() As Boolean
    Public Property RightNeon() As Boolean
    Public Property ArchCover() As Integer
    Public Property Exhaust() As Integer
    Public Property Fender() As Integer
    Public Property RightFender() As Integer
    Public Property DoorSpeakers() As Integer
    Public Property Frame() As Integer
    Public Property Grille() As Integer
    Public Property Hood() As Integer
    Public Property Horns() As Integer
    Public Property Hydraulics() As Integer
    Public Property Livery() As Integer
    Public Property Plaques() As Integer
    Public Property Roof() As Integer
    Public Property Speakers() As Integer
    Public Property Spoilers() As Integer
    Public Property Tank() As Integer
    Public Property Trunk() As Integer
    Public Property Windows() As Integer
    Public Property Turbo() As Boolean
    Public Property Tint() As Integer
    Public Property PrimaryColor() As Integer
    Public Property SecondaryColor() As Integer
    Public Property PearlescentColor() As Integer
    Public Property RimColor() As Integer
    Public Property DashboardColor() As Integer
    Public Property NeonLightsColor() As VsColor
    Public Property TireSmokeColor() As VsColor
    Public Property TireSmoke() As Boolean
    Public Property Livery2() As Integer
    Public Property XenonLightsColor() As Integer
    Public Property BulletProofTyres() As Boolean
    Public Property CustomPrimaryColor() As VsColor
    Public Property CustomSecondaryColor() As VsColor
    Public Property IsPrimaryColorCustom() As Boolean
    Public Property IsSecondaryColorCustom() As Boolean
    Public Property ColorCombination() As Integer
    Public Property Extra0() As Boolean
    Public Property Extra1() As Boolean
    Public Property Extra2() As Boolean
    Public Property Extra3() As Boolean
    Public Property Extra4() As Boolean
    Public Property Extra5() As Boolean
    Public Property Extra6() As Boolean
    Public Property Extra7() As Boolean
    Public Property Extra8() As Boolean
    Public Property Extra9() As Boolean
    Public Property Extra10() As Boolean
    Public Property Extra11() As Boolean
    Public Property Extra12() As Boolean
    Public Property Extra13() As Boolean
    Public Property Extra14() As Boolean
    Public Property Extra15() As Boolean
    Public Property DirtLevel() As Single
    Public Property BodyHealth() As Single
    Public Property EngineHealth() As Single
    Public Property PetrolTankHealth() As Single
    Public Property FuelLevel() As Single
    Public Property RoofState() As Integer
    Public Property SteeringScale() As Single
    Public Property SteeringAngle() As Single
    Public Property Nitrous() As Boolean
    Public Property Livery1() As Integer
    Public Property SubWoofer() As Boolean
    Public Property Hydraulics2() As Boolean

    Public Function LoadVehicle() As Vehicle
        Dim model As New Model(Hash)
        Dim veh = World.CreateVehicle(model, Position, Rotation.ToHeading)
        With veh
            .SetInt(modDecor3, Identification)
            With veh.Mods
                .InstallModKit()
                .Item(VehicleModType.Spoilers).Index = Spoilers
                .Item(VehicleModType.FrontBumper).Index = FrontBumper
                .Item(VehicleModType.RearBumper).Index = RearBumper
                .Item(VehicleModType.SideSkirt).Index = SideSkirt
                'todo
            End With
        End With
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

                Nitrous = .Item(VehicleToggleModType.Nitrous).IsInstalled
                Turbo = .Item(VehicleToggleModType.Turbo).IsInstalled
                SubWoofer = .Item(VehicleToggleModType.SubWoofer).IsInstalled
                TireSmoke = .Item(VehicleToggleModType.TireSmoke).IsInstalled
                Hydraulics2 = .Item(VehicleToggleModType.Hydraulics).IsInstalled
                XenonHeadlights = .Item(VehicleToggleModType.XenonHeadlights).IsInstalled

                FrontNeon = .HasNeonLight(VehicleNeonLight.Front)
                BackNeon = .HasNeonLight(VehicleNeonLight.Back)
                LeftNeon = .HasNeonLight(VehicleNeonLight.Left)
                RightNeon = .HasNeonLight(VehicleNeonLight.Right)

                LicensePlateType = .LicensePlateType
                LicensePlateStyle = .LicensePlateStyle
                LicensePlate = .LicensePlate
                TrimColor = .TrimColor
                WheelType = .WheelType
                Tint = .WindowTint
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
            Extra0 = veh.IsExtraOn(0)
            Extra1 = veh.IsExtraOn(1)
            Extra2 = veh.IsExtraOn(2)
            Extra3 = veh.IsExtraOn(3)
            Extra4 = veh.IsExtraOn(4)
            Extra5 = veh.IsExtraOn(5)
            Extra6 = veh.IsExtraOn(6)
            Extra7 = veh.IsExtraOn(7)
            Extra8 = veh.IsExtraOn(8)
            Extra9 = veh.IsExtraOn(9)
            Extra10 = veh.IsExtraOn(10)
            Extra11 = veh.IsExtraOn(11)
            Extra12 = veh.IsExtraOn(12)
            Extra13 = veh.IsExtraOn(13)
            Extra14 = veh.IsExtraOn(14)
            Extra15 = veh.IsExtraOn(15)
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

    Public Property Red() As Integer
    Public Property Green() As Integer
    Public Property Blue() As Integer

    Public Sub New(r As Integer, g As Integer, b As Integer)
        Red = r
        Green = g
        Blue = b
    End Sub

    Public Function ToColor() As Color
        Return Color.FromArgb(Red, Green, Blue)
    End Function

End Class