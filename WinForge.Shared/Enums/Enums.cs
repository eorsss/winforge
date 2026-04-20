namespace WinForge.Shared.Enums;

public enum OrderStatus
{
    Draft = 0,
    Confirmed = 1,
    InProduction = 2,
    Delivered = 3,
    Invoiced = 4,
    Cancelled = 5
}

public enum MaterialType
{
    PVC = 0,
    Aluminium = 1,
    Wood = 2
}

public enum WindowShape
{
    Rectangle = 0,
    Arch = 1,
    AngleLeft = 2,
    AngleRight = 3,
    Trapezoid = 4,
    Triangle = 5
}

public enum UserRole
{
    Admin = 0,
    Sales = 1,
    Production = 2,
    Viewer = 3
}
