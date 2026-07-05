using SharedKernel.Results;

namespace Application.Features.Settings;

public static class SettingErrors
{
    public static Error NotFound(string code) =>
        Error.NotFound("Setting.NotFound", $"Setting with code '{code}' was not found.");

    public static Error ReadOnly(string code) =>
        Error.Forbidden("Setting.ReadOnly", $"Setting '{code}' is read-only.");

    public static Error InvalidValue(string code) =>
        Error.Business("Setting.InvalidValue", $"Setting '{code}' contains an invalid value.");

    public static Error UnsupportedValueType(string code, short valueType) =>
        Error.Business("Setting.UnsupportedValueType", $"Setting '{code}' uses unsupported value type '{valueType}'.");

    public static Error TypeMismatch(string code, string requestedType) =>
        Error.Business("Setting.TypeMismatch", $"Setting '{code}' cannot be read as '{requestedType}'.");
}
