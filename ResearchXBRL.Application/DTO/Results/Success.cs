namespace ResearchXBRL.Application.DTO.Results;

public sealed class Success<T> : IResult<T>
{
    private readonly T result;
    public T Value => result;

    public Success(T result)
    {
        this.result = result;
    }
}

public sealed class Success : IResult { }
