namespace ResearchXBRL.Application.DTO.Results;

public class Failed : IResult
{
    public string Message { get; init; } = "";
}

public class Failed<T> : IResult<T>
{
    public string Message { get; init; } = "";
}
