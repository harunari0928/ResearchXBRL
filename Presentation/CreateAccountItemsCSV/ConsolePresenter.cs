using ResearchXBRL.Application.Usecase.AccountItems.Transfer;
using System;

namespace ResearchXBRL.Presentaion.CreateAccountItemsCSV
{
    public sealed class ConsolePresenter : ITransferAccountItemsPresenter
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
