namespace ResearchXBRL.Application.Usecase.AccountItems.Transfer
{
    public interface ITransferAccountItemsPresenter
    {
        void Progress(int percentage);
        void Complete();
    }
}
