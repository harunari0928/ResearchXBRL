namespace ResearchXBRL.Application.Usecase.ImportAccountItems
{
    public interface ITransferAccountItemsPresenter
    {
        void Progress(int percentage);
        void Complete();
    }
}
