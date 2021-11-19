using ResearchXBRL.Application.Usecase.AccountElements.Transfer;
using System;

namespace ResearchXBRL.Presentaion.CreateAccountItemsCSV
{
    public sealed class ConsolePresenter : ITransferAccountElementsPresenter
    {
        public void Progress(int percentage)
        {
            Console.WriteLine(percentage + "%");
        }

        public void Complete()
        {
            Console.WriteLine("complete");
        }
    }
}
