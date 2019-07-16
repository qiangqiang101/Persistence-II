Imports System.Drawing
Imports System.IO
Imports System.Xml.Serialization
Imports GTA
Imports GTA.Math
Imports Metadata

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
            Return New PVehicle(FileName) With {.PlayerVehicles = PlayerVehicles}
        End If

        Try
            Dim ser = New XmlSerializer(GetType(PVehicle))
            Dim reader As TextReader = New StreamReader(FileName)
            Dim instance = CType(ser.Deserialize(reader), PVehicle)
            reader.Close()
            Return instance
        Catch ex As Exception
            Return New PVehicle(FileName) With {.PlayerVehicles = PlayerVehicles}
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
    Public Extra1, Extra2, Extra3, Extra4, Extra5, Extra6, Extra7, Extra8, Extra9 As Boolean
    Public DirtLevel As Single
    Public BodyHealth As Single
    Public EngineHealth As Single
    Public PetrolTankHealth As Single
    Public FuelLevel As Single
    Public RoofState As Integer
    Public SteeringScale As Single
    Public HasTrailer As Boolean
    Public HasTowing As Boolean
    Public HasNitro As Boolean
    Public Livery1 As Integer

    Public Sub New(vehicle As Vehicle, _owner As Integer)
        Try
            Make = vehicle.Make
            Name = vehicle.FriendlyName
            Hash = vehicle.Model.Hash
            Position = vehicle.Position
            Rotation = vehicle.Rotation
            Owner = _owner
            Aerials = vehicle.GetMod(VehicleMod.Aerials)
            Suspension = vehicle.GetMod(VehicleMod.Suspension)
            Armor = vehicle.GetMod(VehicleMod.Armor)
            Brakes = vehicle.GetMod(VehicleMod.Brakes)
            Engine = vehicle.GetMod(VehicleMod.Engine)
            Transmission = vehicle.GetMod(VehicleMod.Transmission)
            FrontBumper = vehicle.GetMod(VehicleMod.FrontBumper)
            RearBumper = vehicle.GetMod(VehicleMod.RearBumper)
            SideSkirt = vehicle.GetMod(VehicleMod.SideSkirt)
            Trim = vehicle.GetMod(VehicleMod.Trim)
            EngineBlock = vehicle.GetMod(VehicleMod.EngineBlock)
            AirFilter = vehicle.GetMod(VehicleMod.AirFilter)
            Struts = vehicle.GetMod(VehicleMod.Struts)
            ColumnShifterLevers = vehicle.GetMod(VehicleMod.ColumnShifterLevers)
            Dashboard = vehicle.GetMod(VehicleMod.Dashboard)
            DialDesign = vehicle.GetMod(VehicleMod.DialDesign)
            Ornaments = vehicle.GetMod(VehicleMod.Ornaments)
            Seats = vehicle.GetMod(VehicleMod.Seats)
            SteeringWheel = vehicle.GetMod(VehicleMod.SteeringWheels)
            TrimDesign = vehicle.GetMod(VehicleMod.TrimDesign)
            TrimColor = vehicle.TrimColor
            PlateHolder = vehicle.GetMod(VehicleMod.PlateHolder)
            VanityPlates = vehicle.GetMod(VehicleMod.VanityPlates)
            NumberPlate = vehicle.NumberPlateType
            PlateNumber = vehicle.NumberPlate
            WheelType = vehicle.WheelType
            Frontwheels = vehicle.GetMod(VehicleMod.FrontWheels)
            BackWheels = vehicle.GetMod(VehicleMod.BackWheels)
            WheelsVariation = vehicle.WheelsVariation
            Headlights = vehicle.IsToggleModOn(VehicleToggleMod.XenonHeadlights)
            FrontNeon = vehicle.IsNeonLightsOn(VehicleNeonLight.Front)
            BackNeon = vehicle.IsNeonLightsOn(VehicleNeonLight.Back)
            LeftNeon = vehicle.IsNeonLightsOn(VehicleNeonLight.Left)
            RightNeon = vehicle.IsNeonLightsOn(VehicleNeonLight.Right)
            ArchCover = vehicle.GetMod(VehicleMod.ArchCover)
            Exhaust = vehicle.GetMod(VehicleMod.Exhaust)
            Fender = vehicle.GetMod(VehicleMod.Fender)
            RightFender = vehicle.GetMod(VehicleMod.RightFender)
            DoorSpeakers = vehicle.GetMod(VehicleMod.DoorSpeakers)
            Frame = vehicle.GetMod(VehicleMod.Frame)
            Grille = vehicle.GetMod(VehicleMod.Grille)
            Hood = vehicle.GetMod(VehicleMod.Hood)
            Horn = vehicle.GetMod(VehicleMod.Horns)
            Hydraulics = vehicle.GetMod(VehicleMod.Hydraulics)
            Livery = vehicle.GetMod(VehicleMod.Livery)
            Plaques = vehicle.GetMod(VehicleMod.Plaques)
            Roof = vehicle.GetMod(VehicleMod.Roof)
            Speakers = vehicle.GetMod(VehicleMod.Speakers)
            Spoiler = vehicle.GetMod(VehicleMod.Spoilers)
            Tank = vehicle.GetMod(VehicleMod.Tank)
            Trunk = vehicle.GetMod(VehicleMod.Trunk)
            Windows = vehicle.GetMod(VehicleMod.Windows)
            Turbo = vehicle.IsToggleModOn(VehicleToggleMod.Turbo)
            Tint = vehicle.WindowTint
            PrimaryColor = vehicle.PrimaryColor
            SecondaryColor = vehicle.SecondaryColor
            PearlescentColor = vehicle.PearlescentColor
            RimColor = vehicle.RimColor
            LightsColor = vehicle.DashboardColor
            NeonLightsColor = vehicle.NeonLightsColor.ToVsColor
            TireSmokeColor = vehicle.TireSmokeColor.ToVsColor
            Tiresmoke = vehicle.IsToggleModOn(VehicleToggleMod.TireSmoke)
            Livery2 = vehicle.Livery2
            HeadlightsColor = vehicle.XenonHeadlightsColor
            BulletProofTires = vehicle.CanTiresBurst
            CustomPrimaryColor = vehicle.CustomPrimaryColor.ToVsColor
            CustomSecondaryColor = vehicle.CustomSecondaryColor.ToVsColor
            HasCustomPrimaryColor = vehicle.IsPrimaryColorCustom
            HasCustomSecondaryColor = vehicle.IsSecondaryColorCustom
            Extra1 = vehicle.IsExtraOn(1)
            Extra2 = vehicle.IsExtraOn(2)
            Extra3 = vehicle.IsExtraOn(3)
            Extra4 = vehicle.IsExtraOn(4)
            Extra5 = vehicle.IsExtraOn(5)
            Extra6 = vehicle.IsExtraOn(6)
            Extra7 = vehicle.IsExtraOn(7)
            Extra8 = vehicle.IsExtraOn(8)
            Extra9 = vehicle.IsExtraOn(9)
            DirtLevel = vehicle.DirtLevel
            BodyHealth = vehicle.BodyHealth
            EngineHealth = vehicle.EngineHealth
            PetrolTankHealth = vehicle.PetrolTankHealth
            FuelLevel = vehicle.FuelLevel
            RoofState = vehicle.RoofState
            SteeringScale = vehicle.SteeringScale
            HasTrailer = vehicle.HasTrailer
            HasTowing = vehicle.HasTowing
            Livery1 = vehicle.Livery
            If IsNitroModInstalled() Then HasNitro = vehicle.GetBool(nitroModDecor) Else HasNitro = False
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
