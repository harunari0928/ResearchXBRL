namespace ResearchXBRL.Application.DTO.Results;

public sealed class Succeeded<T> : IResult<T>
{
    private readonly T result;
    public T Value => result;

    public Succeeded(T result)
    {
        this.result = result;
    }
}

public sealed class Succeeded : IResult { }
