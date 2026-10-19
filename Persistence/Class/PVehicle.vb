Imports System.Drawing
Imports System.IO
Imports System.Xml.Serialization
Imports GTA
Imports GTA.Math

Public Structure PVehicle

    Public ReadOnly Property Instance As PVehicle
        Get
            Return ReadFromFile()
        End Get
    End Property

    <XmlIgnore>
    Public Property FileName() As String

    Public PlayerVehicles As Vehicles
    Public TrailerVehicles As Vehicles

    Public Sub New(_filename As String)
        FileName = _filename
    End Sub

    Public Sub Save()
        Dim ser = New XmlSerializer(GetType(PVehicle))
        Dim writer As TextWriter = New StreamWriter(FileName)
        ser.Serialize(writer, Me)
        writer.Close()
    End Sub

    Public Function ReadFromFile() As PVehicle
        If Not File.Exists(FileName) Then
            Return New PVehicle(FileName) With {.PlayerVehicles = PlayerVehicles, .TrailerVehicles = TrailerVehicles}
        End If

        Try
            Dim ser = New XmlSerializer(GetType(PVehicle))
            Dim reader As TextReader = New StreamReader(FileName)
            Dim instance = CType(ser.Deserialize(reader), PVehicle)
            reader.Close()
            Return instance
        Catch ex As Exception
            Return New PVehicle(FileName) With {.PlayerVehicles = PlayerVehicles, .TrailerVehicles = TrailerVehicles}
        End Try
    End Function

End Structure

Public Structure Vehicles

    Public Make As String
    Public Name As String
    Public Hash As Integer
    Public Position As Vector3
    Public Rotation As Vector3
    Public Owner As Integer
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
    Public SteeringWheel As Integer
    Public TrimDesign As Integer
    Public TrimColor As Integer
    Public PlateHolder As Integer
    Public VanityPlates As Integer
    Public NumberPlate As Integer
    Public PlateNumber As String
    Public WheelType As Integer
    Public Frontwheels As Integer
    Public BackWheels As Integer
    Public WheelsVariation As Boolean
    Public Headlights As Boolean
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
    Public Horn As Integer
    Public Hydraulics As Integer
    Public Livery As Integer
    Public Plaques As Integer
    Public Roof As Integer
    Public Speakers As Integer
    Public Spoiler As Integer
    Public Tank As Integer
    Public Trunk As Integer
    Public Windows As Integer
    Public Turbo As Boolean
    Public Tint As Integer
    Public PrimaryColor As Integer
    Public SecondaryColor As Integer
    Public PearlescentColor As Integer
    Public RimColor As Integer
    Public LightsColor As Integer
    Public NeonLightsColor As VsColor
    Public TireSmokeColor As VsColor
    Public Tiresmoke As Boolean
    Public Livery2 As Integer
    Public HeadlightsColor As Integer
    Public BulletProofTires As Boolean
    Public CustomPrimaryColor As VsColor
    Public CustomSecondaryColor As VsColor
    Public HasCustomPrimaryColor As Boolean
    Public HasCustomSecondaryColor As Boolean
    Public Extra1, Extra2, Extra3, Extra4, Extra5, Extra6, Extra7, Extra8, Extra9, Extra10, Extra11, Extra12, Extra13, Extra14, Extra15, Extra16 As Boolean?
    Public DirtLevel As Single
    Public BodyHealth As Single
    Public EngineHealth As Single
    Public PetrolTankHealth As Single
    Public RoofState As Integer
    Public SteeringScale As Single
    Public HasTrailer As Boolean
    Public HasTowing As Boolean
    Public HasNitro As Integer
    Public Livery1 As Integer

    Public Sub New(vehicle As Vehicle, _owner As Integer)
        Try
            Make = Game.GetLocalizedString(Vehicle.GetModelMakeName(vehicle.Model))
            Name = vehicle.LocalizedName
            Hash = vehicle.Model.Hash
            Position = vehicle.Position
            Rotation = vehicle.Rotation
            Owner = _owner
            Aerials = vehicle.Mods(VehicleModType.Aerials).Index
            Suspension = vehicle.Mods(VehicleModType.Suspension).Index
            Armor = vehicle.Mods(VehicleModType.Armor).Index
            Brakes = vehicle.Mods(VehicleModType.Brakes).Index
            Engine = vehicle.Mods(VehicleModType.Engine).Index
            Transmission = vehicle.Mods(VehicleModType.Transmission).Index
            FrontBumper = vehicle.Mods(VehicleModType.FrontBumper).Index
            RearBumper = vehicle.Mods(VehicleModType.RearBumper).Index
            SideSkirt = vehicle.Mods(VehicleModType.SideSkirt).Index
            Trim = vehicle.Mods(VehicleModType.Trim).Index
            EngineBlock = vehicle.Mods(VehicleModType.EngineBlock).Index
            AirFilter = vehicle.Mods(VehicleModType.AirFilter).Index
            Struts = vehicle.Mods(VehicleModType.Struts).Index
            ColumnShifterLevers = vehicle.Mods(VehicleModType.ColumnShifterLevers).Index
            Dashboard = vehicle.Mods(VehicleModType.Dashboard).Index
            DialDesign = vehicle.Mods(VehicleModType.DialDesign).Index
            Ornaments = vehicle.Mods(VehicleModType.Ornaments).Index
            Seats = vehicle.Mods(VehicleModType.Seats).Index
            SteeringWheel = vehicle.Mods(VehicleModType.SteeringWheels).Index
            TrimDesign = vehicle.Mods(VehicleModType.TrimDesign).Index
            PlateHolder = vehicle.Mods(VehicleModType.PlateHolder).Index
            VanityPlates = vehicle.Mods(VehicleModType.VanityPlates).Index
            NumberPlate = vehicle.Mods.LicensePlateStyle
            PlateNumber = vehicle.Mods.LicensePlate
            WheelType = vehicle.Mods.WheelType
            Frontwheels = vehicle.Mods(VehicleModType.FrontWheel).Index
            BackWheels = vehicle.Mods(VehicleModType.RearWheel).Index
            WheelsVariation = vehicle.WheelsVariation
            Headlights = vehicle.Mods(VehicleToggleModType.XenonHeadlights).IsInstalled
            FrontNeon = vehicle.Mods.IsNeonLightsOn(VehicleNeonLight.Front)
            BackNeon = vehicle.Mods.IsNeonLightsOn(VehicleNeonLight.Back)
            LeftNeon = vehicle.Mods.IsNeonLightsOn(VehicleNeonLight.Left)
            RightNeon = vehicle.Mods.IsNeonLightsOn(VehicleNeonLight.Right)
            ArchCover = vehicle.Mods(VehicleModType.ArchCover).Index
            Exhaust = vehicle.Mods(VehicleModType.Exhaust).Index
            Fender = vehicle.Mods(VehicleModType.Fender).Index
            RightFender = vehicle.Mods(VehicleModType.RightFender).Index
            DoorSpeakers = vehicle.Mods(VehicleModType.DoorSpeakers).Index
            Frame = vehicle.Mods(VehicleModType.Frame).Index
            Grille = vehicle.Mods(VehicleModType.Grille).Index
            Hood = vehicle.Mods(VehicleModType.Hood).Index
            Horn = vehicle.Mods(VehicleModType.Horns).Index
            Hydraulics = vehicle.Mods(VehicleModType.Hydraulics).Index
            Livery = vehicle.Mods(VehicleModType.Livery).Index
            Plaques = vehicle.Mods(VehicleModType.Plaques).Index
            Roof = vehicle.Mods(VehicleModType.Roof).Index
            Speakers = vehicle.Mods(VehicleModType.Speakers).Index
            Spoiler = vehicle.Mods(VehicleModType.Spoilers).Index
            Tank = vehicle.Mods(VehicleModType.Tank).Index
            Trunk = vehicle.Mods(VehicleModType.Trunk).Index
            Windows = vehicle.Mods(VehicleModType.Windows).Index
            Turbo = vehicle.Mods(VehicleToggleModType.Turbo).IsInstalled
            Tint = vehicle.Mods.WindowTint
            PrimaryColor = vehicle.Mods.PrimaryColor
            SecondaryColor = vehicle.Mods.SecondaryColor
            PearlescentColor = vehicle.Mods.PearlescentColor
            RimColor = vehicle.Mods.RimColor
            LightsColor = vehicle.Mods.DashboardColor
            TrimColor = vehicle.Mods.TrimColor
            NeonLightsColor = vehicle.Mods.NeonLightsColor.ToVsColor
            TireSmokeColor = vehicle.Mods.TireSmokeColor.ToVsColor
            Tiresmoke = vehicle.Mods(VehicleToggleModType.TireSmoke).IsInstalled
            Livery2 = vehicle.Livery2
            HeadlightsColor = vehicle.XenonHeadlightsColor
            BulletProofTires = vehicle.CanTiresBurst
            CustomPrimaryColor = vehicle.Mods.CustomPrimaryColor.ToVsColor
            CustomSecondaryColor = vehicle.Mods.CustomSecondaryColor.ToVsColor
            HasCustomPrimaryColor = vehicle.Mods.IsPrimaryColorCustom
            HasCustomSecondaryColor = vehicle.Mods.IsSecondaryColorCustom
            Extra1 = vehicle.Extras(VehicleExtraIndex.Extra1).Enabled
            Extra2 = vehicle.Extras(VehicleExtraIndex.Extra2).Enabled
            Extra3 = vehicle.Extras(VehicleExtraIndex.Extra3).Enabled
            Extra4 = vehicle.Extras(VehicleExtraIndex.Extra4).Enabled
            Extra5 = vehicle.Extras(VehicleExtraIndex.Extra5).Enabled
            Extra6 = vehicle.Extras(VehicleExtraIndex.Extra6).Enabled
            Extra7 = vehicle.Extras(VehicleExtraIndex.Extra7).Enabled
            Extra8 = vehicle.Extras(VehicleExtraIndex.Extra8).Enabled
            Extra9 = vehicle.Extras(VehicleExtraIndex.Extra9).Enabled
            Extra10 = vehicle.Extras(VehicleExtraIndex.Extra10).Enabled
            Extra11 = vehicle.Extras(VehicleExtraIndex.Extra11).Enabled
            Extra12 = vehicle.Extras(VehicleExtraIndex.Extra12).Enabled
            Extra13 = vehicle.Extras(VehicleExtraIndex.Extra13).Enabled
            Extra14 = vehicle.Extras(VehicleExtraIndex.Extra14).Enabled
            Extra15 = vehicle.Extras(VehicleExtraIndex.Extra15).Enabled
            Extra16 = vehicle.Extras(VehicleExtraIndex.Extra16).Enabled
            DirtLevel = vehicle.DirtLevel
            BodyHealth = vehicle.BodyHealth
            EngineHealth = vehicle.EngineHealth
            PetrolTankHealth = vehicle.PetrolTankHealth
            RoofState = vehicle.RoofState
            SteeringScale = vehicle.SteeringScale
            HasTrailer = vehicle.HasTrailer
            HasTowing = vehicle.HasTowing
            Livery1 = vehicle.Mods.Livery
            If IsNitroModInstalled() Then HasNitro = DecoratorInterface.GetInt(vehicle, nitroModDecor) Else HasNitro = 0
        Catch ex As Exception
            Logger.Log($"{ex.Message}{ex.HResult}{ex.StackTrace}")
        End Try
    End Sub

End Structure

Public Structure VsColor

    Public Red As Integer
    Public Green As Integer
    Public Blue As Integer

    Public Sub New(r As Integer, g As Integer, b As Integer)
        Red = r
        Green = g
        Blue = b
    End Sub

End Structure
