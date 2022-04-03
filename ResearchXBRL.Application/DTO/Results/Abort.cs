namespace ResearchXBRL.Application.DTO.Results;

public sealed class Abort : IResult
{
    public string Message { get; init; } = "";
}

public sealed class Abort<T> : IResult<T>
{
    public string Message { get; init; } = "";
}
