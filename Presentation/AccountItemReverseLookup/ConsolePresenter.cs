using System;
using ResearchXBRL.Application.Usecase.AccountItemReverseLookup;

namespace AccountItemReverseLookup;

public sealed class ConsolePresenter : IAccountItemReverseLookupPresenter
{
    public void Warn(string message)
    {
        Console.WriteLine(message);
    }
}
