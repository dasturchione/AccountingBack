namespace SharedKernel.Constants;

public static class MovementDirectionIdConst
{
    public const short OUT = -1;
    public const short IN = 1;

    public static bool IsValid(short directionId) =>
        directionId is OUT or IN;

    public static short Reverse(short directionId) => directionId switch
    {
        IN => OUT,
        OUT => IN,
        _ => throw new ArgumentOutOfRangeException(nameof(directionId), directionId, "Unsupported movement direction.")
    };
}
