namespace ResearchXBRL.Application.Usecase.ImportAccountItems.Transfer
{
    public interface ITransferAccountItemsPresenter
    {
        void Progress(int percentage);
        void Complete();
    }
}
