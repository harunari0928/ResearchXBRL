using System;
using ResearchXBRL.Application.Usecase.ReverseLookupAccountItems;

namespace ReverseLookupAccountItems;

public sealed class ConsolePresenter : IReverseLookupAccountItemsPresenter
{
    public void Warn(string message)
    {
        Console.WriteLine(message);
    }
}
